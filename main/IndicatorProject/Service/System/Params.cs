using System;
using System.Collections.Generic;
using System.Linq;


public abstract class Param
{
    public abstract Param Clone();
    public abstract void Generate();
    public abstract object Val();
    public abstract void SetVal(object val);
    public byte CacheId;
    public abstract void SetValById(byte id);
    public abstract object GetValById(byte id);
}

public enum LimitType { Any, Above, Below, Inside, Outside }

public delegate bool Behaviour1Func(IRIndex<double> RList);
public delegate bool LimiterFunc(double x);
public delegate bool Behaviour2Func(IRIndex<double> RList, IRIndex<double> RList2);


public abstract class BehaviourBase
{
    public string Name { get; protected set; }
    public string CompileTemplate { get; protected set; }
    public abstract object CheckObj { get; }
}


public class Behaviour1 : BehaviourBase
{
    public beh1 Type;
    public Behaviour1Func Check { get; private set; }
    public Behaviour1(Behaviour1Func CheckFunc, string Name, string CompileTemplate, beh1 Type)
    {
        this.Type = Type;
        Check = CheckFunc;
        this.Name = Name;
        this.CompileTemplate = CompileTemplate;
    }
    public override object CheckObj { get { return Check; } }
}


public class Behaviour2 : BehaviourBase
{
    public beh2 Type;
    public Behaviour2Func Check { get; private set; }
    public Behaviour2(Behaviour2Func CheckFunc, string Name, string CompileTemplate, beh2 Type)
    {
        this.Type = Type;
        Check = CheckFunc;
        this.Name = Name;
        this.CompileTemplate = CompileTemplate;
    }

    public override object CheckObj { get { return Check; } }
}


public class Behaviour1Param : BehaviourParam<Behaviour1>, IPossibleValsCount
{
    public Behaviour1Param(params Behaviour1[] Behaviours) : base(Behaviours) { }
    public Behaviour1Param(List<Behaviour1> Behaviours) : base(Behaviours) { }

    public Behaviour1Param(params beh1[] BehavioursList)
    {
        for (var i = 0; i < BehavioursList.Length; i++)
            Behaviours.Add(_serv.GetBehaviour1(BehavioursList[i]));

        length = Behaviours.Count;
    }

    public bool Check(IRIndex<double> RList)
    {
        return val.Check(RList);
    }

    public List<object> TruePossibleVals()
    {
        return Behaviours.Select(beh => (object)beh).ToList();
    }
}


public class Behaviour2Param : BehaviourParam<Behaviour2>, IPossibleValsCount
{

    public Behaviour2Param(params Behaviour2[] Behaviours) : base(Behaviours) { }
    public Behaviour2Param(List<Behaviour2> Behaviours) : base(Behaviours) { }

    public Behaviour2Param(params beh2[] BehavioursList)
    {
        for (var i = 0; i < BehavioursList.Length; i++)
            Behaviours.Add(_serv.GetBehaviour2(BehavioursList[i]));

        length = Behaviours.Count;
    }

    public bool Check(IRIndex<double> RList, IRIndex<double> RList2)
    {
        return val.Check(RList, RList2);
    }

    public List<object> TruePossibleVals()
    {
        return Behaviours.Select(beh => (object)beh).ToList();
    }
}


public class BehaviourParam<T> : Param where T : BehaviourBase
{
    protected List<T> Behaviours = new List<T>();
    protected int length;
    public T val;

    public override void SetVal(object x)
    {
        if (x is string)
        {
            byte z = 0;
            foreach (var beh in Behaviours)
            {
                if (beh.Name == (string)x)
                {
                    val = beh;
                    CacheId = z;
                    return;
                }
                z++;
            }
        }
        throw new Exception("Can't set param with val " + x);
    }

    public override string ToString()
    {
        return val.Name;
    }

    public override object Val() { return val.CheckObj; }

    public BehaviourParam(params T[] Behaviours)
    {
        this.Behaviours = Behaviours.ToList();
        length = Behaviours.Length;
    }

    public BehaviourParam(List<T> Behaviours)
    {
        this.Behaviours = Behaviours;
        length = Behaviours.Count;
    }

    public override Param Clone()
    {
        return new BehaviourParam<T>(Behaviours) { val = val, CacheId = CacheId };
    }

    public override void Generate()
    {
        var indx = _serv.rnd.Next(length);
        val = Behaviours[indx];
        CacheId = (byte)indx;
    }

    public override void SetValById(byte id)
    {
        val = Behaviours[id];
        CacheId = id;
    }

    public override object GetValById(byte id)
    {
        return Behaviours[id];
    }


    public int PossibleValsCount()
    {
        return Behaviours.Count;
    }

