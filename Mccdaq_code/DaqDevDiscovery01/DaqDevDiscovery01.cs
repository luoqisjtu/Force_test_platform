// ==============================================================================

//  File:                         DaqDevDiscovery01.CS

//  Library Call Demonstrated:    MccDaq.DaqDeviceManager.GetDaqDeviceInventory()
//                                MccDaq.DaqDeviceManager.CreateDaqDevice()
//                                MccDaq.DaqDeviceManager.ReleaseDaqDevice()

//  Purpose:                      Discovers DAQ devices and assigns 
//							      board number to the detected devices

//  Demonstration:                Displays the detected DAQ devices
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

namespace DaqDevDiscovery01
{
	public class frmDevDiscovery : System.Windows.Forms.Form
	{
        private void frmDeviceDiscovery_Load(object eventSender, System.EventArgs eventArgs)
        {
            InitUL();

            MccDaq.DaqDeviceManager.IgnoreInstaCal();
        }

        private void cmdDiscover_Click(object sender, EventArgs e)
        {
            
            ReleaseDAQDevices();

            lblDevID.Text = "";
            Cursor.Current = Cursors.WaitCursor;

            // Discover DAQ devices with GetDaqDeviceInventory()
	        //  Parameters:
            //    InterfaceType   :interface type of DAQ devices to be discovered

            MccDaq.DaqDeviceDescriptor[] inventory = MccDaq.DaqDeviceManager.GetDaqDeviceInventory(MccDaq.DaqDeviceInterface.Any);

            int numDevDiscovered = inventory.Length;

            cmbBoxDiscoveredDevs.Items.Clear();

            lblStatus.Text = numDevDiscovered + " DAQ Device(s) Discovered";

            if (numDevDiscovered > 0)
            {
                for (int boardNum = 0; boardNum < numDevDiscovered; boardNum++)
                {

                    try
                    {
                        //    Create a new MccBoard object for Board and assign a board number 
                        //    to the specified DAQ device with CreateDaqDevice()

                        //    Parameters:
                        //        BoardNum			: board number to be assigned to the specified DAQ device
                        //        DeviceDescriptor	: device descriptor of the DAQ device 

                        MccDaq.MccBoard daqBoard = MccDaq.DaqDeviceManager.CreateDaqDevice(boardNum, inventory[boardNum]);

                        // Add the board to combobox
                        cmbBoxDiscoveredDevs.Items.Add(daqBoard);
                    }
                    catch (ULException ule)
                    {
                        lblStatus.Text = "Error occured: " + ule.Message;
                    }
                }
            }


            if (cmbBoxDiscoveredDevs.Items.Count > 0)
            {
                cmbBoxDiscoveredDevs.Enabled = true;
                cmbBoxDiscoveredDevs.SelectedIndex = 0;
                cmdFlashLED.Enabled = true;
            }
            else
            {
                cmbBoxDiscoveredDevs.Enabled = false;
                cmdFlashLED.Enabled = false;
            }

            Cursor.Current = Cursors.Default;
        }

        private void cmbBoxDiscoveredDevs_SelectedIndexChanged(object sender, EventArgs e)
        {
            MccDaq.MccBoard daqBoard = (MccDaq.MccBoard)cmbBoxDiscoveredDevs.SelectedItem;

            lblDevID.Text = daqBoard.Descriptor.UniqueID;
        }

        private void cmdFlashLED_Click(object sender, EventArgs e)
        {
            MccDaq.MccBoard daqBoard = (MccDaq.MccBoard)cmbBoxDiscoveredDevs.SelectedItem;

            // Flash the LED of the specified DAQ device with FlashLED()

            if(daqBoard != null)
                daqBoard.FlashLED();
        }

        private void ReleaseDAQDevices()
        {
            foreach (MccDaq.MccBoard daqBoard in cmbBoxDiscoveredDevs.Items)
            {
                // Release resources associated with the specified board number within the Universal Library with cbReleaseDaqDevice()
                //    Parameters:
                //    	MccBoard:			Board object

                MccDaq.DaqDeviceManager.ReleaseDaqDevice(daqBoard);
            }
        }


    

