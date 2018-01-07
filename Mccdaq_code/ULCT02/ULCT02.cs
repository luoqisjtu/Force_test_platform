// ==============================================================================
//
//  File:                         ULCT02.CS
//
//  Library Call Demonstrated:    9513 Counter Functions
//                                Mccdaq.MccBoard.C9513Init()
//                                Mccdaq.MccBoard.C9513Config()
//                                Mccdaq.MccBoard.CLoad()
//                                Mccdaq.MccBoard.CIn()
//
//  Purpose:                      Operate the counter.
//
//  Demonstration:                Initializes, configures, loads and checks
//                                the counter
//
//  Other Library Calls:          MccDaq.MccService.ErrHandling()
//
//  Special Requirements:         Board 0 must have a 9513 Counter.
//                                Program uses internal clock to count.
//
// ==============================================================================
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;

using MccDaq;
using ErrorDefs;

namespace ULCT02
{
public class frm9513Ctr : System.Windows.Forms.Form
{
    int CounterType = Counters.clsCounters.CTR9513;
    int NumCtrs, CounterNum;

    // Create a new MccBoard object for Board 0
	MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

    Counters.clsCounters CtrProps = new Counters.clsCounters();

    private void frm9513Ctr_Load(object sender, EventArgs e)
    {
        InitUL();
        NumCtrs = CtrProps.FindCountersOfType(DaqBoard, CounterType, out CounterNum);
        if (NumCtrs == 0)
        {
            lblDemoFunction.Text = "Board " + DaqBoard.BoardNum.ToString() + 
                " has no 9513 counters.";
            lblDemoFunction.ForeColor = Color.Red;
        }
        else
        {

            //  Initialize the board level features
            //   Parameters:
            //     ChipNum       :Chip to be initialized (1 for CTR05, 1 or 2 for CTR10)
            //     FOutDivider   :the F-Out divider (0-15)
            //     FOutSource    :the signal source for F-Out
            //     Compare1      :status of comparator 1
            //     Compare2      :status of comparator 2
            //     TimeOfDay     :time of day mode control
            short FOutDivider = 0;
            int ChipNum = 1;
            MccDaq.CounterSource FOutSource = MccDaq.CounterSource.Freq4;
            MccDaq.CompareValue Compare1 = MccDaq.CompareValue.Disabled;
            MccDaq.CompareValue Compare2 = MccDaq.CompareValue.Disabled;
            MccDaq.TimeOfDay TimeOfDayCounting = MccDaq.TimeOfDay.Disabled;
            MccDaq.ErrorInfo ULStat = DaqBoard.C9513Init(ChipNum, FOutDivider, FOutSource, Compare1, Compare2, TimeOfDayCounting);


            //  Set the configurable operations of the counter
            //   Parameters:
            //     CounterNum     :the counter to be configured (1 to 5 for CTR05)
            //     GateControl    :gate control value
            //     CounterEdge    :which edge to count
            //     CountSource    :signal source
            //     SpecialGate    :status of special gate
            //     Reload         :method of reloading
            //     RecyleMode     :recyle mode
            //     BCDMode        :counting mode, Binary or BCD
            //     CountDirection :direction for the counting operation (COUNTUP or COUNTDOWN)
            //     OutputControl  :output signal type and level
            MccDaq.GateControl GateControl = MccDaq.GateControl.NoGate;
            MccDaq.CountEdge CounterEdge = MccDaq.CountEdge.PositiveEdge;
            MccDaq.CounterSource CountSource = MccDaq.CounterSource.Freq4;
            MccDaq.OptionState SpecialGate = MccDaq.OptionState.Disabled;
            MccDaq.Reload Reload = MccDaq.Reload.LoadReg;
            MccDaq.RecycleMode RecycleMode = MccDaq.RecycleMode.Recycle;
            MccDaq.BCDMode BCDMode = MccDaq.BCDMode.Disabled;
            MccDaq.CountDirection CountDirection = MccDaq.CountDirection.CountUp;
            MccDaq.C9513OutputControl OutputControl = MccDaq.C9513OutputControl.AlwaysLow;
            ULStat = DaqBoard.C9513Config(CounterNum, GateControl, CounterEdge, CountSource, SpecialGate, Reload, RecycleMode, BCDMode, CountDirection, OutputControl);


            //  Send a starting value to the counter with MccDaq.MccBoard.CLoad()
            //   Parameters:
            //     RegName    :the register for loading the counter with the starting value
            //     LoadValue  :the starting value to place in the counter
            MccDaq.CounterRegister RegName = MccDaq.CounterRegister.LoadReg1; //  name of register in counter 1
            int LoadValue = 1000;
            ULStat = DaqBoard.CLoad(RegName, LoadValue);

            lblLoadValue.Text = "Value loaded to counter " + CounterNum.ToString() + ":";
            lblShowLoadVal.Text = LoadValue.ToString("0");
            this.lblDemoFunction.Text = "Demonstration of 9513 Counter Functions using board "
                + DaqBoard.BoardNum.ToString() + ".";
            tmrReadCounter.Enabled = true;
        }

    }

