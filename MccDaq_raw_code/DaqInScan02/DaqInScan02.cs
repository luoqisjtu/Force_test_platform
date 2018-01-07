//=====================================================================================

// File:                         DaqInScan02.CS

// Library Call Demonstrated:    Mccdaq.MccBoard.DaqInScan()
//                               with scan options = MccDaq.ScanOptions.Background

// Purpose:                      Synchronously scans Analog channels, Digital ports and Counters
//                               in the background.

// Demonstration:                Displays the input channels data.
//                               Calls cbGetStatus to determine the status
//                               of the background operation. Updates the
//                               display until a key is pressed.

// Other Library Calls:          MccDaq.MccService.ErrHandling()
//                               Mccdaq.MccBoard.GetStatus()
//                               Mccdaq.MccBoard.StopBackground()
//                               Mccdaq.MccBoard.CConfigScan()
//                               Mccdaq.MccBoard.DConfigPort()


// Special Requirements:         Board 0 must support cbDaqInScan.
//

//========================================================================================

using System.Diagnostics;
using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using MccDaq;
using ErrorDefs;
using AnalogIO;
using DigitalIO;
using Counters;

namespace DaqInScan02
{
	public class frmStatusDisplay : System.Windows.Forms.Form
	{
        
        // Create a new MccBoard object for Board 0
        MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

        MccDaq.Range Range;
        private int ADResolution, NumAIChans;
        private int LowChan;
        
		const int ChanCount = 4; // Number of channels in scan
		const int NumPoints = 1000; // Number of data points to collect
		const int NumElements = ChanCount * NumPoints;

        AnalogIO.clsAnalogIO AIOProps = new AnalogIO.clsAnalogIO();
        Counters.clsCounters CtrProps = new Counters.clsCounters();
        DigitalIO.clsDigitalIO DioProps = new DigitalIO.clsDigitalIO();
		
		private ushort[] ADData;        // dimension an array to hold the input values
        private short[] ChanArray;      // array to hold channel queue information
        private MccDaq.ChannelType[] ChanTypeArray; // array to hold channel type information
        private MccDaq.Range[] GainArray; // array to hold gain queue information

        // define a variable to contain the handle for memory allocated
		// by Windows through MccDaq.MccService.WinBufAlloc()
        private IntPtr MemHandle;
        public Label lblInstruction; 
		private short UserTerm;

