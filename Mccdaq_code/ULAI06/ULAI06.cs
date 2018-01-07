// ==============================================================================
//
//  File:                         ULAI06.CS
//
//  Library Call Demonstrated:    Mccdaq.MccBoard.AInScan(), continuous background mode
//
//  Purpose:                      Scans a range of A/D Input Channels continuously
//                                in the background and stores the data in an array.
//
//  Demonstration:                Continuously collects data on up to eight channels.
//
//  Other Library Calls:          Mccdaq.MccBoard.GetStatus()
//                                Mccdaq.MccBoard.StopBackground()
//                                Mccdaq.MccBoard.ErrHandling()
//
//  Special Requirements:         Board 0 must have an A/D converter.
//                                Analog signals on up to eight input channels.
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

namespace ULAI06
{
	public class frmStatusDisplay : System.Windows.Forms.Form
	{

        // Create a new MccBoard object for Board 0
        MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

        MccDaq.Range Range;
        private int ADResolution, NumAIChans;
        private int HighChan, LowChan, MaxChan;

        const int NumPoints = 31744;  //  Number of data points to collect
        //                                For some devices, this number must be a
        //                                multiple of the packet size. 31744 is a
        //                                multiple of the most common packet sizes,
        //                                so it satifies this requirement for most devices.

        private ushort[] ADData;      //  dimension an array to hold the input values
        private uint[] ADData32;      //  dimension an array to hold the high resolution input values

        // define a variable to contain the handle for memory allocated by Windows through
        // MccDaq.MccService.WinBufAlloc() or MccDaq.MccService.WinBufAlloc32()
		private IntPtr MemHandle;

        AnalogIO.clsAnalogIO AIOProps = new AnalogIO.clsAnalogIO();

        private void frmStatusDisplay_Load(object sender, EventArgs e)
        {

            MccDaq.TriggerType DefaultTrig;

            InitUL();

            // determine the number of analog channels and their capabilities
            int ChannelType = clsAnalogIO.ANALOGINPUT;
            NumAIChans = AIOProps.FindAnalogChansOfType(DaqBoard, ChannelType,
                out ADResolution, out Range, out LowChan, out DefaultTrig);

            if (NumAIChans == 0)
            {
                lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " does not have analog input channels.";
                cmdStartBgnd.Enabled = false;
                txtHighChan.Enabled = false;
            }
            else
            {
                // Check the resolution of the A/D data and allocate memory accordingly
                if (ADResolution > 16)
                {
                    // set aside memory to hold high resolution data
                    ADData32 = new uint[NumPoints];
                    MemHandle = MccDaq.MccService.WinBufAlloc32Ex(NumPoints);
                }
                else
                {
                    // set aside memory to hold 16-bit data
                    ADData = new ushort[NumPoints];
                    MemHandle = MccDaq.MccService.WinBufAllocEx(NumPoints);
                }
                if (MemHandle == IntPtr.Zero)
                {
                    cmdStartBgnd.Enabled = false;
                    NumAIChans = 0;
                }
                if (NumAIChans > 8) NumAIChans = 8;
                MaxChan = LowChan + NumAIChans - 1;
                lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " collecting analog data on up to " + NumAIChans.ToString() +
                    " channels using AInScan with Range set to " + Range.ToString() + ".";

            }

        }

