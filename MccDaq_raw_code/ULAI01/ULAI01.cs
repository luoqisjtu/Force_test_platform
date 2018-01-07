// ==============================================================================

//  File:                         ULAI01.CS

//  Library Call Demonstrated:    Mccdaq.MccBoard.AIn()

//  Purpose:                      Reads an A/D Input Channel.

//  Demonstration:                Displays the analog input on a user-specified
//                                channel.

//  Other Library Calls:          Mccdaq.MccBoard.ToEngUnits()
//                                MccDaq.MccService.ErrHandling()


//  Special Requirements:         Board 0 must have an A/D converter.
//                                Analog signal on an input channel.

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

namespace ULAI01
{
	public class frmDataDisplay : System.Windows.Forms.Form
	{
        
        // Create a new MccBoard object for Board 0
        MccDaq.MccBoard LuoqiBoard = new MccDaq.MccBoard(0);

        MccDaq.Range Range;
        private int HighChan, NumAIChans;
        private int ADResolution;

        AnalogIO.clsAnalogIO AIOProps = new AnalogIO.clsAnalogIO();

        private void frmDataDisplay_Load(object eventSender, System.EventArgs eventArgs)
        {

            int LowChan;
            MccDaq.TriggerType DefaultTrig;

            InitUL();

            // determine the number of analog channels and their capabilities
            int ChannelType = clsAnalogIO.ANALOGINPUT;
            NumAIChans = AIOProps.FindAnalogChansOfType(LuoqiBoard, ChannelType,
                out ADResolution, out Range, out LowChan, out DefaultTrig);

            if (NumAIChans == 0)
            {
                lblInstruction.Text = "Board " + LuoqiBoard.BoardNum.ToString() +
                    " does not have analog input channels.";
                cmdStartConvert.Enabled = false;
                txtNumChan.Enabled = false;
            }
            else
            {
                string CurFunc = "MccBoard.AIn";
                if (ADResolution > 16)
                    CurFunc = "MccBoard.AIn32";
                lblDemoFunction.Text = "Demonstration of " + CurFunc;
                lblInstruction.Text = "Board " + LuoqiBoard.BoardNum.ToString() +
                    " collecting analog data using " + CurFunc + 
                    " and Range of " + Range.ToString() + ".";
                HighChan = LowChan + NumAIChans - 1;
                this.lblChanPrompt.Text = "Enter a channel (" 
                    + LowChan.ToString() + " - " + HighChan.ToString() + "):";
            }
        }

        private void cmdStartConvert_Click(object eventSender, System.EventArgs eventArgs)
        {

            if (tmrConvert.Enabled)
            {
                cmdStartConvert.Text = "Start";
                tmrConvert.Enabled = false;
            }
            else
            {
                cmdStartConvert.Text = "Stop";
                tmrConvert.Enabled = true;
            }

        }

        private void tmrConvert_Tick(object eventSender, System.EventArgs eventArgs)
        {

            float EngUnits;
            double HighResEngUnits;
            MccDaq.ErrorInfo ULStat;
            System.UInt16 DataValue;
            System.UInt32 DataValue32;
            int Chan;
            int Options = 0;

            tmrConvert.Stop();

            //  Collect the data by calling AIn member function of MccBoard object
            //   Parameters:
            //     Chan       :the input channel number
            //     Range      :the Range for the board.
            //     DataValue  :the name for the value collected

            //  set input channel
            bool ValidChan = int.TryParse(txtNumChan.Text, out Chan);
            if (ValidChan)
            {
                if (Chan > HighChan) Chan = HighChan;
                txtNumChan.Text = Chan.ToString();
            }
            
            if (ADResolution > 16)
            {
                ULStat = LuoqiBoard.AIn32(Chan, Range, out DataValue32, Options);
                //  Convert raw data to Volts by calling ToEngUnits
                //  (member function of MccBoard class)
                ULStat = LuoqiBoard.ToEngUnits32(Range, DataValue32, out HighResEngUnits);
                lblShowData.Text = DataValue32.ToString();                //  print the counts
                lblShowVolts.Text = HighResEngUnits.ToString("F5") + " Volts"; //  print the voltage
            }
            else
            {
                ULStat = LuoqiBoard.AIn(Chan, Range, out DataValue);

                //  Convert raw data to Volts by calling ToEngUnits
                //  (member function of MccBoard class)
                ULStat = LuoqiBoard.ToEngUnits(Range, DataValue, out EngUnits);
                EngUnits = EngUnits * 2;
                lblShowData.Text = DataValue.ToString();                //  print the counts
                lblShowVolts.Text = EngUnits.ToString("F4") + " Volts"; //  print the voltage
            }

            tmrConvert.Start();
        }