		private void frmStatusDisplay_Load(System.Object eventSender, System.EventArgs eventArgs)
		{
            int NumCntrs, NumPorts, CounterNum;
            int FirstBit;
            int ProgAbility = 0;
            int NumBits = 0;
            MccDaq.DigitalPortType PortNum = DigitalPortType.AuxPort;
            MccDaq.DigitalPortDirection Direction;
            MccDaq.ErrorInfo ULStat;
            MccDaq.TriggerType DefaultTrig;

            InitUL();

            NumCntrs = 0;
            NumPorts = 0;
            // determine the number of analog, digital, and counter channels and their capabilities
            int ChannelType = clsAnalogIO.ANALOGINPUT;
            NumAIChans = AIOProps.FindAnalogChansOfType(DaqBoard, ChannelType,
                out ADResolution, out Range, out LowChan, out DefaultTrig);
            ChannelType = DigitalIO.clsDigitalIO.PORTIN;
            if (!clsErrorDefs.GeneralError)
                NumPorts = DioProps.FindPortsOfType(DaqBoard, ChannelType, out ProgAbility, 
                out PortNum, out NumBits, out FirstBit);
            ChannelType = clsCounters.CTRSCAN;
            if (!clsErrorDefs.GeneralError)
                NumCntrs = CtrProps.FindCountersOfType(DaqBoard, ChannelType, out CounterNum);

            if (NumCntrs == 0)
            {
                lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " has no counter devices.";
                cmdStartBgnd.Enabled = false;
            }
            else if (NumAIChans == 0)
            {
                lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " does not have analog input channels.";
                cmdStartBgnd.Enabled = false;
            }
            else if (NumPorts == 0)
            {
                lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " has no digital devices.";
                cmdStartBgnd.Enabled = false;
            }
            else
            {
                cmdStartBgnd.Enabled = true;
                ADData = new ushort[NumElements];
                ChanArray = new short[ChanCount];
                ChanTypeArray = new MccDaq.ChannelType[ChanCount];
                GainArray = new MccDaq.Range[ChanCount];
                MemHandle = MccDaq.MccService.WinBufAllocEx(NumElements);
                if (MemHandle == IntPtr.Zero)
                {
                    this.cmdStartBgnd.Enabled = false;
                    NumAIChans = 0;
                }
                //load the arrays with values
                ChanArray[0] = 0;
                ChanTypeArray[0] = MccDaq.ChannelType.Analog;
                GainArray[0] = Range;

                ChanArray[1] = System.Convert.ToInt16(PortNum);
                ChanTypeArray[1] = MccDaq.ChannelType.Digital8;
                if (NumBits == 16)
                    ChanTypeArray[1] = MccDaq.ChannelType.Digital16;
                GainArray[1] = MccDaq.Range.NotUsed;

                ChanArray[2] = 0;
                ChanTypeArray[2] = MccDaq.ChannelType.Ctr32Low;
                GainArray[2] = MccDaq.Range.NotUsed;

                ChanArray[3] = 0;
                ChanTypeArray[3] = MccDaq.ChannelType.Ctr32High;
                GainArray[3] = MccDaq.Range.NotUsed;

                if (ProgAbility == -1)
                {
                    //configure programmable port for digital input
                    Direction = MccDaq.DigitalPortDirection.DigitalIn;
                    ULStat = DaqBoard.DConfigPort(PortNum, Direction);
                }

                ULStat = DaqBoard.CConfigScan(0, MccDaq.CounterMode.Bit16,
                    MccDaq.CounterDebounceTime.DebounceNone, 
                    MccDaq.CounterDebounceMode.TriggerAfterStable,
                    MccDaq.CounterEdgeDetection.FallingEdge, 
                    MccDaq.CounterTickSize.Tick20833pt3ns, 0);
                lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " collecting analog, digital, and counter data " + 
                    " using DaqInScan with Range set to " + Range.ToString() + ".";
            }

		}

		private void cmdStartBgnd_Click(System.Object eventSender, System.EventArgs eventArgs)
		{
			int CurIndex;
			int CurCount;
			short Status;
			MccDaq.ErrorInfo ULStat;
			int Rate;
			MccDaq.ScanOptions Options;
			int Count;
			int PretrigCount;
			
			cmdStartBgnd.Enabled = false;
			cmdStartBgnd.Visible = false;
			cmdStopConvert.Enabled = true;
			cmdStopConvert.Visible = true;
			cmdQuit.Enabled = false;
			UserTerm = 0; // initialize user terminate flag

			// Collect the values with cbDaqInScan()
			//  Parameters:
			//    BoardNum        :the number used by CB.CFG to describe this board
			//    ChanArray[]     :array of channel values
			//    ChanTypeArray[] : array of channel types
			//    GainArray[]     :array of gain values
			//    ChansCount        :the number of elements in the arrays (0=disable queue)
			//    PretrigCount    :number of pre-trigger A/D samples to collect
			//    Count         :the total number of A/D samples to collect
			//    Rate          :sample rate in samples per second
			//    ADData[]        :the array for the collected data values
			//    Options          :data collection options
			
			PretrigCount = 0;
			Count = NumElements; // Number of data points to collect
			Options =MccDaq.ScanOptions.ConvertData |MccDaq.ScanOptions.Background 
                | MccDaq.ScanOptions.Continuous;
			Rate = 100; // Acquire data at 100 Hz		
			
			ULStat = DaqBoard.DaqInScan(ChanArray, ChanTypeArray, GainArray, ChanCount, 
                ref Rate, ref PretrigCount, ref Count, MemHandle, Options);
			
			ULStat = DaqBoard.GetStatus(out Status, out CurCount, out CurIndex, 
                MccDaq.FunctionType.DaqiFunction);
			
			if (Status == MccDaq.MccBoard.Running)
			{
				lblShowStat.Text = "Running";
				lblShowCount.Text = CurCount.ToString("D");
				lblShowIndex.Text = CurIndex.ToString("D");
			}
			
			tmrCheckStatus.Enabled = true;
			
		}
		
