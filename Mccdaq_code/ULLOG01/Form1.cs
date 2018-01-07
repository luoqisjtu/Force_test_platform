//ULLOG01.VBP================================================================

// File:                         ULLOG01.VBP

// Library Call Demonstrated:    cbLogGetFileName()

// Purpose:                      Lists data logger files.

// Demonstration:                Displays the analog input on a user-specified
//                               channel.

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

namespace ULLOG01
{

    public class frmLogFiles : System.Windows.Forms.Form
	{
		public const int MAX_PATH = 260;
		private string m_Path = "..\\..\\..\\..";
        private TextBox txtFileNum;
		private int m_FileNumber = 0;

        private void frmLogFiles_Load(object sender, EventArgs e)
        {

            //  Initiate error handling
            //   activating error handling will trap errors like
            //   bad channel numbers and non-configured conditions.
            //   Parameters:
            //     MccDaq.ErrorReporting.PrintAll :all warnings and errors encountered will be printed
            //     MccDaq.ErrorHandling.StopAll   :if an error is encountered, the program will stop
            MccDaq.ErrorInfo ULStat = MccDaq.MccService.ErrHandling
                (MccDaq.ErrorReporting.PrintAll, MccDaq.ErrorHandling.StopAll);

        }

        private void OnButtonClick_FirstFile(object sender, System.EventArgs e)
        {
            string filename = new string(' ', MAX_PATH);
            MccDaq.ErrorInfo errorInfo;

            lblComment.Text = "Get first file from directory " + m_Path;

            //  Get the first file in the directory
            //   Parameters:
            //     MccDaq.GetFileOptions.GetFirst :first file
            //     m_Path						  :path to search
            //	   filename						  :receives name of file
            errorInfo = MccDaq.DataLogger.GetFileName((int)
                MccDaq.GetFileOptions.GetFirst, ref m_Path, ref filename);

            if (errorInfo.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
            {
                if (errorInfo.Value == MccDaq.ErrorInfo.ErrorCode.NoMoreFiles)
                    this.lblComment.Text = "There are no more files to display.";
                else
                    MessageBox.Show(errorInfo.Message);
            }
            else
            {
                // Filename is returned with a null terminator
                // which must be removed for proper display
                filename = filename.Trim();
                string newpath = filename.Trim('\0');
                string absolutePath = Path.GetFullPath(newpath);
                lbFileList.Items.Clear();
                lbFileList.Items.Add(absolutePath);
            }
        }

        private void OnButtonClick_NextFile(object sender, System.EventArgs e)
        {
            string filename = new string(' ', MAX_PATH);
            MccDaq.ErrorInfo errorInfo;

            lblComment.Text = "Get next file from directory " + m_Path;

            //  Get the next file in the directory
            //   Parameters:
            //     MccDaq.GetFileOptions.GetNext :next file
            //     m_Path						  :path to search
            //	   filename						  :receives name of file
            errorInfo = MccDaq.DataLogger.GetFileName((int)
                MccDaq.GetFileOptions.GetNext, ref m_Path, ref filename);

            if (errorInfo.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
            {
                if (errorInfo.Value == MccDaq.ErrorInfo.ErrorCode.NoMoreFiles)
                    lblComment.Text = "There are no more files to display.";
                else
                    MessageBox.Show(errorInfo.Message);
            }
            else
            {
                // Filename is returned with a null terminator
                // which must be removed for proper display
                filename = filename.Trim();
                string newpath = filename.Trim('\0');
                string absolutePath = Path.GetFullPath(newpath);
                lbFileList.Items.Clear();
                lbFileList.Items.Add(absolutePath);
            }
        }

        private void OnButtonClick_FileNumber(object sender, System.EventArgs e)
        {
            string filename = new string(' ', MAX_PATH);
            MccDaq.ErrorInfo errorInfo;
            int FileNumber;
            bool ValidNum;

            ValidNum = int.TryParse(this.txtFileNum.Text, out FileNumber);
            lblComment.Text = "Get file number " + FileNumber.ToString()
                + " from directory " + m_Path;

            //  Get the Nth file in the directory
            //   Parameters:
            //     m_FileNumber					  :Nth file in the directory 
            //     m_Path						  :path to search
            //	   filename						  :receives name of file
            errorInfo = MccDaq.DataLogger.GetFileName
                (FileNumber, ref m_Path, ref filename);

            if (errorInfo.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
                lblComment.Text = errorInfo.Message;
            else
            {
                lbFileList.Items.Clear();
                filename = filename.Trim();
                string newpath = filename.Trim('\0');
                string absolutePath = Path.GetFullPath(newpath);
                lbFileList.Items.Add(absolutePath);
            }
        }

        private void OnButtonClick_AllFiles(object sender, System.EventArgs e)
        {
            string filename = new string('\0', MAX_PATH);
            MccDaq.ErrorInfo errorInfo;

            lblComment.Text = "Get all files from directory " + Path.GetFullPath(m_Path);

            //  Get the first file in the directory
            //   Parameters:
            //     MccDaq.GetFileOptions.GetFirst :first file
            //     m_Path						  :path to search
            //	   filename						  :receives name of file
            errorInfo = MccDaq.DataLogger.GetFileName((int)MccDaq.GetFileOptions.GetFirst, ref m_Path, ref filename);
            string newpath = filename.TrimEnd('\0');
            string absolutePath = Path.GetFullPath(newpath);

            if (errorInfo.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
            {
                MessageBox.Show(errorInfo.Message);
                return;
            }
            else
            {
                lbFileList.Items.Clear();
                lbFileList.Items.Add(absolutePath);
            }

            while (errorInfo.Value != MccDaq.ErrorInfo.ErrorCode.NoMoreFiles)
            {
                //  Get the next file in the directory
                //   Parameters:
                //     MccDaq.GetFileOptions.GetNext :next file
                //     m_Path						  :path to search
                //	   filename						  :receives name of file
                errorInfo = MccDaq.DataLogger.GetFileName((int)MccDaq.GetFileOptions.GetNext, ref m_Path, ref filename);
                newpath = filename.TrimEnd('\0');
                absolutePath = Path.GetFullPath(newpath);

                if (errorInfo.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
                {
                    if (errorInfo.Value != MccDaq.ErrorInfo.ErrorCode.NoMoreFiles)
                    {
                        MessageBox.Show(errorInfo.Message);
                        return;
                    }
                }
                else
                    lbFileList.Items.Add(absolutePath);
            }
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
            this.btnFirstFile = new System.Windows.Forms.Button();
            this.btnNextFile = new System.Windows.Forms.Button();
            this.btnFileNumber = new System.Windows.Forms.Button();
            this.btnAllFiles = new System.Windows.Forms.Button();
            this.lbFileList = new System.Windows.Forms.ListBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblComment = new System.Windows.Forms.Label();
            this.txtFileNum = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnFirstFile
            // 
            this.btnFirstFile.Location = new System.Drawing.Point(10, 24);
            this.btnFirstFile.Name = "btnFirstFile";
            this.btnFirstFile.Size = new System.Drawing.Size(75, 23);
            this.btnFirstFile.TabIndex = 0;
            this.btnFirstFile.Text = "First File";
            this.btnFirstFile.Click += new System.EventHandler(this.OnButtonClick_FirstFile);
            // 
            // btnNextFile
            // 
            this.btnNextFile.Location = new System.Drawing.Point(10, 56);
            this.btnNextFile.Name = "btnNextFile";
            this.btnNextFile.Size = new System.Drawing.Size(75, 23);
            this.btnNextFile.TabIndex = 1;
            this.btnNextFile.Text = "Next File";
            this.btnNextFile.Click += new System.EventHandler(this.OnButtonClick_NextFile);
            // 
            // btnFileNumber
            // 
            this.btnFileNumber.Location = new System.Drawing.Point(10, 200);
            this.btnFileNumber.Name = "btnFileNumber";
            this.btnFileNumber.Size = new System.Drawing.Size(75, 23);
            this.btnFileNumber.TabIndex = 2;
            this.btnFileNumber.Text = "File Number";
            this.btnFileNumber.Click += new System.EventHandler(this.OnButtonClick_FileNumber);
            // 
            // btnAllFiles
            // 
            this.btnAllFiles.Location = new System.Drawing.Point(10, 120);
            this.btnAllFiles.Name = "btnAllFiles";
            this.btnAllFiles.Size = new System.Drawing.Size(75, 23);
            this.btnAllFiles.TabIndex = 3;
            this.btnAllFiles.Text = "All Files";
            this.btnAllFiles.Click += new System.EventHandler(this.OnButtonClick_AllFiles);
            // 
            // lbFileList
            // 
            this.lbFileList.ForeColor = System.Drawing.Color.Blue;
            this.lbFileList.Location = new System.Drawing.Point(97, 24);
            this.lbFileList.Name = "lbFileList";
            this.lbFileList.Size = new System.Drawing.Size(347, 121);
            this.lbFileList.TabIndex = 4;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(361, 200);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "Quit";
            this.btnOK.Click += new System.EventHandler(this.OnButtonClick_OK);
            // 
            // lblComment
            // 
            this.lblComment.ForeColor = System.Drawing.Color.Blue;
            this.lblComment.Location = new System.Drawing.Point(17, 149);
            this.lblComment.Name = "lblComment";
            this.lblComment.Size = new System.Drawing.Size(424, 23);
            this.lblComment.TabIndex = 6;
            this.lblComment.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtFileNum
            // 
            this.txtFileNum.Location = new System.Drawing.Point(97, 202);
            this.txtFileNum.Name = "txtFileNum";
            this.txtFileNum.Size = new System.Drawing.Size(37, 20);
            this.txtFileNum.TabIndex = 7;
            this.txtFileNum.Text = "0";
            // 
            // frmLogFiles
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(456, 245);
            this.Controls.Add(this.txtFileNum);
            this.Controls.Add(this.lblComment);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lbFileList);
            this.Controls.Add(this.btnAllFiles);
            this.Controls.Add(this.btnFileNumber);
            this.Controls.Add(this.btnNextFile);
            this.Controls.Add(this.btnFirstFile);
            this.Name = "frmLogFiles";
            this.Text = "List Logger Files";
            this.Load += new System.EventHandler(this.frmLogFiles_Load);
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
			Application.Run(new frmLogFiles());
		}

        public frmLogFiles()
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

        /// <summary>
        /// Required designer variable.
        /// </summary>

        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnFirstFile;
        private System.Windows.Forms.Button btnNextFile;
        private System.Windows.Forms.Button btnFileNumber;
        private System.Windows.Forms.Button btnAllFiles;
        private System.Windows.Forms.ListBox lbFileList;
        private System.Windows.Forms.Label lblComment;

        #endregion

	}
}
