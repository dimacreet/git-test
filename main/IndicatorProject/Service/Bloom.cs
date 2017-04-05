
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ResultViewer.Forms;

public class Bloom
{
    private static void DownloadCloses(string BloomTicker, string TradingTicker, string StartDT = "1.1.2001")
    {
        var Bars =
            BloomberHistory.GetHistoryFields(BloomTicker, new List<string> { "PX_LAST" }, StartDT.ToDT(), DateTime.Now,
                                             true).Where(x => x.Value.Count > 0).Select(x =>
                                                                                        new BarData
                                                                                        {
                                                                                            Asset = TradingTicker,
                                                                                            Close =
                                                                                                x.Value["PX_LAST"].d
                                                                                        (),
                                                                                            Open =
                                                                                                x.Value["PX_LAST"].d
                                                                                        (),
                                                                                            High =
                                                                                                x.Value["PX_LAST"].d(),
                                                                                            Low = x.Value["PX_LAST"].d(),
                                                                                            TF = TimeFrame.D,
                                                                                            DateTime = x.Key,
                                                                                            Volume = 0
                                                                                        }).ToList();

        foreach (var b in Bars)
        {
            b.Asset = TradingTicker;
            b.TF = TimeFrame.D;
        }

        //Application.Run(new EquityResultChart(Bars.Select(x => x.DateTime).ToList(), Bars.Select(x => x.Close).ToList()));

        etc.Serialize2File(Bars, @"D:\BARS\" + TradingTicker + "_D");
        etc.prn("Downloaded " + TradingTicker);
    }


    public static void DownloadBars(string BloomTikcker, string TradingTicker)
    {
        var Bars =
            BloomberHistory.GetBars(BloomTikcker, TimeFrame.D, "1.1.2014".ToDT(), DateTime.Now, false)
                           .Select(x =>
                                   new BarData
                                   {
                                       Asset = TradingTicker,
                                       Close = x.Close,
                                       Open = x.Open,
                                       High = x.High,
                                       Low = x.Low,
                                       TF = TimeFrame.D,
                                       DateTime = x.DateTime,
                                       Volume = 0
                                   })
                           .ToList();


        etc.Serialize2File(Bars, @"D:\Positions\BARS\" + TradingTicker + "_D");
    }


    private static void MakeSpread(string Ticker1, string Ticker2, string ResultTicker)
    {
        var Bars1 = (List<BarData>)etc.DeSerializeFile(@"D:\BARS\" + Ticker1 + "_D");
        var Bars2 = (List<BarData>)etc.DeSerializeFile(@"D:\BARS\" + Ticker2 + "_D");

        var Bars = new List<BarData>();

        var dts = Bars1.Select(x => x.DateTime).ToList();
        var dts2 = Bars2.Select(x => x.DateTime).ToList();

        var all_dts = new List<DateTime>();
        all_dts.AddRange(dts);
        all_dts.AddRange(dts2);

        all_dts = all_dts.Distinct().ToList();
        all_dts.Sort();

        foreach (var dt in all_dts)
        {
            var b1 = Bars1.FirstOrDefault(x => x.DateTime == dt);
            if (b1 == null) continue;

            var b2 = Bars2.FirstOrDefault(x => x.DateTime == dt);
            if (b2 == null) continue;

            var open = b1.Open - b2.Open;
            var close = b1.Close - b2.Close;

            Bars.Add(new BarData
            {
                Asset = ResultTicker,
                Close = close,
                Open = open,
                High = Math.Max(close, open),
                Low = Math.Min(close, open),
                TF = TimeFrame.D,
                DateTime = dt,
                Volume = 0
            });
        }

        etc.Serialize2File(Bars, @"D:\BARS\" + ResultTicker + "_" + TimeFrame.D);

        Application.Run(new EquityResultChart(Bars.Select(x => x.DateTime).ToList(), Bars.Select(x => x.Close).ToList()));
    }

    private static void DownloadBars(string BloomTicker1, string BloomTicker2, string TradingTicker,
                                     TimeFrame TF = TimeFrame.D)
    {
        etc.prn("Downloading " + BloomTicker1 + " - " + " " + BloomTicker2 + " as " + TradingTicker);
        var Bars1 = BloomberHistory.GetBars(BloomTicker1, TF, "1.1.2014".ToDT(), DateTime.Now, false).Select(x =>
                                                                                                             new BarData
                                                                                                             {
                                                                                                                 Asset
                                                                                                                     =
                                                                                                                     BloomTicker1,
                                                                                                                 Close
                                                                                                                     =
                                                                                                                     x
                                                                                                             .Close,
                                                                                                                 Open
                                                                                                                     =
                                                                                                                     x
                                                                                                             .Open,
                                                                                                                 High
                                                                                                                     =
                                                                                                                     x
                                                                                                             .High,
                                                                                                                 Low
                                                                                                                     =
                                                                                                                     x
                                                                                                             .Low,
                                                                                                                 TF
                                                                                                                     =
                                                                                                                     TimeFrame
                                                                                                             .D,
                                                                                                                 DateTime
                                                                                                                     =
                                                                                                                     x
                                                                                                             .DateTime,
                                                                                                                 Volume
                                                                                                                     =
                                                                                                                     0
                                                                                                             })
                                   .ToList();

        var Bars2 = BloomberHistory.GetBars(BloomTicker2, TF, "1.1.2001".ToDT(), DateTime.Now, false).Select(x =>
                                                                                                             new BarData
                                                                                                             {
                                                                                                                 Asset
                                                                                                                     =
                                                                                                                     BloomTicker2,
                                                                                                                 Close
                                                                                                                     =
                                                                                                                     x
                                                                                                             .Close,
                                                                                                                 Open
                                                                                                                     =
                                                                                                                     x
                                                                                                             .Open,
                                                                                                                 High
                                                                                                                     =
                                                                                                                     x
                                                                                                             .High,
                                                                                                                 Low
                                                                                                                     =
                                                                                                                     x
                                                                                                             .Low,
                                                                                                                 TF
                                                                                                                     =
                                                                                                                     TimeFrame
                                                                                                             .D,
                                                                                                                 DateTime
                                                                                                                     =
                                                                                                                     x
                                                                                                             .DateTime,
                                                                                                                 Volume
                                                                                                                     =
                                                                                                                     0
                                                                                                             })
                                   .ToList();

        var Bars = new List<BarData>();

        var dts = Bars1.Select(x => x.DateTime).ToList();
        var dts2 = Bars2.Select(x => x.DateTime).ToList();

        var all_dts = new List<DateTime>();
        all_dts.AddRange(dts);
        all_dts.AddRange(dts2);

        all_dts = all_dts.Distinct().ToList();
        all_dts.Sort();

        foreach (var dt in all_dts)
        {
            var b1 = Bars1.FirstOrDefault(x => x.DateTime == dt);
            if (b1 == null) continue;

            var b2 = Bars2.FirstOrDefault(x => x.DateTime == dt);
            if (b2 == null) continue;

            var open = b1.Open / b2.Open;
            var close = b1.Close / b2.Close;

            Bars.Add(new BarData
            {
                Asset = TradingTicker,
                Close = close,
                Open = open,
                High = Math.Max(close, open),
                Low = Math.Min(close, open),
                TF = TF,
                DateTime = dt,
                Volume = 0
            });

        }

        etc.Serialize2File(Bars, @"D:\BARS\" + TradingTicker + "_" + TF);
    }
}
