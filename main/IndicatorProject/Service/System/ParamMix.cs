using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using TradeFramework;
using TradeFramework.ChartObjects;

public class ParamMix
{

    public Param[] ParseString(string pattern)
    {

        var vals_dict = new Dictionary<string, string>();

        foreach (var s in pattern.Trim().Split('\r'))
        {
            if (s == "") continue;
            var split = s.Split('=');
            var prmName = split[0].Trim();
            var val = split[1].Trim();
            vals_dict.Add(prmName, val);
        }

        var ret_prms = NewParamSet();

        int i = 0;
        foreach (var pair in Dict)
        {
            ret_prms[i].SetVal(vals_dict[pair.Key]);
            i++;
        }

        return ret_prms;

    }

    public Param[] ParseCacheId(ParamsId CacheId)
    {
        return ParseCacheId(CacheId.data);
    }

    public Param[] ParseCacheId(IEnumerable<byte> IdArr)
    {
        var ret_prms = NewParamSet();

        var i = 0;
        foreach (var id in IdArr)
        {
            ret_prms[i].SetValById(id);
            i++;
        }

        return ret_prms;
    }

    int CacheIdLeng;

    public ParamsId GetCacheId(Param[] Params)
    {
        var CacheIdArr = new byte[CacheIdLeng];
        int j = 0;
        foreach (var prm in Params)
        {
            CacheIdArr[j] = prm.CacheId;
            j++;
        }

        return new ParamsId(CacheIdArr);
    }

    public List<Param> ParamList = new List<Param>();
    public Dictionary<string, Param> Dict = new Dictionary<string, Param>();
    public Dictionary<string, int> DictIds = new Dictionary<string, int>();
    public Dictionary<int, string> IdsDict = new Dictionary<int, string>();

    int length = 0;

    public void AddParam(string Name, Param prm)
    {
        Dict.Add(Name, prm);
        DictIds.Add(Name, length);
        IdsDict.Add(length, Name);
        ParamList.Add(prm);

        length++;
        if (prm is IntLimiter) CacheIdLeng += 2;
        else CacheIdLeng++;
    }

    public Param[] NewParamSet()
    {
        var set = new Param[length];
        int i = 0;
        foreach (var param in Dict.Values)
        {
            set[i] = param.Clone();
            i++;
        }
        return set;
    }

    public Param[] NewRandomParamSet()
    {
        var set = NewParamSet();

        while (true)
        {
            foreach (var param in set)
                param.Generate();

            if (_serv.ChromChecker == null) return set;
            if (_serv.ChromChecker(set)) return set;
        }

        return set;
    }

    public bool CanBeCached
    {
        get
        {
            return !Dict.Any(pair => pair.Value is DoubleRangeParam || pair.Value is DoubleLimiter);
        }
    }

    public void Set(string Name, object value)
    {
        Dict[Name].GetType().GetField("val").SetValue(Dict[Name], value);
    }

    public void CheckAbsentParams(Type LaunchParamsType)
    {
        var FieldInfo = LaunchParamsType.GetFields().Select(f => f.Name);
        var _paramsFields = typeof(_params).GetFields().Select(f => f.Name);
        var absent = new List<string>();
        foreach (var f_name in FieldInfo)
        {
            if (_paramsFields.Contains(f_name) || Dict.Keys.Contains(f_name)) continue;
            absent.Add(f_name);
        }
        if (absent.Count == 0) return;

        etc.prn(" ---- Absent params: ----");
        absent.ForEach(etc.prn);
        etc.prn(" ------------------------");
    }

    public long PermutationsAmount()
    {
        long Permutations = 1;
        foreach (var pair in Dict)
        {
            var prm = pair.Value;

            if (!(prm is IPossibleValsCount)) throw new Exception("Param do not support IPossibleValsCount interface!");

            Permutations *= ((IPossibleValsCount)prm).PossibleValsCount();
        }

        return Permutations;
    }

