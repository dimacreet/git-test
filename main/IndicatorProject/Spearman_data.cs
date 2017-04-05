using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using TradeFramework.ChartObjects;


public class SpearmanState: IndicatorState
{
    //Spearman parameters
    private int spearman_low = -80;
    private int spearman_high = 80;
    private int spearman_period = 13;
    private int spearman_ma_period = 5;
    private IRIndex<double> spearman;
    private IRIndex<double> spearman_ma;
    public IRIndex<double> BinSignal=new RIndexList<double>(); 
    public SpearmanState(StrategyBasePortfolio strategy, string Asset, TimeFrame TF)
        : base(strategy, Asset, TF)
    {
        Color = Color.Chartreuse;
        Name = "spearman";
        spearman = new Spearman(strategy.TradeBarStreams[Asset][TF].Close, spearman_period);
        spearman_ma = new SMA(spearman, spearman_ma_period);
        strategy.TradeBarStreams[Asset][TF].Bars.NewDataAction(SpearmanCalc);
    }

    public override void ShowChart()
    {
        strategy.Charts[Asset][TF].chSeries("spearman", spearman, "spearman", Color.Red);
        strategy.Charts[Asset][TF].chSeries("sprmn_sig", BinSignal, "spearman", Color.Aqua,LineType.Solid, 8);
        strategy.Charts[Asset][TF].chSeries("Sprm_high",spearman,"spearman",Color.White).SetConst(spearman_high);
        strategy.Charts[Asset][TF].chSeries("Sprm_low", spearman, "spearman", Color.White).SetConst(spearman_low);
    }
    void SpearmanCalc(BarData bar)
    {
  
        if (spearman > spearman_high)
        {
            inState = State.Overbougt;
            if (spearman < spearman_ma)
            {
                BinSignal.Add(spearman.val);
                inSignal = StateSignal.Sell;
            }
            else
            {
                BinSignal.Add(double.NaN);
                inSignal= StateSignal.Neutral;
            }
        }
        else if (spearman < spearman_low)
        {
            inState = State.Oversold;
            if (spearman > spearman_ma)
            {
                BinSignal.Add(spearman.val);
                inSignal = StateSignal.Buy;
            }
            else
            {
                BinSignal.Add(double.NaN);
                inSignal= StateSignal.Neutral;
            }
        }
        else
        {
            inState = State.Neutral;
            inSignal = StateSignal.Neutral;
            BinSignal.Add(double.NaN);
        }
        
    }
}


public class Spearman : InputBasedIndicator
{
    private int period;
    private IRIndex<double> input;

    public Spearman(IRIndex<double> input, int period)
    {
        this.period = period;
        this.input = input;

        input.NewDataAction(Recalc);

    }

    public void Recalc(double c)
    {
        if (input.Count < period) return;

        var orig_vals = new List<double>();

        for (int pos = 0; pos < period; ++pos)
            orig_vals.Add(input[-pos]);

        var sorted_val = orig_vals.OrderByDescending(x => x).ToList();

        var regr = new _RegressionStat(sorted_val, orig_vals);

        var corr = regr.corr*100;

        vals.Add(corr);
    }
}



