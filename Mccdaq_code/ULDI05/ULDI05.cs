// ==============================================================================
//
//  File:                         ULDI05.CS
//
//  Library Call Demonstrated:    MccDaq.MccBoard.DBitIn()
//
//  Purpose:                      Reads the status of single digital input bits.
//
//  Demonstration:                Configures the first compatible port 
//                                for input (if necessary) and then
//                                reads and displays the bit values.
//
//  Other Library Calls:          MccDaq.MccService.ErrHandling()
//								  MccDaq.MccBoard.DConfigPort()
//
//  Special Requirements:         Board 0 must have a digital input port
//                                or have digital ports programmable as input.
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

namespace ULDI05
{
	public class frmDigIn : System.Windows.Forms.Form
	{

        // Create a new MccBoard object for Board 0
        MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

        int NumPorts, NumBits, FirstBit;
        int PortType, ProgAbility;
        string PortName;

        MccDaq.DigitalPortType PortNum;
        MccDaq.DigitalPortDirection Direction;

        DigitalIO.clsDigitalIO DioProps = new DigitalIO.clsDigitalIO();

        private void frmDigIn_Load(object sender, EventArgs e)
        {

            MccDaq.ErrorInfo ULStat;

            InitUL();

            //determine if digital port exists, its capabilities, etc
            PortType = clsDigitalIO.PORTIN;
            NumPorts = DioProps.FindPortsOfType(DaqBoard, PortType, out ProgAbility,
                out PortNum, out NumBits, out FirstBit);
            if (NumBits > 8)
                NumBits = 8;

            for (int i = NumBits; i < 8; ++i)
            {
                lblShowBitVal[i].Visible = false;
                lblShowBitNum[i].Visible = false;
            }

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
                lblBitNum.Text = "The first " + NumBits.ToString() + " bits are:";
                lblBitVal.Text = PortName + " value read:";
                tmrReadInputs.Enabled = true;
            }
        }

        private void tmrReadInputs_Tick(object eventSender, System.EventArgs eventArgs)
        {
            tmrReadInputs.Stop();
            string PortName;
            int LastBit;

            int BitNum;
            MccDaq.DigitalLogicState BitValue;
            MccDaq.DigitalPortType BitPort;

            BitPort = DigitalPortType.AuxPort;
            BitNum = FirstBit;
            if (PortNum > BitPort) BitPort = DigitalPortType.FirstPortA;
            for (int i = 0; i < NumBits; ++i)
            {
                //  read the bits of digital input and display

                //   Parameters:
                //     PortNum    :the type of port (must be AUXPORT
                //                 or FIRSTPORTA for bit input)
                //     BitNum     :the number of the bit to read from the port
                //     BitValue   :the value read from the port

                BitNum = FirstBit + i;
                MccDaq.ErrorInfo ULStat = DaqBoard.DBitIn(BitPort, BitNum, out BitValue);
                lblShowBitVal[i].Text = Convert.ToInt32(BitValue).ToString("0");

            }
            PortName = BitPort.ToString();
            LastBit = (FirstBit + NumBits) - 1;
            lblBitVal.Text = PortName + ", bit " + FirstBit.ToString() +
                " - " + LastBit.ToString() + " values:";
            tmrReadInputs.Start();
        }

