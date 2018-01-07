// ==============================================================================

//  File:                         DaqDevDiscovery02.CS

//  Library Call Demonstrated:    MccDaq.DaqDeviceManager.GetNetDeviceDescriptor()
//                                MccDaq.DaqDeviceManager.CreateDaqDevice()
//                                MccDaq.DaqDeviceManager.ReleaseDaqDevice()

//  Purpose:                      Discovers a Network DAQ device and assigns 
//							      board number to the detected device

//  Demonstration:                Displays the detected DAQ device
//							      and flashes the LED of the selected device

//  Other Library Calls:          MccDaq.DaqDeviceManager.IgnoreInstaCal()
//                                MccDaq.MccService.ErrHandling()

// ==============================================================================

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;

using MccDaq;
using ErrorDefs;

namespace DaqDevDiscovery02
{
	public class frmDevDiscovery : System.Windows.Forms.Form
	{
        MccDaq.MccBoard DaqBoard = null;

        private void frmDeviceDiscovery_Load(object eventSender, System.EventArgs eventArgs)
        {
            InitUL();

            MccDaq.DaqDeviceManager.IgnoreInstaCal();

            txtHost.ForeColor = Color.DarkGray;
            txtHost.Text = default_txt;
        }

        private void cmdDiscover_Click(object sender, EventArgs e)
        {

            cmdFlashLED.Enabled = false;
            lblDevName.Text = "";
            lblDevID.Text = "";

            if (DaqBoard != null)
                MccDaq.DaqDeviceManager.ReleaseDaqDevice(DaqBoard);

            string host = txtHost.Text;
            int portNum;
            int timeout = 5000;
            
            bool validPortNum = int.TryParse(txtPort.Text, out portNum);

            if (validPortNum)
            {
                Cursor.Current = Cursors.WaitCursor;

                try
                {
                    // Discover an Ethernet DAQ device with GetNetDeviceDescriptor()
                    //  Parameters:
                    //     Host				: Host name or IP address of DAQ device
                    //     Port				: Port Number
                    //     Timeout			: Timeout

                    MccDaq.DaqDeviceDescriptor deviceDescriptor = MccDaq.DaqDeviceManager.GetNetDeviceDescriptor(host, portNum, timeout);

                    if (deviceDescriptor != null)
                    {
                        lblStatus.Text = "DAQ Device Discovered";

                        lblDevName.Text = deviceDescriptor.ProductName;
                        lblDevID.Text = deviceDescriptor.UniqueID;

                   
                        //    Create a new MccBoard object for Board and assign a board number 
                        //    to the specified DAQ device with CreateDaqDevice()

                        //    Parameters:
                        //        BoardNum			: board number to be assigned to the specified DAQ device
                        //        DeviceDescriptor	: device descriptor of the DAQ device 

                        int boardNum = 0;

                        DaqBoard = MccDaq.DaqDeviceManager.CreateDaqDevice(boardNum, deviceDescriptor);

                        cmdFlashLED.Enabled = true;
                    }
                }
                catch (ULException ule)
                {
                    lblStatus.Text = "Error occured: " + ule.Message;
                }
            }
            else
                lblStatus.Text = "Invalid port number";

            Cursor.Current = Cursors.Default;

        }


        private void cmdFlashLED_Click(object sender, EventArgs e)
        {
            // Flash the LED of the specified DAQ device with FlashLED()

            if(DaqBoard != null)
                DaqBoard.FlashLED();
        }