		private void tmrCheckStatus_Tick(System.Object eventSender, System.EventArgs eventArgs)
		{
			int FirstPoint;
			MccDaq.ErrorInfo ULStat;
			int CurIndex;
			int CurCount;
			short Status;
			
			tmrCheckStatus.Stop();
			
			// This timer will check the status of the background data collection
			
			// Parameters:
			//   BoardNum   :the number used by CB.CFG to describe this board
			//   Status     :current status of the background data collection
			//   CurCount   :current number of samples collected
			//   CurIndex   :index to the data buffer pointing to the start of the
			//                most recently collected scan
			//   FunctionType: A/D operation (AIFUNCTIOM)
			
			ULStat = DaqBoard.GetStatus(out Status, out CurCount, out CurIndex, 
                MccDaq.FunctionType.DaqiFunction);
					
			lblShowCount.Text = CurCount.ToString("D");
			lblShowIndex.Text = CurIndex.ToString("D");
			
			// Check if the background operation has finished. If it has, then
			// transfer the data from the memory buffer set up by Windows to an
			// array for use by Visual Basic
			// The BACKGROUND operation must be explicitly stopped
			
			if (Status == MccDaq.MccBoard.Running && UserTerm == 0)
			{
				lblShowStat.Text = "Running";
				ULStat = DaqBoard.GetStatus(out Status, out CurCount, 
                    out CurIndex, MccDaq.FunctionType.DaqiFunction);
				
				lblShowCount.Text = CurCount.ToString("D");
				lblShowIndex.Text = CurIndex.ToString("D");
				
				FirstPoint = CurIndex;
				if (FirstPoint >= 0)
				{
					ULStat = MccDaq.MccService.WinBufToArray(MemHandle, ADData, FirstPoint, ChanCount);
					
					lblADData[0].Text = ADData[0].ToString("D");
					lblADData[1].Text = ADData[1].ToString("D");
					lblADData[2].Text = System.Convert.ToString(System.Convert.ToInt32(ADData[2]) 
                        + System.Convert.ToInt32(ADData[3]) * System.Convert.ToInt32(Math.Pow(2, 16))); 
                    // 32-bit counter
				}
				tmrCheckStatus.Start();
			}
			else if (Status == MccDaq.MccBoard.Idle || UserTerm == 1)
			{
				lblShowStat.Text = "Idle";
				tmrCheckStatus.Stop();
				
				ULStat = DaqBoard.StopBackground(MccDaq.FunctionType.DaqiFunction);
				
				cmdStartBgnd.Enabled = true;
				cmdStartBgnd.Visible = true;
				cmdStopConvert.Enabled = false;
				cmdStopConvert.Visible = false;
				cmdQuit.Enabled = true;
			}
			
		}
		
		private void cmdQuit_Click(System.Object eventSender, System.EventArgs eventArgs)
		{
			MccDaq.ErrorInfo ULStat;
			
			ULStat = MccDaq.MccService.WinBufFreeEx(MemHandle); // Free up memory for use by

			Application.Exit();
			
		}
		
		private void cmdStopConvert_Click(System.Object eventSender, System.EventArgs eventArgs)
		{
			
			UserTerm = 1;
			
		}
		
		#region "Windows Form Designer generated code "