    public List<byte> PossibleValsList()
    {
        return Enumerable.Range(0, Behaviours.Count).Select(x => (byte)x).ToList();
    }
}


public class DoubleLimiter : Param
{
    LimiterFunc CheckAny = x => true;
    LimiterFunc CheckAbove { get { return x => x > val.Item2; } }
    LimiterFunc CheckBelow { get { return x => x < val.Item2; } }

    public override void SetVal(object x)
    {
        throw new Exception("Something wrong with limiter!");
        if (x is string)
        {
            var s = (string)x;
            var splits = s.Split(' ');
            var type = splits[0].Trim();
            if (!type.In("Any", "Above", "Below")) throw new Exception("Something wrong with limiter!");
            val = Tuple.Create(type == "Any"
                                   ? LimitType.Any
                                   : type == "Above"
                                         ? LimitType.Above
                                         : LimitType.Below,
                                         type != "Any" ? Double.Parse(splits[1]) : 0);
            return;
        }
        throw new Exception("Can't set param with val " + x);
    }

    public override object Val()
    {
        return val.Item1 == LimitType.Any
                   ? CheckAny
                   : val.Item1 == LimitType.Above
                         ? CheckAbove
                         : CheckBelow;
    }

    LimitType[] Limits;
    int length;
    public Tuple<LimitType, double> val;
    double from;
    double to;

    public override string ToString()
    {
        if (val.Item1 == LimitType.Any) return val.Item1.ToString();

        return val.Item1 + " " + val.Item2;
    }

    public DoubleLimiter(double from, double to, params LimitType[] Limits)
    {
        this.Limits = Limits;
        length = Limits.Length;
        this.from = from;
        this.to = to;
    }

    public override Param Clone()
    {
        return new DoubleLimiter(from, to, Limits) { val = val };
    }

    public override void Generate()
    {
        var indx = _serv.rnd.Next(length);
        var Limit = Limits[indx];
        val = Limit == LimitType.Any
            ? Tuple.Create(Limit, double.NaN)
            : Tuple.Create(Limit, from + _serv.rnd.NextDouble() * (to - from));
    }

    public bool Check(double x)
    {
        var Limit = val.Item1;
        var level = val.Item2;
        if (Limit == LimitType.Any) return true;
        return Limit == LimitType.Above ? x > level : x < level;
    }

    public override void SetValById(byte id)
    {
        throw new Exception("Doubles can be cached!");
    }


    public override object GetValById(byte id)
    {
        throw new Exception("Doubles can be cached!");
    }
}



public class IntLimiter : Param
{
    LimitType[] Limits;
    int length;
    public Tuple<LimitType, int> val;
    int from;
    int to;

    public override void SetVal(object x)
    {
        if (x is string)
        {
            var s = (string)x;
            var splits = s.Split(' ');
            var type = splits[0].Trim();
            if (!type.In("Any", "Above", "Below")) throw new Exception("Something wrong with limiter!");
            val = Tuple.Create(type == "Any"
                                   ? LimitType.Any
                                   : type == "Above"
                                         ? LimitType.Above
                                         : LimitType.Below,
                               type != "Any" ? Int32.Parse(splits[1]) : 0);
            return;
        }
        throw new Exception("Can't set param with val " + x);
    }

    public override string ToString()
    {
        if (val.Item1 == LimitType.Any) return val.Item1.ToString();

        return val.Item1 + " " + val.Item2;
    }

    public IntLimiter(int from, int to, params LimitType[] Limits)
    {
        this.Limits = Limits;
        length = Limits.Length;
        this.from = from;
        this.to = to;
    }

    LimiterFunc CheckAny = x => true;
    LimiterFunc CheckAbove { get { return x => x > val.Item2; } }
    LimiterFunc CheckBelow { get { return x => x < val.Item2; } }

    public override object Val()
    {
        return val.Item1 == LimitType.Any
                   ? CheckAny
                   : val.Item1 == LimitType.Above
                         ? CheckAbove
                         : CheckBelow;
    }

    public override Param Clone()
    {
        return new IntLimiter(from, to, Limits) { val = val, CacheId = CacheId };
    }

    public override void Generate()
    {
        var indx = _serv.rnd.Next(length);
        var Limit = Limits[indx];
        val = Limit == LimitType.Any
            ? Tuple.Create(Limit, 0)
            : Tuple.Create(Limit, from + _serv.rnd.Next(to - from + 1));

        //CacheId = indx, val.Item2;
    }

    public bool Check(double x)
    {
        var Limit = val.Item1;
        var level = val.Item2;
        if (Limit == LimitType.Any) return true;
        return Limit == LimitType.Above ? x >= level : x <= level;
    }


