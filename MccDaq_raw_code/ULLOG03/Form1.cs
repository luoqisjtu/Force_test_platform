//ULLOG03.SLN================================================================

// File:                         ULLOG03.SLN

// Library Call Demonstrated:    logger.ReadAIChannels()
//                               logger.ReadDIOChannels
//                               logger.ReadCJCChannels

// Purpose:                      Lists data from logger files.

// Demonstration:                Displays MCC data found in the
//                               specified file.

// Other Library Calls:          cbErrHandling()

// Special Requirements:         There must be an MCC data file in
//                               the indicated parent directory.

// (c) Copyright 2005-2011, Measurement Computing Corp.
// All rights reserved.
//==========================================================================

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Microsoft.VisualBasic;

namespace ULLOG03
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
    public class frmLoggerData : System.Windows.Forms.Form
    {
        public const int MAX_PATH = 260;
        private string m_Path = "..\\..\\..\\..";
        private MccDaq.ErrorInfo ULStat;
        private MccDaq.DataLogger logger;
        private int SampleCount;

        private void frmLoggerData_Load(object sender, EventArgs e)
        {

            //  Initiate error handling
            //   activating error handling will trap errors like
            //   bad channel numbers and non-configured conditions.
            //   Parameters:
            //     MccDaq.ErrorReporting.DontPrint :all warnings and errors encountered will be handled locally
            //     MccDaq.ErrorHandling.DontStop   :if an error is encountered, the program will not stop,
            //      errors will be handled locally.

            MccDaq.ErrorInfo ULStat = MccDaq.MccService.ErrHandling
                (MccDaq.ErrorReporting.DontPrint, MccDaq.ErrorHandling.DontStop);

            //  Get the first file in the directory
            //   Parameters:
            //     MccDaq.GetFileOptions.GetFirst :first file
            //     m_Path						  :path to search
            //	   filename						  :receives name of file
            string filename = new string(' ', MAX_PATH);
            ULStat = MccDaq.DataLogger.GetFileName
                ((int)MccDaq.GetFileOptions.GetFirst, ref m_Path, ref filename);
            filename = filename.Trim();
            filename = filename.Trim('\0');

            // create an instance of the data logger
            logger = new MccDaq.DataLogger(filename);


            //  Set the preferences 
            //    Parameters
            //      timeFormat					  :specifies times are 12 or 24 hour format
            //      timeZone					  :specifies local time of GMT
            //      units						  :specifies Fahrenheit, Celsius, or Kelvin
            MccDaq.TimeFormat timeFormat = MccDaq.TimeFormat.TwelveHour;
            MccDaq.TimeZone timeZone = MccDaq.TimeZone.Local;
            MccDaq.TempScale units = MccDaq.TempScale.Fahrenheit;
            logger.SetPreferences(timeFormat, timeZone, units);


            //  Get the sample info for the first file in the directory
            //   Parameters:
            //     sampleInterval					 :receives the sample interval (time between samples)
            //     sampleCount						 :receives the sample count
            //	   startDate						 :receives the start date
            //	   startTime						 :receives the start time
            int sampleInterval = 0;
            int startDate = 0;
            int startTime = 0;
            if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors)
                ULStat = logger.GetSampleInfo(ref sampleInterval, ref SampleCount, ref startDate, ref startTime);

            //  Get the ANALOG channel count for the first file in the directory
            //   Parameters:
            //	   aiChannelCount					:receives the number of AI chennels logged
            int aiChannelCount = 0;
            if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors)
                ULStat = logger.GetAIChannelCount(ref aiChannelCount);

        }

        private void cmdAnalogData_Click(object sender, EventArgs e)
        {

            int aiChannelCount = 0;
            float[] aiChannelData;
            int[] ChannelNumbers;
            int[] Units;
            int i, j, ListSize, Index;
            string DataListStr, StartTimeStr, lbDataStr;
            string ChansStr, UnitsStr, ChanList, UnitList;
            string PostfixStr, StartDateStr;
            int[] DateTags;
            int[] TimeTags;
            int StartSample = 0;
            int Hour, Minute, Second, Postfix;
            int Month, Day, Year;

            //  Get the ANALOG info for the first file in the directory
            //   Parameters:
            //     channelMask		:receives the channel mask to specify which channels were logged
            //     unitMask			:receives the unit mask to specify temp or raw data

            ULStat = logger.GetAIChannelCount(ref aiChannelCount);
            if (ULStat.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
                txtData.Text = ULStat.Message;
            else
            {
                // Get the Analog information
                //  Parameters:
                //    Filename                :name of file to get information from
                //    ChannelNumbers          :array containing channel numbers that were logged
                //    Units                   :array containing the units for each channel that was logged
                //    AIChannelCount          :number of analog channels logged

                if ((aiChannelCount > 0) && (SampleCount > 0))
                {
                    ChannelNumbers = new int[aiChannelCount];
                    Units = new int[aiChannelCount];
                    ULStat = logger.GetAIInfo(ref ChannelNumbers, ref Units);
                    ChanList = "";
                    UnitList = "";
                    if (ULStat.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
                        txtData.Text = ULStat.Message;
                    else
                        for (i = 0; i < aiChannelCount; i++)
                        {
                            ChansStr = ChannelNumbers[i].ToString();
                            UnitsStr = "Temp";
                            if (Units[i] == (int)MccDaq.LoggerUnits.Raw) UnitsStr = "Raw";
                            ChanList = ChanList + "Chan" + ChansStr + "\t";
                            UnitList = UnitList + UnitsStr + "\t";
                        }
                    DataListStr = "Time" + "\t" + "\t" + ChanList + "\r\n" +
                        "\t" + "\t" + UnitList + "\r\n" + "\r\n";
                    DateTags = new int[SampleCount];
                    TimeTags = new int[SampleCount];
                    ULStat = logger.ReadTimeTags(StartSample, SampleCount, ref DateTags, ref TimeTags);
                    if (ULStat.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
                        txtData.Text = ULStat.Message;

                    aiChannelData = new float[SampleCount * aiChannelCount];
                    ULStat = logger.ReadAIChannels(StartSample, SampleCount, ref aiChannelData);
                    ListSize = SampleCount;
                    if (ListSize > 50) ListSize = 50;
                    PostfixStr = "";
                    for (i = 0; i <= ListSize; i++)
                    {
                        //Parse the date from the StartDate parameter
                        Month = (DateTags[i] >> 8) & 0xff;
                        Day = DateTags[i] & 0xff;
                        Year = (DateTags[i] >> 16) & 0xff;
                        StartDateStr = Month.ToString("00") + "/" +
                           Day.ToString("00") + "/" + Year.ToString("0000");

                        //Parse the time from the StartTime parameter
                        Hour = (TimeTags[i] >> 16) & 0xff;
                        Minute = (TimeTags[i] >> 8) & 0xff;
                        Second = TimeTags[i] & 0xff;
                        Postfix = (TimeTags[i] >> 24) & 0xff;
                        if (Postfix == 0) PostfixStr = " AM";
                        if (Postfix == 1) PostfixStr = " PM";
                        StartTimeStr = Hour.ToString("00") + ":" +
                           Minute.ToString("00") + ":" + Second.ToString("00")
                           + PostfixStr;
                        Index = i * aiChannelCount;
                        lbDataStr = "";
                        for (j = 0; j < aiChannelCount; j++)
                            lbDataStr = lbDataStr + "\t" + aiChannelData[Index + j].ToString("0.00");

                        DataListStr = DataListStr + StartDateStr + "  " + StartTimeStr + lbDataStr + "\r\n";
                    }
                    txtData.Text = "Analog data from " + logger.FileName + "\r\n" + "\r\n" + DataListStr;
                }
                else
                    txtData.Text = "There is no analog data in " + logger.FileName + ".";
            }
        }

        private void cmdDigitalData_Click(object sender, EventArgs e)
        {

            int Hour, Minute, Second;
            int Month, Day, Year;
            int Postfix;
            string PostfixStr, StartDateStr, DataListStr;
            string StartTimeStr, lbDataStr;
            int StartSample, i, j;
            int[] DateTags, TimeTags, dioChannelData;
            int Index;
            int dioChannelCount = 0;
            string UnitList, ChanList;
            int ListSize;

            // Get the Digital channel count
            //   Parameters:
            //	   dioChannelCount		:receives the number of DIO chennels logged

            ULStat = logger.GetDIOInfo(ref dioChannelCount);
            ChanList = "";
            UnitList = "";
            if (ULStat.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
                txtData.Text = ULStat.Message;
            else
            {
                dioChannelData = new int[SampleCount * dioChannelCount];
                if ((dioChannelCount > 0) & (SampleCount > 0))
                {
                    DataListStr = "Time" + "\t" + "\t" + ChanList + "\r\n" +
                        "\t" + "\t" + UnitList + "\r\n" + "\r\n";

                    StartSample = 0;
                    DateTags = new int[SampleCount];
                    TimeTags = new int[SampleCount];
                    ULStat = logger.ReadTimeTags(StartSample, SampleCount, ref DateTags, ref TimeTags);

                    dioChannelData = new int[SampleCount * dioChannelCount];
                    ULStat = logger.ReadDIOChannels(StartSample, SampleCount, ref dioChannelData);

                    ListSize = SampleCount;
                    if (ListSize > 50) ListSize = 50;
                    PostfixStr = "";
                    for (i = 0; i < ListSize; i++)
                    {
                        //Parse the date from the StartDate parameter
                        Month = (DateTags[i] >> 8) & 0xff;
                        Day = DateTags[i] & 0xff;
                        Year = (DateTags[i] >> 16) & 0xff;
                        StartDateStr = Month.ToString("00") + "/" +
                            Day.ToString("00") + "/" + Year.ToString("0000");

                        //Parse the time from the StartTime parameter
                        Hour = (TimeTags[i] >> 16) & 0xff;
                        Minute = (TimeTags[i] >> 8) & 0xff;
                        Second = TimeTags[i] & 0xff;
                        Postfix = (TimeTags[i] >> 24) & 0xff;
                        if (Postfix == 0) PostfixStr = " AM";
                        if (Postfix == 1) PostfixStr = " PM";
                        StartTimeStr = Hour.ToString("00") + ":" +
                            Minute.ToString("00") + ":" + Second.ToString("00")
                            + PostfixStr + "\t";
                        Index = i * dioChannelCount;
                        lbDataStr = "";
                        for (j = 0; j < dioChannelCount; j++)
                            lbDataStr = lbDataStr + dioChannelData[Index + j].ToString("0");
                        DataListStr = DataListStr + StartDateStr + "  " + StartTimeStr + lbDataStr + "\r\n";
                    }
                    txtData.Text = "Digital data from " + logger.FileName + "\r\n" + "\r\n" + DataListStr;
                }
                else
                    txtData.Text = "There is no digital data in " + logger.FileName + ".";
            }

        }

        private void cmdCJCData_Click(object sender, EventArgs e)
        {

            int Hour, Minute, Second;
            int Month, Day, Year;
            int Postfix;
            string PostfixStr, StartDateStr, DataListStr;
            string StartTimeStr, lbDataStr;
            int StartSample, i, j;
            int[] DateTags;
            int[] TimeTags;
            int Index;
            float[] CJCChannelData;
            int CJCChannelCount = 0;
            string UnitList, ChanList;
            int ListSize;

            // Get the Digital channel count
            //   Parameters:
            //	   CJCChannelCount		:receives the number of DIO chennels logged

            ULStat = logger.GetCJCInfo(ref CJCChannelCount);
            ChanList = "";
            UnitList = "";
            StartSample = 0;
            if (ULStat.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
                txtData.Text = ULStat.Message;
            else
            {
                CJCChannelData = new float[SampleCount * CJCChannelCount];
                if ((CJCChannelCount > 0) & (SampleCount > 0))
                {
                    DataListStr = "Time" + "\t" + "\t" + ChanList + "\r\n" +
                        "\t" + "\t" + UnitList + "\r\n" + "\r\n";

                    DateTags = new int[SampleCount];
                    TimeTags = new int[SampleCount];
                    ULStat = logger.ReadTimeTags(StartSample, SampleCount, ref DateTags, ref TimeTags);

                    CJCChannelData = new float[SampleCount * CJCChannelCount];
                    ULStat = logger.ReadCJCChannels(StartSample, SampleCount, ref CJCChannelData);

                    ListSize = SampleCount;
                    if (ListSize > 50) ListSize = 50;
                    PostfixStr = "";
                    for (i = 0; i < ListSize; i++)
                    {
                        //Parse the date from the StartDate parameter
                        Month = (DateTags[i] >> 8) & 0xff;
                        Day = DateTags[i] & 0xff;
                        Year = (DateTags[i] >> 16) & 0xff;
                        StartDateStr = Month.ToString("00") + "/" +
                            Day.ToString("00") + "/" + Year.ToString("0000");

                        //Parse the time from the StartTime parameter
                        Hour = (TimeTags[i] >> 16) & 0xff;
                        Minute = (TimeTags[i] >> 8) & 0xff;
                        Second = TimeTags[i] & 0xff;
                        Postfix = (TimeTags[i] >> 24) & 0xff;
                        if (Postfix == 0) PostfixStr = " AM";
                        if (Postfix == 1) PostfixStr = " PM";
                        StartTimeStr = Hour.ToString("00") + ":" +
                            Minute.ToString("00") + ":" + Second.ToString("00")
                            + PostfixStr + "\t";
                        Index = i * CJCChannelCount;
                        lbDataStr = "";
                        for (j = 0; j < CJCChannelCount; j++)
                            lbDataStr = lbDataStr + CJCChannelData[Index + j].ToString("0.00") + "\t";

                        DataListStr = DataListStr + StartDateStr + "  " + StartTimeStr + lbDataStr + "\r\n";
                    }
                    txtData.Text = "CJC data from " + logger.FileName + "\r\n" + "\r\n" + DataListStr;
                }
                else
                    txtData.Text = "There is no CJC data in " + logger.FileName + ".";

            }

        }

        private void OnButtonClick_OK(object sender, System.EventArgs e)
        {
            Close();
        }

        #region Form initialization, variables, and entry point

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new frmLoggerData());
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public frmLoggerData()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Windows Form Designer generated code
        private System.Windows.Forms.Button btnOK;
        private TextBox txtData;
        internal Button cmdCJCData;
        internal Button cmdDigitalData;
        internal Button cmdAnalogData;
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOK = new System.Windows.Forms.Button();
            this.txtData = new System.Windows.Forms.TextBox();
            this.cmdCJCData = new System.Windows.Forms.Button();
            this.cmdDigitalData = new System.Windows.Forms.Button();
            this.cmdAnalogData = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(569, 164);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(83, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "Quit";
            this.btnOK.Click += new System.EventHandler(this.OnButtonClick_OK);
            // 
            // txtData
            // 
            this.txtData.ForeColor = System.Drawing.Color.Blue;
            this.txtData.Location = new System.Drawing.Point(8, 11);
            this.txtData.Multiline = true;
            this.txtData.Name = "txtData";
            this.txtData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtData.Size = new System.Drawing.Size(540, 176);
            this.txtData.TabIndex = 2;
            // 
            // cmdCJCData
            // 
            this.cmdCJCData.Location = new System.Drawing.Point(569, 72);
            this.cmdCJCData.Name = "cmdCJCData";
            this.cmdCJCData.Size = new System.Drawing.Size(83, 23);
            this.cmdCJCData.TabIndex = 10;
            this.cmdCJCData.Text = "CJC Data";
            this.cmdCJCData.Click += new System.EventHandler(this.cmdCJCData_Click);
            // 
            // cmdDigitalData
            // 
            this.cmdDigitalData.Location = new System.Drawing.Point(569, 43);
            this.cmdDigitalData.Name = "cmdDigitalData";
            this.cmdDigitalData.Size = new System.Drawing.Size(83, 23);
            this.cmdDigitalData.TabIndex = 9;
            this.cmdDigitalData.Text = "Digital Data";
            this.cmdDigitalData.Click += new System.EventHandler(this.cmdDigitalData_Click);
            // 
            // cmdAnalogData
            // 
            this.cmdAnalogData.Location = new System.Drawing.Point(569, 14);
            this.cmdAnalogData.Name = "cmdAnalogData";
            this.cmdAnalogData.Size = new System.Drawing.Size(83, 23);
            this.cmdAnalogData.TabIndex = 8;
            this.cmdAnalogData.Text = "Analog Data";
            this.cmdAnalogData.Click += new System.EventHandler(this.cmdAnalogData_Click);
            // 
            // frmLoggerData
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(667, 206);
            this.Controls.Add(this.cmdCJCData);
            this.Controls.Add(this.cmdDigitalData);
            this.Controls.Add(this.cmdAnalogData);
            this.Controls.Add(this.txtData);
            this.Controls.Add(this.btnOK);
            this.Name = "frmLoggerData";
            this.Text = "Logger Data";
            this.Load += new System.EventHandler(this.frmLoggerData_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

    }
}
