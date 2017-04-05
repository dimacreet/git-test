using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using TradeFramework;
using TradeFramework.ChartObjects;

//Markers.Add(new ArrowMarker(xDT.Values.Count - 1, color, up)); 

public class DrawClass /*: DrawClass*/ {
    public List<chSeries> chSeriesDict = new List<chSeries>();
    public List<InfoSerie> chInfoSeries = new List<InfoSerie>();

    public bool CarryLastValue = false;

    public Dictionary<string, Dictionary<TimeFrame, List<object>>> chMarkers =
        new Dictionary<string, Dictionary<TimeFrame, List<object>>>();

    //BarStream BarStream;

    public void AfterBar(SymbolData Symbol)
    {

        foreach (var pair in chSeriesDict.Where(s => s.Asset == Symbol.Asset && s.TF == Symbol.TimeFrame))
        {
            if (pair.Type != SeriesType.Normal) continue;
            if (!pair.ValAdded)
            {


                if (CarryLastValue && pair.vals.Count != 0)
                {

                    /*if(!pair.vals.Last().isNaN() && pair.Name == "ADX")
                        System.Diagnostics.Debugger.Break();/**/
                    pair.Add(pair.vals.Last());
                }
                else
                    pair.Add(double.NaN);
            }
            pair.ValAdded = false;
        }

        foreach (var pair in chInfoSeries.Where(s => s.Asset == Symbol.Asset && s.TF == Symbol.TimeFrame))
        {
            if (!pair.ValAdded) pair.Add(null);
            pair.ValAdded = false;
        }
    }

    bool ShowChart;

    /*public DrawClass(BarStream bs, bool ShowChart)
        : this(bs.Dates, bs.Close, bs.Open, ShowChart)
    { }/**/

    private IRIndex<DateTime> Dates;
    private IRIndex<double> Close, Open;

    /*public DrawClass(BarStream bs, bool ShowChart, SymbolData defSymbol)
        : this(bs.Dates, bs.Close, bs.Open, ShowChart, defSymbol)
    { }/**/

    public DrawClass(IRIndex<DateTime> Dates, IRIndex<double> Close, IRIndex<double> Open, bool ShowChart, SymbolData defSymbol)
    {
        this.ShowChart = ShowChart;
        ext.ShowChart = ShowChart;
        this.defSymbol = defSymbol;
        if (!ShowChart) return;
        chMarkers.Add(defSymbol.Asset,
                      new Dictionary<TimeFrame, List<object>>{
                          {defSymbol.TimeFrame, new List<object>()}
                      });
    }


    public DrawClass(IRIndex<DateTime> Dates, IRIndex<double> Close, IRIndex<double> Open, SymbolData defSymbol, bool ShowChart)
    {
        this.Dates = Dates;
        this.Close = Close;
        this.Open = Open;
        this.defSymbol = defSymbol;
        this.ShowChart = ShowChart;
        ext.ShowChart = ShowChart;
        if (!ShowChart)
            return;
        this.chMarkers.Add(defSymbol.Asset, new Dictionary<TimeFrame, List<object>>()
    {
      {
        defSymbol.TimeFrame,
        new List<object>()
      }
    });
    }

    public void chSeriesValue(chSeries chSeries, double val)
    {
        if (!ShowChart) return;
        if (chSeries.Type != SeriesType.Normal) return;
        chSeries.Add(val);
    }

    public void chSeriesRangeValue(chSeries chSeries, double val, double val2)
    {
        if (!ShowChart) return;
        if (chSeries.Type != SeriesType.Normal) return;
        chSeries.Add(val, val2);
    }

    public void chSeriesPrevValue(chSeries chSeries, double val)
    {
        if (!ShowChart) return;
        if (chSeries.Type != SeriesType.Normal) return;
        chSeries.vals[chSeries.vals.Count - 1] = val;
    }

    private SymbolData defSymbol;

    public chSeries chSeries(string chName)
    {
        if (!ShowChart) return new chSeries();
        chName = GetSeriesName(chName);
        var chSeries = new chSeries
        {
            Name = chName,
            TF = defSymbol.TimeFrame,
            Asset = defSymbol.Asset,
            Color = Color.Green,
            LineType = LineType.Solid,
            LineSize = 2,
            PaneName = ""
        };

        chSeriesDict.Add(chSeries);
        return chSeries;
    }

