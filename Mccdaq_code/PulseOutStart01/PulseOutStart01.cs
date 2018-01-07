// ==============================================================================
//
//  File:                         PulseOutStart01.CS
//
//  Library Call Demonstrated:    MccDaq.MccBoard.PulseOutStart()
//								  MccDaq.MccBoard.PulseOutStop()
//
//  Purpose:                      Controls an Output Timer Channel.
//
//  Demonstration:                Sends a frequency output to Timer 0.
//
//  Other Library Calls:          MccDaq.MccService.ErrHandling()
//
//  Special Requirements:         Board 0 must have a Timer output.
//
// ==============================================================================
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;

using MccDaq;

namespace PulseOutput
{
	public class frmPulseOutput : System.Windows.Forms.Form
	{

        int CounterType = Counters.clsCounters.CTRPULSE;
        int NumCtrs, CounterNum;

        // Create a new MccBoard object for Board 0
        MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

        Counters.clsCounters CtrProps = new Counters.clsCounters();

        private void frmPulseOutput_Load(object sender, EventArgs e)
            {
            InitUL();
            NumCtrs = CtrProps.FindCountersOfType(DaqBoard, CounterType, out CounterNum);
            if (NumCtrs == 0)
                {
                    lblUseScroll.Text = "Device " + 
                        DaqBoard.BoardNum.ToString() +
                        " has no pulse output devices.";
                    this.UpdateButton.Enabled = false;
                    this.txtDutyCycleToSet.Enabled = false;
                    this.txtFrequencyToSet.Enabled = false;
                }
                else
                    lblUseScroll.Text = "Enter a frequency and duty " +
                        "cycle within the timer's range and then " +
                        "click the Update Button. Verify the timer " +
                        "output on device " + DaqBoard.BoardNum.ToString() + ".";
            }

