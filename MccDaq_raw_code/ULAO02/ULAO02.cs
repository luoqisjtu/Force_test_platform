// ==============================================================================
//
//  File:                         ULAO02.CS
//  Library Call Demonstrated:    Mccdaq.MccBoard.AOutScan()
//
//  Purpose:                      Writes to a range of D/A Output Channels.
//
//  Demonstration:                Sends a digital output to the D/A channels
//
//  Other Library Calls:          MccDaq.MccService.ErrHandling()
//
//  Special Requirements:         Board 0 must have 2 or more D/A converters.
//                                This function is designed for boards that
//                                support timed analog output.  It can be used
//                                for polled output boards but only for values
//                                of NumPoints up to the number of channels
//                                that the board supports (i.e., NumPoints =
//                                6 maximum for the six channel CIO-DDA06).
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

namespace ULAO02
{
	public class frmSendAData : System.Windows.Forms.Form
	{

        // Create a new MccBoard object for Board 0
        MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

        int LowChan = 0;
        MccDaq.Range Range;
        int DAResolution, NumAOChans, HighChan;

        int NumPoints = 2;

		private ushort[] DAData;
		private IntPtr MemHandle = IntPtr.Zero;
	//  define a variable to contain the handle for
									//  memory allocated by Windows through MccDaq.MccService.WinBufAlloc()
		private int FirstPoint;
        MccDaq.ErrorInfo ULStat;

        AnalogIO.clsAnalogIO AIOProps = new AnalogIO.clsAnalogIO();

        private void frmSendAData_Load(object sender, EventArgs e)
        {
            
            int ChannelType;
            MccDaq.TriggerType DefaultTrig;
            
            InitUL();
            
            // determine the number of analog channels and their capabilities
            ChannelType = clsAnalogIO.ANALOGOUTPUT;
            NumAOChans = AIOProps.FindAnalogChansOfType(DaqBoard, ChannelType, 
                out DAResolution, out Range, out LowChan, out DefaultTrig);
            
            if (NumAOChans == 0)
            {
                lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString() + 
                    " does not have analog input channels.";
                cmdSendData.Enabled = false;
            }
        else
            {
                if (NumAOChans > 4) NumAOChans = 4;
                NumPoints = NumAOChans;
                MemHandle = MccDaq.MccService.WinBufAllocEx(NumPoints);
                if (MemHandle == IntPtr.Zero)
                {
                    lblInstruction.Text = "Failure creating memory buffer.";
                    cmdSendData.Enabled = false;
                    return;
                }
                DAData = (new ushort[NumPoints]);
                int ValueStep, FSCount;
                ushort StepCount, i;
                
                FSCount = (int)Math.Pow(2, DAResolution);
                ValueStep = FSCount / (NumAOChans + 1);
                for (i = 0; i < NumPoints; i++)
                {
                    StepCount = (ushort)(ValueStep * (i + 1));
                    DAData[i] = StepCount;
                }
                FirstPoint = 0;
                ULStat = MccDaq.MccService.WinArrayToBuf
                    (DAData, MemHandle, FirstPoint, NumPoints);
                lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString("0") + 
                    " generating analog output on up to " + NumAOChans.ToString("0") 
                    + " channels using cbAOutScan() " + 
                    " at a Range of " + Range.ToString() + ".";
                HighChan = LowChan + NumAOChans - 1;
            }

        }

        private void cmdSendData_Click(object eventSender, System.EventArgs eventArgs)
        {
            //  Parameters:
            //    LowChan    :the lower channel of the scan
            //    HighChan   :the upper channel of the scan
            //    NumPoints  :the number of D/A values to send
            //    Rate       :per channel sampling rate ((samples per second) per channel)
            //    DAData     :array of values to send to the scanned channels
            //    Options    :data send options

            FirstPoint = 0;
            int Rate = 100;	//  Rate of data update (ignored if board does not 
            //                  support timed analog output)
            MccDaq.ScanOptions Options = MccDaq.ScanOptions.Default;  // foreground mode scan

            MccDaq.ErrorInfo ULStat = DaqBoard.AOutScan
                (LowChan, HighChan, NumPoints, ref Rate, Range, MemHandle, Options);

            float VoltValue;
            for (int i = 0; i < NumPoints; ++i)
            {
                lblAOutData[i].Text = DAData[i].ToString("0");
                VoltValue = ConvertToVolts(DAData[i]);
                lblAOutVolts[i].Text = VoltValue.ToString("0.000V");
            }
            
            for (int i = HighChan + 1; i <= 3; i++)
                lblAOutData[i].Text = "";

        }

