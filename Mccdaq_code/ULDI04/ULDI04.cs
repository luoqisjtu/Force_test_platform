// ==============================================================================
//
//  File:                         ULDI04.CS
//
//  Library Call Demonstrated:    MccDaq.MccBoard.DIn()
//
//  Purpose:                      Reads a value from Digital Port.
//
//  Demonstration:                Read MccDaq.DigitalPortType
//
//  Other Library Calls:          MccDaq.MccService.ErrHandling()
//								  MccDaq.MccBoard.DConfigPort()
//
//  Special Requirements:         Board 0 must have a digital port
//								  or digital ports programmable as input.
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

namespace ULDI04
{
    public class frmDigAuxIn : System.Windows.Forms.Form
    {

        // Create a new MccBoard object for Board 0
        MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

        int NumPorts, NumBits, FirstBit;
        int PortType, ProgAbility;
        string PortName;

        MccDaq.DigitalPortType PortNum;
        MccDaq.DigitalPortDirection Direction;

        DigitalIO.clsDigitalIO DioProps = new DigitalIO.clsDigitalIO();

        private void frmDigAuxIn_Load(object sender, EventArgs e)
        {

            MccDaq.ErrorInfo ULStat;

            InitUL();

            //determine if digital port exists, its capabilities, etc
            PortType = clsDigitalIO.PORTIN;
            NumPorts = DioProps.FindPortsOfType(DaqBoard, PortType, out ProgAbility,
                out PortNum, out NumBits, out FirstBit);

            if (NumPorts == 0)
            {
                lblInstruct.Text = "Board " + DaqBoard.BoardNum.ToString() + 
                    " has no compatible digital ports.";
            }
            else
            {
                // if programmable, set direction of port to input
                // configure the first port for digital input
                //  Parameters:
                //    PortNum        :the input port
                //    Direction      :sets the port for input or output

                if (ProgAbility == clsDigitalIO.PROGPORT)
                {
                    Direction = MccDaq.DigitalPortDirection.DigitalIn;
                    ULStat = DaqBoard.DConfigPort(PortNum, Direction);
                }
                PortName = PortNum.ToString();
                lblInstruct.Text = "You may change the value read by applying " + 
                    "a TTL high or TTL low to digital inputs on " + PortName + 
                    " on board " + DaqBoard.BoardNum.ToString() + ".";
                lblPortVal.Text = PortName + " value read:";
                tmrReadInputs.Enabled = true;
            }
        }

        private void tmrReadInputs_Tick(object eventSender, System.EventArgs eventArgs)
        {
            tmrReadInputs.Stop();

            ushort DataValue;
            MccDaq.ErrorInfo ULStat = DaqBoard.DIn(PortNum, out DataValue);

            lblShowPortVal.Text = DataValue.ToString("0");

            tmrReadInputs.Start();
        }

        private void cmdEndProgram_Click(object eventSender, System.EventArgs eventArgs)
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
            this.cmdEndProgram = new System.Windows.Forms.Button();
            this.tmrReadInputs = new System.Windows.Forms.Timer(this.components);
            this.lblShowPortVal = new System.Windows.Forms.Label();
            this.lblPortVal = new System.Windows.Forms.Label();
            this.lblInstruct = new System.Windows.Forms.Label();
            this.lblDemoFunction = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdEndProgram
            // 
            this.cmdEndProgram.BackColor = System.Drawing.SystemColors.Control;
            this.cmdEndProgram.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdEndProgram.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdEndProgram.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdEndProgram.Location = new System.Drawing.Point(256, 200);
            this.cmdEndProgram.Name = "cmdEndProgram";
            this.cmdEndProgram.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdEndProgram.Size = new System.Drawing.Size(57, 33);
            this.cmdEndProgram.TabIndex = 3;
            this.cmdEndProgram.Text = "Quit";
            this.cmdEndProgram.UseVisualStyleBackColor = false;
            this.cmdEndProgram.Click += new System.EventHandler(this.cmdEndProgram_Click);
            // 
            // tmrReadInputs
            // 
            this.tmrReadInputs.Enabled = true;
            this.tmrReadInputs.Interval = 200;
            this.tmrReadInputs.Tick += new System.EventHandler(this.tmrReadInputs_Tick);
            // 
            // lblShowPortVal
            // 
            this.lblShowPortVal.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowPortVal.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowPortVal.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowPortVal.ForeColor = System.Drawing.Color.Blue;
            this.lblShowPortVal.Location = new System.Drawing.Point(236, 152);
            this.lblShowPortVal.Name = "lblShowPortVal";
            this.lblShowPortVal.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowPortVal.Size = new System.Drawing.Size(77, 17);
            this.lblShowPortVal.TabIndex = 2;
            // 
            // lblPortVal
            // 
            this.lblPortVal.BackColor = System.Drawing.SystemColors.Window;
            this.lblPortVal.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblPortVal.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPortVal.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblPortVal.Location = new System.Drawing.Point(48, 152);
            this.lblPortVal.Name = "lblPortVal";
            this.lblPortVal.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblPortVal.Size = new System.Drawing.Size(173, 17);
            this.lblPortVal.TabIndex = 1;
            this.lblPortVal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblInstruct
            // 
            this.lblInstruct.BackColor = System.Drawing.SystemColors.Window;
            this.lblInstruct.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblInstruct.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstruct.ForeColor = System.Drawing.Color.Red;
            this.lblInstruct.Location = new System.Drawing.Point(15, 42);
            this.lblInstruct.Name = "lblInstruct";
            this.lblInstruct.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblInstruct.Size = new System.Drawing.Size(298, 63);
            this.lblInstruct.TabIndex = 4;
            this.lblInstruct.Text = "Input a TTL high or low level to auxillary digital inputs to change Data Value.";
            this.lblInstruct.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblDemoFunction
            // 
            this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
            this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDemoFunction.Location = new System.Drawing.Point(12, 8);
            this.lblDemoFunction.Name = "lblDemoFunction";
            this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDemoFunction.Size = new System.Drawing.Size(307, 26);
            this.lblDemoFunction.TabIndex = 0;
            this.lblDemoFunction.Text = "Demonstration of MccDaq.MccBoard.DIn()";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // frmDigAuxIn
            // 
            this.AcceptButton = this.cmdEndProgram;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(331, 246);
            this.Controls.Add(this.cmdEndProgram);
            this.Controls.Add(this.lblShowPortVal);
            this.Controls.Add(this.lblPortVal);
            this.Controls.Add(this.lblInstruct);
            this.Controls.Add(this.lblDemoFunction);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Location = new System.Drawing.Point(7, 103);
            this.Name = "frmDigAuxIn";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Universal Library Digital In";
            this.Load += new System.EventHandler(this.frmDigAuxIn_Load);
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
            Application.Run(new frmDigAuxIn());
        }

        public frmDigAuxIn()
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
        public Button cmdEndProgram;
        public Timer tmrReadInputs;
        public Label lblShowPortVal;
        public Label lblPortVal;
        public Label lblInstruct;
        public Label lblDemoFunction;
        
#endregion

    }
}