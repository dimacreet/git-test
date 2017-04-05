using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Drawing2D;
using TradeFramework;

public class PositionClass
{
    public string Asset;
    public TimeFrame TF;

    public double FX_Slippage = 0d;

    public AssetType AssetType = AssetType.Default;

    public IRIndex<BarData> FXCrossAssetBars = null;

    //FX feature
    private bool SlippageOnlyOpen = true;

    public PositionClass(IRIndex<BarData> Bars, _params LaunchParams,  SymbolData sb, IRIndex<BarData> FXCrossAssetBars = null)
    {
        this.TF = sb.TimeFrame;
        _params = LaunchParams;
        curCash = _params.TradeCapital;
        LastEquityHigh = _params.TradeCapital;
        this.Bars = Bars;
        this.Asset = sb.Asset;
        FX_Slippage = sb.Slippage;
        this.AssetType = sb.AssetType;
        this.FXCrossAssetBars = FXCrossAssetBars;
    }

    private _params _params;

    public List<OrderData> Orders = new List<OrderData>();
    public List<PositionData> OpenPositions = new List<PositionData>();
    List<PositionData> ClosingPositions = new List<PositionData>();
    public List<PositionData> HistoryPositions = new List<PositionData>();
    public bool inPosition { get { return OpenPositions.Count != 0; } }
    public PositionData LastPosition { get { return OpenPositions.LastOrDefault(); } }

    public double Position
    {
        get
        {
            return OpenPositions.Sum(p => p.Size * (p.Long ? 1 : -1));
        }
    }

    #region execution

