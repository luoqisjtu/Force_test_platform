// ==============================================================================

//  File:                         CInScan03.CS
//
//  Library Call Demonstrated:    MccDaq.CConfigScan() and Mccdaq.MccBoard.CInScan()
//
//  Purpose:                      Scans a Counter Input in encoder mode and stores
//                                the sample data in an array.
//
//  Demonstration:                Displays counts from encoder as phase A, phase B,
//                                and totalizes the index on Z.
//
//  Other Library Calls:          MccDaq.MccService.ErrHandling()
//                                MccDaq.MccService.WinBufAlloc32
//                                MccDaq.MccService.WinBufToArray32()
//                                MccDaq.MccService.WinBufFree()
//
//  Special Requirements:         Board 0 must support counter scans in encoder mode.
//                                Phase A from encode connected to counter 0 input.
//                                Phase B from encode connected to counter 1 input.
//                                Index Z from encode connected to counter 2 input.
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

namespace CounterInScan
{
	public class frmDataDisplay : System.Windows.Forms.Form
	{

        int NumCtrs, CounterNum;

        // Create a new MccBoard object for Board 0
        MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);
        
        const int NumPoints = 50;    //  Number of data points to collect
		const int FirstPoint = 0;     //  set first element in buffer to transfer to array

		private int LastCtr;
		private uint[] CounterData;
      
        //  dimension an array to hold the input values
        //  define a variable to contain the handle for memory allocated 
        //  by Windows through MccDaq.MccService.WinBufAlloc()
        private IntPtr MemHandle = IntPtr.Zero;	

        Counters.clsCounters CtrProps = new Counters.clsCounters();
        
        private void frmDataDisplay_Load(object sender, EventArgs e)
        {
            InitUL();

            // determine the number and capabilities of counters on device
            int CounterType = Counters.clsCounters.CTRQUAD;
            NumCtrs = CtrProps.FindCountersOfType(DaqBoard, CounterType, out CounterNum);
            if (NumCtrs == 0)
            {
                CounterType = Counters.clsCounters.CTRSCAN;
                NumCtrs = CtrProps.FindCountersOfType(DaqBoard, CounterType, out CounterNum);
                if (NumCtrs > 0)
                    this.lblDemo.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " contains scan counters. Make sure they are compatible with quadrature operations.";
            }
            if (NumCtrs == 0)
            {
                this.lblDemo.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " has no quad counters.";
                lblDemo.ForeColor = Color.Red;
                cmdStart.Enabled = false;
            }
            else
            {
                CounterData = new uint[NumPoints];
                // Allocate memory buffer to hold data..
                MemHandle = MccDaq.MccService.WinBufAlloc32Ex(NumPoints); //  set aside memory to hold data
                if (CounterType == Counters.clsCounters.CTRQUAD)
                    lblDemo.Text = "Demonstration of MccDaq.CConfigScan() and " + 
                    "Mccdaq.MccBoard.CInScan() used with encoders" +
                    " using board " + DaqBoard.BoardNum.ToString() + ".";
            }
        }

        private void cmdStart_Click(object eventSender, System.EventArgs eventArgs)
        {
            MccDaq.ErrorInfo ULStat;
            int Rate;
            int Count;
            int FirstCtr;
            MccDaq.ScanOptions Options;

            MccDaq.CounterMode Mode;
            MccDaq.CounterDebounceTime DebounceTime;
            MccDaq.CounterDebounceMode DebounceMode;
            MccDaq.CounterEdgeDetection EdgeDetection;
            MccDaq.CounterTickSize TickSize;
            int MappedChannel;

            cmdStart.Enabled = false;

            FirstCtr = 0; //  first channel to acquire
            LastCtr = 0;

            // Setup Counter 0 
            // Parameters:
            //   BoardNum       :the number used by CB.CFG to describe this board
            //   CounterNum     :counter to set up
            //   Mode           :counter Mode
            //   DebounceTime   :debounce Time
            //   DebounceMode   :debounce Mode
            //   EdgeDetection  :determines whether the rising edge or falling edge is to be detected
            //   TickSize       :reserved.
            //   MappedChannel     :mapped channel

            // Counter 0
            Mode = MccDaq.CounterMode.Encoder | MccDaq.CounterMode.EncoderModeX1 | MccDaq.CounterMode.ClearOnZOn;
            DebounceTime = MccDaq.CounterDebounceTime.DebounceNone;
            DebounceMode = 0;
            EdgeDetection = MccDaq.CounterEdgeDetection.RisingEdge;
            TickSize = 0;
            MappedChannel = 2;

            ULStat = DaqBoard.CConfigScan(CounterNum, Mode, DebounceTime, DebounceMode, 
                EdgeDetection, TickSize, MappedChannel);

            //  Collect the values by calling MccDaq.MccBoard.CInScan function
            //  Parameters:
            //    FirstCtr   :the first counter of the scan
            //    LastCtr    :the last counter of the scan
            //    Count      :the total number of counter samples to collect
            //    Rate       :sample rate
            //    MemHandle  :Handle for Windows buffer to store data in
            //    Options    :data collection options

            Count = NumPoints;	//  total number of data points to collect
            Rate = 10;			//  per channel sampling rate ((samples per second) per channel)
            Options = MccDaq.ScanOptions.Ctr32Bit;

            ULStat = DaqBoard.CInScan(FirstCtr, LastCtr, Count, ref Rate, MemHandle, Options);

            //  Transfer the data from the memory buffer set up by Windows to an array
            ULStat = MccDaq.MccService.WinBufToArray32(MemHandle, CounterData, FirstPoint, Count);

            string NL = Environment.NewLine;
            txtEncoderValues.Text = " Counter Data" + NL + NL;

            for (int sample = 0; sample < Count; sample++)
            {
                txtEncoderValues.Text += CounterData[sample].ToString("d").PadLeft(6) + NL;
            }

            cmdStart.Enabled = true;
        }