        private float ConvertToVolts(ushort DataVal)
        {
            float LSBVal, FSVolts, OutVal;

            FSVolts = AIOProps.GetRangeVolts(Range);
            LSBVal = (float)(FSVolts / Math.Pow(2, (double)DAResolution));
            OutVal = LSBVal * DataVal;
            if (Range < Range.Uni10Volts) OutVal = OutVal - (FSVolts / 2);
            return OutVal;
        }

        private void cmdEndProgram_Click(object eventSender, System.EventArgs eventArgs) /* Handles cmdEndProgram.Click */
        {
            //  Free up memory for use by other programs
            MccDaq.ErrorInfo ULStat = MccDaq.MccService.WinBufFreeEx(MemHandle);

            MemHandle = IntPtr.Zero;

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
                (ErrorReporting.PrintAll, ErrorHandling.StopAll);

            lblAOutData = (new Label[] { _lblAOutData_0, _lblAOutData_1,
                _lblAOutData_2, _lblAOutData_3});
            lblAOutVolts = (new Label[] {lblVolts0, lblVolts1,
                lblVolts2, lblVolts3});
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
            this.cmdEndProgram = new System.Windows.Forms.Button();
            this.cmdSendData = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this._lblAOutData_1 = new System.Windows.Forms.Label();
            this._lblAOutData_0 = new System.Windows.Forms.Label();
            this.lblChan1 = new System.Windows.Forms.Label();
            this.lblChan0 = new System.Windows.Forms.Label();
            this.lblDemoFunction = new System.Windows.Forms.Label();
            this.lblInstruction = new System.Windows.Forms.Label();
            this.lblChan2 = new System.Windows.Forms.Label();
            this.lblChan3 = new System.Windows.Forms.Label();
            this._lblAOutData_2 = new System.Windows.Forms.Label();
            this._lblAOutData_3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblVolts3 = new System.Windows.Forms.Label();
            this.lblVolts2 = new System.Windows.Forms.Label();
            this.lblVolts1 = new System.Windows.Forms.Label();
            this.lblVolts0 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdEndProgram
            // 
            this.cmdEndProgram.BackColor = System.Drawing.SystemColors.Control;
            this.cmdEndProgram.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdEndProgram.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdEndProgram.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdEndProgram.Location = new System.Drawing.Point(264, 168);
            this.cmdEndProgram.Name = "cmdEndProgram";
            this.cmdEndProgram.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdEndProgram.Size = new System.Drawing.Size(55, 26);
            this.cmdEndProgram.TabIndex = 1;
            this.cmdEndProgram.Text = "Quit";
            this.cmdEndProgram.UseVisualStyleBackColor = false;
            this.cmdEndProgram.Click += new System.EventHandler(this.cmdEndProgram_Click);
            // 
            // cmdSendData
            // 
            this.cmdSendData.BackColor = System.Drawing.SystemColors.Control;
            this.cmdSendData.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdSendData.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdSendData.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdSendData.Location = new System.Drawing.Point(128, 168);
            this.cmdSendData.Name = "cmdSendData";
            this.cmdSendData.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdSendData.Size = new System.Drawing.Size(81, 26);
            this.cmdSendData.TabIndex = 2;
            this.cmdSendData.Text = "Send Data";
            this.cmdSendData.UseVisualStyleBackColor = false;
            this.cmdSendData.Click += new System.EventHandler(this.cmdSendData_Click);
            // 
            // Label1
            // 
            this.Label1.BackColor = System.Drawing.SystemColors.Window;
            this.Label1.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label1.Location = new System.Drawing.Point(5, 102);
            this.Label1.Name = "Label1";
            this.Label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label1.Size = new System.Drawing.Size(70, 25);
            this.Label1.TabIndex = 7;
            this.Label1.Text = "Raw Data";
            this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _lblAOutData_1
            // 
            this._lblAOutData_1.BackColor = System.Drawing.SystemColors.Window;
            this._lblAOutData_1.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblAOutData_1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblAOutData_1.ForeColor = System.Drawing.Color.Blue;
            this._lblAOutData_1.Location = new System.Drawing.Point(170, 110);
            this._lblAOutData_1.Name = "_lblAOutData_1";
            this._lblAOutData_1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblAOutData_1.Size = new System.Drawing.Size(65, 17);
            this._lblAOutData_1.TabIndex = 6;
            this._lblAOutData_1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblAOutData_0
            // 
            this._lblAOutData_0.BackColor = System.Drawing.SystemColors.Window;
            this._lblAOutData_0.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblAOutData_0.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblAOutData_0.ForeColor = System.Drawing.Color.Blue;
            this._lblAOutData_0.Location = new System.Drawing.Point(90, 110);
            this._lblAOutData_0.Name = "_lblAOutData_0";
            this._lblAOutData_0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblAOutData_0.Size = new System.Drawing.Size(65, 17);
            this._lblAOutData_0.TabIndex = 3;
            this._lblAOutData_0.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblChan1
            // 
            this.lblChan1.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan1.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan1.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan1.Location = new System.Drawing.Point(170, 86);
            this.lblChan1.Name = "lblChan1";
            this.lblChan1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblChan1.Size = new System.Drawing.Size(65, 17);
            this.lblChan1.TabIndex = 5;
            this.lblChan1.Text = "Channel 1";
            this.lblChan1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblChan0
            // 
            this.lblChan0.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan0.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan0.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan0.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan0.Location = new System.Drawing.Point(90, 86);
            this.lblChan0.Name = "lblChan0";
            this.lblChan0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblChan0.Size = new System.Drawing.Size(65, 17);
            this.lblChan0.TabIndex = 4;
            this.lblChan0.Text = "Channel 0";
            this.lblChan0.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblDemoFunction
            // 
            this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
            this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDemoFunction.Location = new System.Drawing.Point(24, 8);
            this.lblDemoFunction.Name = "lblDemoFunction";
            this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDemoFunction.Size = new System.Drawing.Size(350, 17);
            this.lblDemoFunction.TabIndex = 0;
            this.lblDemoFunction.Text = "Demonstration of MccDaq.MccBoard.AOutScan()";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblInstruction
            // 
            this.lblInstruction.BackColor = System.Drawing.SystemColors.Window;
            this.lblInstruction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblInstruction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstruction.ForeColor = System.Drawing.Color.Red;
            this.lblInstruction.Location = new System.Drawing.Point(27, 29);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblInstruction.Size = new System.Drawing.Size(347, 44);
            this.lblInstruction.TabIndex = 13;
            this.lblInstruction.Text = "Board 0 must have an D/A converter.";
            this.lblInstruction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblChan2
            // 
            this.lblChan2.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan2.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan2.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan2.Location = new System.Drawing.Point(251, 85);
            this.lblChan2.Name = "lblChan2";
            this.lblChan2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblChan2.Size = new System.Drawing.Size(65, 17);
            this.lblChan2.TabIndex = 14;
            this.lblChan2.Text = "Channel 2";
            this.lblChan2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblChan3
            // 
            this.lblChan3.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan3.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan3.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan3.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan3.Location = new System.Drawing.Point(327, 85);
            this.lblChan3.Name = "lblChan3";
            this.lblChan3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblChan3.Size = new System.Drawing.Size(65, 17);
            this.lblChan3.TabIndex = 15;
            this.lblChan3.Text = "Channel 3";
            this.lblChan3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblAOutData_2
            // 
            this._lblAOutData_2.BackColor = System.Drawing.SystemColors.Window;
            this._lblAOutData_2.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblAOutData_2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblAOutData_2.ForeColor = System.Drawing.Color.Blue;
            this._lblAOutData_2.Location = new System.Drawing.Point(251, 110);
            this._lblAOutData_2.Name = "_lblAOutData_2";
            this._lblAOutData_2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblAOutData_2.Size = new System.Drawing.Size(65, 17);
            this._lblAOutData_2.TabIndex = 16;
            this._lblAOutData_2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblAOutData_3
            // 
            this._lblAOutData_3.BackColor = System.Drawing.SystemColors.Window;
            this._lblAOutData_3.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblAOutData_3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblAOutData_3.ForeColor = System.Drawing.Color.Blue;
            this._lblAOutData_3.Location = new System.Drawing.Point(328, 110);
            this._lblAOutData_3.Name = "_lblAOutData_3";
            this._lblAOutData_3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblAOutData_3.Size = new System.Drawing.Size(65, 17);
            this._lblAOutData_3.TabIndex = 17;
            this._lblAOutData_3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.SystemColors.Window;
            this.label2.Cursor = System.Windows.Forms.Cursors.Default;
            this.label2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label2.Location = new System.Drawing.Point(18, 129);
            this.label2.Name = "label2";
            this.label2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label2.Size = new System.Drawing.Size(57, 25);
            this.label2.TabIndex = 22;
            this.label2.Text = "Volts";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblVolts3
            // 
            this.lblVolts3.BackColor = System.Drawing.SystemColors.Window;
            this.lblVolts3.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblVolts3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVolts3.ForeColor = System.Drawing.Color.Blue;
            this.lblVolts3.Location = new System.Drawing.Point(327, 134);
            this.lblVolts3.Name = "lblVolts3";
            this.lblVolts3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblVolts3.Size = new System.Drawing.Size(65, 17);
            this.lblVolts3.TabIndex = 21;
            this.lblVolts3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVolts2
            // 
            this.lblVolts2.BackColor = System.Drawing.SystemColors.Window;
            this.lblVolts2.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblVolts2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVolts2.ForeColor = System.Drawing.Color.Blue;
            this.lblVolts2.Location = new System.Drawing.Point(251, 134);
            this.lblVolts2.Name = "lblVolts2";
            this.lblVolts2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblVolts2.Size = new System.Drawing.Size(65, 17);
            this.lblVolts2.TabIndex = 20;
            this.lblVolts2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVolts1
            // 
            this.lblVolts1.BackColor = System.Drawing.SystemColors.Window;
            this.lblVolts1.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblVolts1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVolts1.ForeColor = System.Drawing.Color.Blue;
            this.lblVolts1.Location = new System.Drawing.Point(170, 134);
            this.lblVolts1.Name = "lblVolts1";
            this.lblVolts1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblVolts1.Size = new System.Drawing.Size(65, 17);
            this.lblVolts1.TabIndex = 19;
            this.lblVolts1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVolts0
            // 
            this.lblVolts0.BackColor = System.Drawing.SystemColors.Window;
            this.lblVolts0.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblVolts0.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVolts0.ForeColor = System.Drawing.Color.Blue;
            this.lblVolts0.Location = new System.Drawing.Point(90, 134);
            this.lblVolts0.Name = "lblVolts0";
            this.lblVolts0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblVolts0.Size = new System.Drawing.Size(65, 17);
            this.lblVolts0.TabIndex = 18;
            this.lblVolts0.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // frmSendAData
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(405, 212);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblVolts3);
            this.Controls.Add(this.lblVolts2);
            this.Controls.Add(this.lblVolts1);
            this.Controls.Add(this.lblVolts0);
            this.Controls.Add(this._lblAOutData_3);
            this.Controls.Add(this._lblAOutData_2);
            this.Controls.Add(this.lblChan3);
            this.Controls.Add(this.lblChan2);
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.cmdEndProgram);
            this.Controls.Add(this.cmdSendData);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this._lblAOutData_1);
            this.Controls.Add(this._lblAOutData_0);
            this.Controls.Add(this.lblChan1);
            this.Controls.Add(this.lblChan0);
            this.Controls.Add(this.lblDemoFunction);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Blue;
            this.Location = new System.Drawing.Point(7, 103);
            this.Name = "frmSendAData";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Universal Library Analog Output ";
            this.Load += new System.EventHandler(this.frmSendAData_Load);
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
			Application.Run(new frmSendAData());
		}

        public frmSendAData()
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
        public Button cmdEndProgram;
        public Button cmdSendData;
        public Label Label1;
        public Label _lblAOutData_1;
        public Label _lblAOutData_0;
        public Label lblChan1;
        public Label lblChan0;
        public Label lblDemoFunction;

        public Label[] lblAOutData;
        public Label[] lblAOutVolts;
        public Label lblInstruction;
        public Label lblChan2;
        public Label lblChan3;
        public Label _lblAOutData_2;
        public Label _lblAOutData_3;
        public Label label2;
        public Label lblVolts3;
        public Label lblVolts2;
        public Label lblVolts1;
        public Label lblVolts0;

        #endregion

    }
}