    public List<List<byte>> GetPermutations()
    {

        var AllParams = new List<List<byte>>();

        foreach (var pair in Dict)
        {
            var prm = pair.Value;

            if (!(prm is IPossibleValsCount)) throw new Exception("Param do not support IPossibleValsCount interface!");

            AllParams.Add(((IPossibleValsCount)prm).PossibleValsList());
        }

        var permutations = _serv.GetPermutations(AllParams);
        return permutations;
    }

    public List<List<object>> GetPermutationsFull()
    {
        var AllParams = new List<List<object>>();

        foreach (var pair in Dict)
        {
            var prm = pair.Value;

            if (!(prm is IPossibleValsCount)) throw new Exception("Param do not support IPossibleValsCount interface!");

            AllParams.Add(((IPossibleValsCount)prm).TruePossibleVals());
        }


        var permutations = _serv.GetPermutations(AllParams);
        return permutations;
    }

    public string CastCode(string code, Param[] ParamValues)
    {

        /*  Behaviour1Param
            Behaviour2Param
            DoubleLimiter 
            IntLimiter
            IntRangeParam
            SeparateValsParam
         */

        var i = 0;
        foreach (var pair in Dict)
        {
            var name = pair.Key;
            var param = ParamValues[i];

            i++;

            var t = param.GetType();

            if (t == typeof(Behaviour1Param))
            {
                var args = GetArgs(code, name);

                foreach (var arg in args)
                    code = code.Replace(String.Format("prm.{0}({1})", name, arg), String.Format(((Behaviour1Param)param).val.CompileTemplate, arg));

                continue;
            }

            if (t == typeof(Behaviour2Param))
            {
                var args = GetArgs(code, name);

                foreach (var arg2 in args)
                {
                    var arg = arg2.Split(',');
                    code = code.Replace(String.Format("prm.{0}({1})", name, arg2), String.Format(((Behaviour2Param)param).val.CompileTemplate, arg[0], arg[1]));
                }

                continue;
            }

            if (t == typeof(DoubleLimiter))
            {
                var args = GetArgs(code, name);

                foreach (var arg in args)
                {
                    var limiter = ((DoubleLimiter)param).val;
                    var Prm2Replace = String.Format("prm.{0}({1})", name, arg);
                    if (limiter.Item1 == LimitType.Any) code = code.Replace(Prm2Replace, "true");
                    if (limiter.Item1 == LimitType.Below) code = code.Replace(Prm2Replace, String.Format("{0} < {1}", arg, limiter.Item2));
                    if (limiter.Item1 == LimitType.Above) code = code.Replace(Prm2Replace, String.Format("{0} > {1}", arg, limiter.Item2));
                }

                continue;
            }

            if (t == typeof(IntLimiter))
            {
                var args = GetArgs(code, name);

                foreach (var arg in args)
                {
                    var limiter = ((IntLimiter)param).val;
                    var Prm2Replace = String.Format("prm.{0}({1})", name, arg);
                    if (limiter.Item1 == LimitType.Any) code = code.Replace(Prm2Replace, "true");
                    if (limiter.Item1 == LimitType.Below) code = code.Replace(Prm2Replace, String.Format("{0} < {1}", arg, limiter.Item2));
                    if (limiter.Item1 == LimitType.Above) code = code.Replace(Prm2Replace, String.Format("{0} > {1}", arg, limiter.Item2));
                }

                continue;
            }

            if (t == typeof(IntRangeParam) || t == typeof(DoubleRangeParam))
            {
                code = code.Replace(String.Format("prm.{0}", name), param.Val().ToString());
                continue;
            }

            // Остается SeparateValsParam

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(SeparateValsParam<>))
            {

                if (param.Val() is string)
                {
                    code = code.Replace(String.Format("prm.{0}", name), String.Format("\"{0}\"", param.Val()));
                    continue;
                }

                if (param.Val() is bool)
                {
                    code = code.Replace(String.Format("prm.{0}", name), (bool)param.Val() ? "true" : "false");
                    continue;
                }

                code = code.Replace(String.Format("prm.{0}", name), param.Val().ToString());
                continue;
            }

            throw new Exception("Something wrong!");

        }

