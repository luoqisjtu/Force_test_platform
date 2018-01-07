// ==============================================================================

//  File:                         ULAI16.CS
//
//  Library Call Demonstrated:    Mccdaq.MccBoard.AInScan()in ShuntCal mode
//                               
//  Purpose:                      Executes the bridge nulling and shunt calibration
//                                procedure for a specified channel
//
//  Demonstration:                Displays the offset and gain adjustment factors.
//
//  Other Library Calls:          MccDaq.MccService.ErrHandling()                           
//
//  Special Requirements:         Board 0 must support bridge measurement and
//                                the shunt resistor is connected between
//                                AI+ and Ex- internally
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

namespace ULAI16
{
public class frmDataDisplay : System.Windows.Forms.Form
{
	// Required by the Windows Form Designer
	private System.ComponentModel.IContainer components;

	public ToolTip ToolTip1;
	public Button cmdStopConvert;
	public Button cmdStart;
	public Label Label1;
	public Label lblDemoFunction;

	public System.Windows.Forms.TextBox txtChan;
	public System.Windows.Forms.Label lblGainAdjustmentFactor;
	public System.Windows.Forms.Label lblGainFactor; 


	const int NumPoints = 1000;    //  Number of data points to collect
	const int FirstPoint = 0;     //  set first element in buffer to transfer to array

    private MccDaq.MccBoard DaqBoard;
    private double[] ADData = new double[NumPoints];
	private System.Windows.Forms.GroupBox groupBox1;
	public System.Windows.Forms.Label lblOffsetMeasStrain;
	public System.Windows.Forms.Label lblOffset;
	private System.Windows.Forms.GroupBox groupBox2;
	public System.Windows.Forms.Label lblGainMeas;
	public System.Windows.Forms.Label lblGainSim;
	public System.Windows.Forms.Label lblGainSimStrain;
	public System.Windows.Forms.Label lblGainMeasStrain; //  dimension an array to hold the input values
	private IntPtr MemHandle = IntPtr.Zero;	//  define a variable to contain the handle for memory allocated 
							//  by Windows through MccDaq.MccService.WinBufAlloc()

    private enum StrainConfig
    {
        FullBridgeI = 0, FullBridgeII, FullBridgeIII,
        HalfBridgeI, HalfBridgeII,
        QuarterBridgeI, QuarterBridgeII
    };

 
    public frmDataDisplay()
    {
        MccDaq.ErrorInfo ULStat;

        // This call is required by the Windows Form Designer.
        InitializeComponent();

		//  Initiate error handling
		//   activating error handling will trap errors like
		//   bad channel numbers and non-configured conditions.
		//   Parameters:
		//     MccDaq.ErrorReporting.PrintAll :all warnings and errors encountered will be printed
		//     MccDaq.ErrorHandling.StopAll   :if an error is encountered, the program will stop
		
		ULStat = MccDaq.MccService.ErrHandling(MccDaq.ErrorReporting.PrintAll, MccDaq.ErrorHandling.StopAll);
		

        // Create a new MccBoard object for Board 0
        DaqBoard = new MccDaq.MccBoard(0);

		// Allocate memory buffer to hold data..
		MemHandle = MccDaq.MccService.ScaledWinBufAllocEx(NumPoints); //  set aside memory to hold data
	}


