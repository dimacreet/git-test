using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TradeFramework.ChartObjects;

public class VolStallState : IndicatorState
{

    public static double nn = 250.0;
    double nn_d = 250.0;
    double nn_w = 250.0 / 7.0;
    //volstall parameters
    private int volstall_roc_period = 20;
    private double volstall_roc_threshold = 20;
    private int roc_ma_period = 5;
    private VolMode volmode;
    private Volstall volstall;

    public VolStallState(StrategyBasePortfolio strategy, string Asset, TimeFrame TF, VolMode mode)
        : base(strategy, Asset, TF)
    {
        volmode = mode;
        Color = Color.RosyBrown;
        Name = mode.ToString() + "stall";
        volstall = new Volstall(strategy.TradeBarStreams[Asset][TF].Bars, volstall_roc_period, volstall_roc_threshold,
                                roc_ma_period, mode);
        if (TF == TimeFrame.W) nn = nn_w;
        else nn = nn_d;
    }

    private chSeries TRArea;

    public override void ShowChart()
    {
        string asset;
        if (volmode == VolMode.VIX) asset = Asset.Remove(Asset.Length - 3);
        else asset = Asset;

        TRArea = strategy.Charts[asset][TF].chSeries("TR1", "", Color.Aqua, LineType.Filled, 8);

        volstall.BinStates.NewData += x =>
        {
            if (x == 1)
                TRArea.SetRange(strategy.TradeBarStreams[asset][TF].Low.val,
                                strategy.TradeBarStreams[asset][TF].High.val);
        };

        //TRArea = strategy.Charts[Asset][TF].chSeries("TR2", "", Color.Red, LineType.Filled, 8);
        volstall.BinSignals.NewData += x =>
        {
            if (x == 1)
                strategy.Charts[asset][TF].AddArrowUp(strategy.TradeBarStreams[asset][TF].Dates.val, strategy.TradeBarStreams[asset][TF].Close[0],
                                                           Color.Red);
            //TRArea.SetRange(strategy.TradeBarStreams[Asset][TF].Low.val,
            //strategy.TradeBarStreams[Asset][TF].High.val);
        };

       // strategy.Charts[asset][TF].chSeries("VolStall",volstall.BinStates ,"volstall", Color.Red);
       // strategy.Charts[asset][TF].chSeries("Volstallsgnl", volstall.BinSignals, "volstall", Color.Aqua, LineType.Dot, 8).SetConst(5);
        
    }    

    public override StateSignal Signal
    {
        get { return volstall.StateSignals.val; }
    }

    public override State State
    {
        get { return volstall.States.val; }
    }
}


public enum VolMode
{
    Garch,
    Stdev,
    VIX
}

public class Volstall: InputBasedIndicator
{
    
    private int roc_period;
    private double roc_threshold;
    private int roc_ma_period;
    public RIndexList<State> States = new RIndexList<State>();
    public RIndexList<StateSignal> StateSignals = new RIndexList<StateSignal>();
    private IRIndex<double> vol_roc; 
    private IRIndex<BarData> input;
    private IRIndex<double> vix_input = new RIndexList<double>(); 
    private IRIndex<double> roc_ma;
    private string Asset;
    private List<BarData> HistData = new List<BarData>();
    public IRIndex<double> garch=new RIndexList<double>(); 
    private IRIndex<double> histVol=new RIndexList<double>();
    public  RIndexList<double> BinStates= new RIndexList<double>();
    public RIndexList<double>BinSignals = new RIndexList<double>(); 
    List<BarData> HistBars = new List<BarData>(); 

	public Volstall(IRIndex<BarData>timeseries, int roc_period,double roc_threshold, int roc_ma_period,VolMode mode=VolMode.Garch )
	{
        this.roc_ma_period = roc_ma_period;
        this.roc_threshold = roc_threshold;
        this.roc_period = roc_period;
	    this.roc_ma_period = roc_ma_period;
        input = timeseries;


        if (mode==VolMode.Garch)
        {
  
            input.NewDataAction(ReCalcGarchStdev);
            vol_roc = new ROC(garch, roc_period);
        }
        else if (mode==VolMode.VIX)
        {
            vol_roc=new ROC(vix_input,roc_period);
        }
        else if (mode==VolMode.Stdev)
        {
            input.NewDataAction(ReCalcGarchStdev);
            vol_roc = new ROC(histVol, roc_period);
        }
        else throw new Exception("Parameter type isn't recognized");

        roc_ma = new SMA(vol_roc, roc_ma_period);

        input.NewDataAction(ReCalc);

	}

    public void ReCalc(BarData c)
    {
        vix_input.Add(c.Close);
        if (vol_roc[0] > roc_threshold && vol_roc[0] > roc_ma)
        {
            States.Add(State.OverExtended);
            if (vol_roc < vol_roc[-1])
            {
                BinSignals.Add(1);
                StateSignals.Add(StateSignal.Action);
            }
            else
            {
                BinSignals.Add(double.NaN);
                StateSignals.Add(StateSignal.Neutral);
            }
            
        }

        else
        {
            States.Add(State.Neutral);
            BinSignals.Add(double.NaN);
        }
        HistBars.Add(c);
        BinStates.Add(States==State.OverExtended?1:0);
        //BinSignals.Add(StateSignals == StateSignal.Action ? 1 : double.NaN);
    }

    public void ReCalcGarchStdev(BarData c)
    {
        HistData.Add(input[0]);
        if (HistData.Count > VolStallState.nn.To<int>())
        {
            var HV = HistVol.GetHistoricalVolatility(HistData, HistData.Count - 1, VolStallState.nn.To<int>()-1)*100;
            
            var grch = GARCH.GetGarch1_1Volatility(HistData, HistData.Count - 1, HV, 0.05, 0.95)*100;
            garch.Add(grch);
            histVol.Add(HV);
        }
        else
        {
            garch.Add(0);
            histVol.Add(0);
        }
    }

}