        private void cmdStopConvert_Click(object eventSender, System.EventArgs eventArgs)
        {

            tmrConvert.Enabled = false;
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
                (clsErrorDefs.ReportError, clsErrorDefs.HandleError);

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
            this.cmdStartConvert = new System.Windows.Forms.Button();
            this.cmdStopConvert = new System.Windows.Forms.Button();
            this.txtNumChan = new System.Windows.Forms.TextBox();
            this.tmrConvert = new System.Windows.Forms.Timer(this.components);
            this.lblShowVolts = new System.Windows.Forms.Label();
            this.lblVoltsRead = new System.Windows.Forms.Label();
            this.lblValueRead = new System.Windows.Forms.Label();
            this.lblChanPrompt = new System.Windows.Forms.Label();
            this.lblDemoFunction = new System.Windows.Forms.Label();
            this.lblShowData = new System.Windows.Forms.Label();
            this.lblInstruction = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdStartConvert
            // 
            this.cmdStartConvert.BackColor = System.Drawing.SystemColors.Control;
            this.cmdStartConvert.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdStartConvert.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdStartConvert.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdStartConvert.Location = new System.Drawing.Point(163, 203);
            this.cmdStartConvert.Name = "cmdStartConvert";
            this.cmdStartConvert.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdStartConvert.Size = new System.Drawing.Size(52, 26);
            this.cmdStartConvert.TabIndex = 5;
            this.cmdStartConvert.Text = "Start";
            this.cmdStartConvert.UseVisualStyleBackColor = false;
            this.cmdStartConvert.Click += new System.EventHandler(this.cmdStartConvert_Click);
            // 
            // cmdStopConvert
            // 
            this.cmdStopConvert.BackColor = System.Drawing.SystemColors.Control;
            this.cmdStopConvert.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdStopConvert.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdStopConvert.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdStopConvert.Location = new System.Drawing.Point(232, 203);
            this.cmdStopConvert.Name = "cmdStopConvert";
            this.cmdStopConvert.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdStopConvert.Size = new System.Drawing.Size(52, 26);
            this.cmdStopConvert.TabIndex = 6;
            this.cmdStopConvert.Text = "Quit";
            this.cmdStopConvert.UseVisualStyleBackColor = false;
            this.cmdStopConvert.Click += new System.EventHandler(this.cmdStopConvert_Click);
            // 
            // txtNumChan
            // 
            this.txtNumChan.AcceptsReturn = true;
            this.txtNumChan.BackColor = System.Drawing.SystemColors.Window;
            this.txtNumChan.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNumChan.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtNumChan.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNumChan.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtNumChan.Location = new System.Drawing.Point(237, 92);
            this.txtNumChan.MaxLength = 0;
            this.txtNumChan.Name = "txtNumChan";
            this.txtNumChan.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtNumChan.Size = new System.Drawing.Size(33, 20);
            this.txtNumChan.TabIndex = 0;
            this.txtNumChan.Text = "0";
            this.txtNumChan.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tmrConvert
            // 
            this.tmrConvert.Interval = 10;
            this.tmrConvert.Tick += new System.EventHandler(this.tmrConvert_Tick);
            // 
            // lblShowVolts
            // 
            this.lblShowVolts.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowVolts.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowVolts.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowVolts.ForeColor = System.Drawing.Color.Blue;
            this.lblShowVolts.Location = new System.Drawing.Point(208, 163);
            this.lblShowVolts.Name = "lblShowVolts";
            this.lblShowVolts.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowVolts.Size = new System.Drawing.Size(80, 16);
            this.lblShowVolts.TabIndex = 8;
            this.lblShowVolts.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblShowVolts.Click += new System.EventHandler(this.lblShowVolts_Click);
            // 
            // lblVoltsRead
            // 
            this.lblVoltsRead.BackColor = System.Drawing.SystemColors.Window;
            this.lblVoltsRead.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblVoltsRead.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVoltsRead.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblVoltsRead.Location = new System.Drawing.Point(8, 163);
            this.lblVoltsRead.Name = "lblVoltsRead";
            this.lblVoltsRead.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblVoltsRead.Size = new System.Drawing.Size(184, 16);
            this.lblVoltsRead.TabIndex = 7;
            this.lblVoltsRead.Text = "Value converted to voltage:";
            this.lblVoltsRead.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblValueRead
            // 
            this.lblValueRead.BackColor = System.Drawing.SystemColors.Window;
            this.lblValueRead.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblValueRead.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblValueRead.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblValueRead.Location = new System.Drawing.Point(16, 131);
            this.lblValueRead.Name = "lblValueRead";
            this.lblValueRead.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblValueRead.Size = new System.Drawing.Size(184, 16);
            this.lblValueRead.TabIndex = 3;
            this.lblValueRead.Text = "Value read from selected channel:";
            this.lblValueRead.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblChanPrompt
            // 
            this.lblChanPrompt.BackColor = System.Drawing.SystemColors.Window;
            this.lblChanPrompt.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChanPrompt.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChanPrompt.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChanPrompt.Location = new System.Drawing.Point(11, 92);
            this.lblChanPrompt.Name = "lblChanPrompt";
            this.lblChanPrompt.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblChanPrompt.Size = new System.Drawing.Size(217, 16);
            this.lblChanPrompt.TabIndex = 1;
            this.lblChanPrompt.Text = "Enter the Channel to display: ";
            this.lblChanPrompt.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDemoFunction
            // 
            this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
            this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDemoFunction.Location = new System.Drawing.Point(8, 9);
            this.lblDemoFunction.Name = "lblDemoFunction";
            this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDemoFunction.Size = new System.Drawing.Size(286, 25);
            this.lblDemoFunction.TabIndex = 2;
            this.lblDemoFunction.Text = "Demonstration of MccBoard.AIn";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblShowData
            // 
            this.lblShowData.Font = new System.Drawing.Font("Arial", 8F);
            this.lblShowData.ForeColor = System.Drawing.Color.Blue;
            this.lblShowData.Location = new System.Drawing.Point(208, 131);
            this.lblShowData.Name = "lblShowData";
            this.lblShowData.Size = new System.Drawing.Size(80, 16);
            this.lblShowData.TabIndex = 9;
            this.lblShowData.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInstruction
            // 
            this.lblInstruction.BackColor = System.Drawing.SystemColors.Window;
            this.lblInstruction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblInstruction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstruction.ForeColor = System.Drawing.Color.Red;
            this.lblInstruction.Location = new System.Drawing.Point(9, 35);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblInstruction.Size = new System.Drawing.Size(286, 40);
            this.lblInstruction.TabIndex = 10;
            this.lblInstruction.Text = "Demonstration of MccBoard.AIn";
            this.lblInstruction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // frmDataDisplay
            // 
            this.AcceptButton = this.cmdStartConvert;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(304, 239);
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.lblShowData);
            this.Controls.Add(this.cmdStartConvert);
            this.Controls.Add(this.cmdStopConvert);
            this.Controls.Add(this.txtNumChan);
            this.Controls.Add(this.lblShowVolts);
            this.Controls.Add(this.lblVoltsRead);
            this.Controls.Add(this.lblValueRead);
            this.Controls.Add(this.lblChanPrompt);
            this.Controls.Add(this.lblDemoFunction);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Location = new System.Drawing.Point(182, 100);
            this.Name = "frmDataDisplay";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Universal Library Analog Input";
            this.Load += new System.EventHandler(this.frmDataDisplay_Load);
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
			Application.Run(new frmDataDisplay());
		}

        public frmDataDisplay()
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
        public Button cmdStartConvert;
        public Button cmdStopConvert;
        public TextBox txtNumChan;
        public Timer tmrConvert;
        public Label lblShowVolts;
        public Label lblVoltsRead;
        public Label lblValueRead;
        public Label lblChanPrompt;
        public Label lblDemoFunction;
        public Label lblInstruction;
        public Label lblShowData;

        #endregion

        private void lblShowVolts_Click(object sender, EventArgs e)
        {

        }

    }
}