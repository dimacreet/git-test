#region technical

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TradeFramework;

public enum myOrderType { Stop, StopOrOpen, Market }
public enum OrderBehaviour { CloseAll, None }

public class OrderData
{
    public myOrderType type;
    public double price;
    public double BarsValid;
    public double TP = double.NaN;
    public double SL = double.NaN;
    public int BarsExit;
    public bool Long;
    public double Size = double.NaN;
    public double SizePct = double.NaN;

    public string Asset;

    public double BrutalPrice = double.NaN;
    public double BrutalExit = double.NaN;

    public OrderBehaviour Behaviour;

    public PositionData ClosingPosition = null;
    public CloseReason CloseReason;

    public delegate void EventHandler(PositionData pos);

    public event EventHandler eventExecuted;

    public void Executed(PositionData pos)
    {
        if (eventExecuted != null) eventExecuted(pos);
    }

    public stringDict UserData;

}

[Serializable]
public class PositionData
{
    public string Asset;

    public void setTPHandler(RIndexList<double> input)
    {
        input.NewData += delegate(double val) { TP = val; };
    }

    public void setSLHandler(RIndexList<double> input)
    {
        input.NewData += delegate(double val) { SL = val; };
    }

    public double OpenPrice;
    public double ClosePrice = double.NaN;

    public bool Open
    {
        get { return double.IsNaN(ClosePrice); }
    }

    public double Size;
    public DateTime OpenDT;
    public DateTime CloseDT;

    public double SL = double.NaN;
    public double TP = double.NaN;

    public double MaxPrice;
    public double MinPrice;

    public double MFE
    {
        get { return Long ? (MaxPrice - OpenPrice) : (OpenPrice - MinPrice); }
    }

    public double MAE
    {
        get { return Long ? (OpenPrice - MinPrice) : (MaxPrice - OpenPrice); }
    }

    public int BarsHeld;

    public bool Long;

    public int BarsExit;

    public CloseReason CloseReason = CloseReason.Open;

    public stringDict UserData;

    public double ProfitPct
    {
        get
        {
            if (double.IsNaN(ClosePrice)) return double.NaN;
            return (Long ? 1 : -1) * (ClosePrice / OpenPrice - 1) * 100;
        }
    }

    public IRIndex<BarData> CurrentQuotes;
    public IRIndex<BarData> FX_BaseQuotes;

    public AssetType AssetType = AssetType.Default;

    public double Profit
    {
        get
        {
            if (double.IsNaN(ClosePrice)) return double.NaN;

            if (AssetType == AssetType.Default)
                return (Long ? 1 : -1) * (ClosePrice - OpenPrice) * Size;

            if (AssetType == AssetType.FX_reverse)
                return (Long ? 1 : -1) * (ClosePrice - OpenPrice) * Size * 100000;

            if (AssetType == AssetType.FX_direct)
                return (Long ? 1 : -1) * (ClosePrice - OpenPrice) * Size * 100000 / ClosePrice;

            if (AssetType == AssetType.FX_cross)
                return (Long ? 1 : -1) * (ClosePrice - OpenPrice) * Size * 100000 * FX_BaseQuotes[0].Close / CurrentQuotes[0].Close;

            throw new Exception("There is no such ProfitType");
        }

        /*
         * Расчет стоимости пункта на один лот
Все валютые пары можно условно разделить на три категории — пары с обратной котировкой (EURUSD, GBPUSD и т.п.), пары с прямой котировкой (USDJPY, USDCHF и т.п.) и кросс–курсы (GBPCHF, EURJPY и т.п.).
1.	Для валютных пар с обратной котировкой стоимость пункта, выраженная в долларах, рассчитывается по формуле
PIP = LOT_SIZE × TICK_SIZE,
где LOT_SIZE — размер лота, TICK_SIZE — размер тика.
Для валютных пар с обратной котировкой стоимость пункта постоянна и не зависит от текущей котировки.
Пример:
Для EURUSD размер лота 100,000 евро, размер тика — 0.0001
PIP = 100,000 * 0.0001 = $10.00
2.	Для валютных пар с прямой котировкой стоимость пункта, выраженная в долларах, рассчитывается по формуле
PIP = LOT_SIZE × TICK_SIZE / CURRENT_QUOTE,
где LOT_SIZE — размер лота, TICK_SIZE — размер тика, CURRENT_QUOTE — текущая котировка пары.
Для валютных пар с прямой котировкой стоимость пункта меняется в зависимости от текущей котировки.
Пример:
Для USDJPY размер лота 100,000 долларов, размер тика — 0.01. При котировке USDJPY 114.66
PIP = 100,000 * 0.01 / 114.66 = $8.72
3.	Для кросс–курсов стоимость пункта, выраженная в долларах, рассчитывается по формуле
PIP = LOT_SIZE × TICK_SIZE × BASE_QUOTE / CURRENT_QUOTE,
где LOT_SIZE — размер лота, TICK_SIZE — размер тика, BASE_QUOTE — текущая котировка базовой (первой) валюты к доллару США, CURRENT_QUOTE — текущая котировка пары.
Для кросс–курсов стоимость пункта меняется в зависимости от текущих котировок как самой пары, так и базовой валюты.
Пример:
Для GBPJPY размер лота 100,000 фунтов, размер тика — 0.01, базовая валюта — GBPUSD. При котировкеGBPJPY 230.82 и котировке GBPUSD 2.0107
PIP = 100,000 * 0.01 * 2.0107 / 230.82 = $8.71

         */
    }