    // Form overrides dispose to clean up the component list.
    protected override void  Dispose(bool Disposing)
    {
        if (Disposing)
        {
            if (components != null)
            {
                components.Dispose();
            }

			// be sure to release the memory buffer... 
			if ( MemHandle != IntPtr.Zero)
				MccDaq.MccService.WinBufFreeEx(MemHandle);
        }
        base.Dispose(Disposing);
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
        this.txtChan = new System.Windows.Forms.TextBox();
        this.cmdStopConvert = new System.Windows.Forms.Button();
        this.cmdStart = new System.Windows.Forms.Button();
        this.Label1 = new System.Windows.Forms.Label();
        this.lblGainAdjustmentFactor = new System.Windows.Forms.Label();
        this.lblGainFactor = new System.Windows.Forms.Label();
        this.lblOffsetMeasStrain = new System.Windows.Forms.Label();
        this.lblOffset = new System.Windows.Forms.Label();
        this.lblDemoFunction = new System.Windows.Forms.Label();
        this.groupBox1 = new System.Windows.Forms.GroupBox();
        this.groupBox2 = new System.Windows.Forms.GroupBox();
        this.lblGainSimStrain = new System.Windows.Forms.Label();
        this.lblGainSim = new System.Windows.Forms.Label();
        this.lblGainMeasStrain = new System.Windows.Forms.Label();
        this.lblGainMeas = new System.Windows.Forms.Label();
        this.groupBox1.SuspendLayout();
        this.groupBox2.SuspendLayout();
        this.SuspendLayout();
        // 
        // txtChan
        // 
        this.txtChan.AcceptsReturn = true;
        this.txtChan.BackColor = System.Drawing.SystemColors.Window;
        this.txtChan.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtChan.Cursor = System.Windows.Forms.Cursors.IBeam;
        this.txtChan.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.txtChan.ForeColor = System.Drawing.SystemColors.WindowText;
        this.txtChan.Location = new System.Drawing.Point(143, 64);
        this.txtChan.MaxLength = 0;
        this.txtChan.Name = "txtChan";
        this.txtChan.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.txtChan.Size = new System.Drawing.Size(33, 20);
        this.txtChan.TabIndex = 20;
        this.txtChan.Text = "0";
        this.txtChan.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        // 
        // cmdStopConvert
        // 
        this.cmdStopConvert.BackColor = System.Drawing.SystemColors.Control;
        this.cmdStopConvert.Cursor = System.Windows.Forms.Cursors.Default;
        this.cmdStopConvert.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.cmdStopConvert.ForeColor = System.Drawing.SystemColors.ControlText;
        this.cmdStopConvert.Location = new System.Drawing.Point(280, 296);
        this.cmdStopConvert.Name = "cmdStopConvert";
        this.cmdStopConvert.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.cmdStopConvert.Size = new System.Drawing.Size(58, 26);
        this.cmdStopConvert.TabIndex = 17;
        this.cmdStopConvert.Text = "Quit";
        this.cmdStopConvert.UseVisualStyleBackColor = false;
        this.cmdStopConvert.Click += new System.EventHandler(this.cmdStopConvert_Click);
        // 
        // cmdStart
        // 
        this.cmdStart.BackColor = System.Drawing.SystemColors.Control;
        this.cmdStart.Cursor = System.Windows.Forms.Cursors.Default;
        this.cmdStart.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.cmdStart.ForeColor = System.Drawing.SystemColors.ControlText;
        this.cmdStart.Location = new System.Drawing.Point(208, 296);
        this.cmdStart.Name = "cmdStart";
        this.cmdStart.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.cmdStart.Size = new System.Drawing.Size(58, 26);
        this.cmdStart.TabIndex = 18;
        this.cmdStart.Text = "Start";
        this.cmdStart.UseVisualStyleBackColor = false;
        this.cmdStart.Click += new System.EventHandler(this.cmdStart_Click);
        // 
        // Label1
        // 
        this.Label1.BackColor = System.Drawing.SystemColors.Window;
        this.Label1.Cursor = System.Windows.Forms.Cursors.Default;
        this.Label1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.Label1.ForeColor = System.Drawing.SystemColors.WindowText;
        this.Label1.Location = new System.Drawing.Point(93, 66);
        this.Label1.Name = "Label1";
        this.Label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.Label1.Size = new System.Drawing.Size(51, 17);
        this.Label1.TabIndex = 19;
        this.Label1.Text = "Channel:";
        // 
        // lblGainAdjustmentFactor
        // 
        this.lblGainAdjustmentFactor.BackColor = System.Drawing.SystemColors.Window;
        this.lblGainAdjustmentFactor.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblGainAdjustmentFactor.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblGainAdjustmentFactor.ForeColor = System.Drawing.Color.Blue;
        this.lblGainAdjustmentFactor.Location = new System.Drawing.Point(136, 80);
        this.lblGainAdjustmentFactor.Name = "lblGainAdjustmentFactor";
        this.lblGainAdjustmentFactor.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblGainAdjustmentFactor.Size = new System.Drawing.Size(96, 17);
        this.lblGainAdjustmentFactor.TabIndex = 10;
        // 
        // lblGainFactor
        // 
        this.lblGainFactor.BackColor = System.Drawing.SystemColors.Window;
        this.lblGainFactor.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblGainFactor.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblGainFactor.ForeColor = System.Drawing.SystemColors.WindowText;
        this.lblGainFactor.Location = new System.Drawing.Point(8, 80);
        this.lblGainFactor.Name = "lblGainFactor";
        this.lblGainFactor.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblGainFactor.Size = new System.Drawing.Size(128, 17);
        this.lblGainFactor.TabIndex = 2;
        this.lblGainFactor.Text = "Gain Adjustment Factor:";
        // 
        // lblOffsetMeasStrain
        // 
        this.lblOffsetMeasStrain.BackColor = System.Drawing.SystemColors.Window;
        this.lblOffsetMeasStrain.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblOffsetMeasStrain.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblOffsetMeasStrain.ForeColor = System.Drawing.Color.Blue;
        this.lblOffsetMeasStrain.Location = new System.Drawing.Point(112, 32);
        this.lblOffsetMeasStrain.Name = "lblOffsetMeasStrain";
        this.lblOffsetMeasStrain.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblOffsetMeasStrain.Size = new System.Drawing.Size(128, 17);
        this.lblOffsetMeasStrain.TabIndex = 9;
        // 
        // lblOffset
        // 
        this.lblOffset.BackColor = System.Drawing.SystemColors.Window;
        this.lblOffset.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblOffset.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblOffset.ForeColor = System.Drawing.SystemColors.WindowText;
        this.lblOffset.Location = new System.Drawing.Point(16, 32);
        this.lblOffset.Name = "lblOffset";
        this.lblOffset.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblOffset.Size = new System.Drawing.Size(90, 17);
        this.lblOffset.TabIndex = 1;
        this.lblOffset.Text = "Measured Strain:";
        // 
        // lblDemoFunction
        // 
        this.lblDemoFunction.BackColor = System.Drawing.SystemColors.Window;
        this.lblDemoFunction.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblDemoFunction.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblDemoFunction.ForeColor = System.Drawing.SystemColors.WindowText;
        this.lblDemoFunction.Location = new System.Drawing.Point(8, 8);
        this.lblDemoFunction.Name = "lblDemoFunction";
        this.lblDemoFunction.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblDemoFunction.Size = new System.Drawing.Size(337, 41);
        this.lblDemoFunction.TabIndex = 0;
        this.lblDemoFunction.Text = "Demonstration of the bridge nulling and shunt calibration procedure for a specifi" +
            "ed channel  ";
        this.lblDemoFunction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // groupBox1
        // 
        this.groupBox1.Controls.Add(this.lblOffsetMeasStrain);
        this.groupBox1.Controls.Add(this.lblOffset);
        this.groupBox1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.groupBox1.ForeColor = System.Drawing.Color.DarkBlue;
        this.groupBox1.Location = new System.Drawing.Point(8, 96);
        this.groupBox1.Name = "groupBox1";
        this.groupBox1.Size = new System.Drawing.Size(328, 64);
        this.groupBox1.TabIndex = 21;
        this.groupBox1.TabStop = false;
        this.groupBox1.Text = "Offset Adjustment";
        // 
        // groupBox2
        // 
        this.groupBox2.Controls.Add(this.lblGainSimStrain);
        this.groupBox2.Controls.Add(this.lblGainSim);
        this.groupBox2.Controls.Add(this.lblGainMeasStrain);
        this.groupBox2.Controls.Add(this.lblGainMeas);
        this.groupBox2.Controls.Add(this.lblGainAdjustmentFactor);
        this.groupBox2.Controls.Add(this.lblGainFactor);
        this.groupBox2.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.groupBox2.ForeColor = System.Drawing.Color.DarkBlue;
        this.groupBox2.Location = new System.Drawing.Point(8, 168);
        this.groupBox2.Name = "groupBox2";
        this.groupBox2.Size = new System.Drawing.Size(328, 112);
        this.groupBox2.TabIndex = 22;
        this.groupBox2.TabStop = false;
        this.groupBox2.Text = "Gain Adjustment";
        // 
        // lblGainSimStrain
        // 
        this.lblGainSimStrain.BackColor = System.Drawing.SystemColors.Window;
        this.lblGainSimStrain.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblGainSimStrain.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblGainSimStrain.ForeColor = System.Drawing.Color.Blue;
        this.lblGainSimStrain.Location = new System.Drawing.Point(104, 32);
        this.lblGainSimStrain.Name = "lblGainSimStrain";
        this.lblGainSimStrain.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblGainSimStrain.Size = new System.Drawing.Size(128, 17);
        this.lblGainSimStrain.TabIndex = 14;
        // 
        // lblGainSim
        // 
        this.lblGainSim.BackColor = System.Drawing.SystemColors.Window;
        this.lblGainSim.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblGainSim.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblGainSim.ForeColor = System.Drawing.SystemColors.WindowText;
        this.lblGainSim.Location = new System.Drawing.Point(8, 32);
        this.lblGainSim.Name = "lblGainSim";
        this.lblGainSim.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblGainSim.Size = new System.Drawing.Size(88, 17);
        this.lblGainSim.TabIndex = 13;
        this.lblGainSim.Text = "Simulated Strain:";
        // 
        // lblGainMeasStrain
        // 
        this.lblGainMeasStrain.BackColor = System.Drawing.SystemColors.Window;
        this.lblGainMeasStrain.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblGainMeasStrain.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblGainMeasStrain.ForeColor = System.Drawing.Color.Blue;
        this.lblGainMeasStrain.Location = new System.Drawing.Point(104, 56);
        this.lblGainMeasStrain.Name = "lblGainMeasStrain";
        this.lblGainMeasStrain.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblGainMeasStrain.Size = new System.Drawing.Size(128, 17);
        this.lblGainMeasStrain.TabIndex = 12;
        // 
        // lblGainMeas
        // 
        this.lblGainMeas.BackColor = System.Drawing.SystemColors.Window;
        this.lblGainMeas.Cursor = System.Windows.Forms.Cursors.Default;
        this.lblGainMeas.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblGainMeas.ForeColor = System.Drawing.SystemColors.WindowText;
        this.lblGainMeas.Location = new System.Drawing.Point(8, 56);
        this.lblGainMeas.Name = "lblGainMeas";
        this.lblGainMeas.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.lblGainMeas.Size = new System.Drawing.Size(90, 17);
        this.lblGainMeas.TabIndex = 11;
        this.lblGainMeas.Text = "Measured Strain:";
        // 
        // frmDataDisplay
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
        this.BackColor = System.Drawing.SystemColors.Window;
        this.ClientSize = new System.Drawing.Size(349, 334);
        this.Controls.Add(this.txtChan);
        this.Controls.Add(this.cmdStopConvert);
        this.Controls.Add(this.cmdStart);
        this.Controls.Add(this.Label1);
        this.Controls.Add(this.lblDemoFunction);
        this.Controls.Add(this.groupBox1);
        this.Controls.Add(this.groupBox2);
        this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ForeColor = System.Drawing.Color.Blue;
        this.Location = new System.Drawing.Point(190, 108);
        this.Name = "frmDataDisplay";
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        this.Text = "Universal Library Analog Input Scan";
        this.groupBox1.ResumeLayout(false);
        this.groupBox2.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();

	}

#endregion

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() 
    {
        Application.Run(new frmDataDisplay());
    }
    