        return code;
    }

    List<string> GetArgs(string str, string ParamName)
    {
        List<string> rez = new List<string>();

        Regex pattern = new Regex(String.Format(@"(?<=prm.{0}\().*?(?=\))", ParamName));
        foreach (Match m in pattern.Matches(str))
            if (m.Success)
                //меж скобок ( )  
                rez.Add(m.ToString());

        return rez;
    }

    public void Generate()
    {
        while (true)
        {
            foreach (var param in Dict.Values)
                param.Generate();

            if (_serv.ChromChecker == null) return;
            if (_serv.ChromChecker(Dict.Values.ToArray())) return;
        }
    }

    public void CastVals(ref object obj, Param[] ParamValues)
    {
        var type = obj.GetType();
        var i = 0;
        foreach (var pair in Dict)
        {
            var x = ParamValues[i].Val();
            var field = type.GetField(pair.Key, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            field.SetValue(obj, x);
            i++;
        }
    }


    public string ToStringVals(Param[] Params)
    {
        var s = "";
        var i = 0;
        foreach (var pair in Dict)
        {
            s += pair.Key + " = " + Params[i] + "\r\n";
            i++;
        }
        return s;
    }
}

public enum beh1 { Any, FoundMin, FoundMax, Higher, Lower, Falling, Rising, AboveZero, BelowZero }
public enum beh2 { Any, UpCross, DnCross, Above, Below, Above2, Below2 }

public delegate bool tradeChromChecker(Param[] prms);

public static partial class _serv
{


    public static double getBin(List<double> bins, double val)
    {
        var res_bin = double.NaN;
        for (int i = 0; i < bins.Count - 1; i++)
        {
            if (val >= bins[i] && val < bins[i + 1])
            {
                res_bin = bins[i];
                break;
            }
        }

        return res_bin;
    }


    public static ResultData GetResultData(TradingResults TradingResults, StrategyBasePortfolio strategy)
    {

        var result = new ResultData();
        result.Strategy = strategy.TradeSymbols.First().Key;
        result.Description = strategy.TradeSymbols.First().Key;
        result.Params = new stringDict();
        result.Params["TradeCapital"] = strategy.LaunchParams.TradeCapital;
        result.Positions = TradingResults.Positions;

        result.Result = new ResultsNumbers();
        var r = result.Result;

        r["GrossProfit"] = TradingResults.GrossProfit;
        r["GrossLoss"] = TradingResults.GrossLoss;
        r["NetProfit"] = TradingResults.NetProfit;
        r["MaxGain"] = TradingResults.MaxGain;
        r["MaxLoss"] = TradingResults.MaxLoss;
        r["SizeTotal"] = TradingResults.SizeTotal;
        r["MaxLongSize"] = TradingResults.MaxLongSize;
        r["MaxShortSize"] = TradingResults.MaxShortSize;
        r["PosCount"] = TradingResults.PosCount;
        r["PosCountWins"] = TradingResults.PosCountWins;
        r["PosCountLoose"] = TradingResults.PosCountLoose;
        r["PosCountZero"] = TradingResults.PosCountZero;
        r["PosCountOrder"] = TradingResults.PosCountOrder;
        r["PosCountEOD"] = TradingResults.PosCountEOD;
        r["PosCountStopLoss"] = TradingResults.PosCountStopLoss;
        r["PosCountTakeProfit"] = TradingResults.PosCountTakeProfit;
        r["PosCountInverse"] = TradingResults.PosCountInverse;
        r["MaxDD"] = TradingResults.MaxDD;
        r["AvgDD"] = TradingResults.AvgDD;
        r["DayProfitStDev"] = TradingResults.DayProfitStDev;

        // TODO
        //#COMMENT
        result.Symbol = strategy.SymbolList.First();

        result.Balance = TradingResults.Balance;

        result.ChartData = new ChartData();
        result.ChartData.Account = TradingResults.Account;
        result.ChartData.Drawdown = TradingResults.Drawdown;
        result.ChartData.Exposure = TradingResults.Exposure;

        result.ChartData.xDT = TradingResults.xDT;
        result.ChartData.chSeries = TradingResults.chSeries;
        result.ChartData.chMarkers = TradingResults.chMarkers;
        result.ChartData.InfoSeries = TradingResults.chInfoSeries;
        result.ChartData.Symbols = strategy.SymbolList.Select(x => x.Name).ToList();

        result.Description = "";
        result.Strategy = "";
        return result;
    }