    public void NewTick(BarData bar)
    {
        var curOpen = bar.Open;
        var curDT = bar.DateTime;
        var curClose = bar.Close;

        // Order execution
        var swapOrders = new List<OrderData>(Orders);

        foreach (var order in swapOrders)
        {
            // Closing positions from order
            if (order.ClosingPosition != null)
            {
                var pos = order.ClosingPosition;
                pos.CloseReason = order.CloseReason;
                Orders.Remove(order);

                if (!OpenPositions.Contains(pos)) continue;

                pos.ClosePrice = curOpen;
                pos.CloseDT = curDT;
                ClosingPositions.Add(pos);
                ClosePositions();
                continue;
            }

            double OrderFillPrice = 0;

            if (order.BarsValid > 0)
                order.BarsValid--;

            if (order.type == myOrderType.Market) OrderFillPrice = curOpen;
            else
            {

                if (order.type == myOrderType.StopOrOpen &&
                    ((order.Long && curOpen > order.price) ||
                     (!order.Long && curOpen < order.price)))
                    OrderFillPrice = curOpen;
                else if (bar.inRange(order.price))
                {
                    OrderFillPrice = order.price;
                }

            }

            if (OrderFillPrice != 0)
            {
                //Order Behaviour

                if (order.Behaviour == OrderBehaviour.CloseAll)
                {
                    ClosePositionsNow(OrderFillPrice, CloseReason.AnotherPosition);
                    ClosePositions();
                }
                //--

                // Adding slippage to OrderFillPrice

                /*if (Slippage != 0)
                {
                    try {double slippage = Slippage} catch (Exception EX_NAME) {
System.Console.WriteLine(EX_NAME);
} * OrderFillPrice;
                    OrderFillPrice = OrderFillPrice + (order.Long ? 1 : -1) * slippage;

                    if (order.Long)
                    {
                        if (OrderFillPrice > Bars[0].High) OrderFillPrice = Bars[0].High;
                    }
                    else
                        if (OrderFillPrice < Bars[0].Low) OrderFillPrice = Bars[0].Low;

                }*/

                

                if (!order.BrutalPrice.isNaN())
                    OrderFillPrice = order.BrutalPrice;

                OrderFillPrice = OrderFillPrice + (order.Long ? 1 : -1) * FX_Slippage;

                // Size 
                if (order.Size.isNaN() && order.SizePct.isNaN())
                {
                    if (_params.Reinvest) order.SizePct = 100;
                    else
                    {
                        if (_params.FixedLot != 0)
                            order.Size = _params.FixedLot;
                        else
                        {
                            if (_params.Size2Capital)
                            {
                                order.Size = _params.TradeCapital / OrderFillPrice;
                            }
                            else
                            {
                                order.Size = ((_params.TradeCapital) / OrderFillPrice);
                                if (_params.RoundLot) order.Size = (int)order.Size;
                            }
                        }
                    }
                }

                if (!double.IsNaN(order.SizePct))
                {
                    order.Size = (((curCash * order.SizePct) / 100) / OrderFillPrice);
                    if (_params.RoundLot) order.Size = (int)order.Size;
                }

                // Adding to equity opened position
                if ( /*order.Size * OrderFillPrice <= curCash && */order.Size != 0)
                {
                    // Execute Position
                    var pos = new PositionData
                    {
                        OpenDT = curDT,
                        OpenPrice = OrderFillPrice,
                        TP = order.TP,
                        SL = order.SL,
                        BarsExit = order.BarsExit,
                        Long = order.Long,
                        MaxPrice = Bars[0].High,
                        MinPrice = Bars[0].Low,
                        BarsHeld = 0,
                        Size = order.Size,
                        Asset = Asset,
                        AssetType = AssetType
                    };

                    pos.CurrentQuotes = Bars;

                    if (AssetType == AssetType.FX_cross)
                        pos.FX_BaseQuotes = FXCrossAssetBars;

                    if (order.UserData != null) pos.UserData = order.UserData;

                    OpenPositions.Add(pos);
                    curCash -= order.Size * OrderFillPrice;
                    order.Executed(pos);

                    if (eventOrderFilled != null) eventOrderFilled(pos);

                    if (!order.BrutalExit.isNaN())
                    {
                        pos.ClosePrice = order.BrutalExit;
                        pos.CloseReason = CloseReason.PendingClose;
                        pos.CloseDT = curDT;
                        OpenPositions.Remove(pos);
                        ClosingPositions.Add(pos);
                        ClosePositions();
                    }
                }
                else throw new Exception("Position is zero!");
                Orders.Remove(order);
            }
            else
            {
                if (order.BarsValid == 0) Orders.Remove(order);
            }
        }
        //--

        ClosePositions();

        // Closing positions with BarsExit, SL, TP)
        foreach (var pos in OpenPositions)
        {
            pos.MaxPrice = Math.Max(pos.MaxPrice, Bars[0].High);
            pos.MinPrice = Math.Min(pos.MinPrice, Bars[0].Low);
            pos.BarsHeld++;

            // BarsExit
            if (pos.BarsExit == 0)
            {
                pos.ClosePrice = curClose;

                pos.CloseReason = CloseReason.BarsExit;
                ClosingPositions.Add(pos);
                continue;
            }

            if (pos.BarsExit > 0) pos.BarsExit--;




            // Stop Loss and Take Profit execution
            double SLprice = 0;
            double TPprice = 0;

            

            if (((pos.Long && curOpen < pos.SL) || (!pos.Long && curOpen > pos.SL)) && pos.BarsHeld > 1) SLprice = curOpen;
            else if (bar.inRange(pos.SL)) SLprice = pos.SL;

            if (((pos.Long && curOpen > pos.TP) || (!pos.Long && curOpen < pos.TP)) && pos.BarsHeld > 1) TPprice = curOpen;
            else if (bar.inRange(pos.TP)) TPprice = pos.TP;

            bool SLviolation = SLprice != 0;
            bool TPviolation = TPprice != 0;

            if (SLviolation && !(OptimisticTP && SLviolation && TPviolation))
            {
                pos.ClosePrice = SLprice;
                pos.CloseReason = CloseReason.StopLoss;
                ClosingPositions.Add(pos);

                continue;
            }

            if (TPviolation && !(!OptimisticTP && SLviolation && TPviolation))
            {
                pos.ClosePrice = TPprice;
                pos.CloseReason = CloseReason.TakeProfit;
                ClosingPositions.Add(pos);

                continue;
            }
        }

        ClosePositions();

        foreach (var pos in OpenPositions)
        {
            pos.ClosePrice = Bars.val.Close;
            pos.CloseDT = Bars.val.DateTime;
        }


    }

