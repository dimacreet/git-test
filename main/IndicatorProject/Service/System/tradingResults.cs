
using System;
using System.Collections.Generic;
using System.Linq;
using TradeFramework;
using TradeFramework.ChartObjects;

[Serializable]
public class TradingResults
{
    public double
        GrossProfit,
        GrossLoss,
        NetProfit,
        MaxGain,
        MaxLoss,
        SizeTotal,
        MaxLongSize,
        MaxShortSize,
        PosCount,
        PosCountWins,
        PosCountLoose,
        PosCountZero,
        PosCountOrder,
        PosCountEOD,
        PosCountStopLoss,
        PosCountTakeProfit,
        PosCountInverse,
        MaxDD,
        AvgDD,
        DayProfitStDev;

    public List<double> Exposure = new List<double>();
    public List<double> Account = new List<double>();

    public List<double> Balance = new List<double>();

    public List<double> Drawdown = new List<double>();

    public List<DateTime> xDT = new List<DateTime>();

    public List<binPosition> Positions = new List<binPosition>();

    public List<InfoSerie> chInfoSeries = new List<InfoSerie>();
    public List<chSeries> chSeries = new List<chSeries>();
    public Dictionary<string, Dictionary<TimeFrame, List<object>>> chMarkers = new Dictionary<string, Dictionary<TimeFrame, List<object>>>();

    public override string ToString()
    {
        var sw_dict = new stringDict();

        foreach (var field in GetType().GetFields())
        {
            if (field.FieldType == typeof(double))
                sw_dict[field.Name] = field.GetValue(this);
        }
        return sw_dict.ToString();
    }

    public void ProcessResults(ExecParams ExecParams)
    {

        //System.Diagnostics.Debugger.Break();

        if (_paramsGlobal.SavingResultsMode == SavingResultsMode.None)
            return;

        /*if (_paramsGlobal.ResultsPurpose == ResultsPurpose.View)
            _paramsGlobal.ProjectName = "View";*/

        var ProjectName = _paramsGlobal.ProjectName;

        int ProjectId = 0;

        var ForProjId = etc.db.get("SELECT MAX(ProjectId) as ProjectId FROM Results WHERE Project = '" + ProjectName + "'");

        ForProjId.Read();

        if (!ForProjId.isNull("ProjectId"))
            ProjectId = ForProjId.num("ProjectId") + 1;

        ForProjId.Read();

        if (_paramsGlobal.SavingResultsMode == SavingResultsMode.Numbers)
        {
            var blobs = new Dictionary<string, byte[]>{
                {"@params", etc.Serialize2Mem(ExecParams.Params)}
            };

            etc.db.loadBLOBs(
                String.Format(
                    @"INSERT INTO Results (RunDT, Project, Strategy, Symbol, Params, Results, TaskName, ParamObject, ProjectId) 
                VALUES (GETDATE(),'{0}','{1}','{2}','{3}','{4}','{5}',@params, {6})",
                    ProjectName,
                    (ExecParams.Params).Strategy,
                    ExecParams.defSymbol.Name,
                    ExecParams.Params,
                    ToString(),
                    ExecParams.TaskName,
                    ProjectId
                    ), blobs);
        }


        if (_paramsGlobal.SavingResultsMode == SavingResultsMode.Full ||
            _paramsGlobal.SavingResultsMode == SavingResultsMode.RoughCharts
            )
        {

            var ChartData = new ChartData
            {
                xDT = xDT,
                Account = Account,
                Drawdown = Drawdown,
                Exposure = Exposure,
                chSeries = chSeries,
                chMarkers = chMarkers,
                InfoSeries = chInfoSeries,
                Symbols = ExecParams.Symbols.Select(s => s.Name).ToList()
            };

            var blobs = new Dictionary<string, byte[]>{
                {"@params", etc.Serialize2Mem(ExecParams.Params)},
                {"@chart", etc.Serialize2Mem(ChartData)},
                {"@positions", etc.Serialize2Mem(Positions)}
            };

            etc.db.loadBLOBs(
                String.Format(
                    @"INSERT INTO Results (RunDT, Project, Strategy, Symbol, Params, Results, TaskName, ParamObject, ChartData, PositionData, ProjectId) VALUES (GETDATE(),'{0}','{1}','{2}','{3}','{4}','{5}',@params,@chart,@positions, {6})",
                    ProjectName,
                    (ExecParams.Params).Strategy,
                    ExecParams.defSymbol.Name,
                    ExecParams.Params,
                    ToString(),
                    ExecParams.TaskName,
                    ProjectId
                    ),
                blobs);
        }
    }
}