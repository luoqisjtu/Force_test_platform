// ==============================================================================
//
//  File:                         ULDI06.CS
//
//  Library Call Demonstrated:    MccDaq.MccBoard.DConfigBit()
//
//  Purpose:                      Reads the status of a single bit within a 
//                                digital port after configuring for input.
//
//  Demonstration:                Configures a single bit (within a digital port)
//                                for input (if programmable) and reads the bit status.
//
//  Other Library Calls:          MccDaq.MccBoard.DBitIn()
//                                MccDaq.MccService.ErrHandling()
//
//  Special Requirements:         Board 0 must have a digital port that supports
//                                input or bits that can be configured for input.
//
// ==============================================================================

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;

using DigitalIO;
using MccDaq;
using ErrorDefs;

namespace ULDI06
{
	public class frmDigIn : System.Windows.Forms.Form
	{

        // Create a new MccBoard object for Board 0
        MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

        int NumPorts, NumBits, FirstBit;
        int PortType, ProgAbility;

        MccDaq.DigitalPortType PortNum;
        MccDaq.DigitalPortDirection Direction;

        DigitalIO.clsDigitalIO DioProps = new DigitalIO.clsDigitalIO();

        private void frmDigIn_Load(object sender, EventArgs e)
        {

            string PortName, BitName;
            MccDaq.ErrorInfo ULStat;

            InitUL();

            //determine if digital port exists, its capabilities, etc
            PortType = clsDigitalIO.BITIN;
            NumPorts = DioProps.FindPortsOfType(DaqBoard, PortType, out ProgAbility,
                out PortNum, out NumBits, out FirstBit);
            if (! (ProgAbility == clsDigitalIO.PROGBIT))
                NumPorts = 0;

            if (NumPorts == 0)
            {
                lblInstruct.Text = "Board " + DaqBoard.BoardNum.ToString() + 
                    " has no compatible digital ports.";
            }
            else
            {
                // if programmable, set direction of bit to input
                // configure the first bit for digital input
                //  Parameters:
                //    PortNum        :the input port
                //    Direction      :sets the port for input or output

                Direction = MccDaq.DigitalPortDirection.DigitalIn;
                ULStat = DaqBoard.DConfigBit(PortNum, FirstBit, Direction);
                PortName = PortNum.ToString();
                BitName = FirstBit.ToString();
                lblInstruct.Text = "You may change the bit state by applying a TTL high " +
                "or a TTL low to the corresponding pin on " + PortName + ", bit " +
                BitName + " on board " + DaqBoard.BoardNum.ToString() + ".";
                tmrReadInputs.Enabled = true;
            }
        }

        private void tmrReadInputs_Tick(object eventSender, System.EventArgs eventArgs) /* Handles tmrReadInputs.Tick */
        {

            MccDaq.DigitalPortType BitPort;
            string PortName, BitName;

            tmrReadInputs.Stop();

            //  read a single bit status from the digital port

            //   Parameters:
            //     PortNum    :the digital I/O port type (must be
            //                 AUXPORT or FIRSTPORTA for bit read) 
            //     BitNum     :the bit to read
            //     BitValue   :the value read from the port

            BitPort = DigitalPortType.AuxPort;
            if (PortNum > BitPort) BitPort = DigitalPortType.FifthPortA;
            MccDaq.DigitalLogicState BitValue;
            MccDaq.ErrorInfo ULStat = DaqBoard.DBitIn(BitPort, FirstBit, out BitValue);

            PortName = BitPort.ToString();
            BitName = FirstBit.ToString();
            lblBitNum.Text = "The state of " + PortName + " bit " + BitName 
                + " is " + BitValue.ToString();

            tmrReadInputs.Start();

        }

        private void cmdStopRead_Click(object eventSender, System.EventArgs eventArgs) /* Handles cmdStopRead.Click */
        {
            tmrReadInputs.Enabled = false;
            Application.Exit();
        }

