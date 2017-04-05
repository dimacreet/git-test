using System;
using System.Collections.Generic;
using System.Reflection;
using TradeFramework;

public enum PositionCharting { None, Default, Entry, withCloseReason }

public enum ResultsPurpose { View, Optimization }

public enum EODPosBehaviour { None, CloseAtEOD, ReopenAfterEOD, ReopenAfterEODSLCheck }

public static class _paramsGlobal
{

    public static int TimeTestIterations = 10;
    public static bool DoTimeTest = false;

    //RE settings
    public static bool ShowChart = true;
    public static PositionCharting PositionCharting = PositionCharting.Default;
    public static bool DynamicPositionExpoChart = true;
    public static bool DynamicPositionExpoChartPct = false;

    public static string ProjectName = "View";

    public static bool RunParallel = false;

    //public static int PreviousRangeBars = 30;
    //public static PeriodBreak PeriodBreakType = PeriodBreak.None;

    //Results
    public static SavingResultsMode SavingResultsMode = SavingResultsMode.Full;

    //Automatic
    public static ResultsPurpose ResultsPurpose = ResultsPurpose.View;

    public static bool DeleteWriteSQLIndicators = false;
    public static DateTime? SQLIndicatorDelimiterDate = null;

}

public enum PeriodBreak { None, Day, Month, HalfYear, Year }
public enum SavingResultsMode { None, Numbers, Full, RoughCharts, Optimization }

[Serializable]
public class _params
{
    public EODPosBehaviour EODPosBehaviour = EODPosBehaviour.None;

    public int StatSaverSavingFactor = 1;

    public double TradeCapital = 150000;
    public bool Reinvest = false;

    //public bool DrawdownFromAnyHigh = false;

    public double FixedLot = 0.1d;

    public bool FixedEquityDrawdown = true;

    public bool RoundLot = false;

    public bool Size2Capital = true;

    public bool MFEMAEFromClose = false;

    public Type Strategy;

    public bool DynamicPositions = false;
    public int SizeLimit = 100;

    //public bool StopLossOnClose = false;
    //public bool TakeProfitOnClose = false;

    public override string ToString()
    {

        var sw_dict = new stringDict();

        foreach (var field in GetType().GetFields())
        {
            if (field.Name != "Strategy")
                sw_dict[field.Name] = field.GetValue(this);
        }
        return sw_dict.ToString();
    }

    public void CastVals(ref object objParams)
    {
        var Params = (_params)objParams;

        Params.EODPosBehaviour = EODPosBehaviour;
        Params.TradeCapital = TradeCapital;
        Params.Reinvest = Reinvest;

        Params.FixedLot = FixedLot;

        Params.FixedEquityDrawdown = FixedEquityDrawdown;

        Params.RoundLot = RoundLot;

        Params.Size2Capital = Size2Capital;

        Params.Strategy = Strategy;

        Params.DynamicPositions = DynamicPositions;
        Params.SizeLimit = SizeLimit;

        //Params.StopLossOnClose = StopLossOnClose;
        //Params.TakeProfitOnClose = TakeProfitOnClose;

    }
}

