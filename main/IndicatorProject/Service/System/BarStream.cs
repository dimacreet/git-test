using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TradeFramework;


public class BarStream
{

    public string Asset;
    public TimeFrame TF;

    public IRIndex<BarData> Bars;
    public IRIndex<double> Low;
    public IRIndex<double> High;
    public IRIndex<double> Close;
    public IRIndex<double> Open;
    public IRIndex<DateTime> Dates;

    public delegate void NewBarDelegate();

    public event NewBarDelegate eventNewBar;

    public SymbolData Symbol;

    public BarStream(SymbolData Symbol)
    {
        Asset = Symbol.Asset;
        TF = Symbol.TimeFrame;

        if (!Symbol.BarsArrayUpdated) Symbol.UpdateBarsArray();

        this.Symbol = Symbol;

        Bars = RIndexWrapper.Create(Symbol.BarsArray);
        Open = RIndexWrapper.Create(Symbol.OpenArray);
        High = RIndexWrapper.Create(Symbol.HighArray);
        Low = RIndexWrapper.Create(Symbol.LowArray);
        Close = RIndexWrapper.Create(Symbol.CloseArray);
        Dates = RIndexWrapper.Create(Symbol.DatesArray);

        Bars.NewDataAction(NewBar_for_event);
    }

    void NewBar_for_event(BarData b)
    {
        if (eventNewBar != null)
            eventNewBar();
    }

    public void NewBarNoEvent()
    {
        ((RIndexWrapper<BarData>)Bars).Next_no_event();
        ((RIndexWrapper<double>)Open).Next_no_event();
        ((RIndexWrapper<double>)High).Next_no_event();
        ((RIndexWrapper<double>)Low).Next_no_event();
        ((RIndexWrapper<double>)Close).Next_no_event();
        ((RIndexWrapper<DateTime>)Dates).Next_no_event();
    }

    public void NewBarCommitEvent()
    {
        ((RIndexWrapper<BarData>)Bars).NextCommitEvent();
        ((RIndexWrapper<double>)Open).NextCommitEvent();
        ((RIndexWrapper<double>)High).NextCommitEvent();
        ((RIndexWrapper<double>)Low).NextCommitEvent();
        ((RIndexWrapper<double>)Close).NextCommitEvent();
        ((RIndexWrapper<DateTime>)Dates).NextCommitEvent();
    }
}