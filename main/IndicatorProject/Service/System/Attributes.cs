using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class StrategyAttrsInfo
{
    public Dictionary<string, IndicatorInfo> Indicators;
    public List<string> PredifinedIndis;
    public List<string> NonIndiParams;
    public ParamMix paramMix;
    public Type TargetType;
}

public class IndicatorInfo
{
    public bool isDynamic;
    public string ParentName;
    public string Name;
    public string[] Parameters;
}

public static class StrategyAttrs
{
    public static StrategyAttrsInfo GetAttrStructure(Type t)
    {
        var fields = t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        var paramMix = new ParamMix();

        var Indicators = new Dictionary<string, IndicatorInfo>();

        var PredifinedIndis = new List<string>();
        var NonIndiParams = new List<string>();

        var ret_infos = new StrategyAttrsInfo();

        ret_infos.TargetType = t;

        ret_infos.Indicators = Indicators;
        ret_infos.PredifinedIndis = PredifinedIndis;
        ret_infos.NonIndiParams = NonIndiParams;
        ret_infos.paramMix = paramMix;

        foreach (var fieldInfo in fields)
        {
            System.Attribute[] attrs = System.Attribute.GetCustomAttributes(fieldInfo); // reflection

            foreach (System.Attribute attr in attrs)
            {
                if (attr is OptimizeParamBase)
                {
                    var prm = ((OptimizeParamBase)attr).GetParam();
                    paramMix.Add(fieldInfo.Name, prm);
                    NonIndiParams.Add(fieldInfo.Name);
                }
            }
        }

        foreach (var fieldInfo in fields)
        {
            System.Attribute[] attrs = System.Attribute.GetCustomAttributes(fieldInfo); // reflection
            foreach (System.Attribute attr in attrs)
            {
                if (attr is Indicator)
                {
                    var Indi = ((Indicator)attr);
                    var name = Indi.name;
                    if (Indi.InputParams != null)
                    {
                        var permutParmMix = new ParamMix();
                        foreach (var param in Indi.InputParams)
                        {
                            permutParmMix.AddParam(param, paramMix.Dict[param]);
                        }
                        /*if (!Indicators.ContainsKey(name))
                            Indicators.Add(name, Indi.InputParams.ToList());
                        else
                            Indicators[name].AddRange(Indi.InputParams.ToList());*/

                        Indicators.Add(fieldInfo.Name, new IndicatorInfo { Name = fieldInfo.Name, Parameters = Indi.InputParams.ToArray(), ParentName = name, isDynamic = true });

                        foreach (var prm in Indi.InputParams)
                            NonIndiParams.Remove(prm);

                        var permuts = permutParmMix.GetPermutationsFull();

                        var addPredef =
                            permuts.Select(permut => permut.Aggregate(name, (current, prm) => current + ("_" + prm)));

                        PredifinedIndis.AddRange(addPredef.Where(predef => !PredifinedIndis.Contains(predef)));
                    }
                    else
                    {
                        Indicators.Add(fieldInfo.Name, new IndicatorInfo { Name = fieldInfo.Name, ParentName = name, isDynamic = false });
                        //StaticIndicators.Add(name);
                        PredifinedIndis.Add(name);
                    }
                }
            }
        }

        return ret_infos;
    }
}

public enum RangeType { Intervals, Steps, Continues }

[AttributeUsage(AttributeTargets.Field)]
public class OptimizeParamBase : Attribute
{
    public bool inOptimization = false;

    public object SetOneValue { get; set; }

    public Param prm;

    public Param GetParam()
    {
        return prm;
    }
}

public class Limiter : OptimizeParamBase
{
    public Limiter(int from, int to, params LimitType[] Limits)
    {
        prm = new IntLimiter(from, to, Limits);
    }

    public Limiter(double from, double to, params LimitType[] Limits)
    {
        prm = new DoubleLimiter(from, to, Limits);
    }

}

public class RangeStep : OptimizeParamBase
{
    public RangeStep(double from, double to, double Step)
    {
        prm = new SeparateValsParam<double>(_serv.RangeStep(from, to, Step));

    }

    public RangeStep(int from, int to, int Step)
    {
        prm = new SeparateValsParam<int>(_serv.RangeStep(from, to, Step));

    }
}


public class RangeInterval : OptimizeParamBase
{
    public RangeInterval(double from, double to, int Interval)
    {
        prm = new SeparateValsParam<double>(_serv.RangeInterval(from, to, Interval));

    }

    public RangeInterval(int from, int to, int Interval)
    {
        prm = new SeparateValsParam<int>(_serv.RangeInterval(from, to, Interval));

    }
}


public class ContinuesParam : OptimizeParamBase
{
    public ContinuesParam(double from, double to)
    {
        prm = new DoubleRangeParam(from, to);
    }
}


public class SeparateVals : OptimizeParamBase
{
    public SeparateVals(params float[] PossibleVals)
    {
        prm = new SeparateValsParam<float>(PossibleVals);
    }

    public SeparateVals(params double[] PossibleVals)
    {
        prm = new SeparateValsParam<double>(PossibleVals);
    }

    public SeparateVals(params int[] PossibleVals)
    {
        prm = new SeparateValsParam<int>(PossibleVals);
    }

    public SeparateVals(params bool[] PossibleVals)
    {
        prm = new SeparateValsParam<bool>(PossibleVals);
    }

    public SeparateVals(params string[] PossibleVals)
    {
        prm = new SeparateValsParam<string>(PossibleVals);
    }

    public SeparateVals(params object[] PossibleVals)
    {
        prm = new SeparateValsParam<object>(PossibleVals);
    }

    public SeparateVals(double from, double to)
    {
        prm = new DoubleRangeParam(from, to);
    }

    public SeparateVals(params beh1[] Behs)
    {
        prm = new Behaviour1Param(Behs);
    }

    public SeparateVals(params beh2[] Behs)
    {
        prm = new Behaviour2Param(Behs);
    }

}

[AttributeUsage(AttributeTargets.Field)]
public class Indicator : Attribute
{
    public string name;
    public string[] InputParams = null;
    public Indicator(string name)
    {
        this.name = name;
    }

    public Indicator(string name, params string[] InputParams)
    {
        this.name = name;
        this.InputParams = InputParams;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class GPU_NewBar : Attribute { }
