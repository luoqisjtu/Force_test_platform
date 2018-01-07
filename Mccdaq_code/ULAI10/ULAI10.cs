// ==============================================================================
//
//  File:                         ULAI10.CS
//
//  Library Call Demonstrated:    Mccdaq.MccBoard.ALoadQueue()
//
//  Purpose:                      Loads an A/D board's channel/gain queue.
//
//  Demonstration:                Prepares a channel/gain queue and loads it
//                                to the board. An analog input function
//                                is then called to show how the queue
//                                values work.
//
//  Other Library Calls:          MccDaq.MccService.ErrHandling()
//
//  Special Requirements:         Board 0 must have an A/D converter and
//                                channel gain queue hardware.
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

namespace ULAI10
{
	public class frmDataDisplay : System.Windows.Forms.Form
	{

        // Create a new MccBoard object for Board 0
        MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

        MccDaq.Range Range;
        private int ADResolution, NumAIChans;
        private int LowChan, MaxChan;
        
		const int NumPoints = 120;        //  Number of data points to collect
        const int NumElements = 4;        // Number of elements in queue

		private ushort[] ADData;          //  array to hold the input values
		private uint[] ADData32;          //  dimension an array to hold the high resolution input values
		private IntPtr MemHandle;		  //  define a variable to contain the handle for
										  //  memory allocated by Windows through MccDaq.MccService.WinBufAlloc()

        private short[] ChanArray;        //  array to hold channel queue information
        private MccDaq.Range[] GainArray; //  array to hold gain queue information

        AnalogIO.clsAnalogIO AIOProps = new AnalogIO.clsAnalogIO();

