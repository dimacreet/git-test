using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TradeFramework;

public class StatSaver
{
    private Dictionary<string, PositionClass> Positions;

    private Dictionary<string, int> HistPositionsCount = new Dictionary<string, int>();
    private Dictionary<string, double> RealizedProfits = new Dictionary<string, double>();

    public double TotalRealizedProfit;

    private int SavingFactor = -1;
    private int SavingCounter = -1;

    private Func<PositionData, bool> PositionCheck;

    private List<DateTime> FullxDT;
    private List<DateTime> xDT = new List<DateTime>();

    private _params _params;

    private int counter = 0;

    public StatSaver(Func<PositionData, bool> PositionCheck)
    {
        this.PositionCheck = PositionCheck;
    }

    public void Prepare(List<DateTime> xDT, Dictionary<string, PositionClass> Positions, _params _params)
    {
        this.FullxDT = xDT;
        this.Positions = Positions;
        this._params = _params;

        foreach (var posClass in Positions)
        {
            HistPositionsCount.Add(posClass.Key, 0);
            RealizedProfits.Add(posClass.Key, 0);
        }


        SavingFactor = _params.StatSaverSavingFactor;

        if (SavingFactor != -1)
            SavingCounter = SavingFactor;
    }

    public StatSaver(List<DateTime> xDT, Dictionary<string, PositionClass> Positions, _params _params)
    {
        Prepare(xDT, Positions, _params);
    }

    public TradingResults getTradingResults()
    {
        var Results = new TradingResults();

        Results.Account = Account;
        Results.Drawdown = Drawdowns;
        Results.Exposure = Exposures;
        Results.Balance = Balance;
        Results.xDT = xDT;
        Results.MaxDD = MaxDD;
        Results.AvgDD = AverageDD / AverageDD_divider;

        var Positions = PositionCheck != null
            ? this.Positions.Values.SelectMany(x => x.HistoryPositions.Where(pos => PositionCheck(pos))).ToList()
            : this.Positions.Values.SelectMany(x => x.HistoryPositions).ToList();

        if (Positions.Count == 0) return Results;

        Positions.ForEach(pos => Results.Positions.Add(
                                     new binPosition
                                     {
                                         CloseDT = pos.CloseDT,
                                         ClosePrice = pos.ClosePrice,
                                         CloseReason = pos.CloseReason,
                                         Long = pos.Long,
                                         OpenDT = pos.OpenDT,
                                         OpenPrice = pos.OpenPrice,
                                         Size = pos.Size,
                                         MFE = pos.MFE,
                                         MAE = pos.MAE,
                                         BarsHeld = pos.BarsHeld,
                                         Profit = pos.Profit,
                                         UserData = pos.UserData,
                                         Asset = pos.Asset
                                     }));

        var Wins = Positions.Where(pos => pos.Profit > 0).ToList();
        var Loose = Positions.Where(pos => pos.Profit < 0).ToList();

        var WinsProfits = (from pos in Wins select pos.Profit).ToList();
        var LooseProfits = (from pos in Loose select pos.Profit).ToList();
        var ZeroProfitsCount = Positions.Count - Wins.Count() - Loose.Count();

        double GrossProfit = WinsProfits.Sum();
        double GrossLoss = Math.Abs(LooseProfits.Sum());

        double AllPosCount = Positions.Count;

        Results.GrossProfit = WinsProfits.Sum();
        Results.GrossLoss = Math.Abs(LooseProfits.Sum());
        Results.NetProfit = GrossProfit - GrossLoss;

        if (WinsProfits.Count > 0)
            Results.MaxGain = WinsProfits.Max();

        if (LooseProfits.Count > 0)
            Results.MaxLoss = LooseProfits.Min();

        Results.SizeTotal = (from pos in Positions select pos.Size).Sum();

        Results.PosCount = AllPosCount;
        Results.PosCountWins = Wins.Count;
        Results.PosCountLoose = Loose.Count;
        Results.PosCountZero = ZeroProfitsCount;

        Results.PosCountOrder = Positions.Count(pos => pos.CloseReason == CloseReason.Order);
        Results.PosCountEOD = Positions.Count(pos => pos.CloseReason == CloseReason.EOD);
        Results.PosCountStopLoss = Positions.Count(pos => pos.CloseReason == CloseReason.StopLoss);
        Results.PosCountTakeProfit = Positions.Count(pos => pos.CloseReason == CloseReason.TakeProfit);
        Results.PosCountInverse = Positions.Count(pos => pos.CloseReason == CloseReason.AnotherPosition);

        return Results;
    }