        private void cmdStopRead_Click(object eventSender, System.EventArgs eventArgs)
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
            this._lblShowBitVal_0 = new System.Windows.Forms.Label();
            this._lblShowBitVal_1 = new System.Windows.Forms.Label();
            this._lblShowBitVal_2 = new System.Windows.Forms.Label();
            this._lblShowBitVal_3 = new System.Windows.Forms.Label();
            this._lblShowBitVal_4 = new System.Windows.Forms.Label();
            this._lblShowBitVal_5 = new System.Windows.Forms.Label();
            this._lblShowBitVal_6 = new System.Windows.Forms.Label();
            this._lblShowBitVal_7 = new System.Windows.Forms.Label();
            this.lblBitVal = new System.Windows.Forms.Label();
            this._lblShowBitNum_7 = new System.Windows.Forms.Label();
            this._lblShowBitNum_6 = new System.Windows.Forms.Label();
            this._lblShowBitNum_5 = new System.Windows.Forms.Label();
            this._lblShowBitNum_4 = new System.Windows.Forms.Label();
            this._lblShowBitNum_3 = new System.Windows.Forms.Label();
            this._lblShowBitNum_2 = new System.Windows.Forms.Label();
            this._lblShowBitNum_1 = new System.Windows.Forms.Label();
            this._lblShowBitNum_0 = new System.Windows.Forms.Label();
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
            this.cmdStopRead.Location = new System.Drawing.Point(366, 190);
            this.cmdStopRead.Name = "cmdStopRead";
            this.cmdStopRead.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdStopRead.Size = new System.Drawing.Size(57, 27);
            this.cmdStopRead.TabIndex = 20;
            this.cmdStopRead.Text = "Quit";
            this.cmdStopRead.UseVisualStyleBackColor = false;
            this.cmdStopRead.Click += new System.EventHandler(this.cmdStopRead_Click);
            // 
            // tmrReadInputs
            // 
            this.tmrReadInputs.Interval = 200;
            this.tmrReadInputs.Tick += new System.EventHandler(this.tmrReadInputs_Tick);
            // 
            // _lblShowBitVal_0
            // 
            this._lblShowBitVal_0.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitVal_0.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitVal_0.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitVal_0.ForeColor = System.Drawing.Color.Blue;
            this._lblShowBitVal_0.Location = new System.Drawing.Point(214, 144);
            this._lblShowBitVal_0.Name = "_lblShowBitVal_0";
            this._lblShowBitVal_0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitVal_0.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitVal_0.TabIndex = 1;
            this._lblShowBitVal_0.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowBitVal_1
            // 
            this._lblShowBitVal_1.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitVal_1.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitVal_1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitVal_1.ForeColor = System.Drawing.Color.Blue;
            this._lblShowBitVal_1.Location = new System.Drawing.Point(238, 144);
            this._lblShowBitVal_1.Name = "_lblShowBitVal_1";
            this._lblShowBitVal_1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitVal_1.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitVal_1.TabIndex = 2;
            this._lblShowBitVal_1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowBitVal_2
            // 
            this._lblShowBitVal_2.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitVal_2.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitVal_2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitVal_2.ForeColor = System.Drawing.Color.Blue;
            this._lblShowBitVal_2.Location = new System.Drawing.Point(262, 144);
            this._lblShowBitVal_2.Name = "_lblShowBitVal_2";
            this._lblShowBitVal_2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitVal_2.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitVal_2.TabIndex = 3;
            this._lblShowBitVal_2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowBitVal_3
            // 
            this._lblShowBitVal_3.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitVal_3.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitVal_3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitVal_3.ForeColor = System.Drawing.Color.Blue;
            this._lblShowBitVal_3.Location = new System.Drawing.Point(286, 144);
            this._lblShowBitVal_3.Name = "_lblShowBitVal_3";
            this._lblShowBitVal_3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitVal_3.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitVal_3.TabIndex = 4;
            this._lblShowBitVal_3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowBitVal_4
            // 
            this._lblShowBitVal_4.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitVal_4.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitVal_4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitVal_4.ForeColor = System.Drawing.Color.Blue;
            this._lblShowBitVal_4.Location = new System.Drawing.Point(334, 144);
            this._lblShowBitVal_4.Name = "_lblShowBitVal_4";
            this._lblShowBitVal_4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitVal_4.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitVal_4.TabIndex = 5;
            this._lblShowBitVal_4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowBitVal_5
            // 
            this._lblShowBitVal_5.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitVal_5.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitVal_5.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitVal_5.ForeColor = System.Drawing.Color.Blue;
            this._lblShowBitVal_5.Location = new System.Drawing.Point(358, 144);
            this._lblShowBitVal_5.Name = "_lblShowBitVal_5";
            this._lblShowBitVal_5.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitVal_5.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitVal_5.TabIndex = 6;
            this._lblShowBitVal_5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowBitVal_6
            // 
            this._lblShowBitVal_6.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitVal_6.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitVal_6.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitVal_6.ForeColor = System.Drawing.Color.Blue;
            this._lblShowBitVal_6.Location = new System.Drawing.Point(382, 144);
            this._lblShowBitVal_6.Name = "_lblShowBitVal_6";
            this._lblShowBitVal_6.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitVal_6.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitVal_6.TabIndex = 7;
            this._lblShowBitVal_6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowBitVal_7
            // 
            this._lblShowBitVal_7.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitVal_7.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitVal_7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitVal_7.ForeColor = System.Drawing.Color.Blue;
            this._lblShowBitVal_7.Location = new System.Drawing.Point(406, 144);
            this._lblShowBitVal_7.Name = "_lblShowBitVal_7";
            this._lblShowBitVal_7.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitVal_7.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitVal_7.TabIndex = 0;
            this._lblShowBitVal_7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblBitVal
            // 
            this.lblBitVal.BackColor = System.Drawing.SystemColors.Window;
            this.lblBitVal.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblBitVal.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBitVal.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblBitVal.Location = new System.Drawing.Point(12, 144);
            this.lblBitVal.Name = "lblBitVal";
            this.lblBitVal.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblBitVal.Size = new System.Drawing.Size(187, 17);
            this.lblBitVal.TabIndex = 8;
            this.lblBitVal.Text = "Bit Value:";
            this.lblBitVal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // _lblShowBitNum_7
            // 
            this._lblShowBitNum_7.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitNum_7.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitNum_7.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitNum_7.ForeColor = System.Drawing.SystemColors.WindowText;
            this._lblShowBitNum_7.Location = new System.Drawing.Point(406, 120);
            this._lblShowBitNum_7.Name = "_lblShowBitNum_7";
            this._lblShowBitNum_7.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitNum_7.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitNum_7.TabIndex = 17;
            this._lblShowBitNum_7.Text = "7";
            this._lblShowBitNum_7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowBitNum_6
            // 
            this._lblShowBitNum_6.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitNum_6.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitNum_6.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitNum_6.ForeColor = System.Drawing.SystemColors.WindowText;
            this._lblShowBitNum_6.Location = new System.Drawing.Point(382, 120);
            this._lblShowBitNum_6.Name = "_lblShowBitNum_6";
            this._lblShowBitNum_6.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitNum_6.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitNum_6.TabIndex = 16;
            this._lblShowBitNum_6.Text = "6";
            this._lblShowBitNum_6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowBitNum_5
            // 
            this._lblShowBitNum_5.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitNum_5.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitNum_5.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitNum_5.ForeColor = System.Drawing.SystemColors.WindowText;
            this._lblShowBitNum_5.Location = new System.Drawing.Point(358, 120);
            this._lblShowBitNum_5.Name = "_lblShowBitNum_5";
            this._lblShowBitNum_5.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitNum_5.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitNum_5.TabIndex = 15;
            this._lblShowBitNum_5.Text = "5";
            this._lblShowBitNum_5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowBitNum_4
            // 
            this._lblShowBitNum_4.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitNum_4.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitNum_4.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitNum_4.ForeColor = System.Drawing.SystemColors.WindowText;
            this._lblShowBitNum_4.Location = new System.Drawing.Point(334, 120);
            this._lblShowBitNum_4.Name = "_lblShowBitNum_4";
            this._lblShowBitNum_4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitNum_4.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitNum_4.TabIndex = 14;
            this._lblShowBitNum_4.Text = "4";
            this._lblShowBitNum_4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowBitNum_3
            // 
            this._lblShowBitNum_3.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitNum_3.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitNum_3.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitNum_3.ForeColor = System.Drawing.SystemColors.WindowText;
            this._lblShowBitNum_3.Location = new System.Drawing.Point(286, 120);
            this._lblShowBitNum_3.Name = "_lblShowBitNum_3";
            this._lblShowBitNum_3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitNum_3.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitNum_3.TabIndex = 13;
            this._lblShowBitNum_3.Text = "3";
            this._lblShowBitNum_3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowBitNum_2
            // 
            this._lblShowBitNum_2.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitNum_2.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitNum_2.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitNum_2.ForeColor = System.Drawing.SystemColors.WindowText;
            this._lblShowBitNum_2.Location = new System.Drawing.Point(262, 120);
            this._lblShowBitNum_2.Name = "_lblShowBitNum_2";
            this._lblShowBitNum_2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitNum_2.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitNum_2.TabIndex = 12;
            this._lblShowBitNum_2.Text = "2";
            this._lblShowBitNum_2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowBitNum_1
            // 
            this._lblShowBitNum_1.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitNum_1.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitNum_1.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitNum_1.ForeColor = System.Drawing.SystemColors.WindowText;
            this._lblShowBitNum_1.Location = new System.Drawing.Point(238, 120);
            this._lblShowBitNum_1.Name = "_lblShowBitNum_1";
            this._lblShowBitNum_1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitNum_1.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitNum_1.TabIndex = 11;
            this._lblShowBitNum_1.Text = "1";
            this._lblShowBitNum_1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // _lblShowBitNum_0
            // 
            this._lblShowBitNum_0.BackColor = System.Drawing.SystemColors.Window;
            this._lblShowBitNum_0.Cursor = System.Windows.Forms.Cursors.Default;
            this._lblShowBitNum_0.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblShowBitNum_0.ForeColor = System.Drawing.SystemColors.WindowText;
            this._lblShowBitNum_0.Location = new System.Drawing.Point(214, 120);
            this._lblShowBitNum_0.Name = "_lblShowBitNum_0";
            this._lblShowBitNum_0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._lblShowBitNum_0.Size = new System.Drawing.Size(17, 17);
            this._lblShowBitNum_0.TabIndex = 10;
            this._lblShowBitNum_0.Text = "0";
            this._lblShowBitNum_0.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblBitNum
            // 
            this.lblBitNum.BackColor = System.Drawing.SystemColors.Window;
            this.lblBitNum.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblBitNum.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBitNum.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblBitNum.Location = new System.Drawing.Point(12, 120);
            this.lblBitNum.Name = "lblBitNum";
            this.lblBitNum.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblBitNum.Size = new System.Drawing.Size(187, 17);
            this.lblBitNum.TabIndex = 9;
            this.lblBitNum.Text = "Bit Number:";
            this.lblBitNum.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblInstruct
            // 
            this.lblInstruct.BackColor = System.Drawing.SystemColors.Window;
            this.lblInstruct.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblInstruct.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstruct.ForeColor = System.Drawing.Color.Red;
            this.lblInstruct.Location = new System.Drawing.Point(55, 43);
            this.lblInstruct.Name = "lblInstruct";
            this.lblInstruct.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblInstruct.Size = new System.Drawing.Size(335, 53);
            this.lblInstruct.TabIndex = 19;
            this.lblInstruct.Text = "Input a TTL logic level at digital port inputs to change Bit Value:";
            this.lblInstruct.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblDemoFunction
            // 
            this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
            this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDemoFunction.Location = new System.Drawing.Point(37, 9);
            this.lblDemoFunction.Name = "lblDemoFunction";
            this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDemoFunction.Size = new System.Drawing.Size(375, 28);
            this.lblDemoFunction.TabIndex = 18;
            this.lblDemoFunction.Text = "Demonstration of MccDaq.MccBoard.DBitIn()";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // frmDigIn
            // 
            this.AcceptButton = this.cmdStopRead;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(450, 232);
            this.Controls.Add(this.cmdStopRead);
            this.Controls.Add(this._lblShowBitVal_0);
            this.Controls.Add(this._lblShowBitVal_1);
            this.Controls.Add(this._lblShowBitVal_2);
            this.Controls.Add(this._lblShowBitVal_3);
            this.Controls.Add(this._lblShowBitVal_4);
            this.Controls.Add(this._lblShowBitVal_5);
            this.Controls.Add(this._lblShowBitVal_6);
            this.Controls.Add(this._lblShowBitVal_7);
            this.Controls.Add(this.lblBitVal);
            this.Controls.Add(this._lblShowBitNum_7);
            this.Controls.Add(this._lblShowBitNum_6);
            this.Controls.Add(this._lblShowBitNum_5);
            this.Controls.Add(this._lblShowBitNum_4);
            this.Controls.Add(this._lblShowBitNum_3);
            this.Controls.Add(this._lblShowBitNum_2);
            this.Controls.Add(this._lblShowBitNum_1);
            this.Controls.Add(this._lblShowBitNum_0);
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

