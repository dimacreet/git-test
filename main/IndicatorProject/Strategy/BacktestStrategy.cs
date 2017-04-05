using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Dundas.Charting.WinControl;
using ResultViewer.Forms;
using TradeFramework;
using TradeFramework.ChartObjects;


/*public partial class BackTestStrategy : StrategyBasePortfolio
{
    public List<IndicatorState> IndicatorStates;
    public List<IndicatorSignal> IndicatorSignals;

    private bool all = false;
    public IRIndex<double>  spearman, meandev, skew_bb, surp_bb, rsi_stdev, rsi_mean;
    public IRIndex<double> trendstall_state, spearman_state_buy, spearman_state_sell, rsi_state, volstall_state;
    public IRIndex<double> spearman_ma, stdev, stdev_ma, stdev2, stdev_bb;

    

    //Spearman parameters
    private int spearman_low = -80;
    private int spearman_high = 80;
    private int spearman_period = 13;

    //skew bollinger bands params
    private double skew_bb_threshold = 1;
    private int skew_bb_period = 40;

    //volstall parameters
    private int volstall_period = 10;
    private int volstall_rsi_period = 14;
    private int volstall_roc_period = 20;
    private double volstall_roc_threshold = 20;
    private double volstall_rsi_low = 40;
    private double volstall_rsi_high = 60;

    //StDev param
    private int std_period = 25;
    private int std_ma_period = 120;
    private double stdev2_num = 1;

    private double stDev_num = 1.5;
    private double garch_stdev_num = 1.0;

    public Dictionary<string, Signals> indicator = new Dictionary<string, Signals>();
    public Dictionary<string, IRIndex<double>> state = new Dictionary<string, IRIndex<double>>();

    public InfoSerie BarColoring;

    public List<Patterns> CurrentPattens = new List<Patterns>();

    private RIndexList<double> MAdev = new RIndexList<double>();

    private IRIndex<double> JMA, SMA200, SMA50, SMA100;

    private string Asset;
    private TimeFrame TF;
    private IRIndex<double> asset_change = new RIndexList<double>();

    private IRIndex<double> garch = new RIndexList<double>();
    private IRIndex<double> vol_roc, histvol_roc;
    private IRIndex<double> vol_roc1;
    private IRIndex<double> vol_roc_lag1;
    private IRIndex<double> vol_ema;
    IRIndex<double> histVol = new RIndexList<double>();
    IRIndex<double> stdDev_HV = new RIndexList<double>();
    IRIndex<double> stDev_HV_low = new RIndexList<double>();
    IRIndex<double> stDev_HV_high = new RIndexList<double>();


    public List<double> all_dev_vals = new List<double>();
    public IRIndex<double> dev_z_score = new RIndexList<double>();

    private IRIndex<double> Skew = new RIndexList<double>();
    private IRIndex<double> Surps = new RIndexList<double>();

    private IRIndex<double> volstall;

    private RIndexList<double> Corr = new RIndexList<double>();

    public List<BarData> FutureQuotes;

    private List<BarData> HistBars = new List<BarData>();

    private chSeries ser1, ser2, ser3;


    private IRIndex<double> SkewRSI;

    public override void Startup()
    {
        Asset = TradeSymbols.Keys.First();
        TF = TradeSymbols[Asset].Keys.First();

        var ChartClass = Charts[Asset][TF];

        state.Add("rsi_buy", new RIndexList<double>());
        state.Add("rsi_sell", new RIndexList<double>());
        state.Add("spearman_sell", new RIndexList<double>());
        state.Add("spearman_buy", new RIndexList<double>());
        state.Add("trendstall_sell", new RIndexList<double>());
        state.Add("trendstall_buy", new RIndexList<double>());
        state.Add("volstall_buy", new RIndexList<double>());
        state.Add("volstall_sell", new RIndexList<double>());
        state.Add("garch_sell", new RIndexList<double>());
        state.Add("garch_buy", new RIndexList<double>());
        state.Add("stdev_buy", new RIndexList<double>());
        state.Add("stdev_sell", new RIndexList<double>());


        trendstall_state = new RIndexList<double>();

       // rsi = new RSI(TradeBarStreams[Asset][TF].Close, rsi_period);
        rsi_stdev = new StDev(rsi, rsi_mean_period);
        rsi_mean = new SMA(rsi, rsi_mean_period);

        spearman = new Spearman(TradeBarStreams[Asset][TF].Close, spearman_period);
        spearman_ma = new SMA(spearman, 5);

        skew_bb = new BollingerBands(Skew, skew_bb_period, skew_bb_threshold);

        volstall = new Volstall(TradeBarStreams[Asset][TF].Close, volstall_period, volstall_rsi_period, volstall_rsi_low, volstall_rsi_high, volstall_roc_period, volstall_roc_threshold);

        stdDev_HV = new StDev(histVol, 120);

        vol_roc = new ROC(new JMA(Skew, 5), 5);
        histvol_roc = new ROC(new JMA(histVol, 5), 5);
        vol_roc_lag1 = new RIndexList<double>();

        SkewRSI = new RSI(Skew, 14);

        stdev = new StDev(TradeBarStreams[Asset][TF].Close, std_period);
        stdev_bb = new BollingerBands(histVol, std_ma_period, stdev2_num);

        double d = 0.4;
        /*      
                ChartClass.chSeries("spearman_buy", new QuickIndi(state["spearman_buy"], x => x == 1 ? 3 : 0), "ts-state", Color.YellowGreen, LineType.Histogram, 2);
                ChartClass.chSeries("spearman_sell", new QuickIndi(state["spearman_sell"], x => x == 1 ? 3 : 0), "ts-state", Color.Lime, LineType.Histogram, 2);
        

                ChartClass.chSeries("rsi_buy", new QuickIndi(state["rsi_buy"], x => x == 1 ? 2 : 0), "ts-state", Color.Red, LineType.Histogram, 2);
                ChartClass.chSeries("rsi_sell", new QuickIndi(state["rsi_sell"], x => x == 1 ? 2 : 0), "ts-state", Color.Crimson, LineType.Histogram, 2);
                

        ChartClass.chSeries("STDRSI", new RSI(histVol, 14), "RSI", Color.Red, LineType.Solid, 2);
        ChartClass.chSeries("STDRSI", "RSI", Color.White, LineType.Dash, 1).SetConst(60);

        ChartClass.chSeries("TrendstallState", new QuickIndi(state["trendstall_buy"], x => x == 1 ? 1 - d : double.NaN), "ts-state", Color.LightGreen, LineType.Solid, 10);
        ChartClass.chSeries("garch_buy", new QuickIndi(state["garch_buy"], x => x == 1 ? 4 - d : double.NaN), "ts-state", Color.BlueViolet, LineType.Solid, 10);
        ChartClass.chSeries("rsi_buy", new QuickIndi(state["rsi_buy"], x => x == 1 ? 2 - d : double.NaN), "ts-state", Color.DarkSalmon, LineType.Solid, 10);
        ChartClass.chSeries("spearman_buy", new QuickIndi(state["spearman_buy"], x => x == 1 ? 3 - d : double.NaN), "ts-state", Color.Turquoise, LineType.Solid, 10);
        ChartClass.chSeries("stdev_buy", new QuickIndi(state["stdev_buy"], x => x == 1 ? 6 - d : double.NaN), "ts-state", Color.Lime, LineType.Solid, 10);
        ChartClass.chSeries("volstall_buy", new QuickIndi(state["volstall_buy"], x => x == 1 ? 5 - d : double.NaN), "ts-state", Color.Red, LineType.Solid, 10);


        ChartClass.chSeries("TrendstallState_sell", new QuickIndi(state["trendstall_buy"], x => x == 1 ? 1 - d : double.NaN), "ts-state_sell", Color.LightGreen, LineType.Solid, 10);
        ChartClass.chSeries("garch_sell", new QuickIndi(state["garch_buy"], x => x == 1 ? 4 - d : double.NaN), "ts-state_sell", Color.BlueViolet, LineType.Solid, 10);
        ChartClass.chSeries("rsi_sell", new QuickIndi(state["rsi_sell"], x => x == 1 ? 2 - d : double.NaN), "ts-state_sell", Color.DarkSalmon, LineType.Solid, 10);
        ChartClass.chSeries("spearman_sell", new QuickIndi(state["spearman_sell"], x => x == 1 ? 3 - d : double.NaN), "ts-state_sell", Color.Turquoise, LineType.Solid, 10);
        ChartClass.chSeries("stdev_sell", new QuickIndi(state["stdev_buy"], x => x == 1 ? 6 - d : double.NaN), "ts-state_sell", Color.Lime, LineType.Solid, 10);
        ChartClass.chSeries("volstall_sell", new QuickIndi(state["volstall_buy"], x => x == 1 ? 5 - d : double.NaN), "ts-state_sell", Color.Red, LineType.Solid, 10);

        ChartClass.chSeries("StDev", histVol, "grch", Color.LightCoral, LineType.Solid, 2);
        ChartClass.chSeries("StDev_up", ((BollingerBands)stdev_bb).BollUP, "grch", Color.White, LineType.Solid, 2);
        ChartClass.chSeries("StDev_down", ((BollingerBands)stdev_bb).BollDown, "grch", Color.White, LineType.Solid, 2);


        //ChartClass.chSeries("_3", "ts-state2", Color.Transparent, LineType.Solid, 100).SetConst(0.07);


        ser1 = ChartClass.chSeries("_1", "ts-state", Color.Transparent, LineType.Filled, 0);
        ser2 = ChartClass.chSeries("_2", "ts-state_sell", Color.Transparent, LineType.Filled, 0);
        //ser3 = ChartClass.chSeries("_3", "", Color.Green, LineType.Filled, 2);


        ChartClass.chSeries("spearman", spearman, "sprmn", Color.YellowGreen, LineType.Solid, 2);
        ChartClass.chSeries("sprmn_ma", spearman_ma, "sprmn", Color.White, LineType.Solid, 1);


        SMA100 = new SMA(TradeBarStreams[Asset][TF].Close, 100);
        SMA200 = new SMA(TradeBarStreams[Asset][TF].Close, 200);
        SMA50 = new SMA(TradeBarStreams[Asset][TF].Close, 50);

        JMA = new JMA(TradeBarStreams[Asset][TF].Close, 5);

        ChartClass.chSeries("SM100", SMA100, "", Color.White, LineType.Dot, 1);
        ChartClass.chSeries("SMA50", SMA50, "", Color.White, LineType.Dot, 1);
        ChartClass.chSeries("SMA200", SMA200, "", Color.White, LineType.Dash, 2);

        ChartClass.chSeries("JMA", JMA, "", Color.Cyan, LineType.Dash, 3);


        //ChartClass.chSeries("Garch", garch, "grch", Color.Red, LineType.Solid, 2);
        //ChartClass.chSeries("HistVol", histVol, "grch", Color.Aqua, LineType.Solid, 2);
        //ChartClass.chSeries("HistVol1", stDev_HV_high, "grch", Color.BlueViolet, LineType.Solid, 2);
        //ChartClass.chSeries("HistVol2", stDev_HV_low, "grch", Color.BlueViolet, LineType.Solid, 2);


        BarColoring = ChartClass.RegisterBarsColoring(Asset, TF);


        indicator.Add("Spearman", Signals.None);
        indicator.Add("TrendStall", Signals.None);
        indicator.Add("ValStall", Signals.None);
        indicator.Add("RSI", Signals.None);
    }


    protected override void NewBar(BarStream bs)
    {
        if (bs.Asset != Asset) return;

        IndicatorStates = new List<IndicatorState>();
        IndicatorSignals = new List<IndicatorSignal>();


        ser1.SetRange(0, 1);

        HistBars.Add(bs.Bars.val);



        if (bs.Dates.val == TradeBarStreams[Asset + "VIX"][TF].Dates.val)
            Skew.Add(TradeBarStreams[Asset + "VIX"][TF].Close.val);


        // Vol(HistBars, HistBars.Count-1)
        //VOLS
        if (HistBars.Count > 270)
        {
            //var xx = EWMAVolatility.GetEWMAVolatility(HistBars, HistBars.Count - 1, 252)*100d;

            var HV = HistVol.GetHistoricalVolatility(HistBars, HistBars.Count - 1, 25) * 100;

            var grch = GARCH.GetGarch1_1Volatility(HistBars, HistBars.Count - 1, HV, 0.05, 0.95) * 100;
            garch.Add(grch);
            histVol.Add(HV);
            stDev_HV_high.Add(stDev_num * stdDev_HV + HV);
            stDev_HV_low.Add(-stDev_num * stdDev_HV + HV);

        }
        else
        {
            garch.Add(0);
            histVol.Add(0);
        }
        if (HistBars.Count > 2)
            vol_roc_lag1.Add(vol_roc[-1]);

        else
            vol_roc_lag1.Add(0);


        if (HistBars.Count > 30 && vol_roc.Lower() && sSkewBB() == Signals.Buy)
        {
            //Charts[Asset][TF].AddArrowUp(bs.Dates.val, bs.Open.val, Color.Orange);
            state["volstall_buy"].Add(1);
            if (vol_roc < vol_roc[-1])
                Charts[Asset][TF].AddArrowUp(bs.Dates.val, 0.99 * bs.Low.val, Color.BlueViolet);


        }
        else state["volstall_buy"].Add(0);
        state["volstall_sell"].Add(0);

        if (HistBars.Count > 30 && histvol_roc.Lower() && stdev_bb == -1)
        {
            //Charts[Asset][TF].AddArrowUp(bs.Dates.val, bs.Open.val, Color.Orange);
            state["stdev_buy"].Add(1);
            if (histvol_roc < histvol_roc[-1])
                Charts[Asset][TF].AddArrowUp(bs.Dates.val, 0.99 * bs.Low.val, Color.Red);


        }
        else state["stdev_buy"].Add(0);
        state["stdev_sell"].Add(0);

        if (bs.Bars.Count < 91 || trendstall.val.isNaN() || Skew.val.isNaN()) return;


        asset_change.Add(TradeBarStreams[Asset][TF].Close / TradeBarStreams[Asset][TF].Close[-90] - 1);

        if (trendstall.Count < 2) return;







   
        //SPEARMAN state indicators signals
        if (sSpearman() == Signals.Sell)
        {
            state["spearman_sell"].Add(1);
            state["spearman_buy"].Add(0);
            if (spearman <= spearman_ma)
                Charts[Asset][TF].AddArrowDown(bs.Dates.val, bs.High.val, Color.YellowGreen);
        }
        else if (sSpearman() == Signals.Buy)
        {
            //Charts[Asset][TF].AddArrowUp(bs.Dates.val, bs.High.val, Color.Red);
            //indicator["Spearman"] = Signals.Buy;
            state["spearman_sell"].Add(0);
            state["spearman_buy"].Add(1);
            if (spearman >= spearman_ma)
                Charts[Asset][TF].AddArrowUp(bs.Dates.val, bs.Low.val, Color.YellowGreen);
        }
        else
        {
            //indicator["Spearman"] = Signals.None;
            state["spearman_sell"].Add(0);
            state["spearman_buy"].Add(0);
        }

        //RSI state indicator signals
        
        if (garch > histVol + garch_stdev_num * (stDev_HV_high - histVol) && rsi < 70)
        {
            state["garch_buy"].Add(1);
        }
        else state["garch_buy"].Add(0);
        state["garch_sell"].Add(0);
    }



    private double[] GetLast(IRIndex<double> array, int n)
    {

        double[] res = new double[n];
        try
        {
            for (var i = 0; i < n; i++)
            {
                res[i] = array[-i];
            }
            return res;
        }
        catch (ArgumentOutOfRangeException e)
        {
            for (var i = 0; i < n; i++)
                res[i] = 0.0;
            return res;
        }
    }

    public Signals sVolstall()
    {
        return Signals.None;
    }

    

    public Signals sTrendStall()
    {
        if (trendstall == 1) return Signals.Both;

        return Signals.None;
    }

    public Signals sSpearman()
    {
        if (spearman > spearman_high) return Signals.Sell;
        if (spearman < spearman_low) return Signals.Buy;

        return Signals.None;
    }

    public Signals sSkewBB()
    {
        if (skew_bb == 1) return Signals.Sell;
        if (skew_bb == -1) return Signals.Buy;

        return Signals.None;
    }

    public Signals sSurpBB()
    {
        if (surp_bb == 1) return Signals.Buy;
        if (surp_bb == -1) return Signals.Sell;

        return Signals.None;
    }
}
*/