    public int PossibleValsCount()
    {
        throw new Exception("unimplemented");
        var x = (to - from + 1) * (length - (Limits.Any(l => l == LimitType.Any) ? 1 : 0));
        return x == 0 ? 1 : x;
    }

    public override void SetValById(byte id)
    {
        throw new Exception("asd");
        //val = Tuple.Create(Limits[id.id], (int)id.id2);
    }

    public override object GetValById(byte id)
    {
        throw new Exception("asd");
        //val = Tuple.Create(Limits[id.id], (int)id.id2);
    }
}


public class DoubleRangeParam : Param
{
    public double val;
    double from;
    double to;

    public override void SetVal(object x)
    {
        if (x is string)
        {
            val = Double.Parse((string)x);
            return;
        }
        throw new Exception("Can't set param with val " + x);
    }

    public override string ToString()
    {
        return val.ToString();
    }

    public override object Val() { return val; }

    // TODO отрицательные значения?

    public DoubleRangeParam(double from, double to)
    {
        if (from < 0 || to < 0) throw new Exception("No below zero yet!");
        this.from = from;
        this.to = to;
    }

    public override Param Clone()
    {
        return new DoubleRangeParam(from, to) { val = val };
    }

    public override void Generate()
    {
        val = from + _serv.rnd.NextDouble() * (to - from);
    }

    public override void SetValById(byte id)
    {
        throw new Exception("Doubles can be cached!");
    }

    public override object GetValById(byte id)
    {
        throw new Exception("Doubles can be cached!");
    }
}


public class SeparateValsParam<T> : Param, IPossibleValsCount
{
    public T val;
    public List<T> PossibleVals = new List<T>();
    int length;

    public override void SetVal(object x)
    {
        try
        {
            val = (T)Convert.ChangeType(x, typeof(T));
            CacheId = (byte)PossibleVals.FindIndex<T>(p => p.Equals(val));
        }
        catch (Exception e)
        {
            throw new Exception("Can't set param with val " + x + "\r\n" + e);
        }
    }

    public int PossibleValsCount()
    {
        return PossibleVals.Count;
    }

    public override string ToString()
    {
        return val.ToString();
    }

    public override object Val() { return val; }

    // TODO отрицательные значения?

    public SeparateValsParam(List<T> PossibleVals)
    {
        this.PossibleVals = PossibleVals;
        length = PossibleVals.Count;
    }

    public override Param Clone()
    {
        return new SeparateValsParam<T>(PossibleVals) { val = val, CacheId = CacheId };
    }

    public SeparateValsParam(params T[] PossibleVals)
    {
        this.PossibleVals = PossibleVals.ToList();
        length = this.PossibleVals.Count;
    }

    public override void Generate()
    {
        int indx = _serv.rnd.Next(length);
        // randomize the gene
        val = PossibleVals[indx];
        CacheId = (byte)indx;
    }

    public override void SetValById(byte id)
    {
        val = PossibleVals[id];
        CacheId = id;
    }

    public override object GetValById(byte id)
    {
        return PossibleVals[id];
    }

    public List<byte> PossibleValsList()
    {
        return Enumerable.Range(0, PossibleVals.Count).Select(x => (byte)x).ToList();
    }

    public List<object> TruePossibleVals()
    {
        return PossibleVals.Select(x => (object)x).ToList();
    }

}

public interface IPossibleValsCount
{
    int PossibleValsCount();
    List<byte> PossibleValsList();
    List<object> TruePossibleVals();
}


// Redundant
public class IntRangeParam : Param
{
    public int val;
    int from;
    int to;

    public override void SetVal(object x)
    {
        if (x is string)
        {
            val = Int32.Parse((string)x);
            return;
        }
        throw new Exception("Can't set param with val " + x);
    }

    public override object Val() { return val; }

    public override string ToString()
    {
        return val.ToString();
    }

    // TODO отрицательные значения?

    public IntRangeParam(int from, int to)
    {
        if (from < 0 || to < 0) throw new Exception("No below zero yet!");
        this.from = from;
        this.to = to;
    }

    public override Param Clone()
    {
        return new IntRangeParam(from, to) { val = val, CacheId = CacheId };
    }

    public override void Generate()
    {
        val = from + _serv.rnd.Next(to - from + 1);
        CacheId = (byte)val;
    }

    public int PossibleValsCount()
    {
        return to - from + 1;
    }

    public List<short> PossibleValsList()
    {
        return Enumerable.Range(from, to + 1).Select(x => (short)x).ToList();
    }

    public override void SetValById(byte id)
    {
        throw new Exception("");
        val = id;
        CacheId = id;
    }

    public override object GetValById(byte id)
    {
        throw new Exception("");
    }
}
