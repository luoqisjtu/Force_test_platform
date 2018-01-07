// ==============================================================================
//
//  File:                         ULAI07.CS
//
//  Library Call Demonstrated:    MccDaq.MccBoard.ATrig()
//
//  Purpose:                      Waits for a specified analog input channel to
//                                go above or below a specified value.
//
//  Demonstration:                Displays the digital value of a user-specified
//                                analog input channel when the user-specifed
//                                value is detected.
//
//  Other Library Calls:          Mccdaq.MccBoard.ErrHandling()
//
//  Special Requirements:         Board 0 must have an A/D converter.
//                                Analog signal on an input channel.
//
// ==============================================================================
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;

using AnalogIO;
using MccDaq;
using ErrorDefs;

namespace ULAI07
{
	public class frmAnalogTrig : System.Windows.Forms.Form
	{

        // Create a new MccBoard object for Board 0
        MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

        MccDaq.Range Range;
        private int ADResolution, NumAIChans;
        private int HighChan, LowChan;
        public Label lblWarn;
        public Label lblVoltStatus;

        AnalogIO.clsAnalogIO AIOProps = new AnalogIO.clsAnalogIO();
        ErrorDefs.clsErrorDefs ErrorTypes = new ErrorDefs.clsErrorDefs();

        private void frmAnalogTrig_Load(object sender, EventArgs e)
        {


            MccDaq.TriggerType DefaultTrig;

            InitUL();

            // determine the number of analog channels and their capabilities
            int ChannelType = clsAnalogIO.ANALOGINPUT;
            NumAIChans = AIOProps.FindAnalogChansOfType(DaqBoard, ChannelType,
                out ADResolution, out Range, out LowChan, out DefaultTrig);

            if (NumAIChans == 0)
            {
                lblWarn.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " does not have analog input channels.";
            }
            else if (ADResolution > 16)
            {
                lblWarn.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " resolution is greater than 16-bit. The ATrig function " +
                    "does not support high resolution devices.";
            }
            else
            {
                lblWarn.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " collecting analog data meeting trigger criterea " +
                    " with input Range set to " + Range.ToString() + ".";
                HighChan = LowChan + NumAIChans - 1;
                lblTriggerChan.Text = "Enter a channel (" +
                    LowChan.ToString() + " - " + HighChan.ToString() + "):";
                UpdateTrigCriterea();
                cmdStartConvert.Enabled = true;
                this.txtShowChannel.Enabled = true;
                this.txtShowTrigSet.Enabled = true;
                this.chkNegTrigger.Enabled = true;
                this.chkPosTrigger.Enabled = true;
            }

        }

        private void cmdStartConvert_Click(object eventSender, System.EventArgs eventArgs)
        {
            if (tmrStartConvert.Enabled)
            {
                cmdStartConvert.Text = "Start";
                lblTrigStatus.Text = "";
                tmrStartConvert.Enabled = false;
            }
            else
            {
                cmdStartConvert.Text = "Stop";
                tmrStartConvert.Enabled = true;
                lblTrigStatus.Text = "Waiting for trigger...";
            }
        }

