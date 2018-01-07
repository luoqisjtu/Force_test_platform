// ==============================================================================
//
//  File:                         ULFL01.CS
//
//  Library Call Demonstrated:    LED Functions
//                                Mccdaq.MccBoard.FlashLED()
//
//  Purpose:                      Operate the LED.
//
//  Demonstration:                Flashes onboard LED for visual identification
//
//  Other Library Calls:          MccDaq.MccService.ErrHandling()
//
//  Special Requirements:         Board 0 must have an external LED,
//                                such as the miniLAB 1008 and PMD-1208LS.
//
// ==============================================================================
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace ULFL01
{

    public class frmLEDTest : Form
	{

        private MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

        private void frmLEDTest_Load(object sender, EventArgs e)
        {

            InitUL();

        }

        private void btnFlash_Click(object sender, System.EventArgs e)
        {
            lblResult.Text = "";

            //Flash the LED
            lblResult.Text = "The LED on device " + DaqBoard.BoardNum.ToString()
                + " should flash several times.";
            Application.DoEvents();

            MccDaq.ErrorInfo ULStat = DaqBoard.FlashLED();
            if (ULStat.Value != MccDaq.ErrorInfo.ErrorCode.NoErrors)
                lblResult.Text = "";

        }

        private void InitUL()
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.btnFlash = new System.Windows.Forms.Button();
            this.lblResult = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnFlash
            // 
            this.btnFlash.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFlash.Location = new System.Drawing.Point(128, 64);
            this.btnFlash.Name = "btnFlash";
            this.btnFlash.Size = new System.Drawing.Size(96, 48);
            this.btnFlash.TabIndex = 0;
            this.btnFlash.Text = "Flash LED";
            this.btnFlash.Click += new System.EventHandler(this.btnFlash_Click);
            // 
            // lblResult
            // 
            this.lblResult.ForeColor = System.Drawing.Color.Blue;
            this.lblResult.Location = new System.Drawing.Point(27, 128);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(287, 58);
            this.lblResult.TabIndex = 1;
            this.lblResult.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // frmLEDTest
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(344, 205);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.btnFlash);
            this.Name = "frmLEDTest";
            this.Text = "Universal Library LED Test";
            this.Load += new System.EventHandler(this.frmLEDTest_Load);
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
			Application.Run(new frmLEDTest());
		}

        public frmLEDTest()
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

        private Button btnFlash;
        private Label lblResult;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        #endregion

    }
}