        private void InitUL()
        {

            MccDaq.ErrorInfo ULStat;

            //  Initiate error handling
            //   activating error handling will trap errors like
            //   bad channel numbers and non-configured conditions.
            //   Parameters:
            //     MccDaq.ErrorReporting.PrintAll :all warnings and errors encountered will be printed
            //     MccDaq.ErrorHandling.StopAll   :if an error is encountered, the program will stop

            clsErrorDefs.ReportError = MccDaq.ErrorReporting.PrintAll;
            clsErrorDefs.HandleError = MccDaq.ErrorHandling.StopAll;
            ULStat = MccDaq.MccService.ErrHandling
                (ErrorReporting.PrintAll, ErrorHandling.StopAll);

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
            this.cmdStopRead = new System.Windows.Forms.Button();
            this.tmrReadInputs = new System.Windows.Forms.Timer(this.components);
            this.lblBitNum = new System.Windows.Forms.Label();
            this.lblInstruct = new System.Windows.Forms.Label();
            this.lblDemoFunction = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdStopRead
            // 
            this.cmdStopRead.BackColor = System.Drawing.SystemColors.Control;
            this.cmdStopRead.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdStopRead.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdStopRead.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdStopRead.Location = new System.Drawing.Point(248, 184);
            this.cmdStopRead.Name = "cmdStopRead";
            this.cmdStopRead.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdStopRead.Size = new System.Drawing.Size(57, 33);
            this.cmdStopRead.TabIndex = 1;
            this.cmdStopRead.Text = "Quit";
            this.cmdStopRead.UseVisualStyleBackColor = false;
            this.cmdStopRead.Click += new System.EventHandler(this.cmdStopRead_Click);
            // 
            // tmrReadInputs
            // 
            this.tmrReadInputs.Interval = 200;
            this.tmrReadInputs.Tick += new System.EventHandler(this.tmrReadInputs_Tick);
            // 
            // lblBitNum
            // 
            this.lblBitNum.BackColor = System.Drawing.SystemColors.Window;
            this.lblBitNum.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblBitNum.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBitNum.ForeColor = System.Drawing.Color.Blue;
            this.lblBitNum.Location = new System.Drawing.Point(65, 136);
            this.lblBitNum.Name = "lblBitNum";
            this.lblBitNum.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblBitNum.Size = new System.Drawing.Size(197, 17);
            this.lblBitNum.TabIndex = 4;
            this.lblBitNum.Text = "Bit Number";
            this.lblBitNum.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblInstruct
            // 
            this.lblInstruct.BackColor = System.Drawing.SystemColors.Window;
            this.lblInstruct.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblInstruct.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstruct.ForeColor = System.Drawing.Color.Red;
            this.lblInstruct.Location = new System.Drawing.Point(15, 56);
            this.lblInstruct.Name = "lblInstruct";
            this.lblInstruct.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblInstruct.Size = new System.Drawing.Size(290, 49);
            this.lblInstruct.TabIndex = 3;
            this.lblInstruct.Text = "You may change the bit state by applying a TTL high or a TTL low to the correspon" +
                "ding pin on the port";
            this.lblInstruct.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblDemoFunction
            // 
            this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
            this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDemoFunction.Location = new System.Drawing.Point(12, 16);
            this.lblDemoFunction.Name = "lblDemoFunction";
            this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDemoFunction.Size = new System.Drawing.Size(302, 25);
            this.lblDemoFunction.TabIndex = 0;
            this.lblDemoFunction.Text = "Demonstration of MccDaq.MccBoard.DConfigBit()";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // frmDigIn
            // 
            this.AcceptButton = this.cmdStopRead;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(326, 232);
            this.Controls.Add(this.cmdStopRead);
            this.Controls.Add(this.lblBitNum);
            this.Controls.Add(this.lblInstruct);
            this.Controls.Add(this.lblDemoFunction);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Location = new System.Drawing.Point(7, 103);
            this.Name = "frmDigIn";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Universal Library Digital Bit Input";
            this.Load += new System.EventHandler(this.frmDigIn_Load);
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
			Application.Run(new frmDigIn());
		}

        public frmDigIn()
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
        public Button cmdStopRead;
        public Timer tmrReadInputs;
        public Label lblBitNum;
        public Label lblInstruct;
        public Label lblDemoFunction;

        #endregion

    }
}