        private void cmdQuit_Click(object eventSender, System.EventArgs eventArgs)
        {
            if (DaqBoard != null)
                MccDaq.DaqDeviceManager.ReleaseDaqDevice(DaqBoard);

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
            clsErrorDefs.HandleError = MccDaq.ErrorHandling.DontStop;
            MccDaq.ErrorInfo ULStat = MccDaq.MccService.ErrHandling
                (clsErrorDefs.ReportError, clsErrorDefs.HandleError);

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
            this.cmdQuit = new System.Windows.Forms.Button();
            this.lblDemoFunction = new System.Windows.Forms.Label();
            this.cmdDiscover = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblDevName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdFlashLED = new System.Windows.Forms.Button();
            this.lblDevID = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblHost = new System.Windows.Forms.Label();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdQuit
            // 
            this.cmdQuit.BackColor = System.Drawing.SystemColors.Control;
            this.cmdQuit.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdQuit.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdQuit.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdQuit.Location = new System.Drawing.Point(239, 368);
            this.cmdQuit.Name = "cmdQuit";
            this.cmdQuit.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdQuit.Size = new System.Drawing.Size(52, 26);
            this.cmdQuit.TabIndex = 6;
            this.cmdQuit.Text = "Quit";
            this.cmdQuit.UseVisualStyleBackColor = false;
            this.cmdQuit.Click += new System.EventHandler(this.cmdQuit_Click);
            // 
            // lblDemoFunction
            // 
            this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
            this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDemoFunction.Location = new System.Drawing.Point(8, 9);
            this.lblDemoFunction.Name = "lblDemoFunction";
            this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDemoFunction.Size = new System.Drawing.Size(294, 51);
            this.lblDemoFunction.TabIndex = 2;
            this.lblDemoFunction.Text = "Demonstration of DaqDeviceManager.GetNetDeviceDescriptor() and DaqDeviceManager.C" +
                "reateDaqDevice() ";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // cmdDiscover
            // 
            this.cmdDiscover.BackColor = System.Drawing.SystemColors.Control;
            this.cmdDiscover.Font = new System.Drawing.Font("Arial", 8F);
            this.cmdDiscover.Location = new System.Drawing.Point(81, 147);
            this.cmdDiscover.Name = "cmdDiscover";
            this.cmdDiscover.Size = new System.Drawing.Size(143, 23);
            this.cmdDiscover.TabIndex = 11;
            this.cmdDiscover.Text = "Discover DAQ device";
            this.cmdDiscover.UseVisualStyleBackColor = false;
            this.cmdDiscover.Click += new System.EventHandler(this.cmdDiscover_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblDevName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cmdFlashLED);
            this.groupBox1.Controls.Add(this.lblDevID);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(15, 224);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(276, 132);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Discovered Device";
            // 
            // lblDevName
            // 
            this.lblDevName.AutoSize = true;
            this.lblDevName.ForeColor = System.Drawing.Color.Green;
            this.lblDevName.Location = new System.Drawing.Point(84, 28);
            this.lblDevName.Name = "lblDevName";
            this.lblDevName.Size = new System.Drawing.Size(0, 14);
            this.lblDevName.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 8F);
            this.label1.Location = new System.Drawing.Point(9, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 14);
            this.label1.TabIndex = 16;
            this.label1.Text = "Device Name:";
            // 
            // cmdFlashLED
            // 
            this.cmdFlashLED.BackColor = System.Drawing.SystemColors.Control;
            this.cmdFlashLED.Enabled = false;
            this.cmdFlashLED.Font = new System.Drawing.Font("Arial", 8F);
            this.cmdFlashLED.Location = new System.Drawing.Point(100, 92);
            this.cmdFlashLED.Name = "cmdFlashLED";
            this.cmdFlashLED.Size = new System.Drawing.Size(75, 23);
            this.cmdFlashLED.TabIndex = 15;
            this.cmdFlashLED.Text = "Flash LED";
            this.cmdFlashLED.UseVisualStyleBackColor = false;
            this.cmdFlashLED.Click += new System.EventHandler(this.cmdFlashLED_Click);
            // 
            // lblDevID
            // 
            this.lblDevID.AutoSize = true;
            this.lblDevID.ForeColor = System.Drawing.Color.Green;
            this.lblDevID.Location = new System.Drawing.Point(98, 58);
            this.lblDevID.Name = "lblDevID";
            this.lblDevID.Size = new System.Drawing.Size(0, 14);
            this.lblDevID.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 8F);
            this.label2.Location = new System.Drawing.Point(9, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 14);
            this.label2.TabIndex = 13;
            this.label2.Text = "Device Identifier:";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = System.Drawing.Color.Blue;
            this.lblStatus.Location = new System.Drawing.Point(18, 188);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(42, 14);
            this.lblStatus.TabIndex = 15;
            this.lblStatus.Text = "Status";
            // 
            // lblHost
            // 
            this.lblHost.AutoSize = true;
            this.lblHost.Font = new System.Drawing.Font("Arial", 8.25F);
            this.lblHost.Location = new System.Drawing.Point(21, 76);
            this.lblHost.Name = "lblHost";
            this.lblHost.Size = new System.Drawing.Size(32, 14);
            this.lblHost.TabIndex = 16;
            this.lblHost.Text = "Host:";
            // 
            // txtHost
            // 
            this.txtHost.ForeColor = System.Drawing.Color.DarkGray;
            this.txtHost.Location = new System.Drawing.Point(57, 73);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(228, 20);
            this.txtHost.TabIndex = 17;
            this.txtHost.Text = "<Host name or IP address>";
            this.txtHost.Leave += new System.EventHandler(this.txtHost_Leave);
            this.txtHost.Enter += new System.EventHandler(this.txtHost_Enter);
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Font = new System.Drawing.Font("Arial", 8.25F);
            this.lblPort.Location = new System.Drawing.Point(24, 109);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(29, 14);
            this.lblPort.TabIndex = 18;
            this.lblPort.Text = "Port:";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(59, 106);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(100, 20);
            this.txtPort.TabIndex = 19;
            this.txtPort.Text = "54211";
            // 
            // frmDevDiscovery
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(310, 406);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.txtHost);
            this.Controls.Add(this.lblHost);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cmdDiscover);
            this.Controls.Add(this.cmdQuit);
            this.Controls.Add(this.lblDemoFunction);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Location = new System.Drawing.Point(182, 100);
            this.Name = "frmDevDiscovery";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Universal Library Device Discovery";
            this.Load += new System.EventHandler(this.frmDeviceDiscovery_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
			Application.Run(new frmDevDiscovery());
		}

        public frmDevDiscovery()
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
        public Button cmdQuit;
        public Label lblDemoFunction;
        private Button cmdDiscover;
        private GroupBox groupBox1;
        private Button cmdFlashLED;
        private Label lblDevID;
        private Label label2;
        private Label lblHost;
        private TextBox txtHost;
        private Label lblPort;
        private TextBox txtPort;
        private Label lblDevName;
        private Label label1;
        private Label lblStatus;

        #endregion

        string default_txt = "<Host name or IP address>";

        private void txtHost_Enter(object sender, EventArgs e)
        {
            if (txtHost.Text == default_txt)
            {
                txtHost.Text = "";
                txtHost.ForeColor = Color.Black;
            }
        }

        private void txtHost_Leave(object sender, EventArgs e)
        {
            if (txtHost.Text == "")
            {
                txtHost.ForeColor = Color.DarkGray;
                txtHost.Text = default_txt;
            }

        }

    }
}