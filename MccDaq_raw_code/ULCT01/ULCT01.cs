// ==============================================================================
//
//  File:                         ULCT01.CS
//
//  Library Call Demonstrated:    8254 Counter Functions
//                                Mccdaq.MccBoard.C8254Config()
//                                Mccdaq.MccBoard.CLoad()
//                                Mccdaq.MccBoard.CIn()
//
//  Purpose:                      Operate the counter.
//
//  Demonstration:                Configures, loads and reads the counter.
//
//  Other Library Calls:          MccDaq.MccService.ErrHandling()
//
//  Special Requirements:         Board 0 must have an 8254 Counter.
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

namespace ULCT01
{
public class frmCountTest : System.Windows.Forms.Form
{
    int CounterType = Counters.clsCounters.CTR8254;
    int NumCtrs, CounterNum;

    // Create a new MccBoard object for Board 0
    MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

	MccDaq.CounterRegister RegName;     //  register name of counter
    Counters.clsCounters CtrProps = new Counters.clsCounters();

    private void frmCountTest_Load(object sender, EventArgs e)
    {
        InitUL();
        NumCtrs = CtrProps.FindCountersOfType(DaqBoard, CounterType, out CounterNum);
        if (NumCtrs == 0)
        {
            lblNoteFreqIn.Text = "Board " + DaqBoard.BoardNum.ToString() + 
                " has no 8254 counters.";
        }
        else
        {

            //  Configure the counter for desired operation
            //   Parameters:
            //     CounterNum :the counter to be setup
            //     Config     :the operation mode of counter to be configured
            MccDaq.C8254Mode Config = MccDaq.C8254Mode.HighOnLastCount;
            MccDaq.ErrorInfo ULStat = DaqBoard.C8254Config(CounterNum, Config);

            //  Send a starting value to the counter with MccDaq.MccBoard.CLoad()
            //   Parameters:
            //     RegName    :the register for loading the counter with the starting value
            //     LoadValue  :the starting value to place in the counter
            int LoadValue = 1000;

            RegName = (CounterRegister)Enum.Parse(typeof(CounterRegister), CounterNum.ToString());
            ULStat = DaqBoard.CLoad(RegName, LoadValue);

            lblNoteFreqIn.Text = 
                "NOTE: There must be a TTL frequency at the counter " + 
                CounterNum.ToString() + " input on board " +
                DaqBoard.BoardNum.ToString() + ".";
            this.lblCountRead.Text = "Value read from counter " + CounterNum.ToString() + ":";
            lblCountLoaded.Text = "Counter starting value loaded:";
            lblShowLoadVal.Text = LoadValue.ToString("0");
            tmrReadCount.Enabled = true;
        }
    }

	private void tmrReadCount_Tick(object eventSender, System.EventArgs eventArgs) 
    {
		tmrReadCount.Stop();
		
		//  Read the counter value
		//  Parameters:
        //    CounterNum	:the counter to be read
        //    Count			:the value read from the counter
		ushort Count = 0;
		MccDaq.ErrorInfo ULStat = DaqBoard.CIn(CounterNum, out Count);
		lblShowCountRead.Text = Count.ToString("0");
		
		tmrReadCount.Start();
    }

