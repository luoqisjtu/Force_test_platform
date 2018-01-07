//==========================================================================
//
//  File:                         ULAO03.CS
//
//  Library Call Demonstrated:    MccBoard.AOut()
//                                MccBoard.BoardConfig.DACUpdateMode()
//                                MccBoard.BoardConfig.DACUpdate()
//
//  Purpose:                      Demonstrates difference between DACUpdate.Immdediate
//                                and DACUpdate.OnCommand D/A Update modes
//
//  Demonstration:                Delays outputs until user issues update command DACUpdate()
//
//  Other Library Calls:          MccService.ErrHandling()
//                                MccBoard.FromEngUnits()
//
//  Special Requirements:         Board 0 must support BIDACUPDATEMODE settings,
//                                such as the PCI-DAC6700's.
//
//==========================================================================

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using AnalogIO;
using MccDaq;
using ErrorDefs;

namespace ULAO03
{

    public class frmAOut : Form
	{

        // Create a new MccBoard object for Board 0
        MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

        MccDaq.Range Range;
        private int DAResolution, NumAoChans;
        private int LowChan, MaxChan;

        const int AllChannels = -1;  //negative values specify all available devices/channels

        AnalogIO.clsAnalogIO AIOProps = new AnalogIO.clsAnalogIO();
        public Label lblInstruction;
        public Label lblDemoFunction;

        private void frmAOut_Load(object sender, EventArgs e)
        {

            MccDaq.TriggerType DefaultTrig;

            InitUL();

            // determine the number of analog channels and their capabilities
            int ChannelType = clsAnalogIO.ANALOGOUTPUT;
            NumAoChans = AIOProps.FindAnalogChansOfType(DaqBoard, ChannelType,
                out DAResolution, out Range, out LowChan, out DefaultTrig);

            if (NumAoChans == 0)
            {
                lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " does not have analog output channels.";
                this.btnSendData.Enabled = false;
                this.btnUpdateDACs.Enabled = false;
            }
            else
            {
                if (NumAoChans > 4) NumAoChans = 4;
                MaxChan = LowChan + NumAoChans - 1;
                lblInstruction.Text = "Board " + DaqBoard.BoardNum.ToString() +
                    " updating analog data on up to " + NumAoChans.ToString() +
                    " channels using AOut with Range set to " + Range.ToString() + ".";
            }

        }

        private void rdioOnCommand_CheckedChanged(object sender, System.EventArgs e)
		{
			if (rdioOnCommand.Checked)
			{
				// Set DAC Update mode to hold off updating D/A's until command is sent
				// Parameters
				//	 channel	: D/A channel whose update mode is to be configured. Note
				//				  that negative values selects all channels
				//   DACUpdate.OnCommand : delay D/A output updates from AOut or AOutScan until
				//                         DACUpdate command is issued.
				int channel = AllChannels;
				DaqBoard.BoardConfig.SetDACUpdateMode(channel, (int)MccDaq.DACUpdate.OnCommand);
			}
		}

		private void rdioImmediate_CheckedChanged(object sender, System.EventArgs e)
		{
			if (rdioImmediate.Checked)
			{
				// Set DAC Update mode to update immediately upon cbAOut or cbAOutScan
				// Parameters
				//	 channel	: D/A channel whose update mode is to be configured. Note
				//				  that negative values selects all channels
				//   DACUpdate.Immediate : update D/A outputs immediately upon AOut or AOutScan
				int channel = AllChannels;
				DaqBoard.BoardConfig.SetDACUpdateMode(channel, (int)MccDaq.DACUpdate.Immediate);
			}
		}

		private void btnSendData_Click(object sender, System.EventArgs e)
		{
			float volts=0.0F;
			ushort daCounts = 0;
			int chan = 0;
			foreach(TextBox box in _txtAOVolts)
			{
                if (chan <= MaxChan)
                {
                    //get voltage to output
                    volts = float.Parse(box.Text);

                    // convert from voltage to binary counts
                    DaqBoard.FromEngUnits(Range, volts, out daCounts);

                    // load D/A
                    DaqBoard.AOut(chan, Range, daCounts);
                    ++chan;
                }
			}
		}