    public static void ForEach<T>(this IEnumerable<T> seq, Action<T, int> fun)
    {
        var i = 0;
        foreach (var x in seq) fun(x, i++);
    }

    public static int getSize<T>(this IEnumerable<T> arr)
    {
        var size = Marshal.SizeOf(arr.First());
        return arr.Count() * size;
    }

    public static int ChromChecked = 0;
    public static int ChromSucceed = 0;

    public static AsyncMersenneTwister rnd = new AsyncMersenneTwister();

    public static Jenkins96 ArrayHash = new Jenkins96();

    public static ParamMix paramMix;
    public static tradeChromChecker ChromChecker;

    public static double[] RangeInterval(double from, double to, int interval)
    {
        var res = new double[interval];
        var j = 0;
        for (double i = from; j < interval; i += (to - from) / (interval - 1), j++) res[j] = i;
        return res;
    }

    public static int[] RangeInterval(int from, int to, int interval)
    {
        var res = new int[interval];
        var j = 0;
        for (int i = from; j < interval; i += (int)((double)to - from) / (interval - 1), j++) res[j] = i;
        return res;
    }

    public static double[] RangeStep(double from, double to, double step)
    {
        var vals = new List<double>();
        for (var i = from; i <= to; i += step) vals.Add(i);
        return vals.ToArray();

    }

    public static int[] RangeStep(int from, int to, int step)
    {
        var vals = new List<int>();
        for (var i = from; i <= to; i += step) vals.Add(i);
        return vals.ToArray();
    }

    static List<List<T>> RecursiveAppend<T>(IEnumerable<List<T>> priorPermutations, IEnumerable<T> additions)
    {
        return (from priorPermutation in priorPermutations
                from addition in additions
                select new List<T>(priorPermutation) { addition }).ToList();
    }

    public static List<List<T>> GetPermutations<T>(IEnumerable<List<T>> variants)
    {
        var permutations = variants.First().Select(init => new List<T> { init }).ToList();

        return variants.Skip(1).Aggregate(permutations, RecursiveAppend);
    }

    public static int ChromCount = 0;
    public static long CachedIndiCheck;

    public static Dictionary<DateTime, double> GetIndiFromDb(string Name)
    {
        return (Dictionary<DateTime, double>)etc.GetDataFromDb(Name);
    }

    public static ParamMix Add(this ParamMix ParamMix, string Name, Param param)
    {
        ParamMix.AddParam(Name, param);
        return ParamMix;
    }

    public static ParamMix AddBehaviour1(this ParamMix ParamMix, string Name, params beh1[] BehavioursList)
    {
        ParamMix.AddParam(Name, new Behaviour1Param(BehavioursList));
        return ParamMix;
    }

    public static ParamMix AddBehaviour2(this ParamMix ParamMix, string Name, params beh2[] BehavioursList)
    {
        ParamMix.AddParam(Name, new Behaviour2Param(BehavioursList));
        return ParamMix;
    }