    #region SavingResults

    public double MaxDD = 0;
    private double AverageDD = 0;
    private double AverageDD_divider = 0;

    public List<double> Exposures = new List<double>();
    public List<double> Drawdowns = new List<double>();
    public List<double> Account = new List<double>();
    public List<double> Balance = new List<double>();

    double LastEquityHigh = 0;
    private double curLowAccount = 0d;

    //public Dictionary<string, IRIndex<BarData>> Bars;

    public void SavingResultsRoutine()
    {

        double OpenProfitLow = 0;
        double curExposure = 0;

        foreach (var posClass in Positions.Values)
        {
            foreach (var pos in posClass.OpenPositions)
            {
                if (PositionCheck != null && !PositionCheck(pos)) continue;

                //OpenProfit += pos.Profit;
                OpenProfitLow += pos.ProfitLow;
                curExposure += pos.Size*(pos.Long ? 1 : -1);
            }
        }

        counter++; 
        if (SavingFactor != -1 && SavingCounter > 0)
        {
            SavingCounter--;

            curLowAccount = Math.Min(curLowAccount, OpenProfitLow);

            return;
        }

        //double _OpenProfit = 0;

        TotalRealizedProfit = 0d;

        foreach (var pair in Positions)
        {
            var posClass = pair.Value;
            var RealizedProfit = RealizedProfits[pair.Key];
            if (posClass.HistoryPositions.Count != HistPositionsCount[pair.Key])
            {
                for (int i = HistPositionsCount[pair.Key]; i < posClass.HistoryPositions.Count; i++){
                    var pos = posClass.HistoryPositions[i];
                    if (PositionCheck != null && !PositionCheck(pos)) continue;

                    RealizedProfit += pos.Profit;
                }

                RealizedProfits[pair.Key] = RealizedProfit;

                HistPositionsCount[pair.Key] = posClass.HistoryPositions.Count;
            }

            TotalRealizedProfit += RealizedProfit;
        }

        /*TotalRealizedProfit = 0d;

        foreach (var pair in Positions)
        {
            var posClass = pair.Value;
            foreach (var pos in posClass.HistoryPositions)
            {
                TotalRealizedProfit += pos.Profit;
            }

        }/**/

        var curAccount = OpenProfitLow + TotalRealizedProfit;
        
        if (curAccount > LastEquityHigh)
            LastEquityHigh = curAccount;

        double Drawdown = 0d;

        if (curAccount < LastEquityHigh)
            if (_params.FixedEquityDrawdown)
                Drawdown = ((curAccount - LastEquityHigh) / _params.TradeCapital) * 100;
            else
                Drawdown = ((curAccount - LastEquityHigh) / (LastEquityHigh + _params.TradeCapital)) * 100;

        Account.Add(curAccount);
        Drawdowns.Add(Drawdown);
        Exposures.Add(curExposure);

        MaxDD = Math.Min(MaxDD, Drawdown);
        AverageDD -= Drawdown;
        AverageDD_divider++;

        xDT.Add(FullxDT[counter-1]);

        Balance.Add(TotalRealizedProfit);

        SavingCounter = SavingFactor;
        curLowAccount = double.PositiveInfinity;
    }

    #endregion


}