    public chSeries chSeries(string chName, IRIndex<double> data, Color Color)
    {
        if (!ShowChart) return new chSeries();
        chName = GetSeriesName(chName);
        var chSeries = new chSeries
        {
            Name = chName,
            TF = defSymbol.TimeFrame,
            Asset = defSymbol.Asset,
            Color = Color,
            LineType = LineType.Solid,
            LineSize = 2,
            PaneName = ""
        };

        if (chSeries.Type == SeriesType.Normal) data.NewDataAction(chSeries.Add);

        chSeriesDict.Add(chSeries);
        return chSeries;
    }


    public chSeries chSeries(string chName, IRIndex<double> data, string PaneName, Color Color)
    {
        if (!ShowChart) return new chSeries();
        chName = GetSeriesName(chName);
        var chSeries = new chSeries
        {
            Name = chName,
            TF = defSymbol.TimeFrame,
            Asset = defSymbol.Asset,
            Color = Color,
            LineType = LineType.Solid,
            LineSize = 2,
            PaneName = PaneName
        };

        if (chSeries.Type == SeriesType.Normal) data.NewDataAction(chSeries.Add);

        chSeriesDict.Add(chSeries);
        return chSeries;
    }

    public chSeries chSeries(string chName, Color Color)
    {
        if (!ShowChart) return new chSeries();
        chName = GetSeriesName(chName);
        var chSeries = new chSeries
        {
            Name = chName,
            TF = defSymbol.TimeFrame,
            Asset = defSymbol.Asset,
            Color = Color,
            LineType = LineType.Solid,
            LineSize = 2,
            PaneName = ""
        };

        chSeriesDict.Add(chSeries);
        return chSeries;
    }


    public chSeries chSeries(string chName, string PaneName, Color Color)
    {
        if (!ShowChart) return new chSeries();
        chName = GetSeriesName(chName);
        var chSeries = new chSeries
        {
            Name = chName,
            TF = defSymbol.TimeFrame,
            Asset = defSymbol.Asset,
            Color = Color,
            LineType = LineType.Solid,
            LineSize = 2,
            PaneName = PaneName
        };

        chSeriesDict.Add(chSeries);
        return chSeries;
    }

    string GetSeriesName(string chName)
    {
        if (chSeriesDict.FindIndex(s => s.Name == chName) != -1)
        {
            etc.prn("Series with name " + chName + " already exits!");
            return chName + " " + etc.rnd.Next(0, 10);
        }
        return chName;
    }

    public chSeries chSeries(string chName, string ChartPaneName, Color color, LineType LineType, int LineSize)
    {
        if (!ShowChart) return new chSeries();

        chName = GetSeriesName(chName);

        var chSeries = new chSeries
        {
            Name = chName,
            Color = color,
            LineSize = LineSize,
            LineType = LineType,
            PaneName = ChartPaneName,
            TF = defSymbol.TimeFrame,
            Asset = defSymbol.Asset
        };

        chSeriesDict.Add(chSeries);
        return chSeries;
    }

    public InfoSerie RegisterBarsColoring(SymbolData Symbol)
    {
        var serie = new InfoSerie
        {
            Asset = Symbol.Asset,
            TF = Symbol.TimeFrame,
            vals = new List<object>(),
            Name = "@BarsColoring"
        };
        chInfoSeries.Add(serie);
        return serie;
    }


    public InfoSerie RegisterBarsColoring(string Asset, TimeFrame TF)
    {
        var serie = new InfoSerie
        {
            Asset = Asset,
            TF = TF,
            vals = new List<object>(),
            Name = "@BarsColoring"
        };
        chInfoSeries.Add(serie);
        return serie;
    }

    public InfoSerie newInfoSerie(string Name)
    {
        var serie = new InfoSerie
        {
            TF = defSymbol.TimeFrame,
            Asset = defSymbol.Asset,
            vals = new List<object>(),
            Name = Name
        };
        chInfoSeries.Add(serie);
        return serie;
    }

    public void SetBarColor(InfoSerie BarsSerie, Color color)
    {
        if (color.IsNamedColor) InfoValue(BarsSerie, color.Name);
        else InfoValue(BarsSerie, color.R + @"\, " + color.G + @"\, " + color.B);
    }