    public static ParamMix AddSeparateVals<T>(this ParamMix ParamMix, string Name, params T[] PossibleVals)
    {
        ParamMix.AddParam(Name, new SeparateValsParam<T>(PossibleVals));
        return ParamMix;
    }

    public static Param[] CreateParamMix(params Param[] Params)
    {
        return Params;
    }

    public static Behaviour1 GetBehaviour1(beh1 type)
    {
        if (type == beh1.Any) return new Behaviour1(vals => true, type.ToString(), "true", type);

        if (type == beh1.FoundMin) return new Behaviour1(ext.FoundMin, type.ToString(), "{0}.FoundMin()", type);
        if (type == beh1.FoundMax) return new Behaviour1(ext.FoundMax, type.ToString(), "{0}.FoundMax()", type);

        if (type == beh1.Higher) return new Behaviour1(ext.Higher, type.ToString(), "{0}.Higher()", type);
        if (type == beh1.Lower) return new Behaviour1(ext.Lower, type.ToString(), "{0}.Lower()", type);

        if (type == beh1.Falling) return new Behaviour1(ext.Lower, type.ToString(), "{0}.Falling()", type);
        if (type == beh1.Rising) return new Behaviour1(ext.Rising, type.ToString(), "{0}.Rising()", type);

        if (type == beh1.AboveZero) return new Behaviour1(vals => vals.val > 0, type.ToString(), "{0} > 0", type);
        if (type == beh1.BelowZero) return new Behaviour1(vals => vals.val < 0, type.ToString(), "{0} < 0", type);

        throw new Exception("No type defined!");
    }

    public static bool Above(IRIndex<double> vals1, IRIndex<double> vals2)
    {
        return vals1.val > vals2.val;
    }

    public static bool Above2(IRIndex<double> vals1, IRIndex<double> vals2)
    {
        return vals1.val > vals2.val && vals1[-1] > vals2[-1];
    }

    public static bool Below(IRIndex<double> vals1, IRIndex<double> vals2)
    {
        return vals1.val < vals2.val;
    }

    public static bool Below2(IRIndex<double> vals1, IRIndex<double> vals2)
    {
        return vals1.val < vals2.val && vals1[-1] < vals2[-1];
    }

    public static Behaviour2 GetBehaviour2(beh2 type)
    {
        if (type == beh2.Any) return new Behaviour2((vals1, vals2) => true, type.ToString(), "true", type);

        if (type == beh2.UpCross) return new Behaviour2(Extensions.UpCross, type.ToString(), "{0}.UpCross({1})", type);
        if (type == beh2.DnCross) return new Behaviour2(Extensions.DnCross, type.ToString(), "{0}.DnCross({0})", type);

        if (type == beh2.Above) return new Behaviour2(Above, type.ToString(), "{0} > {1}", type);
        if (type == beh2.Below) return new Behaviour2(Below, type.ToString(), "{0} < {1}", type);

        if (type == beh2.Above2) return new Behaviour2(Above2, type.ToString(), "{0} > {1} && {0}[-1] > {1}[-1]", type);
        if (type == beh2.Below2) return new Behaviour2(Below2, type.ToString(), "{0} < {1} && {0}[-1] < {1}[-1]", type);

        throw new Exception("No type defined!");
    }

    public static bool inRange(this BarData bar, double val)
    {
        return val <= bar.High && val >= bar.Low;
    }