    private void tmrReadCounter_Tick(object eventSender, System.EventArgs eventArgs) 
    {
				
		tmrReadCounter.Stop();
		
		// Read the counter value 
		//  Parameters:
        //    CounterNum	:the counter to be read
        //    Count			:the count value in the counter
		ushort Count = 0;
        MccDaq.ErrorInfo ULStat = DaqBoard.CIn(CounterNum, out Count);

        lblReadValue.Text = "Value read from counter " + CounterNum.ToString() + ":";
        lblShowReadVal.Text = Count.ToString("0");

		tmrReadCounter.Start();
    }

    private void cmdStopRead_Click(object eventSender, System.EventArgs eventArgs)
    {
        Application.Exit();
    }

    private void InitUL()
    {
        //  Initiate error handling
        //   activating error handling will trap errors like
        //   bad channel numbers and non-configured conditions.
        //   Parameters:
        //     MccDaq.ErrorReporting.PrintAll :all warnings and errors encountered will be printed
        //     MccDaq.ErrorHandling.StopAll   :if an error is encountered, the program will stop
        clsErrorDefs.ReportError = MccDaq.ErrorReporting.PrintAll;
        clsErrorDefs.HandleError = MccDaq.ErrorHandling.StopAll;
        MccDaq.ErrorInfo ULStat = MccDaq.MccService.ErrHandling
            (MccDaq.ErrorReporting.PrintAll, MccDaq.ErrorHandling.StopAll);

    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
        this.cmdStopRead = new System.Windows.Forms.Button();
        this.tmrReadCounter = new System.Windows.Forms.Timer(this.components);
        this.lblShowReadVal = new System.Windows.Forms.Label();
        this.lblReadValue = new System.Windows.Forms.Label();
        this.lblShowLoadVal = new System.Windows.Forms.Label();
        this.lblLoadValue = new System.Windows.Forms.Label();
        this.lblDemoFunction = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // cmdStopRead
        // 
        this.cmdStopRead.BackColor = System.Drawing.SystemColors.Control;
        this.cmdStopRead.Cursor = System.Windows.Forms.Cursors.Default;
        this.cmdStopRead.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.cmdStopRead.ForeColor = System.Drawing.SystemColors.ControlText;
        this.cmdStopRead.Location = new System.Drawing.Point(232, 184);
        this.cmdStopRead.Name = "cmdStopRead";
        this.cmdStopRead.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.cmdStopRead.Size = new System.Drawing.Size(54, 27);
        this.cmdStopRead.TabIndex = 5;
        this.cmdStopRead.Text = "Quit";
        this.cmdStopRead.UseVisualStyleBackColor = false;
        this.cmdStopRead.Click += new System.EventHandler(this.cmdStopRead_Click);
        // 
        // tmrReadCounter
        // 
        this.tmrReadCounter.Interval = 500;
        this.tmrReadCounter.Tick += new System.EventHandler(this.tmrReadCounter_Tick);
        // 
        // lblShowReadVal
        // 
        this.lblShowReadVal.BackColor = System.Drawing.SystemColors.Window;
        this.lblShowReadVal.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblShowReadVal.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblShowReadVal.ForeColor = System.Drawing.Color.Blue;
        this.lblShowReadVal.Location = new System.Drawing.Point(232, 120);
        this.lblShowReadVal.Name = "lblShowReadVal";
        this.lblShowReadVal.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblShowReadVal.Size = new System.Drawing.Size(73, 17);
        this.lblShowReadVal.TabIndex = 2;
        // 
        // lblReadValue
        // 
        this.lblReadValue.BackColor = System.Drawing.SystemColors.Window;
        this.lblReadValue.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblReadValue.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblReadValue.ForeColor = System.Drawing.SystemColors.WindowText;
        this.lblReadValue.Location = new System.Drawing.Point(53, 120);
        this.lblReadValue.Name = "lblReadValue";
        this.lblReadValue.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblReadValue.Size = new System.Drawing.Size(161, 17);
        this.lblReadValue.TabIndex = 4;
        this.lblReadValue.TextAlign = System.Drawing.ContentAlignment.TopRight;
        // 
        // lblShowLoadVal
        // 
        this.lblShowLoadVal.BackColor = System.Drawing.SystemColors.Window;
        this.lblShowLoadVal.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblShowLoadVal.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblShowLoadVal.ForeColor = System.Drawing.Color.Blue;
        this.lblShowLoadVal.Location = new System.Drawing.Point(232, 88);
        this.lblShowLoadVal.Name = "lblShowLoadVal";
        this.lblShowLoadVal.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblShowLoadVal.Size = new System.Drawing.Size(73, 17);
        this.lblShowLoadVal.TabIndex = 1;
        // 
        // lblLoadValue
        // 
        this.lblLoadValue.BackColor = System.Drawing.SystemColors.Window;
        this.lblLoadValue.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblLoadValue.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblLoadValue.ForeColor = System.Drawing.SystemColors.WindowText;
        this.lblLoadValue.Location = new System.Drawing.Point(53, 88);
        this.lblLoadValue.Name = "lblLoadValue";
        this.lblLoadValue.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblLoadValue.Size = new System.Drawing.Size(161, 17);
        this.lblLoadValue.TabIndex = 3;
        this.lblLoadValue.TextAlign = System.Drawing.ContentAlignment.TopRight;
        // 
        // lblDemoFunction
        // 
        this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
        this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
        this.lblDemoFunction.Location = new System.Drawing.Point(48, 16);
        this.lblDemoFunction.Name = "lblDemoFunction";
        this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblDemoFunction.Size = new System.Drawing.Size(225, 41);
        this.lblDemoFunction.TabIndex = 0;
        this.lblDemoFunction.Text = "Demonstration of 9513 Counter Functions.";
        this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // frm9513Ctr
        // 
        this.AcceptButton = this.cmdStopRead;
        this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
        this.BackColor = System.Drawing.SystemColors.Window;
        this.ClientSize = new System.Drawing.Size(339, 243);
        this.Controls.Add(this.cmdStopRead);
        this.Controls.Add(this.lblShowReadVal);
        this.Controls.Add(this.lblReadValue);
        this.Controls.Add(this.lblShowLoadVal);
        this.Controls.Add(this.lblLoadValue);
        this.Controls.Add(this.lblDemoFunction);
        this.Cursor = System.Windows.Forms.Cursors.Default;
        this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ForeColor = System.Drawing.SystemColors.WindowText;
        this.Location = new System.Drawing.Point(7, 103);
        this.Name = "frm9513Ctr";
        this.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        this.Text = "Universal Library 9513 Counter Demo";
        this.Load += new System.EventHandler(this.frm9513Ctr_Load);
        this.ResumeLayout(false);

    }

#endregion

    #region Form initialization, variables, and entry point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() 
    {
        Application.Run(new frm9513Ctr());
    }

    public frm9513Ctr()
    {

        // This call is required by the Windows Form Designer.
        InitializeComponent();

    }

    // Form overrides dispose to clean up the component list.
    protected override void Dispose(bool Disposing)
    {
        if (Disposing)
        {
            if (components != null)
            {
                components.Dispose();
            }
        }
        base.Dispose(Disposing);
    }


    // Required by the Windows Form Designer
    private System.ComponentModel.IContainer components;

    public ToolTip ToolTip1;
    public Button cmdStopRead;
    public Timer tmrReadCounter;
    public Label lblShowReadVal;
    public Label lblReadValue;
    public Label lblShowLoadVal;
    public Label lblLoadValue;
    public Label lblDemoFunction;

    #endregion

}
}