    private void cmdStopRead_Click(object eventSender, System.EventArgs eventArgs) /* Handles cmdStopRead.Click */
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
        this.tmrReadCount = new System.Windows.Forms.Timer(this.components);
        this.lblShowCountRead = new System.Windows.Forms.Label();
        this.lblCountRead = new System.Windows.Forms.Label();
        this.lblShowLoadVal = new System.Windows.Forms.Label();
        this.lblCountLoaded = new System.Windows.Forms.Label();
        this.lblNoteFreqIn = new System.Windows.Forms.Label();
        this.lblDemoFunction = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // cmdStopRead
        // 
        this.cmdStopRead.BackColor = System.Drawing.SystemColors.Control;
        this.cmdStopRead.Cursor = System.Windows.Forms.Cursors.Default;
        this.cmdStopRead.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.cmdStopRead.ForeColor = System.Drawing.SystemColors.ControlText;
        this.cmdStopRead.Location = new System.Drawing.Point(200, 208);
        this.cmdStopRead.Name = "cmdStopRead";
        this.cmdStopRead.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.cmdStopRead.Size = new System.Drawing.Size(57, 33);
        this.cmdStopRead.TabIndex = 6;
        this.cmdStopRead.Text = "Quit";
        this.cmdStopRead.UseVisualStyleBackColor = false;
        this.cmdStopRead.Click += new System.EventHandler(this.cmdStopRead_Click);
        // 
        // tmrReadCount
        // 
        this.tmrReadCount.Interval = 500;
        this.tmrReadCount.Tick += new System.EventHandler(this.tmrReadCount_Tick);
        // 
        // lblShowCountRead
        // 
        this.lblShowCountRead.BackColor = System.Drawing.SystemColors.Window;
        this.lblShowCountRead.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblShowCountRead.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblShowCountRead.ForeColor = System.Drawing.Color.Blue;
        this.lblShowCountRead.Location = new System.Drawing.Point(200, 168);
        this.lblShowCountRead.Name = "lblShowCountRead";
        this.lblShowCountRead.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblShowCountRead.Size = new System.Drawing.Size(65, 17);
        this.lblShowCountRead.TabIndex = 5;
        // 
        // lblCountRead
        // 
        this.lblCountRead.BackColor = System.Drawing.SystemColors.Window;
        this.lblCountRead.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblCountRead.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblCountRead.ForeColor = System.Drawing.SystemColors.WindowText;
        this.lblCountRead.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
        this.lblCountRead.Location = new System.Drawing.Point(24, 168);
        this.lblCountRead.Name = "lblCountRead";
        this.lblCountRead.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblCountRead.Size = new System.Drawing.Size(166, 17);
        this.lblCountRead.TabIndex = 3;
        this.lblCountRead.Text = "Value read from counter:";
        this.lblCountRead.TextAlign = System.Drawing.ContentAlignment.TopRight;
        // 
        // lblShowLoadVal
        // 
        this.lblShowLoadVal.BackColor = System.Drawing.SystemColors.Window;
        this.lblShowLoadVal.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblShowLoadVal.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblShowLoadVal.ForeColor = System.Drawing.Color.Blue;
        this.lblShowLoadVal.Location = new System.Drawing.Point(200, 136);
        this.lblShowLoadVal.Name = "lblShowLoadVal";
        this.lblShowLoadVal.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblShowLoadVal.Size = new System.Drawing.Size(65, 17);
        this.lblShowLoadVal.TabIndex = 4;
        // 
        // lblCountLoaded
        // 
        this.lblCountLoaded.BackColor = System.Drawing.SystemColors.Window;
        this.lblCountLoaded.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblCountLoaded.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblCountLoaded.ForeColor = System.Drawing.SystemColors.WindowText;
        this.lblCountLoaded.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
        this.lblCountLoaded.Location = new System.Drawing.Point(14, 136);
        this.lblCountLoaded.Name = "lblCountLoaded";
        this.lblCountLoaded.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblCountLoaded.Size = new System.Drawing.Size(177, 17);
        this.lblCountLoaded.TabIndex = 2;
        this.lblCountLoaded.TextAlign = System.Drawing.ContentAlignment.TopRight;
        // 
        // lblNoteFreqIn
        // 
        this.lblNoteFreqIn.BackColor = System.Drawing.SystemColors.Window;
        this.lblNoteFreqIn.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblNoteFreqIn.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblNoteFreqIn.ForeColor = System.Drawing.Color.Red;
        this.lblNoteFreqIn.Location = new System.Drawing.Point(24, 80);
        this.lblNoteFreqIn.Name = "lblNoteFreqIn";
        this.lblNoteFreqIn.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblNoteFreqIn.Size = new System.Drawing.Size(233, 33);
        this.lblNoteFreqIn.TabIndex = 1;
        this.lblNoteFreqIn.Text = "NOTE: There must be a TTL frequency at the counter 1 input.";
        this.lblNoteFreqIn.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // lblDemoFunction
        // 
        this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
        this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
        this.lblDemoFunction.Location = new System.Drawing.Point(16, 16);
        this.lblDemoFunction.Name = "lblDemoFunction";
        this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblDemoFunction.Size = new System.Drawing.Size(249, 41);
        this.lblDemoFunction.TabIndex = 0;
        this.lblDemoFunction.Text = "Demonstration of 8254 Counter Functions";
        this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // frmCountTest
        // 
        this.AcceptButton = this.cmdStopRead;
        this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
        this.BackColor = System.Drawing.SystemColors.Window;
        this.ClientSize = new System.Drawing.Size(280, 261);
        this.Controls.Add(this.cmdStopRead);
        this.Controls.Add(this.lblShowCountRead);
        this.Controls.Add(this.lblCountRead);
        this.Controls.Add(this.lblShowLoadVal);
        this.Controls.Add(this.lblCountLoaded);
        this.Controls.Add(this.lblNoteFreqIn);
        this.Controls.Add(this.lblDemoFunction);
        this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ForeColor = System.Drawing.SystemColors.WindowText;
        this.Location = new System.Drawing.Point(7, 103);
        this.Name = "frmCountTest";
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        this.Text = "Universal Library 8254 Counter";
        this.Load += new System.EventHandler(this.frmCountTest_Load);
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
        Application.Run(new frmCountTest());
    }

    public frmCountTest()
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
    public Timer tmrReadCount;
    public Label lblShowCountRead;
    public Label lblCountRead;
    public Label lblShowLoadVal;
    public Label lblCountLoaded;
    public Label lblNoteFreqIn;
    public Label lblDemoFunction;
#endregion

}
}