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


public partial class TestStrategy : StrategyBasePortfolio
{
    public int days2recalc = 10;
    public int n_assets = 0;

    public Dictionary<string,double> weights = new Dictionary<string, double>(); 

    private string Asset;
    private TimeFrame TF;

    public string ShowIndicator = "";


    public override void Startup()
    {
        n_assets = TradeSymbols.Keys.Count;
        for (int i = 0; i < n_assets; i++)
        {
            weights.Add(TradeSymbols.Keys.ElementAt(i),1.0);
        }
        Asset = TradeSymbols.Keys.First();
        TF = TradeSymbols[Asset].Keys.First();
    }

    private int day = 0;
    protected override void AfterAllBars()
    {
        var bars = new List<List<BarData>>();
        if (day > days2recalc)
        {
            day = 0;

            ClosePositions();

            foreach (var asset in weights.Keys)
            {
                bars.Add(ToL(TradeBarStreams[asset][TF].Bars));
            }
            Recalc(weights, bars);

            OpenPositions();
        }
        day++;
    }

    void ClosePositions()
    {
        foreach (var asset in weights.Keys)
        {
            Positions[asset].CloseAllOrder();
        }
    }

    void OpenPositions()
    {
        for (int i = 0; i < weights.Count; i++)
        {
            var lng = weights.Values.ElementAt(i) > 0 ? true : false;

            var od = MarketOrder(weights.Keys.ElementAt(i), lng);

            od.Size = Math.Abs(weights.Values.ElementAt(i));
        }
    }

    private Dictionary<string, double> Recalc(Dictionary<string, double> weights, List<List<BarData>> data)
    {
        return weights;
    }

    private List<BarData> ToL(IRIndex<BarData>  a)
    {
        var res = new List<BarData>();
        for (int i = 0; i <a.Count; i++) res.Add(a[-i]);

        return res;
    }
}




public class Test
{
    private int bars_load = 500;
    TimeFrame TF = TimeFrame.D;
    private bool update = false;

    public Test()
    {
        var l = GetAssets(@"D:\BARS\service\Assets-portfolio.csv");
        l = Trim(l);

        var strat = new TestStrategy();
        foreach (var list in l)
        {
            strat.AddSymbol(new SymbolData(list.First().Asset,list,TF,list.First().Asset));
        }
        strat.Run();

        Thread thread = new Thread(() =>
        {
            _ext._Symbols = new Dictionary<string, SymbolData>();

            foreach (var sb in strat.SymbolList)
                _ext._Symbols.Add(sb.Name, sb);

            var f = new OneResultsViewer(_serv.GetResultData(strat.TradingResult, strat), false, false);
            f.WindowState = FormWindowState.Maximized;

            Application.Run(f);

        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    List<List<BarData>> Trim(List<List<BarData>> l)//выровняем по датам
    {
        var start_dt = l.Select(x => x.First().DateTime).Max();
        var end_dt = l.Select(x => x.Last().DateTime).Min();

        var l1= l.Select(x => x.Where(y => (y.DateTime >= start_dt) && (y.DateTime <= end_dt)).ToList()).ToList();
        List<DateTime> dates = new List<DateTime>();
        foreach (var list in l1)
        {
            dates.AddRange(list.Select(x=>x.DateTime));
        }
        dates = dates.Distinct().OrderBy(x=>x).ToList();

        foreach (var list in l1)
        {
            foreach (var date in dates)
            {
                try
                {
                    list.First(x => x.DateTime == date);
                }
                catch (Exception e)
                {
                    var last = list.Last(x => x.DateTime < date);
                    list.Insert(list.IndexOf( last),new BarData
                        {
                            Asset = last.Asset,
                            Close = last.Close,
                            DateTime = date,
                            High = last.High,
                            Low = last.Low,
                            Open = last.Open,
                            TF = last.TF,
                            Volume = 0
                        });
                }
            }
        }

        return l1;
    } 

    private List<List<BarData>> GetAssets(string filename)
    {
        var file = csv.Read(filename, ';');
        List<List<BarData> > res = new List<List<BarData>>();
        var n = file.Count;
        for (int i = 1; i < n; i++)
        {
            var bars = CheckAndLoad(file[i][0].Trim(), file[i][1].Trim(), TF,update);
            if(bars!=null&&bars.Count!=0)res.Add(bars);
            if (bars.Count == 0)
            {
                etc.prn(file[i][0]); 
            }
        }

        return res;
    }
    List<BarData> CheckAndLoad(string asset, string ticker, TimeFrame tf, bool update)
    {

        var bars = new List<BarData>();
        DateTime start_t;
        if (tf == TimeFrame.W) start_t = DateTime.Now.AddDays(-7 * bars_load);
        else if (tf == TimeFrame.D) start_t = DateTime.Now.AddDays(-bars_load);
        else start_t = DateTime.Now.AddDays(-bars_load);
        try
        {

            bars = (List<BarData>)etc.DeSerializeFile(@"D:\BARS\" + asset + "_" + tf);

<<<<<<< HEAD
            var a = ";";

=======
            var nggfg = -1;
            var t = 0;
>>>>>>> origin/master
        }


        catch (Exception e)
        {

            if(update)bars = LoadBars(asset, ticker, tf, start_t, DateTime.Now);
        }
        if (bars.Count!=0&&(bars.Last().DateTime.Year < DateTime.Now.Year || bars.Last().DateTime.Month < DateTime.Now.Month ||
            bars.Last().DateTime.Day < DateTime.Now.Day) && update)
        {
           
            var Bars = LoadBars(asset, ticker, tf, bars.Last().DateTime, DateTime.Now);
            var n = Bars.Count;
            bars.Remove(bars.Last());
            for (int i = 0; i < n; i++)
            {
                bars.Add(Bars[i]);
            }

        }

        if (bars.Count!=0&&(bars.First().DateTime.Year > start_t.Year || bars.First().DateTime.Month > start_t.Month) && update)
        {
            bars = LoadBars(asset, ticker, tf, start_t, DateTime.Now);
        }

        etc.Serialize2File(bars, @"D:\BARS\" + asset + "_" + tf);
        return bars;
    }

    List<BarData> LoadBars(string asset, string ticker, TimeFrame tf, DateTime start, DateTime end)
    {
        var Bars =
           BloomberHistory.GetBars(ticker, tf, start, end, false)
                          .Select(x =>
                                  new BarData
                                  {
                                      Asset = asset,
                                      Close = x.Close,
                                      Open = x.Open,
                                      High = x.High,
                                      Low = x.Low,
                                      TF = tf,
                                      DateTime = x.DateTime,
                                      Volume = 0
                                  })
                          .ToList();
        return Bars;
    }
}