        private void cmdQuit_Click(object eventSender, System.EventArgs eventArgs)
        {
            ReleaseDAQDevices();

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
            this.cmbBoxDiscoveredDevs = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmdFlashLED = new System.Windows.Forms.Button();
            this.lblDevID = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdQuit
            // 
            this.cmdQuit.BackColor = System.Drawing.SystemColors.Control;
            this.cmdQuit.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdQuit.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdQuit.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdQuit.Location = new System.Drawing.Point(239, 303);
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
            this.lblDemoFunction.Size = new System.Drawing.Size(294, 48);
            this.lblDemoFunction.TabIndex = 2;
            this.lblDemoFunction.Text = "Demonstration of DaqDeviceManager.GetDaqDeviceInventory() and DaqDeviceManager.Cr" +
                "eateDaqDevice() ";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // cmdDiscover
            // 
            this.cmdDiscover.BackColor = System.Drawing.SystemColors.Control;
            this.cmdDiscover.Font = new System.Drawing.Font("Arial", 8F);
            this.cmdDiscover.Location = new System.Drawing.Point(81, 75);
            this.cmdDiscover.Name = "cmdDiscover";
            this.cmdDiscover.Size = new System.Drawing.Size(143, 23);
            this.cmdDiscover.TabIndex = 11;
            this.cmdDiscover.Text = "Discover DAQ devices";
            this.cmdDiscover.UseVisualStyleBackColor = false;
            this.cmdDiscover.Click += new System.EventHandler(this.cmdDiscover_Click);
            // 
            // cmbBoxDiscoveredDevs
            // 
            this.cmbBoxDiscoveredDevs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBoxDiscoveredDevs.Enabled = false;
            this.cmbBoxDiscoveredDevs.FormattingEnabled = true;
            this.cmbBoxDiscoveredDevs.Location = new System.Drawing.Point(24, 31);
            this.cmbBoxDiscoveredDevs.Name = "cmbBoxDiscoveredDevs";
            this.cmbBoxDiscoveredDevs.Size = new System.Drawing.Size(238, 22);
            this.cmbBoxDiscoveredDevs.TabIndex = 12;
            this.cmbBoxDiscoveredDevs.SelectedIndexChanged += new System.EventHandler(this.cmbBoxDiscoveredDevs_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmdFlashLED);
            this.groupBox1.Controls.Add(this.lblDevID);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cmbBoxDiscoveredDevs);
            this.groupBox1.Location = new System.Drawing.Point(15, 150);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(276, 135);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Discovered Devices";
            // 
            // cmdFlashLED
            // 
            this.cmdFlashLED.BackColor = System.Drawing.SystemColors.Control;
            this.cmdFlashLED.Enabled = false;
            this.cmdFlashLED.Font = new System.Drawing.Font("Arial", 8F);
            this.cmdFlashLED.Location = new System.Drawing.Point(100, 101);
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
            this.lblDevID.Location = new System.Drawing.Point(113, 69);
            this.lblDevID.Name = "lblDevID";
            this.lblDevID.Size = new System.Drawing.Size(0, 14);
            this.lblDevID.TabIndex = 14;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 8F);
            this.label1.Location = new System.Drawing.Point(24, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 14);
            this.label1.TabIndex = 13;
            this.label1.Text = "Device Identifier:";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = System.Drawing.Color.Blue;
            this.lblStatus.Location = new System.Drawing.Point(18, 119);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(42, 14);
            this.lblStatus.TabIndex = 15;
            this.lblStatus.Text = "Status";
            // 
            // frmDevDiscovery
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(314, 341);
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
        private ComboBox cmbBoxDiscoveredDevs;
        private GroupBox groupBox1;
        private Button cmdFlashLED;
        private Label lblDevID;
        private Label label1;
        private Label lblStatus;

        #endregion

    }
}