        private void tmrStartConvert_Tick(object eventSender, System.EventArgs eventArgs) /* Handles tmrStartConvert.Tick */
        {
            ushort DataValue;
            int Chan = int.Parse(txtShowChannel.Text);
            float EngUnits = float.Parse(txtShowTrigSet.Text);
            ushort TrigValue = GetTrigCounts(Range, EngUnits);

            MccDaq.TriggerType TrigType;
            if (chkNegTrigger.Checked == true)
            {
                TrigType = MccDaq.TriggerType.TrigBelow;
            }
            else
            {
                TrigType = MccDaq.TriggerType.TrigAbove;
            }

            tmrStartConvert.Stop();

            //  Monitor the channel with MccDaq.MccBoard.ATrig
            //   The input value that meets the threshold will become DataValue
            //   The data value will be updated and displayed until a Stop event occurs.
            //   Parameters:
            //     Chan       :the input channel number
            //     TrigType   :specifies whether the trigger is to be above
            //                 or below TrigValue
            //     TrigValue  :the threshold value that will cause the trigger
            //     Range      :the range for the board
            //     DataValue  :the input value read from Chan
            MccDaq.ErrorInfo ULStat = DaqBoard.ATrig(Chan, TrigType, TrigValue, Range, out DataValue);


            //  print the value that meets the threshold
            lblTrigStatus.Text = "The value that caused the last trigger was:";
            lblShowTrigValue.Text = DataValue.ToString("D");

            ULStat = DaqBoard.ToEngUnits(Range, DataValue, out EngUnits);
            lblShowVolts.Text = EngUnits.ToString("0.00###") + "V";

            tmrStartConvert.Start();
        }

        private ushort GetTrigCounts(MccDaq.Range range, float EngUnits)
        {

            float fCounts;
            MccDaq.ErrorInfo ULStat;
            int FSCounts;
            float FSEngUnits;
            bool RangeIsBipolar = false;


            // check if range is bipolar or unipolar
            FSCounts = 0;
            FSEngUnits = 0.0F;
            ULStat = DaqBoard.ToEngUnits(range, System.Convert.ToUInt16(FSCounts), out FSEngUnits);
            if (FSEngUnits < 0) RangeIsBipolar = true;

            FSCounts = (int)Math.Pow(2, ADResolution) - 1;
            ULStat = DaqBoard.ToEngUnits(range, System.Convert.ToUInt16(FSCounts), out FSEngUnits);

            if (RangeIsBipolar)
                fCounts = (float)((FSCounts / 2.0F) * (1.0F + EngUnits / FSEngUnits));
            else
                fCounts = FSCounts * EngUnits / FSEngUnits;
            
            if (fCounts > FSCounts) fCounts = FSCounts;
            if (fCounts < 0) fCounts = 0;

            return Convert.ToUInt16(fCounts);
        }

        private void UpdateTrigCriterea()
        {

            string TrigChan, TrigCondition, TrigVoltage;

            TrigCondition = "below";
            if (chkPosTrigger.Checked) TrigCondition = "above";
            TrigVoltage = txtShowTrigSet.Text;
            if (TrigVoltage == "") TrigVoltage = "0";
            TrigChan = txtShowChannel.Text;
            if (TrigChan == "") TrigChan = "0";
            lblVoltStatus.Text = "Apply a voltage or signal to channel " +
            TrigChan.ToString() + " that meets the trigger criterea  ' " +
            TrigCondition + " " + TrigVoltage + " volts '.";

        }

        private void cmdStopConvert_Click(object eventSender, System.EventArgs eventArgs)
        {
            Application.Exit();
        }

        private void txtShowTrigSet_TextChanged(object sender, EventArgs e)
        {
            UpdateTrigCriterea();
        }

        private void chkNegTrigger_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTrigCriterea();
        }