    public void UpdateOpenPositions()
    {
        foreach (var pos in OpenPositions)
        {
            pos.ClosePrice = Bars.val.Close;
            pos.CloseDT = Bars.val.DateTime;
        }

    }

    public void Execution()
    {
        #region technical

        DateTime curDT = Bars[0].DateTime;
        double curOpen = Bars[0].Open;

        #endregion

        //Closing EOD
        if (_params.EODPosBehaviour != EODPosBehaviour.None && Bars.Count > 1 && curDT.Day != Bars.LookBack(1).DateTime.Day)
        {
            double EODprice = Bars.LookBack(1).Close;
            List<PositionData> NowPositions = null;

            if (_params.EODPosBehaviour == EODPosBehaviour.ReopenAfterEOD || _params.EODPosBehaviour == EODPosBehaviour.ReopenAfterEODSLCheck
                ) NowPositions = new List<PositionData>(OpenPositions);

            ClosePositionsNow(EODprice, CloseReason.EOD);
            foreach (var pos in ClosingPositions)
            {
                pos.ClosePrice = EODprice;
                pos.CloseDT = Bars.LookBack(1).DateTime;
                pos.CloseReason = CloseReason.EOD;
            }
            Orders.Clear();

            if (_params.EODPosBehaviour == EODPosBehaviour.ReopenAfterEOD || _params.EODPosBehaviour == EODPosBehaviour.ReopenAfterEODSLCheck
                )
            {
                var Open = Bars.val.Open;
                foreach (var pos in NowPositions)
                {
                    if (_params.EODPosBehaviour == EODPosBehaviour.ReopenAfterEODSLCheck)
                    {
                        if (!pos.SL.isNaN() && ((pos.Long && Open < pos.SL) || (!pos.Long && Open > pos.SL))) continue;
                        if (!pos.TP.isNaN() && ((pos.Long && Open > pos.TP) || (!pos.Long && Open < pos.TP))) continue;
                    }

                    var order = MarketOrder(pos.Long, pos.UserData);
                    order.SL = pos.SL;
                    order.TP = pos.TP;
                    order.Size = pos.Size;
                }
            }
        }
        else
        {
            // Setting price for positions closed at previous bar
            foreach (var pos in ClosingPositions)
                if (pos.ClosePrice.isNaN()) pos.ClosePrice = curOpen;
        }
        ClosePositions();

        NewTick(Bars[0]);

        /*curEquity = 0;
        curExposure = 0;
        foreach (var pos in OpenPositions)
        {
            curEquity += (pos.OpenPrice + (pos.Long ? 1 : -1) * (Bars[0].Close - pos.OpenPrice)) * pos.Size;
            curExposure += pos.Size * (pos.Long ? 1 : -1);
        }

        /*if (_paramsGlobal.SavingResultsMode != SavingResultsMode.None)
        {
            SavingResultsRoutine();
        }/**/
    }

    private double curExposure = 0;

    #region CloseOrders
    private void ClosePositions()
    {
        if (ClosingPositions.Count == 0) return;
        foreach (var pos in ClosingPositions)
        {

            if (pos.CloseReason == CloseReason.PendingClose)
                pos.CloseReason = CloseReason.Order;

            if (pos.CloseReason != CloseReason.EOD)
                pos.CloseDT = Bars[0].DateTime;

            if (pos.CloseReason != CloseReason.Order && pos.CloseReason != CloseReason.EOD && pos.CloseReason != CloseReason.AnotherPosition)
                OpenPositions.Remove(pos);

            HistoryPositions.Add(pos);

            // slippage
            /*if (Slippage != 0)
            {
                double slippage = Slippage * pos.ClosePrice;

                pos.ClosePrice = pos.ClosePrice + (pos.Long ? -1 : 1) * slippage;

                if (pos.Long)
                {
                    if (pos.ClosePrice < Bars[0].Low) pos.ClosePrice = Bars[0].Low;
                }
                else if (pos.ClosePrice > Bars[0].High) pos.ClosePrice = Bars[0].High;
            }/**/


            curCash += (pos.OpenPrice + (pos.Long ? 1 : -1) * (pos.ClosePrice - pos.OpenPrice)) * pos.Size;
        }

        ClosingPositions.Clear();
    }

