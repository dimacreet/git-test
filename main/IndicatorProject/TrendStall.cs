using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeFramework.ChartObjects;

public class TrendStallState : IndicatorState
{
    //TrendStall parameters
    private TrendStall trendstall;

    public TrendStallState(StrategyBasePortfolio strategy, string Asset,
                           TimeFrame TF = TimeFrame.D)
        : base(strategy, Asset, TF)
    {
        Name = "TrendStall";
        Color = Color.BlueViolet;
        trendstall = new TrendStall(strategy.TradeBarStreams[Asset][TF].Bars);
    }

    private chSeries TRArea;

    public override void ShowChart()
    {

        TRArea = strategy.Charts[Asset][TF].chSeries("TR1", "", Color.Aqua, LineType.Filled, 8);

        trendstall.BinStates.NewData += x =>
            {
                if (x == 1)
                    TRArea.SetRange(strategy.TradeBarStreams[Asset][TF].Low.val,
                                    strategy.TradeBarStreams[Asset][TF].High.val);
            };


        //TRArea = strategy.Charts[Asset][TF].chSeries("TR2", "", Color.Red, LineType.Filled, 8);
        trendstall.BinSignals.NewData += x =>
            {
                if (x == 1)
                strategy.Charts[Asset][TF].AddArrowUp(strategy.TradeBarStreams[Asset][TF].Dates.val, strategy.TradeBarStreams[Asset][TF].Close[0],
                                                           Color.Red);
                //TRArea.SetRange(strategy.TradeBarStreams[Asset][TF].Low.val,
                //strategy.TradeBarStreams[Asset][TF].High.val);
            };
        

        //strategy.Charts[Asset][TF].chSeries("TR", trendstall.BinStates,"trendstall", Color.Red);
        //strategy.Charts[Asset][TF].chSeries("TR1", trendstall.BinSignals, "trendstall", Color.Aqua,LineType.Dot, 8);
    }

    public override StateSignal Signal
    {
        get { return trendstall.StateSignals.val; }
    }

    public override State State
    {
        get { return trendstall.States.val; }
    }

}


public class ROC : InputBasedIndicator
{

    private IRIndex<double> timeSeries;
    private int period;
    public bool _isPercentMode = false;

    public ROC(IRIndex<double> timeSeries, int period)
    {
        timeSeries.NewDataAction(Recalc);
        this.period = period;
        this.timeSeries = timeSeries;
        vals.Add(double.NaN);
    }

    public void Recalc(double c)
    {
        if (timeSeries.Count <= period) return;
        double num = timeSeries[0] / timeSeries[-period];
        vals.Add(_isPercentMode ? num : (num - 1.0) * 100.0);
    }

}

public enum State {Overbougt, Oversold, Neutral, OverExtended}
public enum StateSignal { Buy, Sell, Neutral, Action }

public class TrendStall : InputBasedIndicator
{
    private IRIndex<BarData> timeSeries;
    public RIndexList<State> States = new RIndexList<State>();
    public RIndexList<StateSignal> StateSignals = new RIndexList<StateSignal>();
    public RIndexList<double> BinStates = new RIndexList<double>(); 
    public RIndexList<double> BinSignals = new RIndexList<double>(); 

    private ADX adx;
    private ROC roc_adx;
    private SMA sma_adx;

    private int adx_period;
    private int roc_period;
    private int ma_period;
    private int adx_roc_threshold;


    public TrendStall(IRIndex<BarData> timeSeries, int adx_period = 15, int roc_period = 5, int ma_period = 5,
                      int adx_roc_threshold = 15)
    {
        timeSeries.NewDataAction(Recalc);
        this.adx_period = adx_period;
        this.roc_period = roc_period;
        this.ma_period = ma_period;
        this.adx_roc_threshold = adx_roc_threshold;
        this.timeSeries = timeSeries;
        adx = new ADX(timeSeries, adx_period);
        roc_adx = new ROC(adx, roc_period);
        sma_adx = new SMA(roc_adx, ma_period);
        vals.Add(double.NaN);
    }

    public void Recalc(BarData c)
    {
        var sig = StateSignal.Neutral;
        var state = State.Neutral;

        if (timeSeries.Count < 2*adx_period) return;

        if (roc_adx[-1] > sma_adx[-1] && roc_adx[-1] > adx_roc_threshold)
        {
            state = State.OverExtended;

            if (roc_adx < roc_adx[-1])
                sig = StateSignal.Action;
        }

        States.Add(state);
        StateSignals.Add(sig);
        BinStates.Add((state==State.OverExtended)?1:0);
        BinSignals.Add((sig == StateSignal.Action) ? 1.0 : double.NaN);

    }
}