        //NOTE: The following procedure is required by the Windows Form Designer
        //It can be modified using the Windows Form Designer.
        //Do not modify it using the code editor.
        
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cmdQuit = new System.Windows.Forms.Button();
            this.tmrCheckStatus = new System.Windows.Forms.Timer(this.components);
            this.cmdStopConvert = new System.Windows.Forms.Button();
            this.cmdStartBgnd = new System.Windows.Forms.Button();
            this.lblShowCount = new System.Windows.Forms.Label();
            this.lblCount = new System.Windows.Forms.Label();
            this.lblShowIndex = new System.Windows.Forms.Label();
            this.lblIndex = new System.Windows.Forms.Label();
            this.lblShowStat = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this._lblADData_2 = new System.Windows.Forms.Label();
            this.lblChan2 = new System.Windows.Forms.Label();
            this._lblADData_1 = new System.Windows.Forms.Label();
            this.lblChan1 = new System.Windows.Forms.Label();
            this._lblADData_0 = new System.Windows.Forms.Label();
            this.lblChan0 = new System.Windows.Forms.Label();
            this.lblDemoFunction = new System.Windows.Forms.Label();
            this.lblInstruction = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdQuit
            // 
            this.cmdQuit.BackColor = System.Drawing.SystemColors.Control;
            this.cmdQuit.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdQuit.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdQuit.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdQuit.Location = new System.Drawing.Point(286, 284);
            this.cmdQuit.Name = "cmdQuit";
            this.cmdQuit.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdQuit.Size = new System.Drawing.Size(52, 26);
            this.cmdQuit.TabIndex = 9;
            this.cmdQuit.Text = "Quit";
            this.cmdQuit.UseVisualStyleBackColor = false;
            this.cmdQuit.Click += new System.EventHandler(this.cmdQuit_Click);
            // 
            // tmrCheckStatus
            // 
            this.tmrCheckStatus.Interval = 200;
            this.tmrCheckStatus.Tick += new System.EventHandler(this.tmrCheckStatus_Tick);
            // 
            // cmdStopConvert
            // 
            this.cmdStopConvert.BackColor = System.Drawing.SystemColors.Control;
            this.cmdStopConvert.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdStopConvert.Enabled = false;
            this.cmdStopConvert.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdStopConvert.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdStopConvert.Location = new System.Drawing.Point(89, 116);
            this.cmdStopConvert.Name = "cmdStopConvert";
            this.cmdStopConvert.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdStopConvert.Size = new System.Drawing.Size(180, 27);
            this.cmdStopConvert.TabIndex = 7;
            this.cmdStopConvert.Text = "Stop Background Operation";
            this.cmdStopConvert.UseVisualStyleBackColor = false;
            this.cmdStopConvert.Visible = false;
            this.cmdStopConvert.Click += new System.EventHandler(this.cmdStopConvert_Click);
            // 
            // cmdStartBgnd
            // 
            this.cmdStartBgnd.BackColor = System.Drawing.SystemColors.Control;
            this.cmdStartBgnd.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdStartBgnd.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdStartBgnd.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdStartBgnd.Location = new System.Drawing.Point(89, 116);
            this.cmdStartBgnd.Name = "cmdStartBgnd";
            this.cmdStartBgnd.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdStartBgnd.Size = new System.Drawing.Size(180, 27);
            this.cmdStartBgnd.TabIndex = 8;
            this.cmdStartBgnd.Text = "Start Background Operation";
            this.cmdStartBgnd.UseVisualStyleBackColor = false;
            this.cmdStartBgnd.Click += new System.EventHandler(this.cmdStartBgnd_Click);
            // 
            // lblShowCount
            // 
            this.lblShowCount.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowCount.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowCount.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowCount.ForeColor = System.Drawing.Color.Blue;
            this.lblShowCount.Location = new System.Drawing.Point(205, 299);
            this.lblShowCount.Name = "lblShowCount";
            this.lblShowCount.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowCount.Size = new System.Drawing.Size(59, 14);
            this.lblShowCount.TabIndex = 15;
            // 
            // lblCount
            // 
            this.lblCount.BackColor = System.Drawing.SystemColors.Window;
            this.lblCount.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblCount.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCount.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblCount.Location = new System.Drawing.Point(90, 299);
            this.lblCount.Name = "lblCount";
            this.lblCount.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblCount.Size = new System.Drawing.Size(104, 14);
            this.lblCount.TabIndex = 13;
            this.lblCount.Text = "Current Count:";
            this.lblCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblShowIndex
            // 
            this.lblShowIndex.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowIndex.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowIndex.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowIndex.ForeColor = System.Drawing.Color.Blue;
            this.lblShowIndex.Location = new System.Drawing.Point(205, 280);
            this.lblShowIndex.Name = "lblShowIndex";
            this.lblShowIndex.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowIndex.Size = new System.Drawing.Size(52, 14);
            this.lblShowIndex.TabIndex = 14;
            // 
            // lblIndex
            // 
            this.lblIndex.BackColor = System.Drawing.SystemColors.Window;
            this.lblIndex.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblIndex.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIndex.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblIndex.Location = new System.Drawing.Point(90, 280);
            this.lblIndex.Name = "lblIndex";
            this.lblIndex.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblIndex.Size = new System.Drawing.Size(104, 14);
            this.lblIndex.TabIndex = 12;
            this.lblIndex.Text = "Current Index:";
            this.lblIndex.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblShowStat
            // 
            this.lblShowStat.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowStat.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowStat.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowStat.ForeColor = System.Drawing.Color.Blue;
            this.lblShowStat.Location = new System.Drawing.Point(230, 260);
            this.lblShowStat.Name = "lblShowStat";
            this.lblShowStat.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowStat.Size = new System.Drawing.Size(66, 14);
            this.lblShowStat.TabIndex = 11;
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.SystemColors.Window;
            this.lblStatus.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblStatus.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblStatus.Location = new System.Drawing.Point(12, 260);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblStatus.Size = new System.Drawing.Size(212, 14);
            this.lblStatus.TabIndex = 10;
            this.lblStatus.Text = "Status of Background Operation:";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // _lblADData_2
            // 
            this._lblADData_2.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_2.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_2.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_2.Location = new System.Drawing.Point(142, 223);
            this._lblADData_2.Name = "_lblADData_2";
            this._lblADData_2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_2.Size = new System.Drawing.Size(105, 17);
            this._lblADData_2.TabIndex = 6;
            // 
            // lblChan2
            // 
            this.lblChan2.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan2.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan2.Location = new System.Drawing.Point(46, 223);
            this.lblChan2.Name = "lblChan2";
            this.lblChan2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblChan2.Size = new System.Drawing.Size(81, 17);
            this.lblChan2.TabIndex = 3;
            this.lblChan2.Text = "Counter 0:";
            this.lblChan2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // _lblADData_1
            // 
            this._lblADData_1.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_1.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_1.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_1.Location = new System.Drawing.Point(142, 196);
            this._lblADData_1.Name = "_lblADData_1";
            this._lblADData_1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_1.Size = new System.Drawing.Size(105, 17);
            this._lblADData_1.TabIndex = 5;
            // 
            // lblChan1
            // 
            this.lblChan1.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan1.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan1.Location = new System.Drawing.Point(30, 196);
            this.lblChan1.Name = "lblChan1";
            this.lblChan1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblChan1.Size = new System.Drawing.Size(97, 17);
            this.lblChan1.TabIndex = 2;
            this.lblChan1.Text = "FIRSTPORTA:";
            this.lblChan1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // _lblADData_0
            // 
            this._lblADData_0.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_0.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_0.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_0.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_0.Location = new System.Drawing.Point(142, 169);
            this._lblADData_0.Name = "_lblADData_0";
            this._lblADData_0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_0.Size = new System.Drawing.Size(105, 17);
            this._lblADData_0.TabIndex = 4;
            // 
            // lblChan0
            // 
            this.lblChan0.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan0.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan0.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan0.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan0.Location = new System.Drawing.Point(46, 169);
            this.lblChan0.Name = "lblChan0";
            this.lblChan0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblChan0.Size = new System.Drawing.Size(81, 17);
            this.lblChan0.TabIndex = 1;
            this.lblChan0.Text = "Channel 0:";
            this.lblChan0.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblDemoFunction
            // 
            this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
            this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDemoFunction.Location = new System.Drawing.Point(6, 6);
            this.lblDemoFunction.Name = "lblDemoFunction";
            this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDemoFunction.Size = new System.Drawing.Size(337, 41);
            this.lblDemoFunction.TabIndex = 0;
            this.lblDemoFunction.Text = "Demonstration of MccBoard.DaqInScan() with scan option set to MccDaq.ScanOptions." +
                "Background";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblInstruction
            // 
            this.lblInstruction.BackColor = System.Drawing.SystemColors.Window;
            this.lblInstruction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblInstruction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstruction.ForeColor = System.Drawing.Color.Red;
            this.lblInstruction.Location = new System.Drawing.Point(46, 47);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblInstruction.Size = new System.Drawing.Size(271, 50);
            this.lblInstruction.TabIndex = 22;
            this.lblInstruction.Text = "Board 0 must have analog, digital, and counter inputs that support paced acquisit" +
                "ion.";
            this.lblInstruction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // frmStatusDisplay
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(350, 322);
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.cmdQuit);
            this.Controls.Add(this.cmdStopConvert);
            this.Controls.Add(this.cmdStartBgnd);
            this.Controls.Add(this.lblShowCount);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.lblShowIndex);
            this.Controls.Add(this.lblIndex);
            this.Controls.Add(this.lblShowStat);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this._lblADData_2);
            this.Controls.Add(this.lblChan2);
            this.Controls.Add(this._lblADData_1);
            this.Controls.Add(this.lblChan1);
            this.Controls.Add(this._lblADData_0);
            this.Controls.Add(this.lblChan0);
            this.Controls.Add(this.lblDemoFunction);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Blue;
            this.Location = new System.Drawing.Point(188, 108);
            this.Name = "frmStatusDisplay";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Universal Library DaqInScan()";
            this.Load += new System.EventHandler(this.frmStatusDisplay_Load);
            this.ResumeLayout(false);

        }

        #endregion

        #region Form initialization, variables, and entry point

        [STAThread]
        static void Main()
        {
            Application.Run(new frmStatusDisplay());
        }

        public frmStatusDisplay()
        {

            //This call is required by the Windows Form Designer.
            InitializeComponent();

        }

        //Form overrides dispose to clean up the component list.
        protected override void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                // make sure the scan has stopped..
                DaqBoard.StopBackground(MccDaq.FunctionType.DaqiFunction);

                // be sure to free the memory buffer
                if (MemHandle != IntPtr.Zero)
                    MccDaq.MccService.WinBufFreeEx(MemHandle);
            }
            base.Dispose(Disposing);
        }

        private System.ComponentModel.IContainer components;
		public System.Windows.Forms.ToolTip ToolTip1;
		public System.Windows.Forms.Button cmdQuit;
		public System.Windows.Forms.Timer tmrCheckStatus;
		public System.Windows.Forms.Button cmdStopConvert;
		public System.Windows.Forms.Button cmdStartBgnd;
		public System.Windows.Forms.Label lblShowCount;
		public System.Windows.Forms.Label lblCount;
		public System.Windows.Forms.Label lblShowIndex;
		public System.Windows.Forms.Label lblIndex;
		public System.Windows.Forms.Label lblShowStat;
		public System.Windows.Forms.Label lblStatus;
		public System.Windows.Forms.Label _lblADData_2;
		public System.Windows.Forms.Label lblChan2;
		public System.Windows.Forms.Label _lblADData_1;
		public System.Windows.Forms.Label lblChan1;
		public System.Windows.Forms.Label _lblADData_0;
		public System.Windows.Forms.Label lblChan0;
		public System.Windows.Forms.Label lblDemoFunction;
        public System.Windows.Forms.Label[] lblADData;

#endregion

        #region "Universal Library Initialization - Expand this region to change error handling, etc."
		
		private void InitUL()
		{

            MccDaq.ErrorInfo ULStat;
			
			// Initiate error handling
			//  activating error handling will trap errors like
			//  bad channel numbers and non-configured conditions.
			//  Parameters:
			//    MccDaq.ErrorReporting.PrintAll :all warnings and errors encountered will be printed
			//    MccDaq.ErrorHandling.StopAll   :if any error is encountered, the program will stop

            clsErrorDefs.ReportError = ErrorReporting.PrintAll;
            clsErrorDefs.HandleError = ErrorHandling.StopAll;
            ULStat = MccDaq.MccService.ErrHandling
                (ErrorReporting.PrintAll, ErrorHandling.StopAll);

            // Note: Any change to label names requires a change to the corresponding array element
            lblADData = new System.Windows.Forms.Label[] { this._lblADData_0, 
                this._lblADData_1, this._lblADData_2 };

        }

        #endregion

    }
}