            lblShowBitNum = (new Label[]{_lblShowBitNum_0, _lblShowBitNum_1, 
                _lblShowBitNum_2, _lblShowBitNum_3, _lblShowBitNum_4, 
                _lblShowBitNum_5, _lblShowBitNum_6, _lblShowBitNum_7});

            lblShowBitVal = (new Label[]{_lblShowBitVal_0, _lblShowBitVal_1, 
                _lblShowBitVal_2, _lblShowBitVal_3, _lblShowBitVal_4, 
                _lblShowBitVal_5, _lblShowBitVal_6, _lblShowBitVal_7});

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
        public Label _lblShowBitVal_0;
        public Label _lblShowBitVal_1;
        public Label _lblShowBitVal_2;
        public Label _lblShowBitVal_3;
        public Label _lblShowBitVal_4;
        public Label _lblShowBitVal_5;
        public Label _lblShowBitVal_6;
        public Label _lblShowBitVal_7;
        public Label lblBitVal;
        public Label _lblShowBitNum_7;
        public Label _lblShowBitNum_6;
        public Label _lblShowBitNum_5;
        public Label _lblShowBitNum_4;
        public Label _lblShowBitNum_3;
        public Label _lblShowBitNum_2;
        public Label _lblShowBitNum_1;
        public Label _lblShowBitNum_0;
        public Label lblBitNum;
        public Label lblInstruct;
        public Label lblDemoFunction;

        public Label[] lblShowBitNum;
        public Label[] lblShowBitVal;

        #endregion

    }
}