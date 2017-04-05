using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TradeFramework;

[Serializable]
public class KeyArrayWrapper<T>
{

    public KeyArrayWrapper(T[] data)
    {
        this.data = data;
    }

    public T[] data;

    protected int Hash;

    public override int GetHashCode()
    {
        return Hash;
    }

    public override bool Equals(object obj)
    {
        var right = ((KeyArrayWrapper<T>)obj).data;
        //if (data == null || right == null) return data == right;
        if (data.Length != right.Length) return false;
        for (int i = 0; i < data.Length; i++)
            if (!data[i].Equals(right[i])) return false;

        return true;
    }
}

[Serializable]
public class ParamsId : KeyArrayWrapper<byte>
{
    public ParamsId(byte[] data)
        : base(data)
    {
        // Probably need the more good solution
        Hash = (int)_serv.ArrayHash.ComputeHash(data);
    }
}


/*
public abstract class BaseTradeFitness : IFitnessFunction {

    public ParamMix ParamMix;
    public Dictionary<ParamsId, double> CachedFitness = new Dictionary<ParamsId, double>();
    public bool DoCacheFitness = true;
    
    protected BaseTradeFitness(ParamMix ParamMix){

        _paramsGlobal.SavingResultsMode = SavingResultsMode.Optimization;
        _paramsGlobal.ShowChart = false;

        this.ParamMix = ParamMix;
        DoCacheFitness = ParamMix.CanBeCached;

    }

    public double EvaluateParams(Param[] Params) {
        return FitnessRatio(RunStrategy(Params));
    }

    SimpleSpinLock spinLock = new SimpleSpinLock();

    public double Evaluate(IChromosome chromosome){

        var Params = ((tradeChromosome) chromosome).val;

        Interlocked.Increment(ref _serv.ChromCount);
        ParamsId CacheId = null;

        if (DoCacheFitness){

            CacheId = ParamMix.GetCacheId(Params);

            double CachedVal;
            //lock (CachedFitness){
            spinLock.Enter();
            var GetVal = CachedFitness.TryGetValue(CacheId, out CachedVal);
            spinLock.Leave();
            if (GetVal){
                _serv.CachedIndiCheck++;
                return CachedVal;
            }
        }

        var fitness = FitnessRatio(RunStrategy(Params));

        if (DoCacheFitness){
            //lock (CachedFitness){
            spinLock.Enter();
            //if (!CachedFitness.ContainsKey(CacheId))
            try{
                CachedFitness.Add(CacheId, fitness);
            }
            catch(Exception ex){
                etc.prn(ex);
            }
            finally{
                spinLock.Leave();
            }
        }

        return fitness;
    }

    protected abstract double FitnessRatio(OptimizeTradingSystem system);

    public void ProcessResults(string Description, Param[] Params){
        _paramsGlobal.SavingResultsMode = SavingResultsMode.Full;
        _paramsGlobal.ShowChart = true;

        var strat = RunStrategy(Params);
        var execParams = new ExecParams();
        execParams.Params = strat._params;
        execParams.Symbols = new List<SymbolData> { TradeSymbol };
        //execParams.TaskName = ParamMix.ToStringVals(Params);
        execParams.TaskName = Description;

        strat.Positions.getTradingResults().ProcessResults(execParams);

        _paramsGlobal.SavingResultsMode = SavingResultsMode.Optimization;
        _paramsGlobal.ShowChart = false;
    }

    

    public string csFile;
    public Type LaunchParamsType { get; private set; }
    public _params LaunchParams
    {
        get { return _LaunchParams; }
        set
        {
            LaunchParamsType = value.GetType();
            _LaunchParams = value;
        }
    }

    private _params _LaunchParams;
    public SymbolData TradeSymbol;

    public void WriteCastedCode2File(Param[] Params, string fname) {
        if (csFile == "") throw new Exception("No csFile name!");
        var code = ParamMix.CastCode(csFile, Params);
        etc.Write2File(code, fname, true);
    }


    _params getLaunchParams(Param[] Params){
        object nParams = etc.GetNewObject(LaunchParamsType);
        LaunchParams.CastVals(ref nParams);
        ParamMix.CastVals(ref nParams, Params);
        return (_params) nParams;
    }

    OptimizeLaunchBars launchBars;

    protected OptimizeTradingSystem RunStrategy(Param[] Params){

        if(launchBars == null){

            var leng = TradeSymbol.Bars.Count;

            var Bars = new BarData[leng];

            var Close = new double[leng];
            var Open = new double[leng];
            var High = new double[leng];
            var Low = new double[leng];
            var Dates = new DateTime[leng];

            for (int i = 0; i < leng; i++)
            {
                var bar = TradeSymbol.Bars[i];
                Bars[i] = bar;
                Open[i] = bar.Open;
                High[i] = bar.High;
                Low[i] = bar.Low;
                Close[i] = bar.Close;
                Dates[i] = bar.DateTime;
            }

            launchBars = new OptimizeLaunchBars
            {
                Bars = Bars,
                Close = Close,
                High = High,
                Open = Open,
                Dates = Dates,
                Low = Low
            };
        }
        
        var strategy = (OptimizeStrategyBase)etc.GetNewObject(LaunchParams.Strategy);

        var system = new OptimizeTradingSystem(launchBars, getLaunchParams(Params), strategy);

        strategy.system = system;
        strategy.pStartup();

        system.Go();

        return system;
    }


}









/*public void OLDProcessResults(Param[] Params)
    {
        _paramsGlobal.SavingResultsMode = SavingResultsMode.Full;
        _paramsGlobal.ShowChart = true;

        var nParams = getLaunchParams(Params);

        var strategy = (StrategyBase)etc.GetNewObject((nParams).Strategy);

        var system = new TradingSystem();
        system.Init(nParams, TradeSymbol, new List<SymbolData> { TradeSymbol });
        // Startup

        system.Strategy = strategy;
        strategy.system = system;
        strategy.pStartup();

        system.Go();

        var execParams = new ExecParams();
        execParams.Params = nParams;
        execParams.Symbols = new List<SymbolData> { TradeSymbol };
        execParams.TaskName = _paramsGlobal.ProjectName;

        system.Results.ProcessResults(execParams);

        _paramsGlobal.SavingResultsMode = SavingResultsMode.Optimization;
        _paramsGlobal.ShowChart = false;
    }/**/