    private void cmdStart_Click(object eventSender, System.EventArgs eventArgs) 
    {
        int i;
        MccDaq.ErrorInfo ULStat;
        MccDaq.Range Range;
        MccDaq.ScanOptions Options;
        int Rate;
        int Count;
		int Chan;

        StrainConfig StrainConfiguration = StrainConfig.QuarterBridgeI;

		double InitialVoltage = 0.0;	//Bridge output voltage in the unloaded condition. This value is subtracted from any measurements before scaling equations are applied. 		
		double VInitial = 0.0;
		double OffsetAdjustmentFactor = 0.0;
		double GainAdjustmentFactor = 0.0;
		double Total = 0.0;
		double VOffset = 0.0;
		double RShunt = 100000;			// Resistance of Shunt Resistor
		double RGage = 350;				// Gage Resistance 
		double VExcitation = 2.5;		// Excitation voltage
		double GageFactor = 2;
        double PoissonRatio = 0;
		double VActualBridge;			// Actual bridge voltage
		double REffective;				// Effective resistance
		double VSimulatedBridge;		// Simulated bridge voltage
		double MeasuredStrain;
		double SimulatedStrain;

        cmdStart.Enabled = false;

		// Calculate the offset adjusment factor on a resting gage in software

        //  Collect the values by calling MccDaq.MccBoard.AInScan function
        //  Parameters:
        //    LowChan    :the first channel of the scan
        //    HighChan   :the last channel of the scan
        //    Count      :the total number of A/D samples to collect
        //    Rate       :sample rate
        //    Range      :the range for the board
        //    MemHandle  :Handle for Windows buffer to store data in
        //    Options    :data collection options
      
        Chan = int.Parse(txtChan.Text); // channel to acquire
        if ((Chan > 3)) Chan = 3;
        txtChan.Text = Chan.ToString();

		VInitial = InitialVoltage / VExcitation;

        Count = NumPoints;	//  total number of data points to collect
        Rate = 1000;			//  per channel sampling rate ((samples per second) per channel)

		//  return scaled data
		Options = MccDaq.ScanOptions.ScaleData;
		Range = MccDaq.Range.NotUsed; // set the range
        
        ULStat = DaqBoard.AInScan( Chan, Chan, Count, ref Rate, Range, MemHandle, Options);

		//  Transfer the data from the memory buffer set up by Windows to an array
		ULStat = MccDaq.MccService.ScaledWinBufToArray( MemHandle, ADData, FirstPoint, Count);
		
		for (i = 0; i < NumPoints; i++)
			Total = Total + ADData [i];

		VOffset = Total / Count; 

		VOffset = VOffset - VInitial;

        OffsetAdjustmentFactor = CalculateStrain(StrainConfiguration, VOffset, GageFactor, PoissonRatio);

		lblOffsetMeasStrain.Text = OffsetAdjustmentFactor.ToString("F9");

		//	Enable Shunt Calibration Circuit and Collect the values and
		//  Calculate the Actual Bridge Voltage

		Options = MccDaq.ScanOptions.ScaleData | MccDaq.ScanOptions.ShuntCal;
		ULStat = DaqBoard.AInScan( Chan, Chan, Count, ref Rate, Range, MemHandle, Options);

		//  Transfer the data from the memory buffer set up by Windows to an array
		ULStat = MccDaq.MccService.ScaledWinBufToArray( MemHandle, ADData, FirstPoint, Count);

		Total = 0.0;

		for ( i = 0; i < Count; i++)
			Total = Total + ADData [i];

		VActualBridge = Total / Count;

		VActualBridge = VActualBridge - VInitial;

        MeasuredStrain = CalculateStrain(StrainConfiguration, VActualBridge, GageFactor, PoissonRatio);

		lblGainMeasStrain.Text = MeasuredStrain.ToString("F9");

		// Calculate the Simulated Bridge Strain with a shunt resistor

		REffective = (RGage * RShunt)/(RGage + RShunt);

		VSimulatedBridge =(REffective / (REffective + RGage) - 0.5); 

        SimulatedStrain = CalculateStrain(StrainConfiguration, VSimulatedBridge, GageFactor, PoissonRatio);
	
		lblGainSimStrain.Text = SimulatedStrain.ToString("F9");

		GainAdjustmentFactor = SimulatedStrain / (MeasuredStrain - OffsetAdjustmentFactor);

		lblGainAdjustmentFactor.Text = GainAdjustmentFactor.ToString("F9");
       
        cmdStart.Enabled = true;
    }

