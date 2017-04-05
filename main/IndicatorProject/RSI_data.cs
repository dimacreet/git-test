using System.Drawing;

public class RSIState : IndicatorState
{
    //RSI parameters
    IRIndex<double> rsi;   
    private int rsi_low = 30;
    private int rsi_high = 70;
    private int rsi_period = 13;

    public RSIState(StrategyBasePortfolio strategy, string Asset, TimeFrame TF = TimeFrame.D): base(strategy, Asset, TF)
    {
        Color = Color.Aqua;
        Name = "rsi";
        rsi = new RSI(strategy.TradeBarStreams[Asset][TF].Close, rsi_period);
        strategy.TradeBarStreams[Asset][TF].Bars.NewDataAction(RSICalc);

    }

    public override void ShowChart()
    {
        strategy.Charts[Asset][TF].chSeries("RSI", rsi, "RSI", Color.Red);
        strategy.Charts[Asset][TF].chSeries("RSIhigh", rsi, "RSI", Color.White).SetConst(rsi_high);
        strategy.Charts[Asset][TF].chSeries("RSIlow", rsi, "RSI", Color.White).SetConst(rsi_low);
    }


    void RSICalc(BarData bar)
    {
        if (rsi > rsi_high) inState = State.Overbougt;
        else if (rsi < rsi_low) inState = State.Oversold;
        else inState = State.Neutral; 
    }

    public override StateSignal Signal
    {
        get { return StateSignal.Neutral; }
    }

    public override State State
    {
        get { return inState; }
    }
   
}