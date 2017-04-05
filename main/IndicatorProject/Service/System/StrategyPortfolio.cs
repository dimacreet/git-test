using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TradeFramework;
using TradeFramework.ChartObjects;

public partial class StrategyBasePortfolio
{
    public bool UserRun = true;

    public ParamMix ParamMix
    {
        get { return _pm ?? (_pm = StrategyAttrs.GetAttrStructure(GetType()).paramMix); }
    }

    protected virtual void NewBar(BarStream bs)
    {
        
    }

    protected virtual void BeforeAllBars()
    {

    }

    protected virtual void AfterAllBars()
    {

    }

    public void UpdateTradeSymbols()
    {
        throw new NotImplementedException();
    }

    public void AddSymbol(SymbolData inSb)
    {
        if(!TradeSymbols.ContainsKey(inSb.Asset))
            TradeSymbols.Add(inSb.Asset, new Dictionary<TimeFrame, SymbolData>());

        if (TradeSymbols[inSb.Asset].ContainsKey(inSb.TimeFrame)) return;

        TradeSymbols[inSb.Asset].Add(inSb.TimeFrame, inSb);
    }

    private ParamMix _pm = null;

    public PredifinedBank PredifinedBank = null;

    public _params LaunchParams = new _params();

    protected double
    ProfitFactor,
    AvgDD,
    MaxDD,
    GrossProfit,
    GrossLoss;

    public string StrategyName;

    public Dictionary<string, PositionClass> Positions = new Dictionary<string, PositionClass>();
    public Dictionary<string, Dictionary<TimeFrame, DrawClass>> Charts = new Dictionary<string, Dictionary<TimeFrame, DrawClass>>();
    public Dictionary<string, Dictionary<TimeFrame, BarStream>> TradeBarStreams = new Dictionary<string, Dictionary<TimeFrame, BarStream>>();
    public Dictionary<string, Dictionary<TimeFrame, SymbolData>> TradeSymbols = new Dictionary<string, Dictionary<TimeFrame, SymbolData>>();

    public List<SymbolData> SymbolList
    {
        get
        {
            return TradeSymbols.Values.SelectMany(x=> x.Values).ToList();
        }
    }

    public Dictionary<string, TradingResults> TradingResults = new Dictionary<string, TradingResults>();

    public Dictionary<string, StatSaver> StatSavers = new Dictionary<string, StatSaver>();
    public StatSaver StatSaver;

    protected PositionData LastPosition(string Asset) { return Positions[Asset].LastPosition; }
    protected bool inPosition(string Asset) { return Positions[Asset].inPosition; }
    protected bool inLong(string Asset) { return Positions[Asset].inPosition && Positions[Asset].LastPosition.Long; } 
    protected bool inShort(string Asset) { return Positions[Asset].inPosition && !Positions[Asset].LastPosition.Long; }

    protected OrderData MarketOrder(string Asset, bool Long) { return Positions[Asset].MarketOrder(Long); }
    protected OrderData MarketOrderCloseAll(string Asset, bool Long) { return Positions[Asset].MarketOrderCloseAll(Long); }

    public virtual void Startup() { }

    public virtual void Shutdown() { }

    //public virtual void NewBar() { }

    public TradingResults RunParams(Param[] Params, string Description = null)
    {
        SetParams(Params);
        Run(Description);
        return TradingResult;
    }

    public static int TF2Min(TimeFrame tf)
    {
        if (tf == TimeFrame.M1) return 1;
        if (tf == TimeFrame.M5) return 5;
        if (tf == TimeFrame.M10) return 10;
        if (tf == TimeFrame.M15) return 15;
        if (tf == TimeFrame.M30) return 30;
        if (tf == TimeFrame.H1) return 60;
        if (tf == TimeFrame.D) return 60*24;
        if (tf == TimeFrame.W) return 60 * 24*5;
        throw new Exception("No such span for TF");
    }


    private StrategyAttrsInfo AttrsInfo;