    public static NEWResultData getViewData(TradingResults TradingResults, string SymbolName)
    {

        var result = new NEWResultData();

        result.GrossProfit = TradingResults.GrossProfit;
        result.GrossLoss = TradingResults.GrossLoss;
        result.NetProfit = TradingResults.NetProfit;
        result.MaxGain = TradingResults.MaxGain;
        result.MaxLoss = TradingResults.MaxLoss;
        result.SizeTotal = TradingResults.SizeTotal;
        result.MaxLongSize = TradingResults.MaxLongSize;
        result.MaxShortSize = TradingResults.MaxShortSize;
        result.PosCount = TradingResults.PosCount;
        result.PosCountWins = TradingResults.PosCountWins;
        result.PosCountLoose = TradingResults.PosCountLoose;
        result.PosCountZero = TradingResults.PosCountZero;
        result.PosCountOrder = TradingResults.PosCountOrder;
        result.PosCountEOD = TradingResults.PosCountEOD;
        result.PosCountStopLoss = TradingResults.PosCountStopLoss;
        result.PosCountTakeProfit = TradingResults.PosCountTakeProfit;
        result.PosCountInverse = TradingResults.PosCountInverse;
        result.MaxDD = TradingResults.MaxDD;
        result.AvgDD = TradingResults.AvgDD;
        result.DayProfitStDev = TradingResults.DayProfitStDev;

        result.Positions = TradingResults.Positions;

        result.ChartData = new ChartData();
        result.ChartData.Account = TradingResults.Account;
        result.ChartData.Drawdown = TradingResults.Drawdown;
        result.ChartData.xDT = TradingResults.xDT;
        result.ChartData.chSeries = TradingResults.chSeries;
        result.ChartData.chMarkers = TradingResults.chMarkers;
        result.ChartData.InfoSeries = TradingResults.chInfoSeries;
        result.ChartData.Symbols = new List<string> { SymbolName };

        return result;
    }
}

public class AsyncMersenneTwister : RandomBase
{
    public AsyncMersenneTwister()
    {
        rnd1 = new MersenneTwister();
        rnd2 = new MersenneTwister(rnd1.Next());
        rnd3 = new MersenneTwister(rnd2.Next());
        rnd4 = new MersenneTwister(rnd3.Next());
        rnd5 = new MersenneTwister(rnd4.Next());
        rnd6 = new MersenneTwister(rnd5.Next());
        rnd7 = new MersenneTwister(rnd6.Next());
        rnd8 = new MersenneTwister(rnd7.Next());
        rnd9 = new MersenneTwister(rnd8.Next());
    }

    public AsyncMersenneTwister(int seed)
    {
        rnd1 = new MersenneTwister(seed);
        rnd2 = new MersenneTwister(rnd1.Next());
        rnd3 = new MersenneTwister(rnd2.Next());
        rnd4 = new MersenneTwister(rnd3.Next());
        rnd5 = new MersenneTwister(rnd4.Next());
        rnd6 = new MersenneTwister(rnd5.Next());
        rnd7 = new MersenneTwister(rnd6.Next());
        rnd8 = new MersenneTwister(rnd7.Next());
        rnd9 = new MersenneTwister(rnd8.Next());
    }

    SimpleSpinLock sp = new SimpleSpinLock();

    MersenneTwister rnd1;
    MersenneTwister rnd2;
    MersenneTwister rnd3;
    MersenneTwister rnd4;
    MersenneTwister rnd5;
    MersenneTwister rnd6;
    MersenneTwister rnd7;
    MersenneTwister rnd8;
    MersenneTwister rnd9;

    bool f1 = true, f2 = true, f3 = true, f4 = true, f5 = true, f6 = true, f7 = true, f8 = true, f9 = true;

    public override int Next()
    {
        sp.Enter();
        if (f1)
        {
            f1 = false;
            sp.Leave();
            var z = rnd1.Next();
            f1 = true;
            return z;
        }

        if (f2)
        {
            f2 = false;
            sp.Leave();
            var z = rnd2.Next();
            f2 = true;
            return z;
        }

        if (f3)
        {
            f3 = false;
            sp.Leave();
            var z = rnd3.Next();
            f3 = true;
            return z;
        }

        if (f4)
        {
            f4 = false;
            sp.Leave();
            var z = rnd4.Next();
            f4 = true;
            return z;
        }

        if (f5)
        {
            f5 = false;
            sp.Leave();
            var z = rnd5.Next();
            f5 = true;
            return z;
        }

        if (f6)
        {
            f6 = false;
            sp.Leave();
            var z = rnd6.Next();
            f6 = true;
            return z;
        }

        if (f7)
        {
            f7 = false;
            sp.Leave();
            var z = rnd7.Next();
            f7 = true;
            return z;
        }

        if (f8)
        {
            f8 = false;
            sp.Leave();
            var z = rnd8.Next();
            f8 = true;
            return z;
        }

        if (f9)
        {
            f9 = false;
            sp.Leave();
            var z = rnd9.Next();
            f9 = true;
            return z;
        }


        throw new Exception("Incredible! You have more than 8 (or 9?) processors!");
    }
}