        private void cmdStartBgnd_Click(object eventSender, System.EventArgs eventArgs)
        {
            int CurIndex;
            int CurCount;
            short Status;
            MccDaq.ErrorInfo ULStat;
            MccDaq.ScanOptions Options;
            int Rate;
            int Count;
            bool ValidChan;

            cmdStartBgnd.Enabled = false;
            cmdStartBgnd.Visible = false;
            cmdStopConvert.Enabled = true;
            cmdStopConvert.Visible = true;
            cmdQuit.Enabled = false;

            //  Collect the values by calling MccDaq.MccBoard.AInScan
            //   Parameters:
            //     LowChan    :the first channel of the scan
            //     HighChan   :the last channel of the scan
            //     Count      :the total number of A/D samples to collect
            //     Rate       :sample rate
            //     Range      :the range for the board
            //     MemHandle  :Handle for Windows buffer to store data in
            //     Options    :data collection options

            ValidChan = int.TryParse(txtHighChan.Text, out HighChan);
            if (ValidChan)
            {
                if (HighChan > MaxChan) HighChan = MaxChan;
                txtHighChan.Text = HighChan.ToString();
            }

            Count = NumPoints;     //  total number of data points to collect

            //  per channel sampling rate ((samples per second) per channel)
            Rate = 1000 / ((HighChan - LowChan) + 1);            
            Options = MccDaq.ScanOptions.Background
                    | MccDaq.ScanOptions.Continuous;

            //  collect data in background continuously
            ULStat = DaqBoard.AInScan(LowChan, HighChan, Count, ref Rate, Range, MemHandle, Options);

            ULStat = DaqBoard.GetStatus(out Status, out CurCount, out CurIndex, MccDaq.FunctionType.AiFunction);

            if (Status == MccDaq.MccBoard.Running)
            {
                lblShowStat.Text = "Running";
                lblShowCount.Text = CurCount.ToString("D");
                lblShowIndex.Text = CurIndex.ToString("D");
            }

            tmrCheckStatus.Enabled = true;
        }

        private void tmrCheckStatus_Tick(object eventSender, System.EventArgs eventArgs)
        {
            int j;
            int i;
            int FirstPoint, NumChans;
            MccDaq.ErrorInfo ULStat;
            int CurIndex;
            int CurCount;
            short Status;

            tmrCheckStatus.Stop();

            //  Check the status of the background data collection

            //  Parameters:
            //    Status      :current status of the background data collection
            //    CurCount    :current number of samples collected
            //    CurIndex    :index to the data buffer pointing to the start of the
            //                 most recently collected scan
            //    FunctionType: A/D operation (MccDaq.FunctionType.AiFunction)

            ULStat = DaqBoard.GetStatus(out Status, out CurCount, out CurIndex, MccDaq.FunctionType.AiFunction);

            if (Status == MccDaq.MccBoard.Running) lblShowStat.Text = "Running";
            lblShowCount.Text = CurCount.ToString("D");
            lblShowIndex.Text = CurIndex.ToString("D");
            NumChans = (HighChan - LowChan) + 1;
            if (CurCount > HighChan)
            {
                FirstPoint = CurIndex; // start of latest channel scan in MemHandle buffer

                if (ADResolution > 16)
                {
                    ULStat = MccDaq.MccService.WinBufToArray32(MemHandle, ADData32, FirstPoint, NumChans);

                    for (i = 0; i <= HighChan; ++i)
                        lblADData[i].Text = ADData32[i].ToString("D");
                }
                else
                {
                    ULStat = MccDaq.MccService.WinBufToArray(MemHandle, ADData, FirstPoint, NumChans);

                    for (i = 0; i <= HighChan; ++i)
                        lblADData[i].Text = ADData[i].ToString("D");
                }

                for (j = HighChan + 1; j <= 7; ++j)
                    lblADData[j].Text = "";
            }

            tmrCheckStatus.Start();

        }

        private void cmdStopConvert_Click(object eventSender, System.EventArgs eventArgs)
        {

            int CurIndex;
            int CurCount;
            short Status;
            MccDaq.ErrorInfo ULStat;

            ULStat = DaqBoard.StopBackground(MccDaq.FunctionType.AiFunction);

            cmdStartBgnd.Enabled = true;
            cmdStartBgnd.Visible = true;
            cmdStopConvert.Enabled = false;
            cmdStopConvert.Visible = false;
            cmdQuit.Enabled = true;
            tmrCheckStatus.Enabled = false;

            ULStat = DaqBoard.GetStatus(out Status, out CurCount, out CurIndex, MccDaq.FunctionType.AiFunction);

            if (Status == MccDaq.MccBoard.Idle) lblShowStat.Text = "Idle";
            lblShowCount.Text = CurCount.ToString("D");
            lblShowIndex.Text = CurIndex.ToString("D");

        }

        private void cmdQuit_Click(object eventSender, System.EventArgs eventArgs)
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