        private void chkPosTrigger_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTrigCriterea();
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
                (ErrorReporting.PrintAll, ErrorHandling.StopAll);

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
            this.cmdStopConvert = new System.Windows.Forms.Button();
            this.cmdStartConvert = new System.Windows.Forms.Button();
            this.txtShowTrigSet = new System.Windows.Forms.TextBox();
            this.chkPosTrigger = new System.Windows.Forms.RadioButton();
            this.chkNegTrigger = new System.Windows.Forms.RadioButton();
            this.txtShowChannel = new System.Windows.Forms.TextBox();
            this.tmrStartConvert = new System.Windows.Forms.Timer(this.components);
            this.lblShowVolts = new System.Windows.Forms.Label();
            this.lblShowTrigValue = new System.Windows.Forms.Label();
            this.lblTrigStatus = new System.Windows.Forms.Label();
            this.lblEnterVal = new System.Windows.Forms.Label();
            this.lblTriggerChan = new System.Windows.Forms.Label();
            this.lblDemoFunction = new System.Windows.Forms.Label();
            this.lblWarn = new System.Windows.Forms.Label();
            this.lblVoltStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdStopConvert
            // 
            this.cmdStopConvert.BackColor = System.Drawing.SystemColors.Control;
            this.cmdStopConvert.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdStopConvert.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdStopConvert.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdStopConvert.Location = new System.Drawing.Point(360, 280);
            this.cmdStopConvert.Name = "cmdStopConvert";
            this.cmdStopConvert.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdStopConvert.Size = new System.Drawing.Size(60, 26);
            this.cmdStopConvert.TabIndex = 7;
            this.cmdStopConvert.Text = "Quit";
            this.cmdStopConvert.UseVisualStyleBackColor = false;
            this.cmdStopConvert.Click += new System.EventHandler(this.cmdStopConvert_Click);
            // 
            // cmdStartConvert
            // 
            this.cmdStartConvert.BackColor = System.Drawing.SystemColors.Control;
            this.cmdStartConvert.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdStartConvert.Enabled = false;
            this.cmdStartConvert.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdStartConvert.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdStartConvert.Location = new System.Drawing.Point(270, 280);
            this.cmdStartConvert.Name = "cmdStartConvert";
            this.cmdStartConvert.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdStartConvert.Size = new System.Drawing.Size(60, 26);
            this.cmdStartConvert.TabIndex = 2;
            this.cmdStartConvert.Text = "Start";
            this.cmdStartConvert.UseVisualStyleBackColor = false;
            this.cmdStartConvert.Click += new System.EventHandler(this.cmdStartConvert_Click);
            // 
            // txtShowTrigSet
            // 
            this.txtShowTrigSet.AcceptsReturn = true;
            this.txtShowTrigSet.BackColor = System.Drawing.SystemColors.Window;
            this.txtShowTrigSet.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtShowTrigSet.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtShowTrigSet.Enabled = false;
            this.txtShowTrigSet.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtShowTrigSet.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtShowTrigSet.Location = new System.Drawing.Point(181, 140);
            this.txtShowTrigSet.MaxLength = 0;
            this.txtShowTrigSet.Name = "txtShowTrigSet";
            this.txtShowTrigSet.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtShowTrigSet.Size = new System.Drawing.Size(45, 20);
            this.txtShowTrigSet.TabIndex = 10;
            this.txtShowTrigSet.Text = "1.25";
            this.txtShowTrigSet.TextChanged += new System.EventHandler(this.txtShowTrigSet_TextChanged);
            // 
            // chkPosTrigger
            // 
            this.chkPosTrigger.BackColor = System.Drawing.SystemColors.Window;
            this.chkPosTrigger.Cursor = System.Windows.Forms.Cursors.Default;
            this.chkPosTrigger.Enabled = false;
            this.chkPosTrigger.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkPosTrigger.ForeColor = System.Drawing.SystemColors.WindowText;
            this.chkPosTrigger.Location = new System.Drawing.Point(248, 134);
            this.chkPosTrigger.Name = "chkPosTrigger";
            this.chkPosTrigger.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.chkPosTrigger.Size = new System.Drawing.Size(166, 20);
            this.chkPosTrigger.TabIndex = 4;
            this.chkPosTrigger.TabStop = true;
            this.chkPosTrigger.Text = "Trigger above this value";
            this.chkPosTrigger.UseVisualStyleBackColor = false;
            this.chkPosTrigger.CheckedChanged += new System.EventHandler(this.chkPosTrigger_CheckedChanged);
            // 
            // chkNegTrigger
            // 
            this.chkNegTrigger.BackColor = System.Drawing.SystemColors.Window;
            this.chkNegTrigger.Checked = true;
            this.chkNegTrigger.Cursor = System.Windows.Forms.Cursors.Default;
            this.chkNegTrigger.Enabled = false;
            this.chkNegTrigger.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkNegTrigger.ForeColor = System.Drawing.SystemColors.WindowText;
            this.chkNegTrigger.Location = new System.Drawing.Point(248, 156);
            this.chkNegTrigger.Name = "chkNegTrigger";
            this.chkNegTrigger.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.chkNegTrigger.Size = new System.Drawing.Size(166, 20);
            this.chkNegTrigger.TabIndex = 3;
            this.chkNegTrigger.TabStop = true;
            this.chkNegTrigger.Text = "Trigger below this value";
            this.chkNegTrigger.UseVisualStyleBackColor = false;
            this.chkNegTrigger.CheckedChanged += new System.EventHandler(this.chkNegTrigger_CheckedChanged);
            // 
            // txtShowChannel
            // 
            this.txtShowChannel.AcceptsReturn = true;
            this.txtShowChannel.BackColor = System.Drawing.SystemColors.Window;
            this.txtShowChannel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtShowChannel.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtShowChannel.Enabled = false;
            this.txtShowChannel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtShowChannel.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtShowChannel.Location = new System.Drawing.Point(237, 39);
            this.txtShowChannel.MaxLength = 0;
            this.txtShowChannel.Name = "txtShowChannel";
            this.txtShowChannel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtShowChannel.Size = new System.Drawing.Size(25, 20);
            this.txtShowChannel.TabIndex = 0;
            this.txtShowChannel.Text = "0";
            // 
            // tmrStartConvert
            // 
            this.tmrStartConvert.Interval = 200;
            this.tmrStartConvert.Tick += new System.EventHandler(this.tmrStartConvert_Tick);
            // 
            // lblShowVolts
            // 
            this.lblShowVolts.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowVolts.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowVolts.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowVolts.ForeColor = System.Drawing.Color.Blue;
            this.lblShowVolts.Location = new System.Drawing.Point(307, 224);
            this.lblShowVolts.Name = "lblShowVolts";
            this.lblShowVolts.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowVolts.Size = new System.Drawing.Size(124, 16);
            this.lblShowVolts.TabIndex = 11;
            this.lblShowVolts.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblShowTrigValue
            // 
            this.lblShowTrigValue.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowTrigValue.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowTrigValue.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowTrigValue.ForeColor = System.Drawing.Color.Blue;
            this.lblShowTrigValue.Location = new System.Drawing.Point(307, 200);
            this.lblShowTrigValue.Name = "lblShowTrigValue";
            this.lblShowTrigValue.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowTrigValue.Size = new System.Drawing.Size(124, 17);
            this.lblShowTrigValue.TabIndex = 5;
            this.lblShowTrigValue.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblTrigStatus
            // 
            this.lblTrigStatus.BackColor = System.Drawing.SystemColors.Window;
            this.lblTrigStatus.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblTrigStatus.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTrigStatus.ForeColor = System.Drawing.Color.Blue;
            this.lblTrigStatus.Location = new System.Drawing.Point(47, 200);
            this.lblTrigStatus.Name = "lblTrigStatus";
            this.lblTrigStatus.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblTrigStatus.Size = new System.Drawing.Size(249, 17);
            this.lblTrigStatus.TabIndex = 6;
            this.lblTrigStatus.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblEnterVal
            // 
            this.lblEnterVal.BackColor = System.Drawing.SystemColors.Window;
            this.lblEnterVal.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblEnterVal.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEnterVal.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblEnterVal.Location = new System.Drawing.Point(12, 143);
            this.lblEnterVal.Name = "lblEnterVal";
            this.lblEnterVal.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblEnterVal.Size = new System.Drawing.Size(162, 22);
            this.lblEnterVal.TabIndex = 12;
            this.lblEnterVal.Text = "Enter a trigger value in volts: ";
            this.lblEnterVal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblTriggerChan
            // 
            this.lblTriggerChan.BackColor = System.Drawing.SystemColors.Window;
            this.lblTriggerChan.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblTriggerChan.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTriggerChan.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblTriggerChan.Location = new System.Drawing.Point(16, 40);
            this.lblTriggerChan.Name = "lblTriggerChan";
            this.lblTriggerChan.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblTriggerChan.Size = new System.Drawing.Size(212, 17);
            this.lblTriggerChan.TabIndex = 1;
            this.lblTriggerChan.Text = "Enter the trigger input channel:";
            this.lblTriggerChan.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblDemoFunction
            // 
            this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
            this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDemoFunction.Location = new System.Drawing.Point(8, 4);
            this.lblDemoFunction.Name = "lblDemoFunction";
            this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDemoFunction.Size = new System.Drawing.Size(352, 20);
            this.lblDemoFunction.TabIndex = 9;
            this.lblDemoFunction.Text = "Demonstration of MccDaq.MccBoard.ATrig()";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWarn
            // 
            this.lblWarn.BackColor = System.Drawing.SystemColors.Window;
            this.lblWarn.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblWarn.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWarn.ForeColor = System.Drawing.Color.Red;
            this.lblWarn.Location = new System.Drawing.Point(45, 71);
            this.lblWarn.Name = "lblWarn";
            this.lblWarn.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblWarn.Size = new System.Drawing.Size(345, 53);
            this.lblWarn.TabIndex = 13;
            this.lblWarn.Text = "Note: Channel above must have an input that meets the trigger conditions or progr" +
                "am will appear to hang.";
            this.lblWarn.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVoltStatus
            // 
            this.lblVoltStatus.BackColor = System.Drawing.SystemColors.Window;
            this.lblVoltStatus.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblVoltStatus.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVoltStatus.ForeColor = System.Drawing.Color.Blue;
            this.lblVoltStatus.Location = new System.Drawing.Point(47, 225);
            this.lblVoltStatus.Name = "lblVoltStatus";
            this.lblVoltStatus.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblVoltStatus.Size = new System.Drawing.Size(249, 39);
            this.lblVoltStatus.TabIndex = 14;
            this.lblVoltStatus.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // frmAnalogTrig
            // 
            this.AcceptButton = this.cmdStartConvert;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(443, 320);
            this.Controls.Add(this.lblVoltStatus);
            this.Controls.Add(this.lblWarn);
            this.Controls.Add(this.cmdStartConvert);
            this.Controls.Add(this.txtShowTrigSet);
            this.Controls.Add(this.chkPosTrigger);
            this.Controls.Add(this.chkNegTrigger);
            this.Controls.Add(this.txtShowChannel);
            this.Controls.Add(this.lblShowVolts);
            this.Controls.Add(this.lblShowTrigValue);
            this.Controls.Add(this.lblTrigStatus);
            this.Controls.Add(this.lblEnterVal);
            this.Controls.Add(this.lblTriggerChan);
            this.Controls.Add(this.lblDemoFunction);
            this.Controls.Add(this.cmdStopConvert);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Location = new System.Drawing.Point(221, 99);
            this.Name = "frmAnalogTrig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Universal Library Analog Trigger";
            this.Load += new System.EventHandler(this.frmAnalogTrig_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

	#endregion

        #region Form initialization, variables, and entry point

        /// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new frmAnalogTrig());
		}

        public frmAnalogTrig()
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
        public Button cmdStopConvert;
        public Button cmdStartConvert;
        public TextBox txtShowTrigSet;
        public RadioButton chkPosTrigger;
        public RadioButton chkNegTrigger;
        public TextBox txtShowChannel;
        public Timer tmrStartConvert;
        public Label lblShowVolts;
        public Label lblShowTrigValue;
        public Label lblTrigStatus;
        public Label lblEnterVal;
        public Label lblTriggerChan;
        public Label lblDemoFunction;

        #endregion

    }

}