        private void cmdStopConvert_Click(object eventSender, System.EventArgs eventArgs)
        {
            MccDaq.ErrorInfo ULStat;

            ULStat = MccDaq.MccService.WinBufFreeEx(MemHandle); //  Free up memory for use by
            //  other programs
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
            MccDaq.ErrorInfo ULStat = MccDaq.MccService.ErrHandling(MccDaq.ErrorReporting.PrintAll, MccDaq.ErrorHandling.StopAll);
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
            this.cmdStart = new System.Windows.Forms.Button();
            this.txtEncoderValues = new System.Windows.Forms.TextBox();
            this.lblDemo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdStopConvert
            // 
            this.cmdStopConvert.BackColor = System.Drawing.SystemColors.Control;
            this.cmdStopConvert.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdStopConvert.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdStopConvert.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdStopConvert.Location = new System.Drawing.Point(296, 379);
            this.cmdStopConvert.Name = "cmdStopConvert";
            this.cmdStopConvert.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdStopConvert.Size = new System.Drawing.Size(58, 26);
            this.cmdStopConvert.TabIndex = 17;
            this.cmdStopConvert.Text = "Quit";
            this.cmdStopConvert.UseVisualStyleBackColor = false;
            this.cmdStopConvert.Click += new System.EventHandler(this.cmdStopConvert_Click);
            // 
            // cmdStart
            // 
            this.cmdStart.BackColor = System.Drawing.SystemColors.Control;
            this.cmdStart.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdStart.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdStart.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdStart.Location = new System.Drawing.Point(224, 379);
            this.cmdStart.Name = "cmdStart";
            this.cmdStart.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdStart.Size = new System.Drawing.Size(58, 26);
            this.cmdStart.TabIndex = 18;
            this.cmdStart.Text = "Start";
            this.cmdStart.UseVisualStyleBackColor = false;
            this.cmdStart.Click += new System.EventHandler(this.cmdStart_Click);
            // 
            // txtEncoderValues
            // 
            this.txtEncoderValues.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEncoderValues.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.txtEncoderValues.Location = new System.Drawing.Point(12, 67);
            this.txtEncoderValues.Multiline = true;
            this.txtEncoderValues.Name = "txtEncoderValues";
            this.txtEncoderValues.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtEncoderValues.Size = new System.Drawing.Size(352, 304);
            this.txtEncoderValues.TabIndex = 46;
            // 
            // lblDemo
            // 
            this.lblDemo.BackColor = System.Drawing.SystemColors.Window;
            this.lblDemo.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDemo.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemo.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDemo.Location = new System.Drawing.Point(12, 4);
            this.lblDemo.Name = "lblDemo";
            this.lblDemo.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDemo.Size = new System.Drawing.Size(352, 60);
            this.lblDemo.TabIndex = 47;
            this.lblDemo.Text = "Demonstration of MccDaq.CConfigScan() and Mccdaq.MccBoard.CInScan() used with enc" +
                "oders";
            this.lblDemo.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // frmDataDisplay
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(376, 417);
            this.Controls.Add(this.txtEncoderValues);
            this.Controls.Add(this.cmdStopConvert);
            this.Controls.Add(this.cmdStart);
            this.Controls.Add(this.lblDemo);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Blue;
            this.Location = new System.Drawing.Point(190, 108);
            this.Name = "frmDataDisplay";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Universal Library Counter Input Scan";
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

                // be sure to release the memory buffer... 
                if (MemHandle != IntPtr.Zero)
                    MccDaq.MccService.WinBufFreeEx(MemHandle);
            }
            base.Dispose(Disposing);
        }
        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

        public ToolTip ToolTip1;
        public Button cmdStopConvert;
        public Button cmdStart;
        internal System.Windows.Forms.TextBox txtEncoderValues;
        public System.Windows.Forms.Label lblDemo;

        #endregion

    }
}