            //  This gives us access to labels via an indexed array
            lblADData = (new Label[] {this._lblADData_0, this._lblADData_1, 
                this._lblADData_2, this._lblADData_3, this._lblADData_4, 
                this._lblADData_5, this._lblADData_6, this._lblADData_7});

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
            this.txtHighChan = new System.Windows.Forms.TextBox();
            this.cmdQuit = new System.Windows.Forms.Button();
            this.tmrCheckStatus = new System.Windows.Forms.Timer(this.components);
            this.cmdStartBgnd = new System.Windows.Forms.Button();
            this.cmdStopConvert = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.lblShowCount = new System.Windows.Forms.Label();
            this.lblCount = new System.Windows.Forms.Label();
            this.lblShowIndex = new System.Windows.Forms.Label();
            this.lblIndex = new System.Windows.Forms.Label();
            this.lblShowStat = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this._lblADData_7 = new System.Windows.Forms.Label();
            this.lblChan7 = new System.Windows.Forms.Label();
            this._lblADData_3 = new System.Windows.Forms.Label();
            this.lblChan3 = new System.Windows.Forms.Label();
            this._lblADData_6 = new System.Windows.Forms.Label();
            this.lblChan6 = new System.Windows.Forms.Label();
            this._lblADData_2 = new System.Windows.Forms.Label();
            this.lblChan2 = new System.Windows.Forms.Label();
            this._lblADData_5 = new System.Windows.Forms.Label();
            this.lblChan5 = new System.Windows.Forms.Label();
            this._lblADData_1 = new System.Windows.Forms.Label();
            this.lblChan1 = new System.Windows.Forms.Label();
            this._lblADData_4 = new System.Windows.Forms.Label();
            this.lblChan4 = new System.Windows.Forms.Label();
            this._lblADData_0 = new System.Windows.Forms.Label();
            this.lblChan0 = new System.Windows.Forms.Label();
            this.lblDemoFunction = new System.Windows.Forms.Label();
            this.lblInstruction = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtHighChan
            // 
            this.txtHighChan.AcceptsReturn = true;
            this.txtHighChan.BackColor = System.Drawing.SystemColors.Window;
            this.txtHighChan.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHighChan.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtHighChan.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHighChan.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtHighChan.Location = new System.Drawing.Point(217, 143);
            this.txtHighChan.MaxLength = 0;
            this.txtHighChan.Name = "txtHighChan";
            this.txtHighChan.Size = new System.Drawing.Size(25, 19);
            this.txtHighChan.TabIndex = 27;
            this.txtHighChan.Text = "0";
            this.txtHighChan.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cmdQuit
            // 
            this.cmdQuit.BackColor = System.Drawing.SystemColors.Control;
            this.cmdQuit.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdQuit.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdQuit.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdQuit.Location = new System.Drawing.Point(272, 307);
            this.cmdQuit.Name = "cmdQuit";
            this.cmdQuit.Size = new System.Drawing.Size(65, 26);
            this.cmdQuit.TabIndex = 19;
            this.cmdQuit.Text = "Quit";
            this.cmdQuit.UseVisualStyleBackColor = false;
            this.cmdQuit.Click += new System.EventHandler(this.cmdQuit_Click);
            // 
            // tmrCheckStatus
            // 
            this.tmrCheckStatus.Interval = 200;
            this.tmrCheckStatus.Tick += new System.EventHandler(this.tmrCheckStatus_Tick);
            // 
            // cmdStartBgnd
            // 
            this.cmdStartBgnd.BackColor = System.Drawing.SystemColors.Control;
            this.cmdStartBgnd.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdStartBgnd.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdStartBgnd.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdStartBgnd.Location = new System.Drawing.Point(83, 111);
            this.cmdStartBgnd.Name = "cmdStartBgnd";
            this.cmdStartBgnd.Size = new System.Drawing.Size(180, 27);
            this.cmdStartBgnd.TabIndex = 18;
            this.cmdStartBgnd.Text = "Start Background Operation";
            this.cmdStartBgnd.UseVisualStyleBackColor = false;
            this.cmdStartBgnd.Click += new System.EventHandler(this.cmdStartBgnd_Click);
            // 
            // cmdStopConvert
            // 
            this.cmdStopConvert.BackColor = System.Drawing.SystemColors.Control;
            this.cmdStopConvert.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdStopConvert.Enabled = false;
            this.cmdStopConvert.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdStopConvert.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdStopConvert.Location = new System.Drawing.Point(83, 111);
            this.cmdStopConvert.Name = "cmdStopConvert";
            this.cmdStopConvert.Size = new System.Drawing.Size(180, 27);
            this.cmdStopConvert.TabIndex = 17;
            this.cmdStopConvert.Text = "Stop Background Operation";
            this.cmdStopConvert.UseVisualStyleBackColor = false;
            this.cmdStopConvert.Visible = false;
            this.cmdStopConvert.Click += new System.EventHandler(this.cmdStopConvert_Click);
            // 
            // Label1
            // 
            this.Label1.BackColor = System.Drawing.SystemColors.Window;
            this.Label1.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Label1.Location = new System.Drawing.Point(77, 145);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(137, 17);
            this.Label1.TabIndex = 26;
            this.Label1.Text = "Measure Channels 0 to";
            this.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblShowCount
            // 
            this.lblShowCount.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowCount.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowCount.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowCount.ForeColor = System.Drawing.Color.Blue;
            this.lblShowCount.Location = new System.Drawing.Point(198, 301);
            this.lblShowCount.Name = "lblShowCount";
            this.lblShowCount.Size = new System.Drawing.Size(58, 14);
            this.lblShowCount.TabIndex = 25;
            // 
            // lblCount
            // 
            this.lblCount.BackColor = System.Drawing.SystemColors.Window;
            this.lblCount.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblCount.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCount.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblCount.Location = new System.Drawing.Point(83, 301);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(103, 14);
            this.lblCount.TabIndex = 23;
            this.lblCount.Text = "Current Count:";
            this.lblCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblShowIndex
            // 
            this.lblShowIndex.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowIndex.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowIndex.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowIndex.ForeColor = System.Drawing.Color.Blue;
            this.lblShowIndex.Location = new System.Drawing.Point(198, 282);
            this.lblShowIndex.Name = "lblShowIndex";
            this.lblShowIndex.Size = new System.Drawing.Size(52, 14);
            this.lblShowIndex.TabIndex = 24;
            // 
            // lblIndex
            // 
            this.lblIndex.BackColor = System.Drawing.SystemColors.Window;
            this.lblIndex.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblIndex.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIndex.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblIndex.Location = new System.Drawing.Point(83, 282);
            this.lblIndex.Name = "lblIndex";
            this.lblIndex.Size = new System.Drawing.Size(103, 14);
            this.lblIndex.TabIndex = 22;
            this.lblIndex.Text = "Current Index:";
            this.lblIndex.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblShowStat
            // 
            this.lblShowStat.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowStat.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowStat.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowStat.ForeColor = System.Drawing.Color.Blue;
            this.lblShowStat.Location = new System.Drawing.Point(240, 255);
            this.lblShowStat.Name = "lblShowStat";
            this.lblShowStat.Size = new System.Drawing.Size(65, 17);
            this.lblShowStat.TabIndex = 21;
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.SystemColors.Window;
            this.lblStatus.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblStatus.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblStatus.Location = new System.Drawing.Point(8, 255);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(217, 17);
            this.lblStatus.TabIndex = 20;
            this.lblStatus.Text = "Status of Background Operation:";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // _lblADData_7
            // 
            this._lblADData_7.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_7.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_7.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_7.Location = new System.Drawing.Point(264, 229);
            this._lblADData_7.Name = "_lblADData_7";
            this._lblADData_7.Size = new System.Drawing.Size(65, 17);
            this._lblADData_7.TabIndex = 16;
            // 
            // lblChan7
            // 
            this.lblChan7.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan7.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan7.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan7.Location = new System.Drawing.Point(192, 229);
            this.lblChan7.Name = "lblChan7";
            this.lblChan7.Size = new System.Drawing.Size(65, 17);
            this.lblChan7.TabIndex = 8;
            this.lblChan7.Text = "Channel 7:";
            // 
            // _lblADData_3
            // 
            this._lblADData_3.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_3.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_3.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_3.Location = new System.Drawing.Point(96, 229);
            this._lblADData_3.Name = "_lblADData_3";
            this._lblADData_3.Size = new System.Drawing.Size(65, 17);
            this._lblADData_3.TabIndex = 12;
            // 
            // lblChan3
            // 
            this.lblChan3.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan3.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan3.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan3.Location = new System.Drawing.Point(24, 229);
            this.lblChan3.Name = "lblChan3";
            this.lblChan3.Size = new System.Drawing.Size(65, 17);
            this.lblChan3.TabIndex = 4;
            this.lblChan3.Text = "Channel 3:";
            // 
            // _lblADData_6
            // 
            this._lblADData_6.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_6.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_6.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_6.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_6.Location = new System.Drawing.Point(264, 210);
            this._lblADData_6.Name = "_lblADData_6";
            this._lblADData_6.Size = new System.Drawing.Size(65, 17);
            this._lblADData_6.TabIndex = 15;
            // 
            // lblChan6
            // 
            this.lblChan6.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan6.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan6.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan6.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan6.Location = new System.Drawing.Point(192, 210);
            this.lblChan6.Name = "lblChan6";
            this.lblChan6.Size = new System.Drawing.Size(65, 17);
            this.lblChan6.TabIndex = 7;
            this.lblChan6.Text = "Channel 6:";
            // 
            // _lblADData_2
            // 
            this._lblADData_2.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_2.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_2.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_2.Location = new System.Drawing.Point(96, 210);
            this._lblADData_2.Name = "_lblADData_2";
            this._lblADData_2.Size = new System.Drawing.Size(65, 17);
            this._lblADData_2.TabIndex = 11;
            // 
            // lblChan2
            // 
            this.lblChan2.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan2.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan2.Location = new System.Drawing.Point(24, 210);
            this.lblChan2.Name = "lblChan2";
            this.lblChan2.Size = new System.Drawing.Size(65, 17);
            this.lblChan2.TabIndex = 3;
            this.lblChan2.Text = "Channel 2:";
            // 
            // _lblADData_5
            // 
            this._lblADData_5.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_5.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_5.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_5.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_5.Location = new System.Drawing.Point(264, 191);
            this._lblADData_5.Name = "_lblADData_5";
            this._lblADData_5.Size = new System.Drawing.Size(65, 17);
            this._lblADData_5.TabIndex = 14;
            // 
            // lblChan5
            // 
            this.lblChan5.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan5.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan5.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan5.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan5.Location = new System.Drawing.Point(192, 191);
            this.lblChan5.Name = "lblChan5";
            this.lblChan5.Size = new System.Drawing.Size(65, 17);
            this.lblChan5.TabIndex = 6;
            this.lblChan5.Text = "Channel 5:";
            // 
            // _lblADData_1
            // 
            this._lblADData_1.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_1.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_1.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_1.Location = new System.Drawing.Point(96, 191);
            this._lblADData_1.Name = "_lblADData_1";
            this._lblADData_1.Size = new System.Drawing.Size(65, 17);
            this._lblADData_1.TabIndex = 10;
            // 
            // lblChan1
            // 
            this.lblChan1.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan1.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan1.Location = new System.Drawing.Point(24, 191);
            this.lblChan1.Name = "lblChan1";
            this.lblChan1.Size = new System.Drawing.Size(65, 17);
            this.lblChan1.TabIndex = 2;
            this.lblChan1.Text = "Channel 1:";
            // 
            // _lblADData_4
            // 
            this._lblADData_4.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_4.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_4.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_4.Location = new System.Drawing.Point(264, 172);
            this._lblADData_4.Name = "_lblADData_4";
            this._lblADData_4.Size = new System.Drawing.Size(65, 17);
            this._lblADData_4.TabIndex = 13;
            // 
            // lblChan4
            // 
            this.lblChan4.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan4.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan4.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan4.Location = new System.Drawing.Point(192, 172);
            this.lblChan4.Name = "lblChan4";
            this.lblChan4.Size = new System.Drawing.Size(65, 17);
            this.lblChan4.TabIndex = 5;
            this.lblChan4.Text = "Channel 4:";
            // 
            // _lblADData_0
            // 
            this._lblADData_0.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_0.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_0.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_0.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_0.Location = new System.Drawing.Point(96, 172);
            this._lblADData_0.Name = "_lblADData_0";
            this._lblADData_0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_0.Size = new System.Drawing.Size(65, 17);
            this._lblADData_0.TabIndex = 9;
            // 
            // lblChan0
            // 
            this.lblChan0.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan0.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan0.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan0.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan0.Location = new System.Drawing.Point(24, 172);
            this.lblChan0.Name = "lblChan0";
            this.lblChan0.Size = new System.Drawing.Size(65, 17);
            this.lblChan0.TabIndex = 1;
            this.lblChan0.Text = "Channel 0:";
            // 
            // lblDemoFunction
            // 
            this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
            this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDemoFunction.Location = new System.Drawing.Point(32, 8);
            this.lblDemoFunction.Name = "lblDemoFunction";
            this.lblDemoFunction.Size = new System.Drawing.Size(285, 33);
            this.lblDemoFunction.TabIndex = 0;
            this.lblDemoFunction.Text = "Demonstration of Mccdaq.MccBoard.AInScan() in Continuous Background mode";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblInstruction
            // 
            this.lblInstruction.BackColor = System.Drawing.SystemColors.Window;
            this.lblInstruction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblInstruction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstruction.ForeColor = System.Drawing.Color.Red;
            this.lblInstruction.Location = new System.Drawing.Point(39, 45);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblInstruction.Size = new System.Drawing.Size(273, 60);
            this.lblInstruction.TabIndex = 30;
            this.lblInstruction.Text = "Board 0 must have analog inputs that support paced acquisition.";
            this.lblInstruction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // frmStatusDisplay
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(350, 344);
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.txtHighChan);
            this.Controls.Add(this.cmdStartBgnd);
            this.Controls.Add(this.cmdStopConvert);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.lblShowCount);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.lblShowIndex);
            this.Controls.Add(this.lblIndex);
            this.Controls.Add(this.lblShowStat);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this._lblADData_7);
            this.Controls.Add(this.lblChan7);
            this.Controls.Add(this._lblADData_3);
            this.Controls.Add(this.lblChan3);
            this.Controls.Add(this._lblADData_6);
            this.Controls.Add(this.lblChan6);
            this.Controls.Add(this._lblADData_2);
            this.Controls.Add(this.lblChan2);
            this.Controls.Add(this._lblADData_5);
            this.Controls.Add(this.lblChan5);
            this.Controls.Add(this._lblADData_1);
            this.Controls.Add(this.lblChan1);
            this.Controls.Add(this._lblADData_4);
            this.Controls.Add(this.lblChan4);
            this.Controls.Add(this._lblADData_0);
            this.Controls.Add(this.lblChan0);
            this.Controls.Add(this.lblDemoFunction);
            this.Controls.Add(this.cmdQuit);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Blue;
            this.Location = new System.Drawing.Point(168, 104);
            this.Name = "frmStatusDisplay";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Universal Library Analog Input Scan";
            this.Load += new System.EventHandler(this.frmStatusDisplay_Load);
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
			Application.Run(new frmStatusDisplay());
		}

        public frmStatusDisplay()
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
        public Label lblInstruction;
        
        private System.ComponentModel.IContainer components;

        public ToolTip ToolTip1;
        public TextBox txtHighChan;
        public Button cmdQuit;
        public Timer tmrCheckStatus;
        public Button cmdStartBgnd;
        public Button cmdStopConvert;
        public Label Label1;
        public Label lblShowCount;
        public Label lblCount;
        public Label lblShowIndex;
        public Label lblIndex;
        public Label lblShowStat;
        public Label lblStatus;
        public Label _lblADData_7;
        public Label lblChan7;
        public Label _lblADData_3;
        public Label lblChan3;
        public Label _lblADData_6;
        public Label lblChan6;
        public Label _lblADData_2;
        public Label lblChan2;
        public Label _lblADData_5;
        public Label lblChan5;
        public Label _lblADData_1;
        public Label lblChan1;
        public Label _lblADData_4;
        public Label lblChan4;
        public Label _lblADData_0;
        public Label lblChan0;
        public Label lblDemoFunction;

        public Label[] lblADData;

#endregion

    }
}