    public double ProfitLow
    {
        get
        {
            if (CloseReason != CloseReason.Open) throw new Exception("Not supported!");

            if (AssetType == AssetType.Default)
                return (Long ? 1 : -1) * (CurrentQuotes[0].Low - OpenPrice) * Size;

            if (AssetType == AssetType.FX_reverse)
                return (Long ? 1 : -1) * (CurrentQuotes[0].Low - OpenPrice) * Size * 100000;

            if (AssetType == AssetType.FX_direct)
                return (Long ? 1 : -1) * (CurrentQuotes[0].Low - OpenPrice) * Size * 100000 / CurrentQuotes[0].Low;

            if (AssetType == AssetType.FX_cross)
                return (Long ? 1 : -1) * (CurrentQuotes[0].Low - OpenPrice) * Size * 100000 * FX_BaseQuotes[0].Low / CurrentQuotes[0].Low;

            throw new Exception("There is no such ProfitType");
        }

        /*
         * Расчет стоимости пункта на один лот
Все валютые пары можно условно разделить на три категории — пары с обратной котировкой (EURUSD, GBPUSD и т.п.), пары с прямой котировкой (USDJPY, USDCHF и т.п.) и кросс–курсы (GBPCHF, EURJPY и т.п.).
1.	Для валютных пар с обратной котировкой стоимость пункта, выраженная в долларах, рассчитывается по формуле
PIP = LOT_SIZE × TICK_SIZE,
где LOT_SIZE — размер лота, TICK_SIZE — размер тика.
Для валютных пар с обратной котировкой стоимость пункта постоянна и не зависит от текущей котировки.
Пример:
Для EURUSD размер лота 100,000 евро, размер тика — 0.0001
PIP = 100,000 * 0.0001 = $10.00
2.	Для валютных пар с прямой котировкой стоимость пункта, выраженная в долларах, рассчитывается по формуле
PIP = LOT_SIZE × TICK_SIZE / CURRENT_QUOTE,
где LOT_SIZE — размер лота, TICK_SIZE — размер тика, CURRENT_QUOTE — текущая котировка пары.
Для валютных пар с прямой котировкой стоимость пункта меняется в зависимости от текущей котировки.
Пример:
Для USDJPY размер лота 100,000 долларов, размер тика — 0.01. При котировке USDJPY 114.66
PIP = 100,000 * 0.01 / 114.66 = $8.72
3.	Для кросс–курсов стоимость пункта, выраженная в долларах, рассчитывается по формуле
PIP = LOT_SIZE × TICK_SIZE × BASE_QUOTE / CURRENT_QUOTE,
где LOT_SIZE — размер лота, TICK_SIZE — размер тика, BASE_QUOTE — текущая котировка базовой (первой) валюты к доллару США, CURRENT_QUOTE — текущая котировка пары.
Для кросс–курсов стоимость пункта меняется в зависимости от текущих котировок как самой пары, так и базовой валюты.
Пример:
Для GBPJPY размер лота 100,000 фунтов, размер тика — 0.01, базовая валюта — GBPUSD. При котировкеGBPJPY 230.82 и котировке GBPUSD 2.0107
PIP = 100,000 * 0.01 * 2.0107 / 230.82 = $8.71

         */
    }
}



#endregion

public class ExecParams
{
    public SymbolData defSymbol { get { return Symbols[0]; } }
    public List<SymbolData> Symbols;
    public _params Params;
    public string TaskName;
}

public class SymbolMix
{

    public SymbolMix(SymbolData defaultSymbol)
    {
        defSymbol = defaultSymbol;
    }

    public SymbolMix(SymbolData defaultSymbol, params SymbolData[] AdditionalSymbols)
    {
        defSymbol = defaultSymbol;
        AdditionalInfo.AddRange(AdditionalSymbols);
    }

    public SymbolMix() { }

    public SymbolData defSymbol;
    public List<SymbolData> AdditionalInfo = new List<SymbolData>();

}