public class Jenkins96
{
    uint a, b, c;

    void Mix()
    {
        a -= b; a -= c; a ^= (c >> 13);
        b -= c; b -= a; b ^= (a << 8);
        c -= a; c -= b; c ^= (b >> 13);
        a -= b; a -= c; a ^= (c >> 12);
        b -= c; b -= a; b ^= (a << 16);
        c -= a; c -= b; c ^= (b >> 5);
        a -= b; a -= c; a ^= (c >> 3);
        b -= c; b -= a; b ^= (a << 10);
        c -= a; c -= b; c ^= (b >> 15);
    }

    public uint ComputeHash(byte[] data)
    {
        int len = data.Length;
        a = b = 0x9e3779b9;
        c = 0;
        int i = 0;
        while (i + 12 <= len)
        {
            a += (uint)data[i++] |
                ((uint)data[i++] << 8) |
                ((uint)data[i++] << 16) |
                ((uint)data[i++] << 24);
            b += (uint)data[i++] |
                ((uint)data[i++] << 8) |
                ((uint)data[i++] << 16) |
                ((uint)data[i++] << 24);
            c += (uint)data[i++] |
                ((uint)data[i++] << 8) |
                ((uint)data[i++] << 16) |
                ((uint)data[i++] << 24);
            Mix();
        }
        c += (uint)len;
        if (i < len)
            a += data[i++];
        if (i < len)
            a += (uint)data[i++] << 8;
        if (i < len)
            a += (uint)data[i++] << 16;
        if (i < len)
            a += (uint)data[i++] << 24;
        if (i < len)
            b += (uint)data[i++];
        if (i < len)
            b += (uint)data[i++] << 8;
        if (i < len)
            b += (uint)data[i++] << 16;
        if (i < len)
            b += (uint)data[i++] << 24;
        if (i < len)
            c += (uint)data[i++] << 8;
        if (i < len)
            c += (uint)data[i++] << 16;
        if (i < len)
            c += (uint)data[i++] << 24;
        Mix();
        return c;
    }
}

public static class Extensions2
{
    ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
    ///<param name="items">The enumerable to search.</param>
    ///<param name="predicate">The expression to test the items against.</param>
    ///<returns>The index of the first matching item, or -1 if no items match.</returns>
    public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
    {
        if (items == null) throw new ArgumentNullException("items");
        if (predicate == null) throw new ArgumentNullException("predicate");

        int retVal = 0;
        foreach (var item in items)
        {
            if (predicate(item)) return retVal;
            retVal++;
        }
        return -1;
    }
    ///<summary>Finds the index of the first occurence of an item in an enumerable.</summary>
    ///<param name="items">The enumerable to search.</param>
    ///<param name="item">The item to find.</param>
    ///<returns>The index of the first matching item, or -1 if the item was not found.</returns>
    public static int IndexOf<T>(this IEnumerable<T> items, T item) { return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i)); }

}



public static class Cloner
{
    public static T CloneObject<T>(this T obj) where T : class
    {
        if (obj == null) return null;
        System.Reflection.MethodInfo inst = obj.GetType().GetMethod("MemberwiseClone",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (inst != null)
            return (T)inst.Invoke(obj, null);
        else
            return null;
    }

}
