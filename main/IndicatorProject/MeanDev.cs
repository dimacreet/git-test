using System;
using System.Collections.Generic;
using System.Linq;
using TradeFramework.ChartObjects;
using System.Drawing;

public class MeanDev: InputBasedIndicator
{
    private IRIndex<double> input;
    private int period;
    private IRIndex<double> ma;
    public IRIndex<double> stdev;
    private IRIndex<double> aver; 
    
    public IRIndex<double> diff = new RIndexList<double>(); 
	public MeanDev(IRIndex<double>timeseries,int period)
	{
	    this.input = timeseries;
	    this.period = period;
        ma=new SMA(input,period);
        stdev = new StDev(input,period);
        timeseries.NewDataAction(Recalc);
        diff.Add(0.0);

        aver = new SMA(diff,period);


	}

    public void Recalc(double c)
    {
        diff.Add(Math.Abs(c-input[1]));
        if (input.Count < period) return;
        //var std = HistVol.GetHistoricalStdevMA(ToL(input),ToL(ma), ma.Count-1, period-1);
        vals.Add((input-ma)/aver);

    }

    List<double> ToL(IRIndex<double> a)
    {
        List<double> l = new List<double>();
        for (int i = a.Count-1; i >=0;i-- )
            l.Add(a[-i]);
        return l;
    }
}


public class MaDevState : IndicatorState
{
    //Spearman parameters
    private int ma_period = 200;
    private double madev_threshold = 1.5;
    public IRIndex<double> ma_ratio = new RIndexList<double>();
    private IRIndex<double> stdev; 
    private IRIndex<double> madev;
    private IRIndex<double> spearman_ma;
    public IRIndex<double> BinSignal = new RIndexList<double>();
    private List<BarData> input = new List<BarData>(); 
    public MaDevState(StrategyBasePortfolio strategy, string Asset, TimeFrame TF)
        : base(strategy, Asset, TF)
    {
        Color = Color.Chartreuse;
        Name = "madev";
        madev = new MeanDev(strategy.TradeBarStreams[Asset][TF].Close, ma_period);
        stdev = new MeanDev(strategy.TradeBarStreams[Asset][TF].Close,ma_period).stdev;
        var n = strategy.TradeBarStreams[Asset][TF].Bars.Count;

        

        strategy.TradeBarStreams[Asset][TF].Bars.NewDataAction(madevCalc);
    }

    public override void ShowChart()
    {
        strategy.Charts[Asset][TF].chSeries("madev", new SMA(strategy.TradeBarStreams[Asset][TF].Close,ma_period), "", Color.Red);
        strategy.Charts[Asset][TF].chSeries("ma_ratio", ma_ratio, "madev", Color.Aqua, LineType.Solid, 8);
        //strategy.Charts[Asset][TF].chSeries("Sprm_high", spearman, "madev", Color.White).SetConst(spearman_high);
        //strategy.Charts[Asset][TF].chSeries("Sprm_low", spearman, "madev", Color.White).SetConst(spearman_low);
    }
    void madevCalc(BarData bar)
    {
        input.Add(bar);
        //var histvol =  HistVol.GetHistoricalVolatility(input, input.Count - 1, ma_period-1);
        //var histvol = HistVol.GetHistoricalStdevMA(input.Select(x=>x.Close).ToList(), ma,input.Count - 1, ma_period - 1);
        //etc.prn(histvol, madev[0]);
        ma_ratio.Add(madev*1.0);
        //etc.prn(ma_ratio[0]);
        if (madev/stdev > madev_threshold)
            inState = State.Overbougt;


        else if (madev / stdev < -madev_threshold)
            inState = State.Oversold;
            
        else
        {
            inState = State.Neutral;
            BinSignal.Add(double.NaN);
        }
        inSignal = StateSignal.Neutral;

    }
}