		private void btnUpdateDACs_Click(object sender, System.EventArgs e)
		{
			// Issue D/A Update command
			DaqBoard.BoardConfig.DACUpdate();
		}

        private void InitUL()
        {

            //  Initiate error handling
            //   activating error handling will trap errors like
            //   bad channel numbers and non-configured conditions.
            //   Parameters:
            //     MccDaq.ErrorReporting.PrintAll :all warnings and errors encountered will be printed
            //     MccDaq.ErrorHandling.StopAll   :if an error is encountered, the program will stop

            clsErrorDefs.HandleError = ErrorHandling.DontStop;
            clsErrorDefs.ReportError = ErrorReporting.PrintAll;
            MccDaq.ErrorInfo ULStat = MccDaq.MccService.ErrHandling
                (ErrorReporting.PrintAll, ErrorHandling.DontStop);

            //  This gives us access to text boxes via an indexed array
            _txtAOVolts = (new TextBox[] { this.txtAOVolts0, 
                this.txtAOVolts1, this.txtAOVolts2, this.txtAOVolts3 });

        }

#region Form initialization, variables, and entry point

        private Button btnUpdateDACs;
        private Button btnSendData;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private TextBox txtAOVolts0;
        private TextBox txtAOVolts1;
        private TextBox txtAOVolts2;
        private TextBox txtAOVolts3;
        private TextBox[] _txtAOVolts;
        private RadioButton rdioOnCommand;
        private RadioButton rdioImmediate;
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
            Application.Run(new frmAOut());
        }

        public frmAOut()
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

        /// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>