        private void frmDataDisplay_Load(object sender, EventArgs e)
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
                cmdLoadQueue.Enabled = false;
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
                    cmdLoadQueue.Enabled = false;
                    NumAIChans = 0;
                }
                else
                {
                    ChanArray = new short[NumElements];
                    GainArray = new MccDaq.Range[NumElements];
                    MaxChan = LowChan + NumAIChans - 1;     // allow use of any channel for queue
                    if (NumAIChans > 4) NumAIChans = 4;     // limit to 4 channels for display
                    lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString() +
                        " collecting analog data on up to " + NumAIChans.ToString() +
                        " channels between channel 0 and channel " + MaxChan.ToString() +
                        " using AInScan and ALoadQueue.";
                    tmrConvert.Enabled = true;
                }
            }
            for (int i = 0; i <= 3; ++i)
                lblShowRange[i].Text = Range.ToString();

        }

        private void tmrConvert_Tick(object eventSender, System.EventArgs eventArgs)
        {

            short ChannelNum;
            short SampleNum;
            int i;
            int FirstPoint;
            MccDaq.ErrorInfo ULStat;
            int Rate;
            MccDaq.ScanOptions Options;
            int Count, FirstChan, LastChan;

            tmrConvert.Stop();

            //  Call an analog input function to show how the gain queue values
            //  supercede those passed to the function.

            // ' Collect the values by calling MccDaq.MccBoard.AInScan function
            //   Parameters:
            //     FirstChan     :the first channel of the scan
            //     LastChan    :the last channel of the scan
            //     Count       :the total number of A/D samples to collect
            //     Rate        :sample rate in samples per second
            //     Range       :the gain for the board
            //     MemHandle   :Handle for Windows buffer to store data in
            //     Options     :data collection options

            FirstChan = 0;          //  This is ignored when queue is enabled
            LastChan = 3;           //  This is ignored
            Count = NumPoints;      //  Number of data points to collect

            // per channel sampling rate ((samples per second) per channel)
            Rate = 1000 / ((LastChan - FirstChan) + 1);
            Options = MccDaq.ScanOptions.ConvertData;   //  Return data as 12-bit values

            ULStat = DaqBoard.AInScan(FirstChan, LastChan, Count, ref Rate, Range, MemHandle, Options);


            //  Transfer the data from the memory buffer set up by Windows to an array
            FirstPoint = 0;
            i = 0;

            if (ADResolution > 16)
                ULStat = MccDaq.MccService.WinBufToArray32(MemHandle, ADData32, FirstPoint, Count);
            else
                ULStat = MccDaq.MccService.WinBufToArray(MemHandle, ADData, FirstPoint, Count);

            for (SampleNum = 0; SampleNum <= 9; ++SampleNum)
            {
                for (ChannelNum = 0; ChannelNum <= NumAIChans - 1; ++ChannelNum)
                {
                    if (ADResolution > 16)
                        lblADData[i].Text = ADData32[i].ToString("D");
                    else
                        lblADData[i].Text = ADData[i].ToString("D");
                    i = i + 1;
                }
            }

            tmrConvert.Start();

        }

        private void cmdLoadQueue_Click(object eventSender, System.EventArgs eventArgs)
        {
            cmdLoadQueue.Enabled = false;
            cmdLoadQueue.Visible = false;
            cmdUnloadQ.Enabled = true;
            cmdUnloadQ.Visible = true;
            MccDaq.Range[] ValidRanges;
            System.Random RandomSelect = new System.Random();
            int NumRanges;
            double x;

            // Get a list of valid ranges from the AnalogIO module
            ValidRanges = AIOProps.GetRangeList();
            NumRanges = ValidRanges.GetUpperBound(0);

            // Set up the channel/gain queue for 4 channels - each 
            // channel set to random valid A/D ranges. 
            // Note: Some devices have limitations on the queue,
            // such as not mixing Bipolar/Unipolar ranges or allowing 
            // only unique contiguous channels - see hardware manual

            for (short i = 0; i < NumElements; i++)
            {
                if (chkRanges.Checked)
                {
                    x = RandomSelect.NextDouble();
                    GainArray[i] = ValidRanges[(int) (x * NumRanges)];
                }
                else
                    GainArray[i] = Range;
                if (chkChannels.Checked)
                {
                    x = RandomSelect.NextDouble();
                    ChanArray[i] = (short) (x * MaxChan);
                }
                else
                    ChanArray[i] = i;
            }

            //  Load the channel/gain values into the queue
            //   Parameters:
            //     ChanArray[] :array of channel values
            //     GainArray[] :array of gain values
            //     NumElements :the number of elements in the arrays (0=disable queue)

            MccDaq.ErrorInfo ULStat = DaqBoard.ALoadQueue(ChanArray, GainArray, NumElements);
            if (ULStat.Value == ErrorInfo.ErrorCode.BadAdChan)
            {
                lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " doesn't support random channels. Queue was not changed.";
            }
            else if (!(ULStat.Value == ErrorInfo.ErrorCode.NoErrors))
            {
                lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " error loading queue: " + ULStat.Message;
            }
            else
            {
                lblInstruction.Text = "Queue loaded on board " +
                    DaqBoard.BoardNum.ToString() + ".";
                for (short i = 0; i < NumElements; i++)
                {
                    lblShowRange[i].Text = GainArray[i].ToString();
                    lblChan[i].Text = "Channel " + ChanArray[i].ToString();
                }
            }

        }

        private void cmdUnloadQ_Click(object eventSender, System.EventArgs eventArgs)
        {
            short NoChans;
            short i;

            cmdUnloadQ.Enabled = false;
            cmdUnloadQ.Visible = false;
            cmdLoadQueue.Enabled = true;
            cmdLoadQueue.Visible = true;
            for (i = 0; i <= 3; ++i)
            {
                lblShowRange[i].Text = Range.ToString();
                lblChan[i].Text = "Channel " + i.ToString();
            }
            NoChans = 0; //  set to zero to disable queue

            MccDaq.ErrorInfo ULStat = DaqBoard.ALoadQueue(ChanArray, GainArray, NoChans);
            if (!(ULStat.Value == ErrorInfo.ErrorCode.NoErrors))
            {
                lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " error unloading queue: " + ULStat.Message;
            }
            else
            {
                lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString() 
                    + " scanning contiguous channels with with Range set to " 
                    + Range.ToString() + ".";
            }

        }

        private void cmdStopConvert_Click(object eventSender, System.EventArgs eventArgs)
        {
            //  Free up memory for use by other programs
            MccDaq.ErrorInfo ULStat = MccDaq.MccService.WinBufFreeEx(MemHandle);


            Application.Exit();
        }

        private void InitUL()
        {

            //  Initiate error handling
            //   activating error handling will trap errors like
            //   bad channel numbers and non-configured conditions.
            //   Parameters:
            //     MccDaq.ErrorReporting.DontPrint  :all warnings and errors will be handled locally
            //     MccDaq.ErrorHandling.DontStop    :if any error is encountered, the program continues

            clsErrorDefs.ReportError = MccDaq.ErrorReporting.DontPrint;
            clsErrorDefs.HandleError = MccDaq.ErrorHandling.DontStop;
            MccDaq.ErrorInfo ULStat = MccDaq.MccService.ErrHandling
                (clsErrorDefs.ReportError, clsErrorDefs.HandleError);

            //  Note: Any change to label names requires 
            //  a change to the corresponding array element
            lblADData = (new Label[]{_lblADData_0,_lblADData_1, 
                _lblADData_2, _lblADData_3, _lblADData_4, _lblADData_5, 
                _lblADData_6, _lblADData_7, _lblADData_8, _lblADData_9,
                _lblADData_10,_lblADData_11, _lblADData_12, _lblADData_13, 
                _lblADData_14, _lblADData_15, _lblADData_16, _lblADData_17, 
                _lblADData_18, _lblADData_19, _lblADData_20,_lblADData_21, 
                _lblADData_22, _lblADData_23, _lblADData_24, _lblADData_25, 
                _lblADData_26, _lblADData_27, _lblADData_28, _lblADData_29,
                _lblADData_30,_lblADData_31, _lblADData_32, _lblADData_33, 
                _lblADData_34, _lblADData_35, _lblADData_36, _lblADData_37, 
                _lblADData_38, _lblADData_39,});

            lblShowRange = (new Label[] { _lblShowRange_0, _lblShowRange_1, 
                _lblShowRange_2, _lblShowRange_3 });

            lblChan = (new Label[] {lblChan0, lblChan1, lblChan2, lblChan3});

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
            this.cmdUnloadQ = new System.Windows.Forms.Button();
            this.cmdLoadQueue = new System.Windows.Forms.Button();
            this.tmrConvert = new System.Windows.Forms.Timer(this.components);
            this._lblADData_39 = new System.Windows.Forms.Label();
            this._lblADData_38 = new System.Windows.Forms.Label();
            this._lblADData_37 = new System.Windows.Forms.Label();
            this._lblADData_36 = new System.Windows.Forms.Label();
            this._lblADData_35 = new System.Windows.Forms.Label();
            this._lblADData_34 = new System.Windows.Forms.Label();
            this._lblADData_33 = new System.Windows.Forms.Label();
            this._lblADData_32 = new System.Windows.Forms.Label();
            this._lblADData_31 = new System.Windows.Forms.Label();
            this._lblADData_30 = new System.Windows.Forms.Label();
            this._lblADData_29 = new System.Windows.Forms.Label();
            this._lblADData_28 = new System.Windows.Forms.Label();
            this._lblADData_27 = new System.Windows.Forms.Label();
            this._lblADData_26 = new System.Windows.Forms.Label();
            this._lblADData_25 = new System.Windows.Forms.Label();
            this._lblADData_24 = new System.Windows.Forms.Label();
            this._lblADData_23 = new System.Windows.Forms.Label();
            this._lblADData_22 = new System.Windows.Forms.Label();
            this._lblADData_21 = new System.Windows.Forms.Label();
            this._lblADData_20 = new System.Windows.Forms.Label();
            this._lblADData_11 = new System.Windows.Forms.Label();
            this._lblADData_10 = new System.Windows.Forms.Label();
            this._lblADData_9 = new System.Windows.Forms.Label();
            this._lblADData_8 = new System.Windows.Forms.Label();
            this._lblADData_19 = new System.Windows.Forms.Label();
            this._lblADData_18 = new System.Windows.Forms.Label();
            this._lblADData_17 = new System.Windows.Forms.Label();
            this._lblADData_16 = new System.Windows.Forms.Label();
            this._lblADData_15 = new System.Windows.Forms.Label();
            this._lblADData_14 = new System.Windows.Forms.Label();
            this._lblADData_13 = new System.Windows.Forms.Label();
            this._lblADData_12 = new System.Windows.Forms.Label();
            this._lblADData_7 = new System.Windows.Forms.Label();
            this._lblADData_6 = new System.Windows.Forms.Label();
            this._lblADData_5 = new System.Windows.Forms.Label();
            this._lblADData_4 = new System.Windows.Forms.Label();
            this._lblADData_3 = new System.Windows.Forms.Label();
            this._lblADData_2 = new System.Windows.Forms.Label();
            this._lblADData_1 = new System.Windows.Forms.Label();
            this._lblADData_0 = new System.Windows.Forms.Label();
            this._lblShowRange_3 = new System.Windows.Forms.Label();
            this._lblShowRange_2 = new System.Windows.Forms.Label();
            this._lblShowRange_1 = new System.Windows.Forms.Label();
            this._lblShowRange_0 = new System.Windows.Forms.Label();
            this.lblRange = new System.Windows.Forms.Label();
            this.lblChan3 = new System.Windows.Forms.Label();
            this.lblChan2 = new System.Windows.Forms.Label();
            this.lblChan1 = new System.Windows.Forms.Label();
            this.lblChan0 = new System.Windows.Forms.Label();
            this.lblDemoFunction = new System.Windows.Forms.Label();
            this.lblInstruction = new System.Windows.Forms.Label();
            this.chkRanges = new System.Windows.Forms.CheckBox();
            this.chkChannels = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cmdStopConvert
            // 
            this.cmdStopConvert.BackColor = System.Drawing.SystemColors.Control;
            this.cmdStopConvert.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdStopConvert.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdStopConvert.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdStopConvert.Location = new System.Drawing.Point(379, 347);
            this.cmdStopConvert.Name = "cmdStopConvert";
            this.cmdStopConvert.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdStopConvert.Size = new System.Drawing.Size(57, 33);
            this.cmdStopConvert.TabIndex = 13;
            this.cmdStopConvert.Text = "Quit";
            this.cmdStopConvert.UseVisualStyleBackColor = false;
            this.cmdStopConvert.Click += new System.EventHandler(this.cmdStopConvert_Click);
            // 
            // cmdUnloadQ
            // 
            this.cmdUnloadQ.BackColor = System.Drawing.SystemColors.Control;
            this.cmdUnloadQ.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdUnloadQ.Enabled = false;
            this.cmdUnloadQ.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdUnloadQ.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdUnloadQ.Location = new System.Drawing.Point(243, 347);
            this.cmdUnloadQ.Name = "cmdUnloadQ";
            this.cmdUnloadQ.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdUnloadQ.Size = new System.Drawing.Size(97, 33);
            this.cmdUnloadQ.TabIndex = 47;
            this.cmdUnloadQ.Text = "Unload Queue";
            this.cmdUnloadQ.UseVisualStyleBackColor = false;
            this.cmdUnloadQ.Visible = false;
            this.cmdUnloadQ.Click += new System.EventHandler(this.cmdUnloadQ_Click);
            // 
            // cmdLoadQueue
            // 
            this.cmdLoadQueue.BackColor = System.Drawing.SystemColors.Control;
            this.cmdLoadQueue.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdLoadQueue.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdLoadQueue.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdLoadQueue.Location = new System.Drawing.Point(243, 347);
            this.cmdLoadQueue.Name = "cmdLoadQueue";
            this.cmdLoadQueue.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdLoadQueue.Size = new System.Drawing.Size(97, 33);
            this.cmdLoadQueue.TabIndex = 14;
            this.cmdLoadQueue.Text = "Load Queue";
            this.cmdLoadQueue.UseVisualStyleBackColor = false;
            this.cmdLoadQueue.Click += new System.EventHandler(this.cmdLoadQueue_Click);
            // 
            // tmrConvert
            // 
            this.tmrConvert.Interval = 1000;
            this.tmrConvert.Tick += new System.EventHandler(this.tmrConvert_Tick);
            // 
            // _lblADData_39
            // 
            this._lblADData_39.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_39.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_39.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_39.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_39.Location = new System.Drawing.Point(360, 305);
            this._lblADData_39.Name = "_lblADData_39";
            this._lblADData_39.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_39.Size = new System.Drawing.Size(65, 17);
            this._lblADData_39.TabIndex = 46;
            this._lblADData_39.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_38
            // 
            this._lblADData_38.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_38.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_38.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_38.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_38.Location = new System.Drawing.Point(268, 305);
            this._lblADData_38.Name = "_lblADData_38";
            this._lblADData_38.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_38.Size = new System.Drawing.Size(65, 17);
            this._lblADData_38.TabIndex = 45;
            this._lblADData_38.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_37
            // 
            this._lblADData_37.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_37.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_37.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_37.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_37.Location = new System.Drawing.Point(168, 305);
            this._lblADData_37.Name = "_lblADData_37";
            this._lblADData_37.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_37.Size = new System.Drawing.Size(65, 17);
            this._lblADData_37.TabIndex = 44;
            this._lblADData_37.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_36
            // 
            this._lblADData_36.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_36.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_36.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_36.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_36.Location = new System.Drawing.Point(69, 305);
            this._lblADData_36.Name = "_lblADData_36";
            this._lblADData_36.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_36.Size = new System.Drawing.Size(65, 17);
            this._lblADData_36.TabIndex = 43;
            this._lblADData_36.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_35
            // 
            this._lblADData_35.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_35.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_35.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_35.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_35.Location = new System.Drawing.Point(360, 289);
            this._lblADData_35.Name = "_lblADData_35";
            this._lblADData_35.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_35.Size = new System.Drawing.Size(65, 17);
            this._lblADData_35.TabIndex = 42;
            this._lblADData_35.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_34
            // 
            this._lblADData_34.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_34.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_34.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_34.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_34.Location = new System.Drawing.Point(268, 289);
            this._lblADData_34.Name = "_lblADData_34";
            this._lblADData_34.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_34.Size = new System.Drawing.Size(65, 17);
            this._lblADData_34.TabIndex = 41;
            this._lblADData_34.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_33
            // 
            this._lblADData_33.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_33.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_33.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_33.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_33.Location = new System.Drawing.Point(168, 289);
            this._lblADData_33.Name = "_lblADData_33";
            this._lblADData_33.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_33.Size = new System.Drawing.Size(65, 17);
            this._lblADData_33.TabIndex = 40;
            this._lblADData_33.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_32
            // 
            this._lblADData_32.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_32.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_32.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_32.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_32.Location = new System.Drawing.Point(69, 289);
            this._lblADData_32.Name = "_lblADData_32";
            this._lblADData_32.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_32.Size = new System.Drawing.Size(65, 17);
            this._lblADData_32.TabIndex = 39;
            this._lblADData_32.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_31
            // 
            this._lblADData_31.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_31.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_31.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_31.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_31.Location = new System.Drawing.Point(360, 273);
            this._lblADData_31.Name = "_lblADData_31";
            this._lblADData_31.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_31.Size = new System.Drawing.Size(65, 17);
            this._lblADData_31.TabIndex = 38;
            this._lblADData_31.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_30
            // 
            this._lblADData_30.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_30.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_30.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_30.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_30.Location = new System.Drawing.Point(268, 273);
            this._lblADData_30.Name = "_lblADData_30";
            this._lblADData_30.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_30.Size = new System.Drawing.Size(65, 17);
            this._lblADData_30.TabIndex = 37;
            this._lblADData_30.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_29
            // 
            this._lblADData_29.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_29.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_29.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_29.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_29.Location = new System.Drawing.Point(168, 273);
            this._lblADData_29.Name = "_lblADData_29";
            this._lblADData_29.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_29.Size = new System.Drawing.Size(65, 17);
            this._lblADData_29.TabIndex = 36;
            this._lblADData_29.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_28
            // 
            this._lblADData_28.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_28.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_28.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_28.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_28.Location = new System.Drawing.Point(69, 273);
            this._lblADData_28.Name = "_lblADData_28";
            this._lblADData_28.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_28.Size = new System.Drawing.Size(65, 17);
            this._lblADData_28.TabIndex = 35;
            this._lblADData_28.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_27
            // 
            this._lblADData_27.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_27.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_27.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_27.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_27.Location = new System.Drawing.Point(360, 257);
            this._lblADData_27.Name = "_lblADData_27";
            this._lblADData_27.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_27.Size = new System.Drawing.Size(65, 17);
            this._lblADData_27.TabIndex = 34;
            this._lblADData_27.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_26
            // 
            this._lblADData_26.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_26.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_26.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_26.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_26.Location = new System.Drawing.Point(268, 257);
            this._lblADData_26.Name = "_lblADData_26";
            this._lblADData_26.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_26.Size = new System.Drawing.Size(65, 17);
            this._lblADData_26.TabIndex = 33;
            this._lblADData_26.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_25
            // 
            this._lblADData_25.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_25.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_25.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_25.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_25.Location = new System.Drawing.Point(168, 257);
            this._lblADData_25.Name = "_lblADData_25";
            this._lblADData_25.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_25.Size = new System.Drawing.Size(65, 17);
            this._lblADData_25.TabIndex = 32;
            this._lblADData_25.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_24
            // 
            this._lblADData_24.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_24.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_24.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_24.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_24.Location = new System.Drawing.Point(69, 257);
            this._lblADData_24.Name = "_lblADData_24";
            this._lblADData_24.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_24.Size = new System.Drawing.Size(65, 17);
            this._lblADData_24.TabIndex = 31;
            this._lblADData_24.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_23
            // 
            this._lblADData_23.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_23.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_23.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_23.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_23.Location = new System.Drawing.Point(360, 241);
            this._lblADData_23.Name = "_lblADData_23";
            this._lblADData_23.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_23.Size = new System.Drawing.Size(65, 17);
            this._lblADData_23.TabIndex = 30;
            this._lblADData_23.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_22
            // 
            this._lblADData_22.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_22.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_22.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_22.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_22.Location = new System.Drawing.Point(268, 241);
            this._lblADData_22.Name = "_lblADData_22";
            this._lblADData_22.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_22.Size = new System.Drawing.Size(65, 17);
            this._lblADData_22.TabIndex = 29;
            this._lblADData_22.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_21
            // 
            this._lblADData_21.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_21.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_21.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_21.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_21.Location = new System.Drawing.Point(168, 241);
            this._lblADData_21.Name = "_lblADData_21";
            this._lblADData_21.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_21.Size = new System.Drawing.Size(65, 17);
            this._lblADData_21.TabIndex = 28;
            this._lblADData_21.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_20
            // 
            this._lblADData_20.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_20.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_20.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_20.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_20.Location = new System.Drawing.Point(69, 241);
            this._lblADData_20.Name = "_lblADData_20";
            this._lblADData_20.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_20.Size = new System.Drawing.Size(65, 17);
            this._lblADData_20.TabIndex = 27;
            this._lblADData_20.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_11
            // 
            this._lblADData_11.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_11.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_11.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_11.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_11.Location = new System.Drawing.Point(360, 225);
            this._lblADData_11.Name = "_lblADData_11";
            this._lblADData_11.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_11.Size = new System.Drawing.Size(65, 17);
            this._lblADData_11.TabIndex = 18;
            this._lblADData_11.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_10
            // 
            this._lblADData_10.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_10.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_10.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_10.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_10.Location = new System.Drawing.Point(268, 225);
            this._lblADData_10.Name = "_lblADData_10";
            this._lblADData_10.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_10.Size = new System.Drawing.Size(65, 17);
            this._lblADData_10.TabIndex = 17;
            this._lblADData_10.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_9
            // 
            this._lblADData_9.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_9.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_9.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_9.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_9.Location = new System.Drawing.Point(168, 225);
            this._lblADData_9.Name = "_lblADData_9";
            this._lblADData_9.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_9.Size = new System.Drawing.Size(65, 17);
            this._lblADData_9.TabIndex = 16;
            this._lblADData_9.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_8
            // 
            this._lblADData_8.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_8.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_8.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_8.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_8.Location = new System.Drawing.Point(69, 225);
            this._lblADData_8.Name = "_lblADData_8";
            this._lblADData_8.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_8.Size = new System.Drawing.Size(65, 17);
            this._lblADData_8.TabIndex = 15;
            this._lblADData_8.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_19
            // 
            this._lblADData_19.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_19.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_19.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_19.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_19.Location = new System.Drawing.Point(360, 209);
            this._lblADData_19.Name = "_lblADData_19";
            this._lblADData_19.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_19.Size = new System.Drawing.Size(65, 17);
            this._lblADData_19.TabIndex = 26;
            this._lblADData_19.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_18
            // 
            this._lblADData_18.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_18.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_18.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_18.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_18.Location = new System.Drawing.Point(268, 209);
            this._lblADData_18.Name = "_lblADData_18";
            this._lblADData_18.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_18.Size = new System.Drawing.Size(65, 17);
            this._lblADData_18.TabIndex = 25;
            this._lblADData_18.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_17
            // 
            this._lblADData_17.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_17.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_17.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_17.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_17.Location = new System.Drawing.Point(168, 209);
            this._lblADData_17.Name = "_lblADData_17";
            this._lblADData_17.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_17.Size = new System.Drawing.Size(65, 17);
            this._lblADData_17.TabIndex = 24;
            this._lblADData_17.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_16
            // 
            this._lblADData_16.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_16.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_16.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_16.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_16.Location = new System.Drawing.Point(69, 209);
            this._lblADData_16.Name = "_lblADData_16";
            this._lblADData_16.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_16.Size = new System.Drawing.Size(65, 17);
            this._lblADData_16.TabIndex = 23;
            this._lblADData_16.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_15
            // 
            this._lblADData_15.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_15.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_15.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_15.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_15.Location = new System.Drawing.Point(360, 193);
            this._lblADData_15.Name = "_lblADData_15";
            this._lblADData_15.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_15.Size = new System.Drawing.Size(65, 17);
            this._lblADData_15.TabIndex = 22;
            this._lblADData_15.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_14
            // 
            this._lblADData_14.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_14.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_14.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_14.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_14.Location = new System.Drawing.Point(268, 193);
            this._lblADData_14.Name = "_lblADData_14";
            this._lblADData_14.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_14.Size = new System.Drawing.Size(65, 17);
            this._lblADData_14.TabIndex = 21;
            this._lblADData_14.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_13
            // 
            this._lblADData_13.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_13.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_13.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_13.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_13.Location = new System.Drawing.Point(168, 193);
            this._lblADData_13.Name = "_lblADData_13";
            this._lblADData_13.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_13.Size = new System.Drawing.Size(65, 17);
            this._lblADData_13.TabIndex = 20;
            this._lblADData_13.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_12
            // 
            this._lblADData_12.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_12.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_12.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_12.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_12.Location = new System.Drawing.Point(69, 193);
            this._lblADData_12.Name = "_lblADData_12";
            this._lblADData_12.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_12.Size = new System.Drawing.Size(65, 17);
            this._lblADData_12.TabIndex = 19;
            this._lblADData_12.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_7
            // 
            this._lblADData_7.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_7.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_7.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_7.Location = new System.Drawing.Point(360, 177);
            this._lblADData_7.Name = "_lblADData_7";
            this._lblADData_7.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_7.Size = new System.Drawing.Size(65, 17);
            this._lblADData_7.TabIndex = 12;
            this._lblADData_7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_6
            // 
            this._lblADData_6.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_6.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_6.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_6.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_6.Location = new System.Drawing.Point(268, 177);
            this._lblADData_6.Name = "_lblADData_6";
            this._lblADData_6.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_6.Size = new System.Drawing.Size(65, 17);
            this._lblADData_6.TabIndex = 11;
            this._lblADData_6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_5
            // 
            this._lblADData_5.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_5.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_5.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_5.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_5.Location = new System.Drawing.Point(168, 177);
            this._lblADData_5.Name = "_lblADData_5";
            this._lblADData_5.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_5.Size = new System.Drawing.Size(65, 17);
            this._lblADData_5.TabIndex = 10;
            this._lblADData_5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_4
            // 
            this._lblADData_4.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_4.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_4.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_4.Location = new System.Drawing.Point(69, 177);
            this._lblADData_4.Name = "_lblADData_4";
            this._lblADData_4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_4.Size = new System.Drawing.Size(65, 17);
            this._lblADData_4.TabIndex = 9;
            this._lblADData_4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_3
            // 
            this._lblADData_3.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_3.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_3.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_3.Location = new System.Drawing.Point(360, 161);
            this._lblADData_3.Name = "_lblADData_3";
            this._lblADData_3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_3.Size = new System.Drawing.Size(65, 17);
            this._lblADData_3.TabIndex = 8;
            this._lblADData_3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_2
            // 
            this._lblADData_2.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_2.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_2.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_2.Location = new System.Drawing.Point(268, 161);
            this._lblADData_2.Name = "_lblADData_2";
            this._lblADData_2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_2.Size = new System.Drawing.Size(65, 17);
            this._lblADData_2.TabIndex = 7;
            this._lblADData_2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_1
            // 
            this._lblADData_1.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_1.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_1.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_1.Location = new System.Drawing.Point(168, 161);
            this._lblADData_1.Name = "_lblADData_1";
            this._lblADData_1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_1.Size = new System.Drawing.Size(65, 17);
            this._lblADData_1.TabIndex = 6;
            this._lblADData_1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblADData_0
            // 
            this._lblADData_0.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_0.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_0.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_0.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_0.Location = new System.Drawing.Point(69, 161);
            this._lblADData_0.Name = "_lblADData_0";
            this._lblADData_0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_0.Size = new System.Drawing.Size(65, 17);
            this._lblADData_0.TabIndex = 5;
            this._lblADData_0.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowRange_3
            // 
            this._lblShowRange_3.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowRange_3.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowRange_3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowRange_3.ForeColor = System.Drawing.Color.Blue;
            this._lblShowRange_3.Location = new System.Drawing.Point(352, 129);
            this._lblShowRange_3.Name = "_lblShowRange_3";
            this._lblShowRange_3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowRange_3.Size = new System.Drawing.Size(80, 17);
            this._lblShowRange_3.TabIndex = 52;
            this._lblShowRange_3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowRange_2
            // 
            this._lblShowRange_2.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowRange_2.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowRange_2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowRange_2.ForeColor = System.Drawing.Color.Blue;
            this._lblShowRange_2.Location = new System.Drawing.Point(260, 129);
            this._lblShowRange_2.Name = "_lblShowRange_2";
            this._lblShowRange_2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowRange_2.Size = new System.Drawing.Size(80, 17);
            this._lblShowRange_2.TabIndex = 51;
            this._lblShowRange_2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowRange_1
            // 
            this._lblShowRange_1.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowRange_1.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowRange_1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowRange_1.ForeColor = System.Drawing.Color.Blue;
            this._lblShowRange_1.Location = new System.Drawing.Point(160, 129);
            this._lblShowRange_1.Name = "_lblShowRange_1";
            this._lblShowRange_1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowRange_1.Size = new System.Drawing.Size(80, 17);
            this._lblShowRange_1.TabIndex = 50;
            this._lblShowRange_1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowRange_0
            // 
            this._lblShowRange_0.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowRange_0.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowRange_0.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowRange_0.ForeColor = System.Drawing.Color.Blue;
            this._lblShowRange_0.Location = new System.Drawing.Point(61, 129);
            this._lblShowRange_0.Name = "_lblShowRange_0";
            this._lblShowRange_0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowRange_0.Size = new System.Drawing.Size(80, 17);
            this._lblShowRange_0.TabIndex = 49;
            this._lblShowRange_0.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblRange
            // 
            this.lblRange.BackColor = System.Drawing.SystemColors.Window;
            this.lblRange.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblRange.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRange.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblRange.Location = new System.Drawing.Point(10, 129);
            this.lblRange.Name = "lblRange";
            this.lblRange.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblRange.Size = new System.Drawing.Size(49, 17);
            this.lblRange.TabIndex = 48;
            this.lblRange.Text = "Range:";
            // 
            // lblChan3
            // 
            this.lblChan3.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan3.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan3.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan3.Location = new System.Drawing.Point(360, 105);
            this.lblChan3.Name = "lblChan3";
            this.lblChan3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblChan3.Size = new System.Drawing.Size(65, 17);
            this.lblChan3.TabIndex = 4;
            this.lblChan3.Text = "Channel 3";
            this.lblChan3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblChan2
            // 
            this.lblChan2.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan2.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan2.Location = new System.Drawing.Point(268, 105);
            this.lblChan2.Name = "lblChan2";
            this.lblChan2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblChan2.Size = new System.Drawing.Size(65, 17);
            this.lblChan2.TabIndex = 3;
            this.lblChan2.Text = "Channel 2";
            this.lblChan2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblChan1
            // 
            this.lblChan1.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan1.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan1.Location = new System.Drawing.Point(168, 105);
            this.lblChan1.Name = "lblChan1";
            this.lblChan1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblChan1.Size = new System.Drawing.Size(65, 17);
            this.lblChan1.TabIndex = 2;
            this.lblChan1.Text = "Channel 1";
            this.lblChan1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblChan0
            // 
            this.lblChan0.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan0.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan0.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan0.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan0.Location = new System.Drawing.Point(69, 105);
            this.lblChan0.Name = "lblChan0";
            this.lblChan0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblChan0.Size = new System.Drawing.Size(65, 17);
            this.lblChan0.TabIndex = 1;
            this.lblChan0.Text = "Channel 0";
            this.lblChan0.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblDemoFunction
            // 
            this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
            this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDemoFunction.Location = new System.Drawing.Point(9, 4);
            this.lblDemoFunction.Name = "lblDemoFunction";
            this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDemoFunction.Size = new System.Drawing.Size(464, 19);
            this.lblDemoFunction.TabIndex = 0;
            this.lblDemoFunction.Text = "Demonstration of MccDaq.MccBoard..ALoadQueue()";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblInstruction
            // 
            this.lblInstruction.BackColor = System.Drawing.SystemColors.Window;
            this.lblInstruction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblInstruction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstruction.ForeColor = System.Drawing.Color.Red;
            this.lblInstruction.Location = new System.Drawing.Point(36, 28);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblInstruction.Size = new System.Drawing.Size(413, 60);
            this.lblInstruction.TabIndex = 54;
            this.lblInstruction.Text = "Board 0 must have analog inputs that support paced acquisition.";
            this.lblInstruction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // chkRanges
            // 
            this.chkRanges.AutoSize = true;
            this.chkRanges.Checked = true;
            this.chkRanges.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRanges.Location = new System.Drawing.Point(34, 345);
            this.chkRanges.Name = "chkRanges";
            this.chkRanges.Size = new System.Drawing.Size(115, 18);
            this.chkRanges.TabIndex = 57;
            this.chkRanges.Text = "Random Ranges";
            this.chkRanges.UseVisualStyleBackColor = true;
            // 
            // chkChannels
            // 
            this.chkChannels.AutoSize = true;
            this.chkChannels.Location = new System.Drawing.Point(34, 366);
            this.chkChannels.Name = "chkChannels";
            this.chkChannels.Size = new System.Drawing.Size(126, 18);
            this.chkChannels.TabIndex = 56;
            this.chkChannels.Text = "Random Channels";
            this.chkChannels.UseVisualStyleBackColor = true;
            // 
            // frmDataDisplay
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(484, 396);
            this.Controls.Add(this.chkRanges);
            this.Controls.Add(this.chkChannels);
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.cmdStopConvert);
            this.Controls.Add(this.cmdLoadQueue);
            this.Controls.Add(this._lblADData_39);
            this.Controls.Add(this._lblADData_38);
            this.Controls.Add(this._lblADData_37);
            this.Controls.Add(this._lblADData_36);
            this.Controls.Add(this._lblADData_35);
            this.Controls.Add(this._lblADData_34);
            this.Controls.Add(this._lblADData_33);
            this.Controls.Add(this._lblADData_32);
            this.Controls.Add(this._lblADData_31);
            this.Controls.Add(this._lblADData_30);
            this.Controls.Add(this._lblADData_29);
            this.Controls.Add(this._lblADData_28);
            this.Controls.Add(this._lblADData_27);
            this.Controls.Add(this._lblADData_26);
            this.Controls.Add(this._lblADData_25);
            this.Controls.Add(this._lblADData_24);
            this.Controls.Add(this._lblADData_23);
            this.Controls.Add(this._lblADData_22);
            this.Controls.Add(this._lblADData_21);
            this.Controls.Add(this._lblADData_20);
            this.Controls.Add(this._lblADData_11);
            this.Controls.Add(this._lblADData_10);
            this.Controls.Add(this._lblADData_9);
            this.Controls.Add(this._lblADData_8);
            this.Controls.Add(this._lblADData_19);
            this.Controls.Add(this._lblADData_18);
            this.Controls.Add(this._lblADData_17);
            this.Controls.Add(this._lblADData_16);
            this.Controls.Add(this._lblADData_15);
            this.Controls.Add(this._lblADData_14);
            this.Controls.Add(this._lblADData_13);
            this.Controls.Add(this._lblADData_12);
            this.Controls.Add(this._lblADData_7);
            this.Controls.Add(this._lblADData_6);
            this.Controls.Add(this._lblADData_5);
            this.Controls.Add(this._lblADData_4);
            this.Controls.Add(this._lblADData_3);
            this.Controls.Add(this._lblADData_2);
            this.Controls.Add(this._lblADData_1);
            this.Controls.Add(this._lblADData_0);
            this.Controls.Add(this._lblShowRange_3);
            this.Controls.Add(this._lblShowRange_2);
            this.Controls.Add(this._lblShowRange_1);
            this.Controls.Add(this._lblShowRange_0);
            this.Controls.Add(this.lblRange);
            this.Controls.Add(this.lblChan3);
            this.Controls.Add(this.lblChan2);
            this.Controls.Add(this.lblChan1);
            this.Controls.Add(this.lblChan0);
            this.Controls.Add(this.lblDemoFunction);
            this.Controls.Add(this.cmdUnloadQ);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Blue;
            this.Location = new System.Drawing.Point(7, 103);
            this.Name = "frmDataDisplay";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Universal Library Gain Queue";
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
        public Button cmdStopConvert;
        public Button cmdUnloadQ;
        public Button cmdLoadQueue;
        public Timer tmrConvert;
        public Label _lblADData_39;
        public Label _lblADData_38;
        public Label _lblADData_37;
        public Label _lblADData_36;
        public Label _lblADData_35;
        public Label _lblADData_34;
        public Label _lblADData_33;
        public Label _lblADData_32;
        public Label _lblADData_31;
        public Label _lblADData_30;
        public Label _lblADData_29;
        public Label _lblADData_28;
        public Label _lblADData_27;
        public Label _lblADData_26;
        public Label _lblADData_25;
        public Label _lblADData_24;
        public Label _lblADData_23;
        public Label _lblADData_22;
        public Label _lblADData_21;
        public Label _lblADData_20;
        public Label _lblADData_11;
        public Label _lblADData_10;
        public Label _lblADData_9;
        public Label _lblADData_8;
        public Label _lblADData_19;
        public Label _lblADData_18;
        public Label _lblADData_17;
        public Label _lblADData_16;
        public Label _lblADData_15;
        public Label _lblADData_14;
        public Label _lblADData_13;
        public Label _lblADData_12;
        public Label _lblADData_7;
        public Label _lblADData_6;
        public Label _lblADData_5;
        public Label _lblADData_4;
        public Label _lblADData_3;
        public Label _lblADData_2;
        public Label _lblADData_1;
        public Label _lblADData_0;
        public Label _lblShowRange_3;
        public Label _lblShowRange_2;
        public Label _lblShowRange_1;
        public Label _lblShowRange_0;
        public Label lblRange;
        public Label lblChan3;
        public Label lblChan2;
        public Label lblChan1;
        public Label lblChan0;
        public Label lblDemoFunction;

        public Label[] lblADData;
        public Label[] lblShowRange;
        public Label[] lblChan;
        public Label lblInstruction;
        internal CheckBox chkRanges;
        internal CheckBox chkChannels;

        #endregion

    }
}