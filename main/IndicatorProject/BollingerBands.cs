using System.Collections.Generic;
using System.Linq;

public class BollingerBands:InputBasedIndicator
{
    private IRIndex<double> input;
    private double enter;
    private int period;
    public IRIndex<double> ma;
    private IRIndex<double> stdev;
    public IRIndex<double> BollUP = new RIndexList<double>();
    public IRIndex<double> BollDown = new RIndexList<double>();

	public BollingerBands(IRIndex<double> timeseries, int period, double threshold )
	{
	    this.enter = threshold;
	    this.period = period;
	    this.input = timeseries;
        input.NewDataAction(ReCalc);
        ma = new SMA(input, period);
        stdev = new StDev(input, period);
	}

    public void ReCalc(double c)
    {
        BollUP.Add(ma + enter*stdev);
        BollDown.Add(ma - enter * stdev);

        if (input >ma + enter*stdev) vals.Add(-1);
        else if (input < ma - enter * stdev) vals.Add(1);
        else vals.Add(0);

        return;

    }
}