    private double CalculateStrain(StrainConfig StrainCfg, double U, double GageFactor, double PoissonRatio)
    {
        double starin = 0;
        switch (StrainCfg)
        {
            case StrainConfig.FullBridgeI:
                starin = (-U) / GageFactor;
                break;
            case StrainConfig.FullBridgeII:
                starin = (-2 * U) / (GageFactor * (1 + PoissonRatio));
                break;
            case StrainConfig.FullBridgeIII:
                starin = (-2 * U) / (GageFactor * ((PoissonRatio + 1) - (U * (PoissonRatio - 1))));
                break;
            case StrainConfig.HalfBridgeI:
                starin = (-4 * U) / (GageFactor * ((PoissonRatio + 1) - 2 * U * (PoissonRatio - 1)));
                break;
            case StrainConfig.HalfBridgeII:
                starin = (-2 * U) / GageFactor;
                break;
            case StrainConfig.QuarterBridgeI:
            case StrainConfig.QuarterBridgeII:
                starin = (-4 * U) / (GageFactor * ((1 + 2 * U)));
                break;
        }

        return starin;
    }


    private void cmdStopConvert_Click(object eventSender, System.EventArgs eventArgs)
    {
        MccDaq.ErrorInfo ULStat;

        ULStat = MccDaq.MccService.WinBufFreeEx(MemHandle); //  Free up memory for use by
														  //  other programs
		MemHandle = IntPtr.Zero;

        Application.Exit();
    }

}
}