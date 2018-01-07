// ==============================================================================
//
//  File:                         ULDO05.CS
//
//  Library Call Demonstrated:    MccDaq.MccBoard.DBitOut()
//
//  Purpose:                      Sets the state of a single digital output bit.
//
//  Demonstration:                Configures the first digital bit for output
//                                (if necessary) and writes a value to the bit.
//
//  Other Library Calls:          MccDaq.MccBoard.DConfigPort()
//                                MccDaq.MccService.ErrHandling()
//
//  Special Requirements:         Board 0 must have a digital output port
//                                or have digital ports programmable as output.
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

namespace ULDO05
{
	public class frmSetBitOut : System.Windows.Forms.Form
	{

        // Create a new MccBoard object for Board 0
        MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

        int NumPorts, NumBits, FirstBit;
        int PortType, ProgAbility;

        MccDaq.DigitalPortType PortNum;
        MccDaq.DigitalPortDirection Direction;
        public Label lblInstruct;
        public Label lblValueSet;

        DigitalIO.clsDigitalIO DioProps = new DigitalIO.clsDigitalIO();

        private void frmSetBitOut_Load(object sender, EventArgs e)
        {

            string PortName;
            MccDaq.ErrorInfo ULStat;

            InitUL();

            //determine if digital port exists, its capabilities, etc
            PortType = clsDigitalIO.PORTOUT;
            NumPorts = DioProps.FindPortsOfType(DaqBoard, PortType, out ProgAbility,
                out PortNum, out NumBits, out FirstBit);
            if (NumBits > 8) NumBits = 8;
            for (int I = NumBits; I < 8; ++I)
                chkSetBit[I].Visible = false;

            if (NumPorts == 0)
            {
                lblInstruct.Text = "Board " + DaqBoard.BoardNum.ToString() + 
                    " has no compatible digital ports.";
                for (int I = 0; I < 8; ++I)
                    chkSetBit[I].Enabled = false;
            }
            else
            {
                // if programmable, set direction of port to output
                // configure the first port for digital output
                //  Parameters:
                //    PortNum        :the output port
                //    Direction      :sets the port for input or output

                if (ProgAbility == clsDigitalIO.PROGPORT)
                {
                    Direction = MccDaq.DigitalPortDirection.DigitalOut;
                    ULStat = DaqBoard.DConfigPort(PortNum, Direction);
                }
                PortName = PortNum.ToString();
                lblInstruct.Text = "Set the output value of " + PortName +
                " bits on board " + DaqBoard.BoardNum.ToString() + 
                " using the check boxes.";
            }
        }

        private void chkSetBit_CheckStateChanged(object eventSender, System.EventArgs eventArgs)
        {

            string PortName;
            int BitNum = Array.IndexOf(chkSetBit, eventSender);
            MccDaq.DigitalPortType BitPort;

            MccDaq.DigitalLogicState BitValue = MccDaq.DigitalLogicState.Low;
            if (chkSetBit[BitNum].Checked)
                BitValue = MccDaq.DigitalLogicState.High;

            //the port must be AuxPort or FirstPortA for bit output
            BitPort = MccDaq.DigitalPortType.AuxPort;
            if (PortNum > MccDaq.DigitalPortType.AuxPort)
                BitPort = MccDaq.DigitalPortType.FirstPortA;

            MccDaq.ErrorInfo ULStat = DaqBoard.DBitOut(BitPort, FirstBit + BitNum, BitValue);

            PortName = BitPort.ToString();
            int BitSet = FirstBit + BitNum;
            lblValueSet.Text = PortName + ", bit " + BitSet.ToString()
                + " value set to " + BitValue.ToString();

        }

