// ==============================================================================
//
//  File:                         ULFI03.CS
//
//  Library Call Demonstrated:    File Operations:
//                                Mccdaq.MccBoard.FilePretrig()
//                                MccDaq.MccService.FileRead()
//                                MccDaq.MccService.FileGetInfo()
//
//  Purpose:                      Stream data continuously to a streamer file
//                                until a trigger is received, continue data
//                                streaming until total number of samples minus
//                                the number of pretrigger samples is reached.
//
//  Demonstration:                Creates a file and scans analog data to the
//                                file continuously, overwriting previous data.
//                                When a trigger is received, acquisition stops
//                                after (TotalCount& - PreTrigCount&) samples
//                                are stored. Displays the data in the file and
//                                the information in the file header. Prints
//                                data from PreTrigger-10 to PreTrigger+10.
//
//  Other Library Calls:          MccDaq.MccService.ErrHandling()
//
//  Special Requirements:         Board 0 must be capable of pretrigger.
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

namespace ULFI03
{
	public class frmFilePreTrig : System.Windows.Forms.Form
	{

        // Create a new MccBoard object for Board 0
        MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

        MccDaq.Range Range;
        private int ADResolution, NumAIChans;
        private int LowChan;
        MccDaq.TriggerType DefaultTrig;

        const int TestPoints = 4096;  //  Number of data points to collect
        private int Rate = 1000;
		const int BufSize = TestPoints + 512; //  set buffer size large enough to hold all data

        AnalogIO.clsAnalogIO AIOProps = new AnalogIO.clsAnalogIO();

        private void frmFilePreTrig_Load(object sender, EventArgs e)
        {

            InitUL();

            // determine the number of analog channels and their capabilities
            int ChannelType = clsAnalogIO.PRETRIGIN;
            NumAIChans = AIOProps.FindAnalogChansOfType(DaqBoard, ChannelType,
                out ADResolution, out Range, out LowChan, out DefaultTrig);

            if (NumAIChans == 0)
            {
                lblAcqStat.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " does not have analog input channels that support pretrigger.";
                lblAcqStat.ForeColor = Color.Red;
                cmdTrigEnable.Enabled = false;
            }
            else
                lblAcqStat.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " collecting analog data on channel 0 using FilePretrig " +
                    "in foreground mode with Range set to " + Range.ToString() + ".";

        }

        private void cmdTrigEnable_Click(object eventSender, System.EventArgs eventArgs)
        {

            bool DataAvailable;
            MccDaq.ErrorInfo ULStat;
            float engUnits;
            short FileLowChan, FileHighChan;

            lblAcqStat.Text =
                "Waiting for trigger on trigger input and acquiring data.";
            cmdTrigEnable.Enabled = false;
            Cursor = Cursors.WaitCursor;
            Application.DoEvents();
            DataAvailable = false;

            if (DefaultTrig == MccDaq.TriggerType.TrigAbove)
            {
                //The default trigger configuration for most devices is
                //rising edge digital trigger, but some devices do not
                //support this type for pretrigger functions.
                short MidScale;
                MidScale = Convert.ToInt16((Math.Pow(2, ADResolution) / 2) - 1);
                ULStat = DaqBoard.SetTrigger(DefaultTrig, MidScale, MidScale);
                ULStat = DaqBoard.ToEngUnits(Range, MidScale, out engUnits);
                lblAcqStat.Text = "Waiting for trigger on analog input above " +
                    engUnits.ToString("0.00") + "V and acquiring data.";
                Application.DoEvents();
            }

            //  Monitor a range of channels for a trigger then collect the values
            //  with MccDaq.MccBoard.APretrig()
            //  Parameters:
            //    FileName      :file where data will be stored
            //    LowChan       :first A/D channel of the scan
            //    HighChan      :last A/D channel of the scan
            //    PretrigCount  :number of pre-trigger A/D samples to collect
            //    TotalCount    :total number of A/D samples to collect
            //    Rate          :per channel sampling rate ((samples per second) per channel)
            //    Range         :the gain for the board
            //    Options       :data collection options

            int TotalCount = TestPoints;
            int PretrigCount = 200;
            string FileName = txtFileName.Text; //  it may be necessary to specify path here
            short HighChan = (short) LowChan;
            MccDaq.ScanOptions Options = MccDaq.ScanOptions.Default;

            ULStat = DaqBoard.FilePretrig(LowChan, HighChan, 
                ref PretrigCount, ref TotalCount, ref Rate, Range, FileName, Options);

            if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.BadFileName)
            {
                MessageBox.Show("Enter the name of the file in which to store " +
                    "the data in the text box.", "Bad File Name", 0);
                cmdTrigEnable.Enabled = true;
                txtFileName.Focus();
                Cursor = Cursors.Default;
                return;
            }
            Cursor = Cursors.Default;