    public void CloseAllOrder()
    {
        CloseAllOrder(null, false, double.NaN);
    }

    public void CloseAllAtBarClose()
    {
        CloseAllOrder(null, true, double.NaN);
    }

    public void CloseAllAtBarClose(stringDict UserData)
    {
        CloseAllOrder(UserData, true, double.NaN);
    }

    public void CloseAllOrder(stringDict UserData)
    {
        CloseAllOrder(UserData, false, double.NaN);
    }

    public void CloseAllOrder(stringDict UserData, bool CloseAtBarClose)
    {
        CloseAllOrder(UserData, CloseAtBarClose, double.NaN);
    }

    public void CloseAtBrutalPrice(double BrutalPrice)
    {
        CloseAllOrder(null, false, BrutalPrice);
    }

    public void CancelAllOrders()
    {
        Orders.Clear();
    }

    public bool OrdersExists { get { return Orders.Count != 0; } }

    public void CloseAllOrder(stringDict UserData, bool CloseAtBarClose, double BrutalPrice)
    {
        if (OpenPositions.Count == 0) return;

        foreach (var pos in OpenPositions)
        {
            pos.CloseReason = CloseReason.PendingClose;
            if (UserData != null)
            {
                if (pos.UserData == null) pos.UserData = UserData;
                else pos.UserData.AddDict(UserData);
            }
            if (CloseAtBarClose)
            {
                pos.ClosePrice = Bars.val.Close;
                pos.CloseDT = Bars.val.DateTime;
            }

            if (!BrutalPrice.isNaN())
            {
                pos.ClosePrice = BrutalPrice;
                pos.CloseDT = Bars.val.DateTime;
            }

            ClosingPositions.Add(pos);
        }
        if (CloseAtBarClose || !BrutalPrice.isNaN())
            ClosePositions();

        OpenPositions.Clear();
    }

    private void ClosePositionsNow(double price, CloseReason reason)
    {

        foreach (var pos in OpenPositions)
        {
            pos.CloseReason = reason;
            pos.ClosePrice = price;
            ClosingPositions.Add(pos);
        }

        OpenPositions.Clear();
    }
    #endregion

    #endregion

    #region SavingResults

    public double MaxDD = 0;
    private double AverageDD = 0;
    private double AverageDD_divider = 0;

    public IRIndex<BarData> Bars;

    public void SaveStatistics()
    {
        if (OpenPositions.Count != 0)
        {
            foreach (var pos in OpenPositions)
            {
                pos.CloseDT = Bars.val.DateTime;
                pos.ClosePrice = Bars.val.Close;
                pos.CloseReason = CloseReason.Open;
                HistoryPositions.Add(pos);
            }
        }
    }


    #endregion

    #region Technical
    public bool OptimisticTP = false; // if true we will take TP, not SL if TP and SL alltogether inside one bar

    public List<double> Exposures = new List<double>();
    public List<double> Drawdowns = new List<double>();
    public List<double> Account = new List<double>();

    double LastEquityHigh = double.NaN;

    public  double curCash;
    public double curEquity = 0;

    public delegate void OrderFilledHandler(PositionData pos);

    public event OrderFilledHandler eventOrderFilled;

    #endregion

    #region Orders
    public OrderData AddOrder(OrderData order)
    {
        // write here checking TODO 
        Orders.Add(order);
        return order;
    }

    public void ClosePositionOrder(PositionData pos, double BrutalPrice = double.NaN)
    {
        OpenPositions.Remove(pos);
        pos.CloseReason = CloseReason.Order;
        ClosingPositions.Add(pos);

        if (!BrutalPrice.isNaN())
        {
            pos.ClosePrice = BrutalPrice;
            pos.CloseDT = Bars.val.DateTime;
        }

        ClosePositions();
    }

    public OrderData MarketOrderCloseAll(bool Long, stringDict UserData)
    {
        return MarketOrderCloseAll(Long, UserData, null);
    }

    public OrderData StopOrOpenOrder(bool Long, double StopPrice, stringDict UserData)
    {
        return StopOrOpenOrder(Long, StopPrice, UserData, null);
    }