        private void InitializeComponent()
		{
            this.btnUpdateDACs = new System.Windows.Forms.Button();
            this.btnSendData = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtAOVolts0 = new System.Windows.Forms.TextBox();
            this.txtAOVolts1 = new System.Windows.Forms.TextBox();
            this.txtAOVolts2 = new System.Windows.Forms.TextBox();
            this.txtAOVolts3 = new System.Windows.Forms.TextBox();
            this.rdioOnCommand = new System.Windows.Forms.RadioButton();
            this.rdioImmediate = new System.Windows.Forms.RadioButton();
            this.lblInstruction = new System.Windows.Forms.Label();
            this.lblDemoFunction = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnUpdateDACs
            // 
            this.btnUpdateDACs.BackColor = System.Drawing.SystemColors.Control;
            this.btnUpdateDACs.Location = new System.Drawing.Point(192, 115);
            this.btnUpdateDACs.Name = "btnUpdateDACs";
            this.btnUpdateDACs.Size = new System.Drawing.Size(136, 31);
            this.btnUpdateDACs.TabIndex = 1;
            this.btnUpdateDACs.Text = "Update Outputs";
            this.btnUpdateDACs.UseVisualStyleBackColor = false;
            this.btnUpdateDACs.Click += new System.EventHandler(this.btnUpdateDACs_Click);
            // 
            // btnSendData
            // 
            this.btnSendData.BackColor = System.Drawing.SystemColors.Control;
            this.btnSendData.Location = new System.Drawing.Point(32, 115);
            this.btnSendData.Name = "btnSendData";
            this.btnSendData.Size = new System.Drawing.Size(136, 31);
            this.btnSendData.TabIndex = 2;
            this.btnSendData.Text = "Send Data";
            this.btnSendData.UseVisualStyleBackColor = false;
            this.btnSendData.Click += new System.EventHandler(this.btnSendData_Click);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label2.Location = new System.Drawing.Point(8, 155);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Channel 0";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label3.Location = new System.Drawing.Point(96, 155);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Channel 1";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label4.Location = new System.Drawing.Point(184, 155);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "Channel 2";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label5.Location = new System.Drawing.Point(272, 155);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 15);
            this.label5.TabIndex = 6;
            this.label5.Text = "Channel 3";
            // 
            // txtAOVolts0
            // 
            this.txtAOVolts0.Location = new System.Drawing.Point(8, 179);
            this.txtAOVolts0.Name = "txtAOVolts0";
            this.txtAOVolts0.Size = new System.Drawing.Size(72, 20);
            this.txtAOVolts0.TabIndex = 7;
            this.txtAOVolts0.Text = "0.00";
            // 
            // txtAOVolts1
            // 
            this.txtAOVolts1.Location = new System.Drawing.Point(96, 179);
            this.txtAOVolts1.Name = "txtAOVolts1";
            this.txtAOVolts1.Size = new System.Drawing.Size(72, 20);
            this.txtAOVolts1.TabIndex = 8;
            this.txtAOVolts1.Text = "0.00";
            // 
            // txtAOVolts2
            // 
            this.txtAOVolts2.Location = new System.Drawing.Point(184, 179);
            this.txtAOVolts2.Name = "txtAOVolts2";
            this.txtAOVolts2.Size = new System.Drawing.Size(72, 20);
            this.txtAOVolts2.TabIndex = 9;
            this.txtAOVolts2.Text = "0.00";
            // 
            // txtAOVolts3
            // 
            this.txtAOVolts3.Location = new System.Drawing.Point(272, 179);
            this.txtAOVolts3.Name = "txtAOVolts3";
            this.txtAOVolts3.Size = new System.Drawing.Size(72, 20);
            this.txtAOVolts3.TabIndex = 10;
            this.txtAOVolts3.Text = "0.00";
            // 
            // rdioOnCommand
            // 
            this.rdioOnCommand.Checked = true;
            this.rdioOnCommand.Location = new System.Drawing.Point(96, 219);
            this.rdioOnCommand.Name = "rdioOnCommand";
            this.rdioOnCommand.Size = new System.Drawing.Size(168, 23);
            this.rdioOnCommand.TabIndex = 11;
            this.rdioOnCommand.TabStop = true;
            this.rdioOnCommand.Text = "Update On Command";
            this.rdioOnCommand.CheckedChanged += new System.EventHandler(this.rdioOnCommand_CheckedChanged);
            // 
            // rdioImmediate
            // 
            this.rdioImmediate.Location = new System.Drawing.Point(96, 251);
            this.rdioImmediate.Name = "rdioImmediate";
            this.rdioImmediate.Size = new System.Drawing.Size(168, 23);
            this.rdioImmediate.TabIndex = 12;
            this.rdioImmediate.Text = "Update Immediately";
            this.rdioImmediate.CheckedChanged += new System.EventHandler(this.rdioImmediate_CheckedChanged);
            // 
            // lblInstruction
            // 
            this.lblInstruction.BackColor = System.Drawing.SystemColors.Window;
            this.lblInstruction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblInstruction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstruction.ForeColor = System.Drawing.Color.Red;
            this.lblInstruction.Location = new System.Drawing.Point(43, 60);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblInstruction.Size = new System.Drawing.Size(271, 43);
            this.lblInstruction.TabIndex = 23;
            this.lblInstruction.Text = "Board 0 must have analog outputs that support DacUpdate.";
            this.lblInstruction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblDemoFunction
            // 
            this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
            this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDemoFunction.Location = new System.Drawing.Point(12, 19);
            this.lblDemoFunction.Name = "lblDemoFunction";
            this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDemoFunction.Size = new System.Drawing.Size(337, 33);
            this.lblDemoFunction.TabIndex = 22;
            this.lblDemoFunction.Text = "Demonstration of MccBoard.AOut() with DacUpdate.";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // frmAOut
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(360, 304);
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.lblDemoFunction);
            this.Controls.Add(this.rdioImmediate);
            this.Controls.Add(this.rdioOnCommand);
            this.Controls.Add(this.txtAOVolts3);
            this.Controls.Add(this.txtAOVolts2);
            this.Controls.Add(this.txtAOVolts1);
            this.Controls.Add(this.txtAOVolts0);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSendData);
            this.Controls.Add(this.btnUpdateDACs);
            this.Name = "frmAOut";
            this.Text = "Universal Library Analog Output";
            this.Load += new System.EventHandler(this.frmAOut_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

#endregion

	}
}