            if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.BadBoardType)
                lblAcqStat.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " doesn't support the cbAPretrig function.";
            else if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.TooFew)
            {
                lblAcqStat.Text = "Premature trigger occurred at sample " 
                    + (PretrigCount - 1).ToString() + ".";
                DataAvailable = true;
            }
            else if (ULStat.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
            {
                lblAcqStat.Text = ULStat.Message + ".";
                lblAcqStat.ForeColor = Color.Red;
                Application.DoEvents();
            }
            else
            {
                lblAcqStat.Text = "";
                DataAvailable = true;
            }

            if (DataAvailable)
            {

                //  show the information in the file header with MccDaq.MccService.FileGetInfo
                //   Parameters:
                //     FileName      :the filename containing the data
                //     LowChan       :first A/D channel of the scan
                //     HighChan      :last A/D channel of the scan
                //     PreTrigCount  :the number of pretrigger samples in the file
                //     Count         :the total number of A/D samples in the file
                //     Rate          :per channel sampling rate ((samples per second) per channel)
                //     Range         :the gain at which the samples were collected

                ULStat = MccDaq.MccService.FileGetInfo(FileName, out FileLowChan, 
                    out FileHighChan, out PretrigCount, out TotalCount, out Rate, out Range);

                lblShowFileName.Text = FileName;
                lblShowLoChan.Text = LowChan.ToString("0");
                lblShowHiChan.Text = HighChan.ToString("0");
                lblShowPT.Text = PretrigCount.ToString("0");
                lblShowNumSam.Text = TotalCount.ToString("0");
                lblShowRate.Text = Rate.ToString("0");
                lblShowGain.Text = Range.ToString();

                //  show the data using MccDaq.MccService.FileRead()
                //   Parameters:
                //     FileName      :the filename containing the data
                //     NumPoints     :the number of data values to read from the file
                //     FirstPoint    :index of the first data value to read
                //     DataBuffer()  :array to read data into

                int NumPoints = 20;                 //  read the first twenty data points
                int FirstPoint = PretrigCount - 11; //  start at the trigger - 10
                if (FirstPoint < 0) FirstPoint = 0;
                ushort[] DataBuffer = new ushort[NumPoints];
                ULStat = MccDaq.MccService.FileRead(FileName, FirstPoint, ref NumPoints, DataBuffer);

                for (int i = 0; i < 20; ++i)
                {
                    lblPreTrig[i].Text = DataBuffer[i].ToString("0");
                    lblPre[i].Text = (FirstPoint + i).ToString();
                }

            }
                
            cmdTrigEnable.Enabled = true;

        }

        private void cmdQuit_Click(object eventSender, System.EventArgs eventArgs)
        {
            Application.Exit();
        }

        private void InitUL()
        {

            //  Initiate error handling
            //   activating error handling will trap errors like
            //   bad channel numbers and non-configured conditions.
            //   Parameters:
            //     MccDaq.ErrorReporting.DontPrint :all warnings and errors encountered will be handled locally
            //     MccDaq.ErrorHandling.DontStop   :if an error is encountered, the program will not stop

            clsErrorDefs.ReportError = MccDaq.ErrorReporting.DontPrint;
            clsErrorDefs.HandleError = MccDaq.ErrorHandling.DontStop;
            MccDaq.ErrorInfo ULStat = MccDaq.MccService.ErrHandling
                (ErrorReporting.DontPrint, ErrorHandling.DontStop);

            lblPreTrig = (new Label[]{_lblPreTrig_0, _lblPreTrig_1, 
                _lblPreTrig_2, _lblPreTrig_3, _lblPreTrig_4, _lblPreTrig_5, 
                _lblPreTrig_6, _lblPreTrig_7, _lblPreTrig_8, _lblPreTrig_9,
                _lblPostTrig_1, _lblPostTrig_2, _lblPostTrig_3, _lblPostTrig_4, 
                _lblPostTrig_5, _lblPostTrig_6, _lblPostTrig_7, _lblPostTrig_8, 
                _lblPostTrig_9, _lblPostTrig_10});

            lblPre = (new Label[]{lblPre10, lblPre9, lblPre8, lblPre7,
                lblPre6, lblPre5, lblPre4, lblPre3, lblPre2, lblPre1, 
                lblPost1, lblPost2, lblPost3, lblPost4, lblPost5, 
                lblPost6, lblPost7, lblPost8, lblPost9, lblPost10});

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
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.cmdQuit = new System.Windows.Forms.Button();
            this.cmdTrigEnable = new System.Windows.Forms.Button();
            this.lblFileInstruct = new System.Windows.Forms.Label();
            this.lblShowGain = new System.Windows.Forms.Label();
            this.lblGain = new System.Windows.Forms.Label();
            this.lblShowRate = new System.Windows.Forms.Label();
            this.lblRate = new System.Windows.Forms.Label();
            this.lblShowNumSam = new System.Windows.Forms.Label();
            this.lblNumSam = new System.Windows.Forms.Label();
            this.lblShowPT = new System.Windows.Forms.Label();
            this.lblNumPTSam = new System.Windows.Forms.Label();
            this.lblShowHiChan = new System.Windows.Forms.Label();
            this.lblHiChan = new System.Windows.Forms.Label();
            this.lblShowLoChan = new System.Windows.Forms.Label();
            this.lblLoChan = new System.Windows.Forms.Label();
            this.lblShowFileName = new System.Windows.Forms.Label();
            this.lblFileName = new System.Windows.Forms.Label();
            this._lblPostTrig_10 = new System.Windows.Forms.Label();
            this._lblPreTrig_9 = new System.Windows.Forms.Label();
            this._lblPostTrig_9 = new System.Windows.Forms.Label();
            this._lblPreTrig_8 = new System.Windows.Forms.Label();
            this._lblPostTrig_8 = new System.Windows.Forms.Label();
            this._lblPreTrig_7 = new System.Windows.Forms.Label();
            this._lblPostTrig_7 = new System.Windows.Forms.Label();
            this._lblPreTrig_6 = new System.Windows.Forms.Label();
            this._lblPostTrig_6 = new System.Windows.Forms.Label();
            this._lblPreTrig_5 = new System.Windows.Forms.Label();
            this._lblPostTrig_5 = new System.Windows.Forms.Label();
            this._lblPreTrig_4 = new System.Windows.Forms.Label();
            this._lblPostTrig_4 = new System.Windows.Forms.Label();
            this._lblPreTrig_3 = new System.Windows.Forms.Label();
            this._lblPostTrig_2 = new System.Windows.Forms.Label();
            this._lblPreTrig_2 = new System.Windows.Forms.Label();
            this._lblPostTrig_3 = new System.Windows.Forms.Label();
            this._lblPreTrig_1 = new System.Windows.Forms.Label();
            this._lblPostTrig_1 = new System.Windows.Forms.Label();
            this._lblPreTrig_0 = new System.Windows.Forms.Label();
            this.lblPostTrigData = new System.Windows.Forms.Label();
            this.lblPreTrigData = new System.Windows.Forms.Label();
            this.lblDemoFunction = new System.Windows.Forms.Label();
            this.lblAcqStat = new System.Windows.Forms.Label();
            this.lblPre1 = new System.Windows.Forms.Label();
            this.lblPre2 = new System.Windows.Forms.Label();
            this.lblPre3 = new System.Windows.Forms.Label();
            this.lblPre4 = new System.Windows.Forms.Label();
            this.lblPre5 = new System.Windows.Forms.Label();
            this.lblPre6 = new System.Windows.Forms.Label();
            this.lblPre7 = new System.Windows.Forms.Label();
            this.lblPre8 = new System.Windows.Forms.Label();
            this.lblPre9 = new System.Windows.Forms.Label();
            this.lblPre10 = new System.Windows.Forms.Label();
            this.lblPost10 = new System.Windows.Forms.Label();
            this.lblPost9 = new System.Windows.Forms.Label();
            this.lblPost8 = new System.Windows.Forms.Label();
            this.lblPost7 = new System.Windows.Forms.Label();
            this.lblPost6 = new System.Windows.Forms.Label();
            this.lblPost5 = new System.Windows.Forms.Label();
            this.lblPost4 = new System.Windows.Forms.Label();
            this.lblPost3 = new System.Windows.Forms.Label();
            this.lblPost2 = new System.Windows.Forms.Label();
            this.lblPost1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtFileName
            // 
            this.txtFileName.AcceptsReturn = true;
            this.txtFileName.BackColor = System.Drawing.SystemColors.Window;
            this.txtFileName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFileName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtFileName.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFileName.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtFileName.Location = new System.Drawing.Point(195, 380);
            this.txtFileName.MaxLength = 0;
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtFileName.Size = new System.Drawing.Size(161, 20);
            this.txtFileName.TabIndex = 63;
            this.txtFileName.Text = "DEMO.DAT";
            // 
            // cmdQuit
            // 
            this.cmdQuit.BackColor = System.Drawing.SystemColors.Control;
            this.cmdQuit.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdQuit.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdQuit.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdQuit.Location = new System.Drawing.Point(277, 335);
            this.cmdQuit.Name = "cmdQuit";
            this.cmdQuit.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdQuit.Size = new System.Drawing.Size(90, 26);
            this.cmdQuit.TabIndex = 17;
            this.cmdQuit.Text = "Quit";
            this.cmdQuit.UseVisualStyleBackColor = false;
            this.cmdQuit.Click += new System.EventHandler(this.cmdQuit_Click);
            // 
            // cmdTrigEnable
            // 
            this.cmdTrigEnable.BackColor = System.Drawing.SystemColors.Control;
            this.cmdTrigEnable.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdTrigEnable.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdTrigEnable.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdTrigEnable.Location = new System.Drawing.Point(277, 303);
            this.cmdTrigEnable.Name = "cmdTrigEnable";
            this.cmdTrigEnable.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdTrigEnable.Size = new System.Drawing.Size(90, 26);
            this.cmdTrigEnable.TabIndex = 18;
            this.cmdTrigEnable.Text = "Enable Trigger";
            this.cmdTrigEnable.UseVisualStyleBackColor = false;
            this.cmdTrigEnable.Click += new System.EventHandler(this.cmdTrigEnable_Click);
            // 
            // lblFileInstruct
            // 
            this.lblFileInstruct.BackColor = System.Drawing.SystemColors.Window;
            this.lblFileInstruct.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblFileInstruct.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileInstruct.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblFileInstruct.Location = new System.Drawing.Point(8, 371);
            this.lblFileInstruct.Name = "lblFileInstruct";
            this.lblFileInstruct.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblFileInstruct.Size = new System.Drawing.Size(169, 41);
            this.lblFileInstruct.TabIndex = 62;
            this.lblFileInstruct.Text = "Enter the name of the file that you want to store data in:";
            this.lblFileInstruct.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblShowGain
            // 
            this.lblShowGain.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowGain.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowGain.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowGain.ForeColor = System.Drawing.Color.Blue;
            this.lblShowGain.Location = new System.Drawing.Point(181, 353);
            this.lblShowGain.Name = "lblShowGain";
            this.lblShowGain.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowGain.Size = new System.Drawing.Size(52, 14);
            this.lblShowGain.TabIndex = 61;
            // 
            // lblGain
            // 
            this.lblGain.BackColor = System.Drawing.SystemColors.Window;
            this.lblGain.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblGain.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGain.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblGain.Location = new System.Drawing.Point(46, 353);
            this.lblGain.Name = "lblGain";
            this.lblGain.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblGain.Size = new System.Drawing.Size(129, 14);
            this.lblGain.TabIndex = 54;
            this.lblGain.Text = "Gain:";
            this.lblGain.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblShowRate
            // 
            this.lblShowRate.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowRate.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowRate.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowRate.ForeColor = System.Drawing.Color.Blue;
            this.lblShowRate.Location = new System.Drawing.Point(181, 340);
            this.lblShowRate.Name = "lblShowRate";
            this.lblShowRate.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowRate.Size = new System.Drawing.Size(52, 14);
            this.lblShowRate.TabIndex = 60;
            // 
            // lblRate
            // 
            this.lblRate.BackColor = System.Drawing.SystemColors.Window;
            this.lblRate.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblRate.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRate.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblRate.Location = new System.Drawing.Point(46, 340);
            this.lblRate.Name = "lblRate";
            this.lblRate.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblRate.Size = new System.Drawing.Size(129, 14);
            this.lblRate.TabIndex = 53;
            this.lblRate.Text = "Collection Rate:";
            this.lblRate.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblShowNumSam
            // 
            this.lblShowNumSam.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowNumSam.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowNumSam.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowNumSam.ForeColor = System.Drawing.Color.Blue;
            this.lblShowNumSam.Location = new System.Drawing.Point(181, 327);
            this.lblShowNumSam.Name = "lblShowNumSam";
            this.lblShowNumSam.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowNumSam.Size = new System.Drawing.Size(52, 14);
            this.lblShowNumSam.TabIndex = 59;
            // 
            // lblNumSam
            // 
            this.lblNumSam.BackColor = System.Drawing.SystemColors.Window;
            this.lblNumSam.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblNumSam.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNumSam.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblNumSam.Location = new System.Drawing.Point(46, 327);
            this.lblNumSam.Name = "lblNumSam";
            this.lblNumSam.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblNumSam.Size = new System.Drawing.Size(129, 14);
            this.lblNumSam.TabIndex = 52;
            this.lblNumSam.Text = "No. of Samples:";
            this.lblNumSam.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblShowPT
            // 
            this.lblShowPT.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowPT.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowPT.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowPT.ForeColor = System.Drawing.Color.Blue;
            this.lblShowPT.Location = new System.Drawing.Point(181, 314);
            this.lblShowPT.Name = "lblShowPT";
            this.lblShowPT.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowPT.Size = new System.Drawing.Size(52, 14);
            this.lblShowPT.TabIndex = 58;
            // 
            // lblNumPTSam
            // 
            this.lblNumPTSam.BackColor = System.Drawing.SystemColors.Window;
            this.lblNumPTSam.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblNumPTSam.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNumPTSam.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblNumPTSam.Location = new System.Drawing.Point(30, 314);
            this.lblNumPTSam.Name = "lblNumPTSam";
            this.lblNumPTSam.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblNumPTSam.Size = new System.Drawing.Size(145, 14);
            this.lblNumPTSam.TabIndex = 51;
            this.lblNumPTSam.Text = "No. of Pretrig Samples:";
            this.lblNumPTSam.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblShowHiChan
            // 
            this.lblShowHiChan.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowHiChan.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowHiChan.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowHiChan.ForeColor = System.Drawing.Color.Blue;
            this.lblShowHiChan.Location = new System.Drawing.Point(181, 301);
            this.lblShowHiChan.Name = "lblShowHiChan";
            this.lblShowHiChan.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowHiChan.Size = new System.Drawing.Size(52, 14);
            this.lblShowHiChan.TabIndex = 57;
            // 
            // lblHiChan
            // 
            this.lblHiChan.BackColor = System.Drawing.SystemColors.Window;
            this.lblHiChan.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblHiChan.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHiChan.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblHiChan.Location = new System.Drawing.Point(46, 301);
            this.lblHiChan.Name = "lblHiChan";
            this.lblHiChan.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblHiChan.Size = new System.Drawing.Size(129, 14);
            this.lblHiChan.TabIndex = 50;
            this.lblHiChan.Text = "High Channel:";
            this.lblHiChan.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblShowLoChan
            // 
            this.lblShowLoChan.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowLoChan.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowLoChan.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowLoChan.ForeColor = System.Drawing.Color.Blue;
            this.lblShowLoChan.Location = new System.Drawing.Point(181, 289);
            this.lblShowLoChan.Name = "lblShowLoChan";
            this.lblShowLoChan.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowLoChan.Size = new System.Drawing.Size(52, 14);
            this.lblShowLoChan.TabIndex = 56;
            // 
            // lblLoChan
            // 
            this.lblLoChan.BackColor = System.Drawing.SystemColors.Window;
            this.lblLoChan.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblLoChan.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLoChan.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblLoChan.Location = new System.Drawing.Point(46, 289);
            this.lblLoChan.Name = "lblLoChan";
            this.lblLoChan.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblLoChan.Size = new System.Drawing.Size(129, 14);
            this.lblLoChan.TabIndex = 49;
            this.lblLoChan.Text = "Low Channel:";
            this.lblLoChan.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblShowFileName
            // 
            this.lblShowFileName.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowFileName.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowFileName.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowFileName.ForeColor = System.Drawing.Color.Blue;
            this.lblShowFileName.Location = new System.Drawing.Point(181, 276);
            this.lblShowFileName.Name = "lblShowFileName";
            this.lblShowFileName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowFileName.Size = new System.Drawing.Size(183, 14);
            this.lblShowFileName.TabIndex = 55;
            // 
            // lblFileName
            // 
            this.lblFileName.BackColor = System.Drawing.SystemColors.Window;
            this.lblFileName.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblFileName.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileName.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblFileName.Location = new System.Drawing.Point(46, 276);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblFileName.Size = new System.Drawing.Size(129, 14);
            this.lblFileName.TabIndex = 48;
            this.lblFileName.Text = "Streamer File Name:";
            this.lblFileName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // _lblPostTrig_10
            // 
            this._lblPostTrig_10.BackColor = System.Drawing.SystemColors.Window;
            this._lblPostTrig_10.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPostTrig_10.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPostTrig_10.ForeColor = System.Drawing.Color.Blue;
            this._lblPostTrig_10.Location = new System.Drawing.Point(296, 228);
            this._lblPostTrig_10.Name = "_lblPostTrig_10";
            this._lblPostTrig_10.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPostTrig_10.Size = new System.Drawing.Size(65, 13);
            this._lblPostTrig_10.TabIndex = 42;
            this._lblPostTrig_10.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPreTrig_9
            // 
            this._lblPreTrig_9.BackColor = System.Drawing.SystemColors.Window;
            this._lblPreTrig_9.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPreTrig_9.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPreTrig_9.ForeColor = System.Drawing.Color.Blue;
            this._lblPreTrig_9.Location = new System.Drawing.Point(96, 228);
            this._lblPreTrig_9.Name = "_lblPreTrig_9";
            this._lblPreTrig_9.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPreTrig_9.Size = new System.Drawing.Size(65, 13);
            this._lblPreTrig_9.TabIndex = 18;
            this._lblPreTrig_9.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPostTrig_9
            // 
            this._lblPostTrig_9.BackColor = System.Drawing.SystemColors.Window;
            this._lblPostTrig_9.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPostTrig_9.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPostTrig_9.ForeColor = System.Drawing.Color.Blue;
            this._lblPostTrig_9.Location = new System.Drawing.Point(296, 214);
            this._lblPostTrig_9.Name = "_lblPostTrig_9";
            this._lblPostTrig_9.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPostTrig_9.Size = new System.Drawing.Size(65, 13);
            this._lblPostTrig_9.TabIndex = 41;
            this._lblPostTrig_9.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPreTrig_8
            // 
            this._lblPreTrig_8.BackColor = System.Drawing.SystemColors.Window;
            this._lblPreTrig_8.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPreTrig_8.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPreTrig_8.ForeColor = System.Drawing.Color.Blue;
            this._lblPreTrig_8.Location = new System.Drawing.Point(96, 214);
            this._lblPreTrig_8.Name = "_lblPreTrig_8";
            this._lblPreTrig_8.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPreTrig_8.Size = new System.Drawing.Size(65, 13);
            this._lblPreTrig_8.TabIndex = 17;
            this._lblPreTrig_8.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPostTrig_8
            // 
            this._lblPostTrig_8.BackColor = System.Drawing.SystemColors.Window;
            this._lblPostTrig_8.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPostTrig_8.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPostTrig_8.ForeColor = System.Drawing.Color.Blue;
            this._lblPostTrig_8.Location = new System.Drawing.Point(296, 200);
            this._lblPostTrig_8.Name = "_lblPostTrig_8";
            this._lblPostTrig_8.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPostTrig_8.Size = new System.Drawing.Size(65, 13);
            this._lblPostTrig_8.TabIndex = 38;
            this._lblPostTrig_8.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPreTrig_7
            // 
            this._lblPreTrig_7.BackColor = System.Drawing.SystemColors.Window;
            this._lblPreTrig_7.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPreTrig_7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPreTrig_7.ForeColor = System.Drawing.Color.Blue;
            this._lblPreTrig_7.Location = new System.Drawing.Point(96, 200);
            this._lblPreTrig_7.Name = "_lblPreTrig_7";
            this._lblPreTrig_7.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPreTrig_7.Size = new System.Drawing.Size(65, 13);
            this._lblPreTrig_7.TabIndex = 16;
            this._lblPreTrig_7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPostTrig_7
            // 
            this._lblPostTrig_7.BackColor = System.Drawing.SystemColors.Window;
            this._lblPostTrig_7.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPostTrig_7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPostTrig_7.ForeColor = System.Drawing.Color.Blue;
            this._lblPostTrig_7.Location = new System.Drawing.Point(296, 186);
            this._lblPostTrig_7.Name = "_lblPostTrig_7";
            this._lblPostTrig_7.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPostTrig_7.Size = new System.Drawing.Size(65, 13);
            this._lblPostTrig_7.TabIndex = 34;
            this._lblPostTrig_7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPreTrig_6
            // 
            this._lblPreTrig_6.BackColor = System.Drawing.SystemColors.Window;
            this._lblPreTrig_6.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPreTrig_6.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPreTrig_6.ForeColor = System.Drawing.Color.Blue;
            this._lblPreTrig_6.Location = new System.Drawing.Point(96, 186);
            this._lblPreTrig_6.Name = "_lblPreTrig_6";
            this._lblPreTrig_6.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPreTrig_6.Size = new System.Drawing.Size(65, 13);
            this._lblPreTrig_6.TabIndex = 15;
            this._lblPreTrig_6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPostTrig_6
            // 
            this._lblPostTrig_6.BackColor = System.Drawing.SystemColors.Window;
            this._lblPostTrig_6.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPostTrig_6.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPostTrig_6.ForeColor = System.Drawing.Color.Blue;
            this._lblPostTrig_6.Location = new System.Drawing.Point(296, 171);
            this._lblPostTrig_6.Name = "_lblPostTrig_6";
            this._lblPostTrig_6.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPostTrig_6.Size = new System.Drawing.Size(65, 13);
            this._lblPostTrig_6.TabIndex = 30;
            this._lblPostTrig_6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPreTrig_5
            // 
            this._lblPreTrig_5.BackColor = System.Drawing.SystemColors.Window;
            this._lblPreTrig_5.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPreTrig_5.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPreTrig_5.ForeColor = System.Drawing.Color.Blue;
            this._lblPreTrig_5.Location = new System.Drawing.Point(96, 171);
            this._lblPreTrig_5.Name = "_lblPreTrig_5";
            this._lblPreTrig_5.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPreTrig_5.Size = new System.Drawing.Size(65, 13);
            this._lblPreTrig_5.TabIndex = 14;
            this._lblPreTrig_5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPostTrig_5
            // 
            this._lblPostTrig_5.BackColor = System.Drawing.SystemColors.Window;
            this._lblPostTrig_5.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPostTrig_5.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPostTrig_5.ForeColor = System.Drawing.Color.Blue;
            this._lblPostTrig_5.Location = new System.Drawing.Point(296, 157);
            this._lblPostTrig_5.Name = "_lblPostTrig_5";
            this._lblPostTrig_5.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPostTrig_5.Size = new System.Drawing.Size(65, 14);
            this._lblPostTrig_5.TabIndex = 26;
            this._lblPostTrig_5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPreTrig_4
            // 
            this._lblPreTrig_4.BackColor = System.Drawing.SystemColors.Window;
            this._lblPreTrig_4.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPreTrig_4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPreTrig_4.ForeColor = System.Drawing.Color.Blue;
            this._lblPreTrig_4.Location = new System.Drawing.Point(96, 157);
            this._lblPreTrig_4.Name = "_lblPreTrig_4";
            this._lblPreTrig_4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPreTrig_4.Size = new System.Drawing.Size(65, 13);
            this._lblPreTrig_4.TabIndex = 13;
            this._lblPreTrig_4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPostTrig_4
            // 
            this._lblPostTrig_4.BackColor = System.Drawing.SystemColors.Window;
            this._lblPostTrig_4.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPostTrig_4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPostTrig_4.ForeColor = System.Drawing.Color.Blue;
            this._lblPostTrig_4.Location = new System.Drawing.Point(296, 142);
            this._lblPostTrig_4.Name = "_lblPostTrig_4";
            this._lblPostTrig_4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPostTrig_4.Size = new System.Drawing.Size(65, 14);
            this._lblPostTrig_4.TabIndex = 36;
            this._lblPostTrig_4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPreTrig_3
            // 
            this._lblPreTrig_3.BackColor = System.Drawing.SystemColors.Window;
            this._lblPreTrig_3.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPreTrig_3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPreTrig_3.ForeColor = System.Drawing.Color.Blue;
            this._lblPreTrig_3.Location = new System.Drawing.Point(96, 142);
            this._lblPreTrig_3.Name = "_lblPreTrig_3";
            this._lblPreTrig_3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPreTrig_3.Size = new System.Drawing.Size(65, 13);
            this._lblPreTrig_3.TabIndex = 12;
            this._lblPreTrig_3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPostTrig_2
            // 
            this._lblPostTrig_2.BackColor = System.Drawing.SystemColors.Window;
            this._lblPostTrig_2.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPostTrig_2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPostTrig_2.ForeColor = System.Drawing.Color.Blue;
            this._lblPostTrig_2.Location = new System.Drawing.Point(296, 127);
            this._lblPostTrig_2.Name = "_lblPostTrig_2";
            this._lblPostTrig_2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPostTrig_2.Size = new System.Drawing.Size(65, 13);
            this._lblPostTrig_2.TabIndex = 28;
            this._lblPostTrig_2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPreTrig_2
            // 
            this._lblPreTrig_2.BackColor = System.Drawing.SystemColors.Window;
            this._lblPreTrig_2.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPreTrig_2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPreTrig_2.ForeColor = System.Drawing.Color.Blue;
            this._lblPreTrig_2.Location = new System.Drawing.Point(96, 127);
            this._lblPreTrig_2.Name = "_lblPreTrig_2";
            this._lblPreTrig_2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPreTrig_2.Size = new System.Drawing.Size(65, 13);
            this._lblPreTrig_2.TabIndex = 11;
            this._lblPreTrig_2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPostTrig_3
            // 
            this._lblPostTrig_3.BackColor = System.Drawing.SystemColors.Window;
            this._lblPostTrig_3.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPostTrig_3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPostTrig_3.ForeColor = System.Drawing.Color.Blue;
            this._lblPostTrig_3.Location = new System.Drawing.Point(296, 111);
            this._lblPostTrig_3.Name = "_lblPostTrig_3";
            this._lblPostTrig_3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPostTrig_3.Size = new System.Drawing.Size(65, 14);
            this._lblPostTrig_3.TabIndex = 32;
            this._lblPostTrig_3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPreTrig_1
            // 
            this._lblPreTrig_1.BackColor = System.Drawing.SystemColors.Window;
            this._lblPreTrig_1.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPreTrig_1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPreTrig_1.ForeColor = System.Drawing.Color.Blue;
            this._lblPreTrig_1.Location = new System.Drawing.Point(96, 111);
            this._lblPreTrig_1.Name = "_lblPreTrig_1";
            this._lblPreTrig_1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPreTrig_1.Size = new System.Drawing.Size(65, 13);
            this._lblPreTrig_1.TabIndex = 10;
            this._lblPreTrig_1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPostTrig_1
            // 
            this._lblPostTrig_1.BackColor = System.Drawing.SystemColors.Window;
            this._lblPostTrig_1.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPostTrig_1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPostTrig_1.ForeColor = System.Drawing.Color.Blue;
            this._lblPostTrig_1.Location = new System.Drawing.Point(296, 97);
            this._lblPostTrig_1.Name = "_lblPostTrig_1";
            this._lblPostTrig_1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPostTrig_1.Size = new System.Drawing.Size(65, 13);
            this._lblPostTrig_1.TabIndex = 24;
            this._lblPostTrig_1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblPreTrig_0
            // 
            this._lblPreTrig_0.BackColor = System.Drawing.SystemColors.Window;
            this._lblPreTrig_0.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblPreTrig_0.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPreTrig_0.ForeColor = System.Drawing.Color.Blue;
            this._lblPreTrig_0.Location = new System.Drawing.Point(96, 97);
            this._lblPreTrig_0.Name = "_lblPreTrig_0";
            this._lblPreTrig_0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblPreTrig_0.Size = new System.Drawing.Size(65, 13);
            this._lblPreTrig_0.TabIndex = 9;
            this._lblPreTrig_0.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblPostTrigData
            // 
            this.lblPostTrigData.BackColor = System.Drawing.SystemColors.Window;
            this.lblPostTrigData.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPostTrigData.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPostTrigData.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblPostTrigData.Location = new System.Drawing.Point(192, 74);
            this.lblPostTrigData.Name = "lblPostTrigData";
            this.lblPostTrigData.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPostTrigData.Size = new System.Drawing.Size(164, 14);
            this.lblPostTrigData.TabIndex = 44;
            this.lblPostTrigData.Text = "Data acquired after trigger";
            this.lblPostTrigData.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblPreTrigData
            // 
            this.lblPreTrigData.BackColor = System.Drawing.SystemColors.Window;
            this.lblPreTrigData.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPreTrigData.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPreTrigData.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblPreTrigData.Location = new System.Drawing.Point(13, 74);
            this.lblPreTrigData.Name = "lblPreTrigData";
            this.lblPreTrigData.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPreTrigData.Size = new System.Drawing.Size(161, 14);
            this.lblPreTrigData.TabIndex = 43;
            this.lblPreTrigData.Text = "Data acquired before trigger";
            this.lblPreTrigData.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblDemoFunction
            // 
            this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
            this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDemoFunction.Location = new System.Drawing.Point(22, 6);
            this.lblDemoFunction.Name = "lblDemoFunction";
            this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDemoFunction.Size = new System.Drawing.Size(345, 22);
            this.lblDemoFunction.TabIndex = 0;
            this.lblDemoFunction.Text = "Demonstration of Mccdaq.MccBoard.FilePretrig()";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblAcqStat
            // 
            this.lblAcqStat.BackColor = System.Drawing.SystemColors.Window;
            this.lblAcqStat.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblAcqStat.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAcqStat.ForeColor = System.Drawing.Color.Blue;
            this.lblAcqStat.Location = new System.Drawing.Point(18, 30);
            this.lblAcqStat.Name = "lblAcqStat";
            this.lblAcqStat.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblAcqStat.Size = new System.Drawing.Size(354, 36);
            this.lblAcqStat.TabIndex = 65;
            this.lblAcqStat.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblPre1
            // 
            this.lblPre1.BackColor = System.Drawing.SystemColors.Window;
            this.lblPre1.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPre1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPre1.ForeColor = System.Drawing.Color.Blue;
            this.lblPre1.Location = new System.Drawing.Point(14, 227);
            this.lblPre1.Name = "lblPre1";
            this.lblPre1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPre1.Size = new System.Drawing.Size(73, 13);
            this.lblPre1.TabIndex = 75;
            this.lblPre1.Text = "Trigger -1";
            this.lblPre1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPre2
            // 
            this.lblPre2.BackColor = System.Drawing.SystemColors.Window;
            this.lblPre2.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPre2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPre2.ForeColor = System.Drawing.Color.Blue;
            this.lblPre2.Location = new System.Drawing.Point(14, 214);
            this.lblPre2.Name = "lblPre2";
            this.lblPre2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPre2.Size = new System.Drawing.Size(73, 13);
            this.lblPre2.TabIndex = 74;
            this.lblPre2.Text = "Trigger -2";
            this.lblPre2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPre3
            // 
            this.lblPre3.BackColor = System.Drawing.SystemColors.Window;
            this.lblPre3.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPre3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPre3.ForeColor = System.Drawing.Color.Blue;
            this.lblPre3.Location = new System.Drawing.Point(14, 200);
            this.lblPre3.Name = "lblPre3";
            this.lblPre3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPre3.Size = new System.Drawing.Size(73, 13);
            this.lblPre3.TabIndex = 73;
            this.lblPre3.Text = "Trigger -3";
            this.lblPre3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPre4
            // 
            this.lblPre4.BackColor = System.Drawing.SystemColors.Window;
            this.lblPre4.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPre4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPre4.ForeColor = System.Drawing.Color.Blue;
            this.lblPre4.Location = new System.Drawing.Point(14, 186);
            this.lblPre4.Name = "lblPre4";
            this.lblPre4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPre4.Size = new System.Drawing.Size(73, 13);
            this.lblPre4.TabIndex = 72;
            this.lblPre4.Text = "Trigger -4";
            this.lblPre4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPre5
            // 
            this.lblPre5.BackColor = System.Drawing.SystemColors.Window;
            this.lblPre5.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPre5.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPre5.ForeColor = System.Drawing.Color.Blue;
            this.lblPre5.Location = new System.Drawing.Point(14, 171);
            this.lblPre5.Name = "lblPre5";
            this.lblPre5.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPre5.Size = new System.Drawing.Size(73, 13);
            this.lblPre5.TabIndex = 71;
            this.lblPre5.Text = "Trigger -5";
            this.lblPre5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPre6
            // 
            this.lblPre6.BackColor = System.Drawing.SystemColors.Window;
            this.lblPre6.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPre6.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPre6.ForeColor = System.Drawing.Color.Blue;
            this.lblPre6.Location = new System.Drawing.Point(14, 157);
            this.lblPre6.Name = "lblPre6";
            this.lblPre6.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPre6.Size = new System.Drawing.Size(73, 13);
            this.lblPre6.TabIndex = 70;
            this.lblPre6.Text = "Trigger -6";
            this.lblPre6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPre7
            // 
            this.lblPre7.BackColor = System.Drawing.SystemColors.Window;
            this.lblPre7.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPre7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPre7.ForeColor = System.Drawing.Color.Blue;
            this.lblPre7.Location = new System.Drawing.Point(14, 142);
            this.lblPre7.Name = "lblPre7";
            this.lblPre7.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPre7.Size = new System.Drawing.Size(73, 13);
            this.lblPre7.TabIndex = 69;
            this.lblPre7.Text = "Trigger -7";
            this.lblPre7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPre8
            // 
            this.lblPre8.BackColor = System.Drawing.SystemColors.Window;
            this.lblPre8.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPre8.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPre8.ForeColor = System.Drawing.Color.Blue;
            this.lblPre8.Location = new System.Drawing.Point(14, 127);
            this.lblPre8.Name = "lblPre8";
            this.lblPre8.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPre8.Size = new System.Drawing.Size(73, 13);
            this.lblPre8.TabIndex = 68;
            this.lblPre8.Text = "Trigger -8";
            this.lblPre8.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPre9
            // 
            this.lblPre9.BackColor = System.Drawing.SystemColors.Window;
            this.lblPre9.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPre9.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPre9.ForeColor = System.Drawing.Color.Blue;
            this.lblPre9.Location = new System.Drawing.Point(14, 111);
            this.lblPre9.Name = "lblPre9";
            this.lblPre9.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPre9.Size = new System.Drawing.Size(73, 13);
            this.lblPre9.TabIndex = 67;
            this.lblPre9.Text = "Trigger -9";
            this.lblPre9.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPre10
            // 
            this.lblPre10.BackColor = System.Drawing.SystemColors.Window;
            this.lblPre10.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPre10.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPre10.ForeColor = System.Drawing.Color.Blue;
            this.lblPre10.Location = new System.Drawing.Point(14, 97);
            this.lblPre10.Name = "lblPre10";
            this.lblPre10.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPre10.Size = new System.Drawing.Size(73, 13);
            this.lblPre10.TabIndex = 66;
            this.lblPre10.Text = "Trigger -10";
            this.lblPre10.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPost10
            // 
            this.lblPost10.BackColor = System.Drawing.SystemColors.Window;
            this.lblPost10.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPost10.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPost10.ForeColor = System.Drawing.Color.Blue;
            this.lblPost10.Location = new System.Drawing.Point(204, 227);
            this.lblPost10.Name = "lblPost10";
            this.lblPost10.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPost10.Size = new System.Drawing.Size(73, 13);
            this.lblPost10.TabIndex = 85;
            this.lblPost10.Text = "Trigger +9";
            this.lblPost10.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPost9
            // 
            this.lblPost9.BackColor = System.Drawing.SystemColors.Window;
            this.lblPost9.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPost9.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPost9.ForeColor = System.Drawing.Color.Blue;
            this.lblPost9.Location = new System.Drawing.Point(204, 214);
            this.lblPost9.Name = "lblPost9";
            this.lblPost9.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPost9.Size = new System.Drawing.Size(73, 13);
            this.lblPost9.TabIndex = 84;
            this.lblPost9.Text = "Trigger +8";
            this.lblPost9.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPost8
            // 
            this.lblPost8.BackColor = System.Drawing.SystemColors.Window;
            this.lblPost8.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPost8.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPost8.ForeColor = System.Drawing.Color.Blue;
            this.lblPost8.Location = new System.Drawing.Point(204, 200);
            this.lblPost8.Name = "lblPost8";
            this.lblPost8.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPost8.Size = new System.Drawing.Size(73, 13);
            this.lblPost8.TabIndex = 83;
            this.lblPost8.Text = "Trigger +7";
            this.lblPost8.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPost7
            // 
            this.lblPost7.BackColor = System.Drawing.SystemColors.Window;
            this.lblPost7.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPost7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPost7.ForeColor = System.Drawing.Color.Blue;
            this.lblPost7.Location = new System.Drawing.Point(204, 186);
            this.lblPost7.Name = "lblPost7";
            this.lblPost7.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPost7.Size = new System.Drawing.Size(73, 14);
            this.lblPost7.TabIndex = 81;
            this.lblPost7.Text = "Trigger +6";
            this.lblPost7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPost6
            // 
            this.lblPost6.BackColor = System.Drawing.SystemColors.Window;
            this.lblPost6.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPost6.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPost6.ForeColor = System.Drawing.Color.Blue;
            this.lblPost6.Location = new System.Drawing.Point(204, 171);
            this.lblPost6.Name = "lblPost6";
            this.lblPost6.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPost6.Size = new System.Drawing.Size(73, 13);
            this.lblPost6.TabIndex = 79;
            this.lblPost6.Text = "Trigger +5";
            this.lblPost6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPost5
            // 
            this.lblPost5.BackColor = System.Drawing.SystemColors.Window;
            this.lblPost5.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPost5.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPost5.ForeColor = System.Drawing.Color.Blue;
            this.lblPost5.Location = new System.Drawing.Point(204, 157);
            this.lblPost5.Name = "lblPost5";
            this.lblPost5.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPost5.Size = new System.Drawing.Size(73, 13);
            this.lblPost5.TabIndex = 77;
            this.lblPost5.Text = "Trigger +4";
            this.lblPost5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPost4
            // 
            this.lblPost4.BackColor = System.Drawing.SystemColors.Window;
            this.lblPost4.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPost4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPost4.ForeColor = System.Drawing.Color.Blue;
            this.lblPost4.Location = new System.Drawing.Point(204, 142);
            this.lblPost4.Name = "lblPost4";
            this.lblPost4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPost4.Size = new System.Drawing.Size(73, 13);
            this.lblPost4.TabIndex = 82;
            this.lblPost4.Text = "Trigger +3";
            this.lblPost4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPost3
            // 
            this.lblPost3.BackColor = System.Drawing.SystemColors.Window;
            this.lblPost3.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPost3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPost3.ForeColor = System.Drawing.Color.Blue;
            this.lblPost3.Location = new System.Drawing.Point(204, 127);
            this.lblPost3.Name = "lblPost3";
            this.lblPost3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPost3.Size = new System.Drawing.Size(73, 13);
            this.lblPost3.TabIndex = 80;
            this.lblPost3.Text = "Trigger +2";
            this.lblPost3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPost2
            // 
            this.lblPost2.BackColor = System.Drawing.SystemColors.Window;
            this.lblPost2.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPost2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPost2.ForeColor = System.Drawing.Color.Blue;
            this.lblPost2.Location = new System.Drawing.Point(204, 111);
            this.lblPost2.Name = "lblPost2";
            this.lblPost2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPost2.Size = new System.Drawing.Size(73, 13);
            this.lblPost2.TabIndex = 78;
            this.lblPost2.Text = "Trigger +1";
            this.lblPost2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblPost1
            // 
            this.lblPost1.BackColor = System.Drawing.SystemColors.Window;
            this.lblPost1.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPost1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPost1.ForeColor = System.Drawing.Color.Blue;
            this.lblPost1.Location = new System.Drawing.Point(204, 97);
            this.lblPost1.Name = "lblPost1";
            this.lblPost1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPost1.Size = new System.Drawing.Size(73, 13);
            this.lblPost1.TabIndex = 76;
            this.lblPost1.Text = "Trigger";
            this.lblPost1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // frmFilePreTrig
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(390, 418);
            this.Controls.Add(this.lblPost10);
            this.Controls.Add(this.lblPost9);
            this.Controls.Add(this.lblPost8);
            this.Controls.Add(this.lblPost7);
            this.Controls.Add(this.lblPost6);
            this.Controls.Add(this.lblPost5);
            this.Controls.Add(this.lblPost4);
            this.Controls.Add(this.lblPost3);
            this.Controls.Add(this.lblPost2);
            this.Controls.Add(this.lblPost1);
            this.Controls.Add(this.lblPre1);
            this.Controls.Add(this.lblPre2);
            this.Controls.Add(this.lblPre3);
            this.Controls.Add(this.lblPre4);
            this.Controls.Add(this.lblPre5);
            this.Controls.Add(this.lblPre6);
            this.Controls.Add(this.lblPre7);
            this.Controls.Add(this.lblPre8);
            this.Controls.Add(this.lblPre9);
            this.Controls.Add(this.lblPre10);
            this.Controls.Add(this.lblAcqStat);
            this.Controls.Add(this.txtFileName);
            this.Controls.Add(this.cmdQuit);
            this.Controls.Add(this.cmdTrigEnable);
            this.Controls.Add(this.lblFileInstruct);
            this.Controls.Add(this.lblShowGain);
            this.Controls.Add(this.lblGain);
            this.Controls.Add(this.lblShowRate);
            this.Controls.Add(this.lblRate);
            this.Controls.Add(this.lblShowNumSam);
            this.Controls.Add(this.lblNumSam);
            this.Controls.Add(this.lblShowPT);
            this.Controls.Add(this.lblNumPTSam);
            this.Controls.Add(this.lblShowHiChan);
            this.Controls.Add(this.lblHiChan);
            this.Controls.Add(this.lblShowLoChan);
            this.Controls.Add(this.lblLoChan);
            this.Controls.Add(this.lblShowFileName);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this._lblPostTrig_10);
            this.Controls.Add(this._lblPreTrig_9);
            this.Controls.Add(this._lblPostTrig_9);
            this.Controls.Add(this._lblPreTrig_8);
            this.Controls.Add(this._lblPostTrig_8);
            this.Controls.Add(this._lblPreTrig_7);
            this.Controls.Add(this._lblPostTrig_7);
            this.Controls.Add(this._lblPreTrig_6);
            this.Controls.Add(this._lblPostTrig_6);
            this.Controls.Add(this._lblPreTrig_5);
            this.Controls.Add(this._lblPostTrig_5);
            this.Controls.Add(this._lblPreTrig_4);
            this.Controls.Add(this._lblPostTrig_4);
            this.Controls.Add(this._lblPreTrig_3);
            this.Controls.Add(this._lblPostTrig_2);
            this.Controls.Add(this._lblPreTrig_2);
            this.Controls.Add(this._lblPostTrig_3);
            this.Controls.Add(this._lblPreTrig_1);
            this.Controls.Add(this._lblPostTrig_1);
            this.Controls.Add(this._lblPreTrig_0);
            this.Controls.Add(this.lblPostTrigData);
            this.Controls.Add(this.lblPreTrigData);
            this.Controls.Add(this.lblDemoFunction);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Blue;
            this.Location = new System.Drawing.Point(7, 103);
            this.Name = "frmFilePreTrig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Universal Library Analog Input to File";
            this.Load += new System.EventHandler(this.frmFilePreTrig_Load);
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
			Application.Run(new frmFilePreTrig());
		}

        public frmFilePreTrig()
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
        public TextBox txtFileName;
        public Button cmdQuit;
        public Button cmdTrigEnable;
        public Label lblFileInstruct;
        public Label lblShowGain;
        public Label lblGain;
        public Label lblShowRate;
        public Label lblRate;
        public Label lblShowNumSam;
        public Label lblNumSam;
        public Label lblShowPT;
        public Label lblNumPTSam;
        public Label lblShowHiChan;
        public Label lblHiChan;
        public Label lblShowLoChan;
        public Label lblLoChan;
        public Label lblShowFileName;
        public Label lblFileName;
        public Label _lblPostTrig_10;
        public Label _lblPreTrig_9;
        public Label _lblPostTrig_9;
        public Label _lblPreTrig_8;
        public Label _lblPostTrig_8;
        public Label _lblPreTrig_7;
        public Label _lblPostTrig_7;
        public Label _lblPreTrig_6;
        public Label _lblPostTrig_6;
        public Label _lblPreTrig_5;
        public Label _lblPostTrig_5;
        public Label _lblPreTrig_4;
        public Label _lblPostTrig_4;
        public Label _lblPreTrig_3;
        public Label _lblPostTrig_2;
        public Label _lblPreTrig_2;
        public Label _lblPostTrig_3;
        public Label _lblPreTrig_1;
        public Label _lblPostTrig_1;
        public Label _lblPreTrig_0;
        public Label _lblPostTrig_0;
        public Label lblPostTrigData;
        public Label lblPreTrigData;
        public Label lblDemoFunction;

        public Label[] lblPre;
        public Label[] lblPreTrig;
        public Label[] lblPostTrig;
        public Label lblAcqStat;
        public Label lblPre1;
        public Label lblPre2;
        public Label lblPre3;
        public Label lblPre4;
        public Label lblPre5;
        public Label lblPre6;
        public Label lblPre7;
        public Label lblPre8;
        public Label lblPre9;
        public Label lblPre10;
        public Label lblPost10;
        public Label lblPost9;
        public Label lblPost8;
        public Label lblPost7;
        public Label lblPost6;
        public Label lblPost5;
        public Label lblPost4;
        public Label lblPost3;
        public Label lblPost2;
        public Label lblPost1;

        #endregion

    }
}