        private void cmdEndProgram_Click(object eventSender, System.EventArgs eventArgs)
        {
            ushort DataValue = 0;

            if (ProgAbility == clsDigitalIO.PROGPORT)
            {
                MccDaq.ErrorInfo ULStat = DaqBoard.DOut(PortNum, DataValue);

                Direction = MccDaq.DigitalPortDirection.DigitalIn;
                ULStat = DaqBoard.DConfigPort(PortNum, Direction);
            }

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
            this._chkSetBit_7 = new System.Windows.Forms.CheckBox();
            this._chkSetBit_3 = new System.Windows.Forms.CheckBox();
            this._chkSetBit_6 = new System.Windows.Forms.CheckBox();
            this._chkSetBit_2 = new System.Windows.Forms.CheckBox();
            this._chkSetBit_5 = new System.Windows.Forms.CheckBox();
            this._chkSetBit_1 = new System.Windows.Forms.CheckBox();
            this._chkSetBit_4 = new System.Windows.Forms.CheckBox();
            this._chkSetBit_0 = new System.Windows.Forms.CheckBox();
            this.lblDemoFunction = new System.Windows.Forms.Label();
            this.lblInstruct = new System.Windows.Forms.Label();
            this.lblValueSet = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdEndProgram
            // 
            this.cmdEndProgram.BackColor = System.Drawing.SystemColors.Control;
            this.cmdEndProgram.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdEndProgram.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdEndProgram.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdEndProgram.Location = new System.Drawing.Point(264, 260);
            this.cmdEndProgram.Name = "cmdEndProgram";
            this.cmdEndProgram.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdEndProgram.Size = new System.Drawing.Size(57, 25);
            this.cmdEndProgram.TabIndex = 9;
            this.cmdEndProgram.Text = "Quit";
            this.cmdEndProgram.UseVisualStyleBackColor = false;
            this.cmdEndProgram.Click += new System.EventHandler(this.cmdEndProgram_Click);
            // 
            // _chkSetBit_7
            // 
            this._chkSetBit_7.BackColor = System.Drawing.SystemColors.Window;
            this._chkSetBit_7.Checked = true;
            this._chkSetBit_7.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this._chkSetBit_7.Cursor = System.Windows.Forms.Cursors.Default;
            this._chkSetBit_7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._chkSetBit_7.ForeColor = System.Drawing.SystemColors.WindowText;
            this._chkSetBit_7.Location = new System.Drawing.Point(192, 181);
            this._chkSetBit_7.Name = "_chkSetBit_7";
            this._chkSetBit_7.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._chkSetBit_7.Size = new System.Drawing.Size(81, 17);
            this._chkSetBit_7.TabIndex = 2;
            this._chkSetBit_7.Text = "Set bit 7";
            this._chkSetBit_7.UseVisualStyleBackColor = false;
            this._chkSetBit_7.CheckStateChanged += new System.EventHandler(this.chkSetBit_CheckStateChanged);
            // 
            // _chkSetBit_3
            // 
            this._chkSetBit_3.BackColor = System.Drawing.SystemColors.Window;
            this._chkSetBit_3.Checked = true;
            this._chkSetBit_3.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this._chkSetBit_3.Cursor = System.Windows.Forms.Cursors.Default;
            this._chkSetBit_3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._chkSetBit_3.ForeColor = System.Drawing.SystemColors.WindowText;
            this._chkSetBit_3.Location = new System.Drawing.Point(48, 181);
            this._chkSetBit_3.Name = "_chkSetBit_3";
            this._chkSetBit_3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._chkSetBit_3.Size = new System.Drawing.Size(81, 17);
            this._chkSetBit_3.TabIndex = 6;
            this._chkSetBit_3.Text = "Set bit 3";
            this._chkSetBit_3.UseVisualStyleBackColor = false;
            this._chkSetBit_3.CheckStateChanged += new System.EventHandler(this.chkSetBit_CheckStateChanged);
            // 
            // _chkSetBit_6
            // 
            this._chkSetBit_6.BackColor = System.Drawing.SystemColors.Window;
            this._chkSetBit_6.Checked = true;
            this._chkSetBit_6.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this._chkSetBit_6.Cursor = System.Windows.Forms.Cursors.Default;
            this._chkSetBit_6.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._chkSetBit_6.ForeColor = System.Drawing.SystemColors.WindowText;
            this._chkSetBit_6.Location = new System.Drawing.Point(192, 157);
            this._chkSetBit_6.Name = "_chkSetBit_6";
            this._chkSetBit_6.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._chkSetBit_6.Size = new System.Drawing.Size(81, 17);
            this._chkSetBit_6.TabIndex = 3;
            this._chkSetBit_6.Text = "Set bit 6";
            this._chkSetBit_6.UseVisualStyleBackColor = false;
            this._chkSetBit_6.CheckStateChanged += new System.EventHandler(this.chkSetBit_CheckStateChanged);
            // 
            // _chkSetBit_2
            // 
            this._chkSetBit_2.BackColor = System.Drawing.SystemColors.Window;
            this._chkSetBit_2.Checked = true;
            this._chkSetBit_2.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this._chkSetBit_2.Cursor = System.Windows.Forms.Cursors.Default;
            this._chkSetBit_2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._chkSetBit_2.ForeColor = System.Drawing.SystemColors.WindowText;
            this._chkSetBit_2.Location = new System.Drawing.Point(48, 157);
            this._chkSetBit_2.Name = "_chkSetBit_2";
            this._chkSetBit_2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._chkSetBit_2.Size = new System.Drawing.Size(81, 17);
            this._chkSetBit_2.TabIndex = 7;
            this._chkSetBit_2.Text = "Set bit 2";
            this._chkSetBit_2.UseVisualStyleBackColor = false;
            this._chkSetBit_2.CheckStateChanged += new System.EventHandler(this.chkSetBit_CheckStateChanged);
            // 
            // _chkSetBit_5
            // 
            this._chkSetBit_5.BackColor = System.Drawing.SystemColors.Window;
            this._chkSetBit_5.Checked = true;
            this._chkSetBit_5.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this._chkSetBit_5.Cursor = System.Windows.Forms.Cursors.Default;
            this._chkSetBit_5.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._chkSetBit_5.ForeColor = System.Drawing.SystemColors.WindowText;
            this._chkSetBit_5.Location = new System.Drawing.Point(192, 133);
            this._chkSetBit_5.Name = "_chkSetBit_5";
            this._chkSetBit_5.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._chkSetBit_5.Size = new System.Drawing.Size(81, 17);
            this._chkSetBit_5.TabIndex = 4;
            this._chkSetBit_5.Text = "Set bit 5";
            this._chkSetBit_5.UseVisualStyleBackColor = false;
            this._chkSetBit_5.CheckStateChanged += new System.EventHandler(this.chkSetBit_CheckStateChanged);
            // 
            // _chkSetBit_1
            // 
            this._chkSetBit_1.BackColor = System.Drawing.SystemColors.Window;
            this._chkSetBit_1.Checked = true;
            this._chkSetBit_1.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this._chkSetBit_1.Cursor = System.Windows.Forms.Cursors.Default;
            this._chkSetBit_1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._chkSetBit_1.ForeColor = System.Drawing.SystemColors.WindowText;
            this._chkSetBit_1.Location = new System.Drawing.Point(48, 133);
            this._chkSetBit_1.Name = "_chkSetBit_1";
            this._chkSetBit_1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._chkSetBit_1.Size = new System.Drawing.Size(81, 17);
            this._chkSetBit_1.TabIndex = 8;
            this._chkSetBit_1.Text = "Set bit 1";
            this._chkSetBit_1.UseVisualStyleBackColor = false;
            this._chkSetBit_1.CheckStateChanged += new System.EventHandler(this.chkSetBit_CheckStateChanged);
            // 
            // _chkSetBit_4
            // 
            this._chkSetBit_4.BackColor = System.Drawing.SystemColors.Window;
            this._chkSetBit_4.Checked = true;
            this._chkSetBit_4.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this._chkSetBit_4.Cursor = System.Windows.Forms.Cursors.Default;
            this._chkSetBit_4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._chkSetBit_4.ForeColor = System.Drawing.SystemColors.WindowText;
            this._chkSetBit_4.Location = new System.Drawing.Point(192, 109);
            this._chkSetBit_4.Name = "_chkSetBit_4";
            this._chkSetBit_4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._chkSetBit_4.Size = new System.Drawing.Size(81, 17);
            this._chkSetBit_4.TabIndex = 5;
            this._chkSetBit_4.Text = "Set bit 4";
            this._chkSetBit_4.UseVisualStyleBackColor = false;
            this._chkSetBit_4.CheckStateChanged += new System.EventHandler(this.chkSetBit_CheckStateChanged);
            // 
            // _chkSetBit_0
            // 
            this._chkSetBit_0.BackColor = System.Drawing.SystemColors.Window;
            this._chkSetBit_0.Checked = true;
            this._chkSetBit_0.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this._chkSetBit_0.Cursor = System.Windows.Forms.Cursors.Default;
            this._chkSetBit_0.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._chkSetBit_0.ForeColor = System.Drawing.SystemColors.WindowText;
            this._chkSetBit_0.Location = new System.Drawing.Point(48, 109);
            this._chkSetBit_0.Name = "_chkSetBit_0";
            this._chkSetBit_0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._chkSetBit_0.Size = new System.Drawing.Size(81, 17);
            this._chkSetBit_0.TabIndex = 1;
            this._chkSetBit_0.Text = "Set bit 0";
            this._chkSetBit_0.UseVisualStyleBackColor = false;
            this._chkSetBit_0.CheckStateChanged += new System.EventHandler(this.chkSetBit_CheckStateChanged);
            // 
            // lblDemoFunction
            // 
            this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
            this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDemoFunction.Location = new System.Drawing.Point(16, 8);
            this.lblDemoFunction.Name = "lblDemoFunction";
            this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDemoFunction.Size = new System.Drawing.Size(305, 19);
            this.lblDemoFunction.TabIndex = 0;
            this.lblDemoFunction.Text = "Demonstration of MccDaq.MccBoard.DBitOut()";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblInstruct
            // 
            this.lblInstruct.BackColor = System.Drawing.SystemColors.Window;
            this.lblInstruct.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblInstruct.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstruct.ForeColor = System.Drawing.Color.Red;
            this.lblInstruct.Location = new System.Drawing.Point(17, 36);
            this.lblInstruct.Name = "lblInstruct";
            this.lblInstruct.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblInstruct.Size = new System.Drawing.Size(305, 52);
            this.lblInstruct.TabIndex = 10;
            this.lblInstruct.Text = "Monitor the bit values at digital output port.";
            this.lblInstruct.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblValueSet
            // 
            this.lblValueSet.BackColor = System.Drawing.SystemColors.Window;
            this.lblValueSet.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblValueSet.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblValueSet.ForeColor = System.Drawing.Color.Blue;
            this.lblValueSet.Location = new System.Drawing.Point(17, 215);
            this.lblValueSet.Name = "lblValueSet";
            this.lblValueSet.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblValueSet.Size = new System.Drawing.Size(305, 20);
            this.lblValueSet.TabIndex = 11;
            this.lblValueSet.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // frmSetBitOut
            // 
            this.AcceptButton = this.cmdEndProgram;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(339, 297);
            this.Controls.Add(this.lblValueSet);
            this.Controls.Add(this.lblInstruct);
            this.Controls.Add(this.cmdEndProgram);
            this.Controls.Add(this._chkSetBit_7);
            this.Controls.Add(this._chkSetBit_3);
            this.Controls.Add(this._chkSetBit_6);
            this.Controls.Add(this._chkSetBit_2);
            this.Controls.Add(this._chkSetBit_5);
            this.Controls.Add(this._chkSetBit_1);
            this.Controls.Add(this._chkSetBit_4);
            this.Controls.Add(this._chkSetBit_0);
            this.Controls.Add(this.lblDemoFunction);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Location = new System.Drawing.Point(7, 103);
            this.Name = "frmSetBitOut";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Universal Library Digital Bit Out";
            this.Load += new System.EventHandler(this.frmSetBitOut_Load);
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
			Application.Run(new frmSetBitOut());
		}

		public frmSetBitOut()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			chkSetBit = (new CheckBox[]{_chkSetBit_0, _chkSetBit_1, 
                _chkSetBit_2, _chkSetBit_3, _chkSetBit_4, _chkSetBit_5, 
                _chkSetBit_6, _chkSetBit_7});


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
        public CheckBox _chkSetBit_7;
        public CheckBox _chkSetBit_3;
        public CheckBox _chkSetBit_6;
        public CheckBox _chkSetBit_2;
        public CheckBox _chkSetBit_5;
        public CheckBox _chkSetBit_1;
        public CheckBox _chkSetBit_4;
        public CheckBox _chkSetBit_0;
        public Label lblDemoFunction;

        public CheckBox[] chkSetBit;

        #endregion

    }
}