    public void SetParams(ParamsId prms)
    {
        SetParams(ParamMix.ParseCacheId(prms));
    }

    public void SetParams(Param[] prms)
    {
        if (AttrsInfo == null)
            AttrsInfo = StrategyAttrs.GetAttrStructure(GetType());

        prms.ForEach(
            (prm, i) =>
            {
                var name = AttrsInfo.paramMix.IdsDict[i];
                var field = AttrsInfo.TargetType.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                field.SetValue(this, prm.Val());
            }
            );
    }
    
    public TradingResults TradingResult;

    public TradingResults Run(string DescriptionToSave = null, bool ShowChartBrutal = false)
    {
        _paramsGlobal.ShowChart = DescriptionToSave != null;

        foreach (var p1 in TradeSymbols)
            foreach (var p2 in p1.Value)
                SymbolList.Add(p2.Value);

        ext.ShowChart = _paramsGlobal.ShowChart;
        UserRun = DescriptionToSave != null;

        if (ShowChartBrutal)
        {
            ext.ShowChart = true;
            _paramsGlobal.ShowChart = true;
        }

        _paramsGlobal.SavingResultsMode = SavingResultsMode.Full;

        var BarsDTMix = new SortedDictionary<DateTime, List<Tuple<BarData, bool>>>();

        foreach (var sb in SymbolList)
        {
            var asset = sb.Asset;

            var bs = new BarStream(sb);

            if (!TradeBarStreams.ContainsKey(asset))
                TradeBarStreams.Add(asset, new Dictionary<TimeFrame, BarStream>());

            TradeBarStreams[asset].Add(bs.TF, bs);

            if(!Charts.ContainsKey(asset))
                Charts.Add(asset, new Dictionary<TimeFrame, DrawClass>());

            Charts[asset].Add(bs.TF, new DrawClass(bs.Dates, bs.Close, bs.Open, sb, _paramsGlobal.ShowChart));
        }

        var assets = SymbolList.Select(x => x.Asset).Distinct();

        foreach (var asset in assets)
        {
            var sb = SymbolList.Where(x => x.Asset == asset).MinBy(x => TF2Min(x.TimeFrame));
            var bs = TradeBarStreams[sb.Asset][sb.TimeFrame];

            var posClass = new PositionClass(bs.Bars, LaunchParams, sb);
            Positions.Add(asset, posClass);
        }

        foreach (var sb in SymbolList)
        {

            foreach (var bar in sb.BarsArray)
            {
                if (!BarsDTMix.ContainsKey(bar.DateTime))
                    BarsDTMix.Add(bar.DateTime, new List<Tuple<BarData, bool>>());

                BarsDTMix[bar.DateTime].Add(Tuple.Create(bar, bar.TF == Positions[bar.Asset].TF));
            }
        }

        foreach (var pair in Positions)
        {
            var posClass = pair.Value;
            
            if(posClass.AssetType == AssetType.FX_cross)
            {
                var firstPair = posClass.Asset.Substring(0, 3);
                //var secondPair = posClass.Asset.Substring(3, 3);
                var CrossAsset = firstPair +"USD";
                posClass.FXCrossAssetBars = TradeBarStreams[CrossAsset][posClass.TF].Bars;
            }
        }

        var BarsMix = BarsDTMix.Values.ToList();

        Startup();

        var leng = BarsMix.Count;
        var dts = BarsDTMix.Keys.ToList();

        /*var BarsDict = new Dictionary<string, IRIndex<BarData>>();

        foreach (var bs in TradeBarStreams)
        {
            BarsDict.Add(bs.Key, bs.Value.Bars);
        }/**/

        StatSaver = new StatSaver(dts, Positions, LaunchParams);
        
        for (int i = 0; i < leng; i++)
        {
            var bars = BarsMix[i];

            BeforeAllBars();

            foreach (var tpl in bars)
            {
                var bar = tpl.Item1;

                if(tpl.Item2)
                    Positions[bar.Asset].Execution();

                TradeBarStreams[bar.Asset][bar.TF].NewBarCommitEvent();

                NewBar(TradeBarStreams[bar.Asset][bar.TF]);

                if (_paramsGlobal.ShowChart)
                    Charts[bar.Asset][bar.TF].AfterBar(TradeSymbols[bar.Asset][bar.TF]);

                if (TradeBarStreams[bar.Asset][bar.TF].Bars.AbsCount < TradeSymbols[bar.Asset][bar.TF].Bars.Count - 1)
                    TradeBarStreams[bar.Asset][bar.TF].NewBarNoEvent();
            }

            AfterAllBars();

            foreach (var posClas in Positions.Values)
                posClas.UpdateOpenPositions();

            

            StatSaver.SavingResultsRoutine();
        }


        

        foreach (var pos in Positions)
            pos.Value.SaveStatistics();

        // Get results

        TradingResult = StatSaver.getTradingResults(); 

        TradingResult.chMarkers = new Dictionary<string, Dictionary<TimeFrame, List<object>>>();

        if (_paramsGlobal.ShowChart)
        {
            foreach (var chart_pair in Charts)
            {
                TradingResult.chMarkers.Add(chart_pair.Key, new Dictionary<TimeFrame, List<object>>());

                foreach (var ch in chart_pair.Value)
                {
                    TradingResult.chMarkers[chart_pair.Key].Add(ch.Key, new List<object>());

                    foreach (var ser in ch.Value.chSeriesDict)
                    {
                        ser.Asset = chart_pair.Key;
                        ser.TF = ch.Key;
                        TradingResult.chSeries.Add(ser);
                    }

                    foreach (var ser in ch.Value.chInfoSeries)
                    {
                        ser.Asset = chart_pair.Key;
                        ser.TF = ch.Key;
                        TradingResult.chInfoSeries.Add(ser);
                    }

                    TradingResult.chMarkers[chart_pair.Key][ch.Key].AddRange(
                        ch.Value.chMarkers.First().Value.First().Value
                        );
                }
            }

            Shutdown(); /**/
        }

        return TradingResult;
    }