    public OrderData StopOrOpenOrder(bool Long, double StopPrice, stringDict UserData, object Tag)
    {
        var order =
            new OrderData
            {
                BarsExit = -1,
                BarsValid = 1,
                Long = Long,
                type = myOrderType.Stop,
                price = StopPrice,
                Behaviour = OrderBehaviour.CloseAll
            };

        if (UserData != null) order.UserData = UserData;

        Orders.Add(order);
        return order;
    }

    public OrderData MarketOrderCloseAll(bool Long, stringDict UserData, object Tag)
    {

        var order =
            new OrderData
            {
                BarsExit = -1,
                BarsValid = 1,
                Long = Long,
                type = myOrderType.Market,
                Behaviour = OrderBehaviour.CloseAll
            };

        if (UserData != null) order.UserData = UserData;

        Orders.Add(order);
        return order;
    }

    public OrderData MarketOrder(bool Long)
    {
        return MarketOrder(Long, null);
    }

    public OrderData MarketOrder(bool Long, stringDict UserData)
    {

        var order =
            new OrderData
            {
                BarsExit = -1,
                BarsValid = 1,
                Long = Long,
                type = myOrderType.Market,
                Behaviour = OrderBehaviour.None
            };

        if (UserData != null) order.UserData = UserData;

        Orders.Add(order);
        return order;
    }

    public OrderData MarketOrderCloseAll(bool Long)
    {
        return MarketOrderCloseAll(Long, null);
    }

    public OrderData StopOrOpenOrder(bool Long, double price)
    {
        var order =
            new OrderData
            {
                BarsExit = -1,
                BarsValid = 1,
                Long = Long,
                price = price,
                type = myOrderType.StopOrOpen,
                Behaviour = OrderBehaviour.CloseAll
            };
        Orders.Add(order);
        return order;
    }

    public OrderData StopOrder(bool Long, double price)
    {
        var order =
            new OrderData
            {
                BarsExit = -1,
                BarsValid = 1,
                Long = Long,
                price = price,
                type = myOrderType.Stop,
                Behaviour = OrderBehaviour.None
            };
        Orders.Add(order);
        return order;
    }

    public OrderData StopOrderTillCancel(bool Long, double price)
    {
        var order =
            new OrderData
            {
                BarsExit = -1,
                BarsValid = -1,
                Long = Long,
                price = price,
                type = myOrderType.Stop,
                Behaviour = OrderBehaviour.None
            };
        Orders.Add(order);
        return order;
    }

    public OrderData StopOrderCloseAll(bool Long, double price)
    {
        var order =
            new OrderData
            {
                BarsExit = -1,
                BarsValid = 1,
                Long = Long,
                price = price,
                type = myOrderType.Stop,
                Behaviour = OrderBehaviour.CloseAll
            };
        Orders.Add(order);
        return order;
    }

    #endregion


}

/*
if (_params.StopLossOnClose && !double.IsNaN(pos.SL) && ((pos.Long && pos.SL >= curClose) || (!pos.Long && pos.SL <= curClose)))
{
    var order = new OrderData
    {
        ClosingPosition = pos,
        CloseReason = CloseReason.StopLoss
    };
    Orders.Add(order);

}

if (_params.TakeProfitOnClose && !double.IsNaN(pos.TP) && ((pos.Long && pos.TP <= curClose) || (!pos.Long && pos.TP >= curClose)))
{
    var order = new OrderData
    {
        ClosingPosition = pos,
        CloseReason = CloseReason.TakeProfit
    };
    Orders.Add(order);

}/**/

/*public OrderData AddEntry(bool Long, double price) {
    var order = new OrderData{
        BarsExit = 1,
        BarsValid = 1,
        Long = Long,
        Size = 1,
        price = price,
        type = myOrderType.StopOrOpen,
        Behaviour = OrderBehaviour.CloseAll
    };
    Orders.Add(order);
    return order;
}

public OrderData AddEntry(bool Long)
{
    var order = new OrderData{
        BarsExit = 1,
        BarsValid = 1,
        Long = Long,
        Size = 1,
        type = myOrderType.Market,
        Behaviour = OrderBehaviour.CloseAll
    };

    Orders.Add(order);
    return order;
}*/