    public void InfoValue(InfoSerie Serie, object value)
    {
        if (!ShowChart) return;

        Serie.Add(value);
    }

    public chSeries chSeries(string chName, IRIndex<double> data, string ChartPaneName, Color color, LineType LineType, int LineSize)
    {

        chName = GetSeriesName(chName);

        if (!ShowChart) return new chSeries();
        var chSeries = new chSeries
        {
            Name = chName,
            Color = color,
            LineSize = LineSize,
            LineType = LineType,
            PaneName = ChartPaneName,
            TF = defSymbol.TimeFrame,
            Asset = defSymbol.Asset
        };

        if (chSeries.Type == SeriesType.Normal) data.NewDataAction(chSeries.Add);
        chSeriesDict.Add(chSeries);
        return chSeries;
    }

    public chAnnotation AddAnnotation(DateTime datetime, double price, string text, Color color)
    {
        if (!ShowChart) return new chAnnotation();
        var ann = new chAnnotation { Color = color, pos = datetime, Text = text, Price = price };
        
        chMarkers[defSymbol.Asset][defSymbol.TimeFrame].Add(ann);
        return ann;
    }

    public chLabel AddLabel(DateTime datetime, double price, string text, Color color)
    {
        if (!ShowChart) return new chLabel();
        var ann = new chLabel { Color = color, pos = datetime, Text = text, Price = price };
        chMarkers[defSymbol.Asset][defSymbol.TimeFrame].Add(ann);
        return ann;
    }


    public chAnnotation AddArrowUp(DateTime datetime, double price, Color color)
    {
        if (!ShowChart) return new chAnnotation();
        var ann = new chAnnotation { Color = color, pos = datetime, Text = "", Price = price };
        ann.Type = AnnotationTypes.ArrowUp;
        chMarkers[defSymbol.Asset][defSymbol.TimeFrame].Add(ann);
        return ann;
    }

    public chAnnotation AddArrowDown(DateTime datetime, double price, Color color)
    {
        if (!ShowChart) return new chAnnotation();
        var ann = new chAnnotation { Color = color, pos = datetime, Text = "", Price = price };
        ann.Type = AnnotationTypes.ArrowDown;
        chMarkers[defSymbol.Asset][defSymbol.TimeFrame].Add(ann);
        return ann;
    }

    public chLabel AddLabel(string text, Color color)
    {
        return AddLabel(Dates.val, (Open.val + Close.val) / 2, text, color);
    }


    public chVerticalLine AddVerticalLine(DateTime datetime, string text, Color color)
    {
        if (!ShowChart) return new chVerticalLine();
        var ann = new chVerticalLine { Color = color, pos = datetime, Text = text };
        chMarkers[defSymbol.Asset][defSymbol.TimeFrame].Add(ann);
        return ann;
    }


    public chAnnotation AddAnnotation(string text, Color color)
    {
        return AddAnnotation(Dates.val, (Open.val + Close.val) / 2, text, color);
    }

    public chAnnotation AddAnnotation(string text)
    {
        return AddAnnotation(text, Color.White);
    }

    public chLine DrawLine(DateTime x1, double y1, DateTime x2, double y2, Color color, LineType LineType, int LineSize)
    {
        if (!ShowChart) return new chLine();
        return DrawPaneLine("", x1, y1, x2, y2, color, LineType, LineSize);
    }

    public chLine DrawPaneLine(string PaneName, DateTime x1, double y1, DateTime x2, double y2, Color color, LineType LineType, int LineSize)
    {
        if (!ShowChart) return new chLine();
        var line = new chLine
        {
            Color = color,
            x1 = x1,
            x2 = x2,
            y1 = y1,
            y2 = y2,
            Size = LineSize,
            LineType = LineType,
            Pane = PaneName
        };
        chMarkers[defSymbol.Asset][defSymbol.TimeFrame].Add(line);
        return line;
    }

    public void SetLineTimeEnd(chLine Line, DateTime xLast)
    {
        Line.x2 = xLast;
    }

    public void SetLineEnd(chLine Line, DateTime xLast, double yLast)
    {
        Line.x2 = xLast;
        Line.y2 = yLast;
    }

    public void SaveChartData(ref TradingResults Results)
    {
        Results.chSeries = chSeriesDict;
        Results.chMarkers = chMarkers;
        Results.chInfoSeries = chInfoSeries;
    }


}

