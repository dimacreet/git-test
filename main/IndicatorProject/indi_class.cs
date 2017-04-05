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

public class IndicatorState
{




    public string Name;
    public Color Color;
    protected string Asset;
    protected TimeFrame TF;
    protected StrategyBasePortfolio strategy;

    public virtual State State
    {
        get { return inState; }
    }

    public virtual void ShowChart()
    {
        
    }

    public virtual StateSignal Signal
    {
        get { return inSignal; }
    }

    protected State inState = State.Neutral;
    protected StateSignal inSignal = StateSignal.Neutral;

    public IndicatorState(StrategyBasePortfolio strategy, string Asset, TimeFrame TF = TimeFrame.D)
    {
        this.strategy = strategy;
        this.Asset = Asset;
        this.TF = TF;
    }

}

public class ReportNode
{
    public string Asset;
    public TimeFrame TF;
    public string type;
    public List<IndicatorState> IndicatorStates;

    public ReportNode(string name, string type)
    {
        this.Asset = name;
        this.type = type;
        IndicatorStates = new List<IndicatorState>();
    }

    public ReportNode(string name, string type, TimeFrame tf,List<IndicatorState> indicators)
    {
        Asset = name;
        this.type = type;
        IndicatorStates = indicators;
        TF = tf;
    }
}