        private void UpdateButton_Click(object sender, System.EventArgs e)
        {
            bool IsValidNumber = true;
            double frequency = 1000000.0;
            double dutyCycle = .5;
            uint pulseCount = 0;
            double initialDelay = 0;
            IdleState idleState = IdleState.Low;
            PulseOutOptions options = PulseOutOptions.Default;

            try
            {
                frequency = double.Parse(txtFrequencyToSet.Text);
            }
            catch (Exception)
            {
                MessageBox.Show(txtFrequencyToSet.Text + " is not a valid frequency value", "Invalid Frequency ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                IsValidNumber = false;
            }

            try
            {
                dutyCycle = double.Parse(txtDutyCycleToSet.Text);
            }

            catch (Exception)
            {
                MessageBox.Show(txtDutyCycleToSet.Text + " is not a valid duty cycle value", "Invalid Duty Cycle ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                IsValidNumber = false;
            }

            if (IsValidNumber)
            {
                double frequencySet = frequency;
                double dutyCycleSet = dutyCycle;


                //  Parameters:
                //    TimerNum       :the timer output channel
                //    Frequency      :the frequency to output
                MccDaq.ErrorInfo ULStat = DaqBoard.PulseOutStart(0, ref frequency, ref dutyCycle, pulseCount, ref initialDelay, idleState, options);
                if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors)
                {
                    lblValueSent.Text = "The frequency sent to timer " 
                        + CounterNum.ToString() + " was:";
                    lblFrequency.Text = "The frequency output from timer channel " 
                        + CounterNum.ToString() + " is:";
                    lblShowValue.Text = frequencySet.ToString("0.0######") + " Hz";
                    lblShowFrequency.Text = frequency.ToString("0.0#####") + " Hz";

                    lblDCValueSent.Text = "The duty cycle sent to timer " 
                        + CounterNum.ToString() + " was:";
                    lblDutyCycle.Text = "The duty cycle output from timer channel " 
                        + CounterNum.ToString() + " is:";
                    lblDCShowValue.Text = dutyCycleSet.ToString("0.0#####");
                    lblShowDutyCycle.Text = dutyCycle.ToString("0.0#####");
                }
            }
        }

        private void cmdEndProgram_Click(object eventSender, System.EventArgs eventArgs)
        {
            if (NumCtrs > 0) DaqBoard.PulseOutStop(0);
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
            MccDaq.ErrorInfo ULStat = MccDaq.MccService.ErrHandling(MccDaq.ErrorReporting.PrintAll, MccDaq.ErrorHandling.StopAll);

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
            this.txtFrequencyToSet = new System.Windows.Forms.TextBox();
            this.lblShowFrequency = new System.Windows.Forms.Label();
            this.lblFrequency = new System.Windows.Forms.Label();
            this.lblShowValue = new System.Windows.Forms.Label();
            this.lblValueSent = new System.Windows.Forms.Label();
            this.lblUseScroll = new System.Windows.Forms.Label();
            this.lblDemoFunction = new System.Windows.Forms.Label();
            this.UpdateButton = new System.Windows.Forms.Button();
            this.txtDutyCycleToSet = new System.Windows.Forms.TextBox();
            this.lblFreq = new System.Windows.Forms.Label();
            this.lblDC = new System.Windows.Forms.Label();
            this.lblShowDutyCycle = new System.Windows.Forms.Label();
            this.lblDutyCycle = new System.Windows.Forms.Label();
            this.lblDCShowValue = new System.Windows.Forms.Label();
            this.lblDCValueSent = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdEndProgram
            // 
            this.cmdEndProgram.BackColor = System.Drawing.SystemColors.Control;
            this.cmdEndProgram.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdEndProgram.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdEndProgram.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdEndProgram.Location = new System.Drawing.Point(248, 280);
            this.cmdEndProgram.Name = "cmdEndProgram";
            this.cmdEndProgram.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdEndProgram.Size = new System.Drawing.Size(55, 26);
            this.cmdEndProgram.TabIndex = 5;
            this.cmdEndProgram.Text = "Quit";
            this.cmdEndProgram.UseVisualStyleBackColor = false;
            this.cmdEndProgram.Click += new System.EventHandler(this.cmdEndProgram_Click);
            // 
            // txtFrequencyToSet
            // 
            this.txtFrequencyToSet.AcceptsReturn = true;
            this.txtFrequencyToSet.BackColor = System.Drawing.SystemColors.Window;
            this.txtFrequencyToSet.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFrequencyToSet.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtFrequencyToSet.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFrequencyToSet.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtFrequencyToSet.Location = new System.Drawing.Point(120, 125);
            this.txtFrequencyToSet.MaxLength = 0;
            this.txtFrequencyToSet.Name = "txtFrequencyToSet";
            this.txtFrequencyToSet.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtFrequencyToSet.Size = new System.Drawing.Size(81, 20);
            this.txtFrequencyToSet.TabIndex = 0;
            this.txtFrequencyToSet.Text = "100000";
            // 
            // lblShowFrequency
            // 
            this.lblShowFrequency.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowFrequency.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowFrequency.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowFrequency.ForeColor = System.Drawing.Color.Blue;
            this.lblShowFrequency.Location = new System.Drawing.Point(257, 208);
            this.lblShowFrequency.Name = "lblShowFrequency";
            this.lblShowFrequency.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowFrequency.Size = new System.Drawing.Size(80, 17);
            this.lblShowFrequency.TabIndex = 6;
            // 
            // lblFrequency
            // 
            this.lblFrequency.BackColor = System.Drawing.SystemColors.Window;
            this.lblFrequency.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblFrequency.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFrequency.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblFrequency.Location = new System.Drawing.Point(8, 208);
            this.lblFrequency.Name = "lblFrequency";
            this.lblFrequency.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblFrequency.Size = new System.Drawing.Size(240, 17);
            this.lblFrequency.TabIndex = 7;
            this.lblFrequency.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblShowValue
            // 
            this.lblShowValue.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowValue.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowValue.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowValue.ForeColor = System.Drawing.Color.Blue;
            this.lblShowValue.Location = new System.Drawing.Point(257, 192);
            this.lblShowValue.Name = "lblShowValue";
            this.lblShowValue.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowValue.Size = new System.Drawing.Size(80, 17);
            this.lblShowValue.TabIndex = 4;
            // 
            // lblValueSent
            // 
            this.lblValueSent.BackColor = System.Drawing.SystemColors.Window;
            this.lblValueSent.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblValueSent.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblValueSent.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblValueSent.Location = new System.Drawing.Point(8, 192);
            this.lblValueSent.Name = "lblValueSent";
            this.lblValueSent.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblValueSent.Size = new System.Drawing.Size(240, 17);
            this.lblValueSent.TabIndex = 3;
            this.lblValueSent.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblUseScroll
            // 
            this.lblUseScroll.BackColor = System.Drawing.SystemColors.Window;
            this.lblUseScroll.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblUseScroll.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUseScroll.ForeColor = System.Drawing.Color.Red;
            this.lblUseScroll.Location = new System.Drawing.Point(43, 61);
            this.lblUseScroll.Name = "lblUseScroll";
            this.lblUseScroll.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblUseScroll.Size = new System.Drawing.Size(260, 51);
            this.lblUseScroll.TabIndex = 2;
            this.lblUseScroll.Text = "Enter a frequency and then a duty cycle within the timer\'s range and click Update" +
                " Button.";
            this.lblUseScroll.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblDemoFunction
            // 
            this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
            this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDemoFunction.Location = new System.Drawing.Point(11, 9);
            this.lblDemoFunction.Name = "lblDemoFunction";
            this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDemoFunction.Size = new System.Drawing.Size(334, 46);
            this.lblDemoFunction.TabIndex = 1;
            this.lblDemoFunction.Text = "Demonstration of MccDaq.MccBoard.PulseOutStart() and MccDaq.MccBoard.PulseOutStop" +
                "()";
            this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // UpdateButton
            // 
            this.UpdateButton.BackColor = System.Drawing.SystemColors.Control;
            this.UpdateButton.Location = new System.Drawing.Point(245, 136);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.Size = new System.Drawing.Size(75, 23);
            this.UpdateButton.TabIndex = 8;
            this.UpdateButton.Text = "Update";
            this.UpdateButton.UseVisualStyleBackColor = false;
            this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
            // 
            // txtDutyCycleToSet
            // 
            this.txtDutyCycleToSet.AcceptsReturn = true;
            this.txtDutyCycleToSet.BackColor = System.Drawing.SystemColors.Window;
            this.txtDutyCycleToSet.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDutyCycleToSet.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtDutyCycleToSet.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDutyCycleToSet.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtDutyCycleToSet.Location = new System.Drawing.Point(120, 152);
            this.txtDutyCycleToSet.MaxLength = 0;
            this.txtDutyCycleToSet.Name = "txtDutyCycleToSet";
            this.txtDutyCycleToSet.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtDutyCycleToSet.Size = new System.Drawing.Size(81, 20);
            this.txtDutyCycleToSet.TabIndex = 9;
            this.txtDutyCycleToSet.Text = ".5";
            // 
            // lblFreq
            // 
            this.lblFreq.Location = new System.Drawing.Point(40, 125);
            this.lblFreq.Name = "lblFreq";
            this.lblFreq.Size = new System.Drawing.Size(72, 16);
            this.lblFreq.TabIndex = 10;
            this.lblFreq.Text = "Frequency:";
            // 
            // lblDC
            // 
            this.lblDC.Location = new System.Drawing.Point(40, 152);
            this.lblDC.Name = "lblDC";
            this.lblDC.Size = new System.Drawing.Size(72, 16);
            this.lblDC.TabIndex = 11;
            this.lblDC.Text = "Duty Cycle:";
            // 
            // lblShowDutyCycle
            // 
            this.lblShowDutyCycle.BackColor = System.Drawing.SystemColors.Window;
            this.lblShowDutyCycle.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblShowDutyCycle.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowDutyCycle.ForeColor = System.Drawing.Color.Blue;
            this.lblShowDutyCycle.Location = new System.Drawing.Point(257, 248);
            this.lblShowDutyCycle.Name = "lblShowDutyCycle";
            this.lblShowDutyCycle.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblShowDutyCycle.Size = new System.Drawing.Size(80, 17);
            this.lblShowDutyCycle.TabIndex = 14;
            // 
            // lblDutyCycle
            // 
            this.lblDutyCycle.BackColor = System.Drawing.SystemColors.Window;
            this.lblDutyCycle.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDutyCycle.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDutyCycle.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDutyCycle.Location = new System.Drawing.Point(8, 248);
            this.lblDutyCycle.Name = "lblDutyCycle";
            this.lblDutyCycle.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDutyCycle.Size = new System.Drawing.Size(240, 17);
            this.lblDutyCycle.TabIndex = 15;
            this.lblDutyCycle.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblDCShowValue
            // 
            this.lblDCShowValue.BackColor = System.Drawing.SystemColors.Window;
            this.lblDCShowValue.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDCShowValue.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDCShowValue.ForeColor = System.Drawing.Color.Blue;
            this.lblDCShowValue.Location = new System.Drawing.Point(257, 232);
            this.lblDCShowValue.Name = "lblDCShowValue";
            this.lblDCShowValue.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDCShowValue.Size = new System.Drawing.Size(80, 17);
            this.lblDCShowValue.TabIndex = 13;
            // 
            // lblDCValueSent
            // 
            this.lblDCValueSent.BackColor = System.Drawing.SystemColors.Window;
            this.lblDCValueSent.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDCValueSent.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDCValueSent.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDCValueSent.Location = new System.Drawing.Point(8, 232);
            this.lblDCValueSent.Name = "lblDCValueSent";
            this.lblDCValueSent.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDCValueSent.Size = new System.Drawing.Size(240, 17);
            this.lblDCValueSent.TabIndex = 12;
            this.lblDCValueSent.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // frmPulseOutput
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(357, 318);
            this.Controls.Add(this.lblShowDutyCycle);
            this.Controls.Add(this.lblDutyCycle);
            this.Controls.Add(this.lblDCShowValue);
            this.Controls.Add(this.lblDCValueSent);
            this.Controls.Add(this.lblDC);
            this.Controls.Add(this.lblFreq);
            this.Controls.Add(this.txtDutyCycleToSet);
            this.Controls.Add(this.UpdateButton);
            this.Controls.Add(this.cmdEndProgram);
            this.Controls.Add(this.txtFrequencyToSet);
            this.Controls.Add(this.lblShowFrequency);
            this.Controls.Add(this.lblFrequency);
            this.Controls.Add(this.lblShowValue);
            this.Controls.Add(this.lblValueSent);
            this.Controls.Add(this.lblUseScroll);
            this.Controls.Add(this.lblDemoFunction);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Location = new System.Drawing.Point(7, 103);
            this.Name = "frmPulseOutput";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Universal Library Pulse Output ";
            this.Load += new System.EventHandler(this.frmPulseOutput_Load);
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
			Application.Run(new frmPulseOutput());
		}

        public frmPulseOutput()
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
        public Label lblShowFrequency;
        public Label lblFrequency;
        public Label lblShowValue;
        public Label lblValueSent;
        public Label lblUseScroll;
        public Label lblDemoFunction;
        public Button UpdateButton;
        public System.Windows.Forms.TextBox txtDutyCycleToSet;
        private System.Windows.Forms.Label lblFreq;
        private System.Windows.Forms.Label lblDC;
        public System.Windows.Forms.Label lblShowDutyCycle;
        public System.Windows.Forms.Label lblDutyCycle;
        public System.Windows.Forms.Label lblDCShowValue;
        public System.Windows.Forms.Label lblDCValueSent;
        public System.Windows.Forms.TextBox txtFrequencyToSet;
        #endregion
    }
}