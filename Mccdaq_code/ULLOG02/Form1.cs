//ULLOG02.SLN================================================================

// File:                         ULLOG02.SLN

// Library Call Demonstrated:    logger.GetSampleInfo
//                               logger.GetAIInfo
//                               logger.GetCJCInfo
//                               logger.GetDIOInfo

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
using System.IO;

namespace ULLOG02
{

    public class frmLogInfo : System.Windows.Forms.Form
	{
		public const int MAX_PATH = 260;
		private string m_Path = "..\\..\\..\\..";
        private MccDaq.ErrorInfo ULStat;
        private MccDaq.DataLogger logger;

        private void Form1_Load(object sender, EventArgs e)
        {

            //  Initiate error handling
            //   activating error handling will trap errors like
            //   bad channel numbers and non-configured conditions.
            //   Parameters:
            //     MccDaq.ErrorReporting.DontPrint : all warnings and errors encountered will be handled locally
            //     MccDaq.ErrorHandling.DontStop   : if an error is encountered, the program will not stop,
            //                                       errors must be handled locally

            MccDaq.ErrorInfo errorInfo = MccDaq.MccService.ErrHandling
                (MccDaq.ErrorReporting.DontPrint, MccDaq.ErrorHandling.DontStop);

        }

        private void cmdGetFile_Click(object sender, EventArgs e)
        {

            string filename = new string(' ', MAX_PATH);

            //  Get the first file in the directory
            //   Parameters:
            //     MccDaq.GetFileOptions.GetFirst  :first file
            //     m_Path						  :path to search
            //	  filename						  :receives name of file

            ULStat = MccDaq.DataLogger.GetFileName((int)MccDaq.GetFileOptions.GetFirst, ref m_Path, ref filename);
            if (ULStat.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
                lblComment.Text = ULStat.Message;
            else
            {
                //Filename is returned with a null terminator
                //which must be removed for proper display
                filename = filename.Trim();
                filename = filename.Trim('\0');

                // create an instance of the data logger
                logger = new MccDaq.DataLogger(filename);
                txtResults.Text =
                    "The name of the first file found is '"
                    + logger.FileName + "'.";
                cmdFileInfo.Enabled = true;
                cmdAnalogInfo.Enabled = true;
                cmdCJCInfo.Enabled = true;
                cmdDigitalInfo.Enabled = true;
                cmdSampInfo.Enabled = true;
            }

        }

        private void cmdFileInfo_Click(object sender, EventArgs e)
        {

            int version = 0;
            int size = 0;

            //  Get the file info for the first file in the directory
            //   Parameters:
            //     filename			:file to retrieve information from
            //     version			:receives the version of the file
            //	  size				:receives the size of file

            ULStat = logger.GetFileInfo(ref version, ref size);
            if (ULStat.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
                lblComment.Text = ULStat.Message;
            else
                txtResults.Text =
                    "The file properties of '" + logger.FileName + "' are:"
                    + "\r\n" + "\r\n" + "\t" + "Version: " + "\t" +
                    version.ToString("0") + "\r\n" + "\t" + "Size: "
                    + "\t" + size.ToString("0");

        }

        private void cmdSampInfo_Click(object sender, EventArgs e)
        {

            int Hour, Minute, Second;
            int Month, Day, Year;
            int SampleInterval, SampleCount;
            int StartDate, StartTime;
            int Postfix;
            string PostfixStr, StartDateStr;
            string StartTimeStr;

            PostfixStr = "";
            SampleInterval = 0;
            SampleCount = 0;
            StartDate = 0;
            StartTime = 0;

            // Get the sample information
            //  Parameters:
            //    Filename            :name of file to get information from
            //    SampleInterval      :time between samples
            //    SampleCount         :number of samples in the file
            //    StartDate           :date of first sample
            //    StartTime           :time of first sample

            ULStat = logger.GetSampleInfo(ref SampleInterval, ref SampleCount, ref StartDate, ref StartTime);
            if (ULStat.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
                lblComment.Text = ULStat.Message + ".";
            else
            {
                //Parse the date from the StartDate parameter
                Month = (StartDate >> 8) & 0xff;
                Day = StartDate & 0xff;
                Year = (StartDate >> 16) & 0xffff;
                StartDateStr = Month.ToString("00") + "/" +
                    Day.ToString("00") + "/" + Year.ToString("0000");

                //Parse the time from the StartTime parameter
                Hour = (StartTime >> 16) & 0xffff;
                Minute = (StartTime >> 8) & 0xff;
                Second = StartTime & 0xff;
                Postfix = (StartTime >> 24) & 0xff;
                if (Postfix == 0) PostfixStr = " AM";
                if (Postfix == 1) PostfixStr = " PM";
                StartTimeStr = Hour.ToString("00") + ":" +
                    Minute.ToString("00") + ":" + Second.ToString("00")
                    + PostfixStr;

                txtResults.Text =
                    "The sample properties of '" + logger.FileName + "' are:" +
                    "\r\n" + "\r\n" + "\t" + "SampleInterval: " + "\t" +
                    SampleInterval.ToString("0") + "\r\n" + "\t" + "SampleCount: " +
                    "\t" + SampleCount.ToString("0") + "\r\n" + "\t" +
                    "Start Date: " + "\t" + StartDateStr + "\r\n" + "\t" +
                    "Start Time: " + "\t" + StartTimeStr;

            }

        }

        private void cmdAnalogInfo_Click(object sender, EventArgs e)
        {

            int AIChannelCount = 0;
            int[] ChannelNumbers;
            int[] Units;
            string ChansStr, UnitsStr;
            string ChanList = "";
            short i;

            // Get the Analog channel count
            //  Parameters:
            //    Filename                :name of file to get information from
            //    AIChannelCount          :number of analog channels logged

            ULStat = logger.GetAIChannelCount(ref AIChannelCount);
            if (ULStat.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
                lblComment.Text = ULStat.Message + ".";
            else
            {
                // Get the Analog information
                //  Parameters:
                //    Filename                :name of file to get information from
                //    ChannelNumbers          :array containing channel numbers that were logged
                //    Units                   :array containing the units for each channel that was logged
                //    AIChannelCount          :number of analog channels logged

                ChannelNumbers = new int[AIChannelCount];
                Units = new int[AIChannelCount];

                ULStat = logger.GetAIInfo(ref ChannelNumbers, ref Units);
                if (ULStat.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
                    lblComment.Text = ULStat.Message + ".";
                else
                    for (i = 0; i < AIChannelCount; i++)
                    {
                        ChansStr = ChannelNumbers[i].ToString();
                        UnitsStr = "Temperature";
                        if (Units[i] == (int)MccDaq.LoggerUnits.Raw) UnitsStr = "Raw";
                        ChanList = ChanList + "Channel " + ChansStr + ": " + "\t" + UnitsStr + "\r\n" + "\t";
                    }
                txtResults.Text =
                    "The analog channel properties of '" + logger.FileName + "' are:" +
                    "\r\n" + "\r\n" + "\t" + "Number of channels: " + "\t" +
                    AIChannelCount.ToString("0") + "\r\n" + "\r\n" + "\t" + ChanList;
            }

        }

        private void cmdCJCInfo_Click(object sender, EventArgs e)
        {

            int cjcChannelCount = 0;

            // Get the CJC information
            //  Parameters:
            //    CJCChannelCount         :number of CJC channels logged

            ULStat = logger.GetCJCInfo(ref cjcChannelCount);
            if (ULStat.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
                lblComment.Text = ULStat.Message + ".";
            else
                txtResults.Text =
                    "The CJC properties of '" + logger.FileName + "' are:" +
                    "\r\n" + "\r\n" + "\t" + "Number of CJC channels: " +
                    "\t" + cjcChannelCount.ToString("0");
        }

        private void cmdDigitalInfo_Click(object sender, EventArgs e)
        {

            int dioChannelCount = 0;

            ULStat = logger.GetDIOInfo(ref dioChannelCount);
            if (ULStat.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
                lblComment.Text = ULStat.Message + ".";
            else
                txtResults.Text =
                    "The Digital properties of '" + logger.FileName + "' are:" +
                    "\r\n" + "\r\n" + "\t" + "Number of digital channels: " +
                    "\t" + dioChannelCount.ToString("0");
        }

        private void OnButtonClick_OK(object sender, System.EventArgs e)
        {
            Close();
        }

        #region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.btnOK = new System.Windows.Forms.Button();
            this.cmdDigitalInfo = new System.Windows.Forms.Button();
            this.cmdCJCInfo = new System.Windows.Forms.Button();
            this.cmdAnalogInfo = new System.Windows.Forms.Button();
            this.cmdSampInfo = new System.Windows.Forms.Button();
            this.cmdFileInfo = new System.Windows.Forms.Button();
            this.cmdGetFile = new System.Windows.Forms.Button();
            this.txtResults = new System.Windows.Forms.TextBox();
            this.lblComment = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(612, 143);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(120, 23);
            this.btnOK.TabIndex = 24;
            this.btnOK.Text = "Quit";
            this.btnOK.Click += new System.EventHandler(this.OnButtonClick_OK);
            // 
            // cmdDigitalInfo
            // 
            this.cmdDigitalInfo.Enabled = false;
            this.cmdDigitalInfo.Location = new System.Drawing.Point(612, 108);
            this.cmdDigitalInfo.Name = "cmdDigitalInfo";
            this.cmdDigitalInfo.Size = new System.Drawing.Size(120, 23);
            this.cmdDigitalInfo.TabIndex = 97;
            this.cmdDigitalInfo.Text = "Get Digital Info";
            this.cmdDigitalInfo.UseVisualStyleBackColor = true;
            this.cmdDigitalInfo.Click += new System.EventHandler(this.cmdDigitalInfo_Click);
            // 
            // cmdCJCInfo
            // 
            this.cmdCJCInfo.Enabled = false;
            this.cmdCJCInfo.Location = new System.Drawing.Point(612, 73);
            this.cmdCJCInfo.Name = "cmdCJCInfo";
            this.cmdCJCInfo.Size = new System.Drawing.Size(120, 23);
            this.cmdCJCInfo.TabIndex = 96;
            this.cmdCJCInfo.Text = "Get CJC Info";
            this.cmdCJCInfo.UseVisualStyleBackColor = true;
            this.cmdCJCInfo.Click += new System.EventHandler(this.cmdCJCInfo_Click);
            // 
            // cmdAnalogInfo
            // 
            this.cmdAnalogInfo.Enabled = false;
            this.cmdAnalogInfo.Location = new System.Drawing.Point(481, 143);
            this.cmdAnalogInfo.Name = "cmdAnalogInfo";
            this.cmdAnalogInfo.Size = new System.Drawing.Size(120, 23);
            this.cmdAnalogInfo.TabIndex = 95;
            this.cmdAnalogInfo.Text = "Get Analog Chan Info";
            this.cmdAnalogInfo.UseVisualStyleBackColor = true;
            this.cmdAnalogInfo.Click += new System.EventHandler(this.cmdAnalogInfo_Click);
            // 
            // cmdSampInfo
            // 
            this.cmdSampInfo.Enabled = false;
            this.cmdSampInfo.Location = new System.Drawing.Point(481, 108);
            this.cmdSampInfo.Name = "cmdSampInfo";
            this.cmdSampInfo.Size = new System.Drawing.Size(120, 23);
            this.cmdSampInfo.TabIndex = 94;
            this.cmdSampInfo.Text = "Get Sample Info";
            this.cmdSampInfo.UseVisualStyleBackColor = true;
            this.cmdSampInfo.Click += new System.EventHandler(this.cmdSampInfo_Click);
            // 
            // cmdFileInfo
            // 
            this.cmdFileInfo.Enabled = false;
            this.cmdFileInfo.Location = new System.Drawing.Point(482, 73);
            this.cmdFileInfo.Name = "cmdFileInfo";
            this.cmdFileInfo.Size = new System.Drawing.Size(120, 23);
            this.cmdFileInfo.TabIndex = 93;
            this.cmdFileInfo.Text = "Get File Info";
            this.cmdFileInfo.UseVisualStyleBackColor = true;
            this.cmdFileInfo.Click += new System.EventHandler(this.cmdFileInfo_Click);
            // 
            // cmdGetFile
            // 
            this.cmdGetFile.Location = new System.Drawing.Point(482, 6);
            this.cmdGetFile.Name = "cmdGetFile";
            this.cmdGetFile.Size = new System.Drawing.Size(120, 23);
            this.cmdGetFile.TabIndex = 92;
            this.cmdGetFile.Text = "Find File";
            this.cmdGetFile.UseVisualStyleBackColor = true;
            this.cmdGetFile.Click += new System.EventHandler(this.cmdGetFile_Click);
            // 
            // txtResults
            // 
            this.txtResults.ForeColor = System.Drawing.Color.Blue;
            this.txtResults.Location = new System.Drawing.Point(11, 6);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.Size = new System.Drawing.Size(420, 115);
            this.txtResults.TabIndex = 98;
            // 
            // lblComment
            // 
            this.lblComment.ForeColor = System.Drawing.Color.Blue;
            this.lblComment.Location = new System.Drawing.Point(8, 129);
            this.lblComment.Name = "lblComment";
            this.lblComment.Size = new System.Drawing.Size(423, 37);
            this.lblComment.TabIndex = 99;
            // 
            // frmLogInfo
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(744, 176);
            this.Controls.Add(this.lblComment);
            this.Controls.Add(this.txtResults);
            this.Controls.Add(this.cmdDigitalInfo);
            this.Controls.Add(this.cmdCJCInfo);
            this.Controls.Add(this.cmdAnalogInfo);
            this.Controls.Add(this.cmdSampInfo);
            this.Controls.Add(this.cmdFileInfo);
            this.Controls.Add(this.cmdGetFile);
            this.Controls.Add(this.btnOK);
            this.Name = "frmLogInfo";
            this.Text = "Log Info";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

        #region Form initialization, variables, and entry point

        private System.Windows.Forms.Button btnOK;
        internal Button cmdDigitalInfo;
        internal Button cmdCJCInfo;
        internal Button cmdAnalogInfo;
        internal Button cmdSampInfo;
        internal Button cmdFileInfo;
        internal Button cmdGetFile;
        private TextBox txtResults;
        internal Label lblComment;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new frmLogInfo());
		}

        public frmLogInfo()
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

    }
}