    // Optimizing
    public virtual double Fitness()
    {
        return double.NaN;
    }

    // Optimizing
    public virtual bool ResultReadyToSave()
    {
        return true;
    }

    // GPU parsing 
    /*public virtual double GPUFitness()
    {
        return double.NaN;
    }*/

    protected void SetStopLoss(string Asset, double SL)
    {
        Positions[Asset].Orders[0].SL = SL;
    }

    //protected int Counter { get { return Close.Count; } }

    protected bool isnan(IRIndex<double> x)
    {
        return x.val.isNaN();
    }

    protected bool isnan(double x)
    {
        return x.isNaN();
    }

    protected void ClosePosition(string Asset)
    {
        Positions[Asset].CloseAllOrder();
    }

    Param[] getParams()
    {
        var t = GetType();
        var pm = StrategyAttrs.GetAttrStructure(t).paramMix;
        var prms = pm.NewRandomParamSet();

        int i = 0;
        foreach (var name in pm.Dict.Keys)
        {
            var val = GetType().GetField(name).GetValue(this);
            prms[i].SetVal(val);
            i++;
        }

        return prms;
    }

    public StrategyBasePortfolio Clone()
    {
        var strat = etc.GetNewObject(GetType());
        var fields = Service.GetAllFields(GetType());

        foreach (var f in fields)
        {
            var name = f.Name;
            var val = f.GetValue(this);

            /*if (name == "Ranges")
                System.Diagnostics.Debugger.Break();/**/

            if (val == null)
            {
                f.SetValue(strat, null);
                continue;
            }
            if (val.GetType().IsPrimitive)
                f.SetValue(strat, val);
            else
            {
                var z = val.CloneObject();
                f.SetValue(strat, z);
            }
        }

        return (StrategyBasePortfolio)strat;
    }

}