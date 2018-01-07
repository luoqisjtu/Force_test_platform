// ==============================================================================

//  File:                         ULAI02.CS
//
//  Library Call Demonstrated:    Mccdaq.MccBoard.AInScan() in Foreground mode
//
//  Purpose:                      Scans a range of A/D Input Channels and stores
//                                the sample data in an array.
//
//  Demonstration:                Displays the analog input on up to eight channels.
//
//  Other Library Calls:          MccDaq.MccService.ErrHandling()
//                                MccDaq.MccService.WinBufAlloc
//                                MccDaq.MccService.WinBufToArray()
//                                MccDaq.MccService.WinBufFree()
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

namespace ULAI02
{

    public class frmDataDisplay : System.Windows.Forms.Form
    {

        // Create a new MccBoard object for Board 0
        MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

        MccDaq.Range Range;
        private int ADResolution, NumAIChans;
        private int HighChan, LowChan, MaxChan;
        
        const int NumPoints = 600;    //  Number of data points to collect
	    const int FirstPoint = 0;     //  set first element in buffer to transfer to array

        private ushort[] ADData; //  dimension an array to hold the input values
        private uint[] ADData32; //  dimension an array to hold the high resolution input values

        //  define a variable to contain the handle for memory allocated 
        //  by Windows through MccDaq.MccService.WinBufAlloc()
        private IntPtr MemHandle = IntPtr.Zero;

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
                cmdStart.Enabled = false;
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
                    cmdStart.Enabled = false;
                    NumAIChans = 0;
                }
                if (NumAIChans > 8) NumAIChans = 8;
                MaxChan = LowChan + NumAIChans - 1;
                lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " collecting analog data on up to " + NumAIChans.ToString() +
                    " channels using AInScan with Range set to " + Range.ToString() + ".";
            }

        }

        private void cmdStart_Click(object eventSender, System.EventArgs eventArgs) 
        {
            int j;
            int i;
            MccDaq.ErrorInfo ULStat;
            MccDaq.ScanOptions Options;
            int Rate;
            int Count;
            bool ValidChan;

            cmdStart.Enabled = false;

            //  Collect the values by calling MccDaq.MccBoard.AInScan function
            //  Parameters:
            //    LowChan    :the first channel of the scan
            //    HighChan   :the last channel of the scan
            //    Count      :the total number of A/D samples to collect
            //    Rate       :sample rate
            //    Range      :the range for the board
            //    MemHandle  :Handle for Windows buffer to store data in
            //    Options    :data collection options

            ValidChan = int.TryParse(txtHighChan.Text, out HighChan);
            if (ValidChan)
            {
                if ((HighChan > MaxChan)) HighChan = MaxChan;
                txtHighChan.Text = HighChan.ToString();
            }
            else
            {
                HighChan = 0;
            }

            Count = NumPoints;	//  total number of data points to collect

            //  per channel sampling rate ((samples per second) per channel)
            Rate = 1000 / ((HighChan - LowChan) + 1);

            //  return data as 12-bit values (ignored for 16-bit boards)
            Options = MccDaq.ScanOptions.ConvertData;

            ULStat = DaqBoard.AInScan( LowChan, HighChan, Count, ref Rate, Range, MemHandle, Options);

            //  Transfer the data from the memory buffer set up by Windows to an array
		    if(ADResolution > 16)
		    {
			    ULStat = MccDaq.MccService.WinBufToArray32( MemHandle, ADData32, FirstPoint, Count);

			    for (i=0; i<=HighChan; ++i)
				    lblADData[i].Text = ADData32[i].ToString("D");
		    }
		    else
		    {
			    ULStat = MccDaq.MccService.WinBufToArray( MemHandle, ADData, FirstPoint, Count);
            
			    for (i=0; i<=HighChan; ++i)
                    lblADData[i].Text = ADData[i].ToString("D");
		    }
            
            for (j=HighChan + 1; j<=7; ++j)
               lblADData[j].Text = "";
           
            cmdStart.Enabled = true;
        }

        private void cmdStopConvert_Click(object eventSender, System.EventArgs eventArgs)
        {
            
            MccDaq.ErrorInfo ULStat;

            //  Free up memory for use by other programs
            ULStat = MccDaq.MccService.WinBufFreeEx(MemHandle); 
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

            clsErrorDefs.ReportError = ErrorReporting.PrintAll;
            clsErrorDefs.HandleError = ErrorHandling.StopAll;
            MccDaq.ErrorInfo ULStat = MccDaq.MccService.ErrHandling
                (ErrorReporting.PrintAll, ErrorHandling.StopAll);

            //  This gives us access to labels via an indexed array
            lblADData = (new Label[] {this._lblADData_0, this._lblADData_1, 
                this._lblADData_2, this._lblADData_3, this._lblADData_4, 
                this._lblADData_5, this._lblADData_6, this._lblADData_7});

        }

        private void frmDataDisplay_FormClosing(object sender, FormClosingEventArgs e)
        {
            // be sure to release the memory buffer... 
            if (MemHandle != IntPtr.Zero)
                MccDaq.MccService.WinBufFreeEx(MemHandle);

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
            this.cmdStopConvert = new System.Windows.Forms.Button();
            this.cmdStart = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
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
            this.txtHighChan.Location = new System.Drawing.Point(202, 102);
            this.txtHighChan.MaxLength = 0;
            this.txtHighChan.Name = "txtHighChan";
            this.txtHighChan.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtHighChan.Size = new System.Drawing.Size(33, 20);
            this.txtHighChan.TabIndex = 20;
            this.txtHighChan.Text = "0";
            this.txtHighChan.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cmdStopConvert
            // 
            this.cmdStopConvert.BackColor = System.Drawing.SystemColors.Control;
            this.cmdStopConvert.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdStopConvert.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdStopConvert.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdStopConvert.Location = new System.Drawing.Point(280, 247);
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
            this.cmdStart.Location = new System.Drawing.Point(208, 247);
            this.cmdStart.Name = "cmdStart";
            this.cmdStart.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdStart.Size = new System.Drawing.Size(58, 26);
            this.cmdStart.TabIndex = 18;
            this.cmdStart.Text = "Start";
            this.cmdStart.UseVisualStyleBackColor = false;
            this.cmdStart.Click += new System.EventHandler(this.cmdStart_Click);
            // 
            // Label1
            // 
            this.Label1.BackColor = System.Drawing.SystemColors.Window;
            this.Label1.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Label1.Location = new System.Drawing.Point(77, 104);
            this.Label1.Name = "Label1";
            this.Label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label1.Size = new System.Drawing.Size(120, 17);
            this.Label1.TabIndex = 19;
            this.Label1.Text = "Measure Channels 0 to";
            this.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // _lblADData_7
            // 
            this._lblADData_7.BackColor = System.Drawing.SystemColors.Window;
            this._lblADData_7.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblADData_7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblADData_7.ForeColor = System.Drawing.Color.Blue;
            this._lblADData_7.Location = new System.Drawing.Point(264, 208);
            this._lblADData_7.Name = "_lblADData_7";
            this._lblADData_7.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_7.Size = new System.Drawing.Size(65, 17);
            this._lblADData_7.TabIndex = 16;
            // 
            // lblChan7
            // 
            this.lblChan7.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan7.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan7.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan7.Location = new System.Drawing.Point(192, 208);
            this.lblChan7.Name = "lblChan7";
            this.lblChan7.RightToLeft = System.Windows.Forms.RightToLeft.No;
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
            this._lblADData_3.Location = new System.Drawing.Point(96, 208);
            this._lblADData_3.Name = "_lblADData_3";
            this._lblADData_3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_3.Size = new System.Drawing.Size(65, 17);
            this._lblADData_3.TabIndex = 12;
            // 
            // lblChan3
            // 
            this.lblChan3.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan3.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan3.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan3.Location = new System.Drawing.Point(24, 208);
            this.lblChan3.Name = "lblChan3";
            this.lblChan3.RightToLeft = System.Windows.Forms.RightToLeft.No;
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
            this._lblADData_6.Location = new System.Drawing.Point(264, 183);
            this._lblADData_6.Name = "_lblADData_6";
            this._lblADData_6.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_6.Size = new System.Drawing.Size(65, 17);
            this._lblADData_6.TabIndex = 15;
            // 
            // lblChan6
            // 
            this.lblChan6.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan6.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan6.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan6.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan6.Location = new System.Drawing.Point(192, 183);
            this.lblChan6.Name = "lblChan6";
            this.lblChan6.RightToLeft = System.Windows.Forms.RightToLeft.No;
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
            this._lblADData_2.Location = new System.Drawing.Point(96, 183);
            this._lblADData_2.Name = "_lblADData_2";
            this._lblADData_2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_2.Size = new System.Drawing.Size(65, 17);
            this._lblADData_2.TabIndex = 11;
            // 
            // lblChan2
            // 
            this.lblChan2.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan2.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan2.Location = new System.Drawing.Point(24, 183);
            this.lblChan2.Name = "lblChan2";
            this.lblChan2.RightToLeft = System.Windows.Forms.RightToLeft.No;
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
            this._lblADData_5.Location = new System.Drawing.Point(264, 157);
            this._lblADData_5.Name = "_lblADData_5";
            this._lblADData_5.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_5.Size = new System.Drawing.Size(65, 17);
            this._lblADData_5.TabIndex = 14;
            // 
            // lblChan5
            // 
            this.lblChan5.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan5.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan5.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan5.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan5.Location = new System.Drawing.Point(192, 157);
            this.lblChan5.Name = "lblChan5";
            this.lblChan5.RightToLeft = System.Windows.Forms.RightToLeft.No;
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
            this._lblADData_1.Location = new System.Drawing.Point(96, 157);
            this._lblADData_1.Name = "_lblADData_1";
            this._lblADData_1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_1.Size = new System.Drawing.Size(65, 17);
            this._lblADData_1.TabIndex = 10;
            // 
            // lblChan1
            // 
            this.lblChan1.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan1.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan1.Location = new System.Drawing.Point(24, 157);
            this.lblChan1.Name = "lblChan1";
            this.lblChan1.RightToLeft = System.Windows.Forms.RightToLeft.No;
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
            this._lblADData_4.Location = new System.Drawing.Point(264, 132);
            this._lblADData_4.Name = "_lblADData_4";
            this._lblADData_4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblADData_4.Size = new System.Drawing.Size(65, 17);
            this._lblADData_4.TabIndex = 13;
            // 
            // lblChan4
            // 
            this.lblChan4.BackColor = System.Drawing.SystemColors.Window;
            this.lblChan4.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblChan4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChan4.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblChan4.Location = new System.Drawing.Point(192, 132);
            this.lblChan4.Name = "lblChan4";
            this.lblChan4.RightToLeft = System.Windows.Forms.RightToLeft.No;
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
            this._lblADData_0.Location = new System.Drawing.Point(96, 132);
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
            this.lblChan0.Location = new System.Drawing.Point(24, 132);
            this.lblChan0.Name = "lblChan0";
            this.lblChan0.RightToLeft = System.Windows.Forms.RightToLeft.No;
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
            this.lblDemoFunction.Location = new System.Drawing.Point(8, 8);
            this.lblDemoFunction.Name = "lblDemoFunction";
            this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDemoFunction.Size = new System.Drawing.Size(337, 41);
            this.lblDemoFunction.TabIndex = 0;
            this.lblDemoFunction.Text = "Demonstration of MccBoard.AInScan() with scan option set to MccDaq.ScanOptions.Fo" +
                "reground";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblInstruction
            // 
            this.lblInstruction.BackColor = System.Drawing.SystemColors.Window;
            this.lblInstruction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblInstruction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstruction.ForeColor = System.Drawing.Color.Red;
            this.lblInstruction.Location = new System.Drawing.Point(39, 49);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblInstruction.Size = new System.Drawing.Size(271, 50);
            this.lblInstruction.TabIndex = 21;
            this.lblInstruction.Text = "Board 0 must have analog inputs that support paced acquisition.";
            this.lblInstruction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // frmDataDisplay
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(349, 286);
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.txtHighChan);
            this.Controls.Add(this.cmdStopConvert);
            this.Controls.Add(this.cmdStart);
            this.Controls.Add(this.Label1);
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
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Blue;
            this.Location = new System.Drawing.Point(190, 108);
            this.Name = "frmDataDisplay";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Universal Library Analog Input Scan";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmDataDisplay_FormClosing);
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

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

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

        public Label lblInstruction;	
        public ToolTip ToolTip1;
        public TextBox txtHighChan;
        public Button cmdStopConvert;
        public Button cmdStart;
        public Label Label1;
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