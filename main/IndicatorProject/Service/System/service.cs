using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TradeFramework.ChartObjects;



// Type: LogWriter
// Assembly: TradeFramework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Assembly location: D:\BackTestWorkshop - portfolio edition\BackTestWorkshop\bin\Debug\TradeFramework.dll

using System.IO;
using System.Text;

public class LogWriter
{
    public bool ShowOnConsole = false;
    private TextWriter log;

    public LogWriter(string fname)
    {
        this.init(fname, false);
    }

    public LogWriter(string fname, bool append)
    {
        this.init(fname, append);
    }

    private void init(string fname, bool append)
    {
        Encoding encoding = Encoding.GetEncoding(1251) ?? Encoding.UTF8;
        this.log = (TextWriter)new StreamWriter(fname, append, encoding);
    }

    public void Log<T>(T Msg)
    {
        this.WriteLine((object)Msg);
        this.Flush();
    }

    public void Log(params object[] Msgs)
    {
        this.dLog(" ", Msgs);
        this.Flush();
    }

    public void dLog(string delim, params object[] Msgs)
    {
        string str = "";
        for (int index = 0; index < Msgs.Length; ++index)
            str = str + Msgs[index] + (string)(index != Msgs.Length - 1 ? (object)delim : (object)"");
        this.WriteLine((object)str);
        this.Flush();
    }

    public void WriteLine(object msg)
    {
        if (this.ShowOnConsole)
            etc.prn(new object[1]
      {
        msg
      });
        if (msg != null)
            this.log.WriteLine(msg.ToString());
        else
            this.log.WriteLine("");
    }

    public void Write(object msg)
    {
        if (this.ShowOnConsole)
            etc.prn(new object[1]
      {
        msg
      });
        if (msg == null)
            return;
        this.log.Write(msg.ToString());
    }

    public void Flush()
    {
        this.log.Flush();
    }

    public void Close()
    {
        log.Close();
        log.Dispose();
    }
}


public class _RegressionStat
{

    public double Point(double x) { return a * x + b; }

    public List<double> xval, yval = new List<double>();

    public double a, b, cov, stdx, stdy, R2, corr;

    public _RegressionStat(List<double> xvals, List<double> yvals)
    {
        Calc(xvals, yvals);
    }


    public _RegressionStat(List<double> yvals)
    {
        var cnt = yvals.Count;
        var step = yvals.Last() / cnt;
        var xvals = new List<double>();

        for (int i = 0; i < cnt; i++)
            xvals.Add(xvals.Count != 0 ? xvals.Last() : 0 + step);

        Calc(xvals, yvals);
    }

    public _RegressionStat(double[] xvals, double[] yvals)
    {
        Calc(new List<double>(xvals), new List<double>(yvals));
    }

    private void Calc(List<double> xvals, List<double> yvals)
    {
        double
            v1 = 0,
            v2 = 0,
            SST = 0,
            SSE = 0,
            xAvg = 0,
            yAvg = 0;

        if (xvals.Count != yvals.Count) return;

        for (int i = 0; i < xvals.Count; i++)
        {
            xAvg += xvals[i];
            yAvg += yvals[i];
        }

        xAvg = xAvg / xvals.Count;
        yAvg = yAvg / xvals.Count;

        for (int i = 0; i < xvals.Count; i++)
        {
            v1 += (xvals[i] - xAvg) * (yvals[i] - yAvg);
            v2 += Math.Pow(xvals[i] - xAvg, 2);
            SST += Math.Pow(yvals[i] - yAvg, 2);
        }

        a = v1 / v2;
        b = yAvg - a * xAvg;

        for (int i = 0; i < xvals.Count; i++)
        {
            yval.Add(Point(xvals[i]));
            SSE += Math.Pow(yval[i] - yvals[i], 2);
        }

        cov = v1 / xvals.Count;
        stdx = Math.Sqrt(v2 / xvals.Count);
        stdy = Math.Sqrt(SST / yvals.Count);
        R2 = 1 - SSE / SST;
        corr = cov / (stdx * stdy);

        xval = xvals;

    }

}

public class WebProcessor
{
    private string GeneratedSource { get; set; }
    private string URL { get; set; }

    public string GetGeneratedHTML(string url)
    {
        URL = url;

        Thread t = new Thread(new ThreadStart(WebBrowserThread));
        t.SetApartmentState(ApartmentState.STA);
        t.Start();
        t.Join();

        return GeneratedSource;
    }

    private void WebBrowserThread()
    {
        WebBrowser wb = new WebBrowser();
        wb.Navigate(URL);

        wb.DocumentCompleted +=
            new WebBrowserDocumentCompletedEventHandler(
                wb_DocumentCompleted);

        while (wb.ReadyState != WebBrowserReadyState.Complete)
            Application.DoEvents();

        //Added this line, because the final HTML takes a while to show up
        GeneratedSource = wb.Document.Body.InnerHtml;

        wb.Dispose();
    }

    private void wb_DocumentCompleted(object sender,
        WebBrowserDocumentCompletedEventArgs e)
    {
        WebBrowser wb = (WebBrowser)sender;

        GeneratedSource = wb.Document.Body.InnerHtml;
    }
}


public struct BotPostionAction
{
    public ActionType Type;
    public BotPositionInfo Info;
}

public enum ActionType { Open, Close, OpenAndClose }

[Serializable]
public class BotPositionInfo
{
    public string Asset;

    public bool Long;

    public DateTime OpenDT;
    public double OpenPrice;

    public DateTime CloseDT;

    public double ClosePrice;

    public stringDict userData;

    public double Size;

    public TradeFramework.CloseReason CloseReason;
}


public static partial class MoreEnumerable
{
    /// <summary>
    /// Returns the minimal element of the given sequence, based on
    /// the given projection.
    /// </summary>
    /// <remarks>
    /// If more than one element has the minimal projected value, the first
    /// one encountered will be returned. This overload uses the default comparer
    /// for the projected type. This operator uses immediate execution, but
    /// only buffers a single result (the current minimal element).
    /// </remarks>
    /// <typeparam name="TSource">Type of the source sequence</typeparam>
    /// <typeparam name="TKey">Type of the projected element</typeparam>
    /// <param name="source">Source sequence</param>
    /// <param name="selector">Selector to use to pick the results to compare</param>
    /// <returns>The minimal element, according to the projection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
    {
        return source.MinBy(selector, Comparer<TKey>.Default);
    }

    /// <summary>
    /// Returns the minimal element of the given sequence, based on
    /// the given projection and the specified comparer for projected values.
    /// </summary>
    /// <remarks>
    /// If more than one element has the minimal projected value, the first
    /// one encountered will be returned. This overload uses the default comparer
    /// for the projected type. This operator uses immediate execution, but
    /// only buffers a single result (the current minimal element).
    /// </remarks>
    /// <typeparam name="TSource">Type of the source sequence</typeparam>
    /// <typeparam name="TKey">Type of the projected element</typeparam>
    /// <param name="source">Source sequence</param>
    /// <param name="selector">Selector to use to pick the results to compare</param>
    /// <param name="comparer">Comparer to use to compare projected values</param>
    /// <returns>The minimal element, according to the projection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="selector"/> 
    /// or <paramref name="comparer"/> is null</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
    {
        //source.ThrowIfNull("source");
        //selector.ThrowIfNull("selector");
        //comparer.ThrowIfNull("comparer");
        using (IEnumerator<TSource> sourceIterator = source.GetEnumerator())
        {
            if (!sourceIterator.MoveNext())
            {
                throw new InvalidOperationException("Sequence was empty");
            }
            TSource min = sourceIterator.Current;
            TKey minKey = selector(min);
            while (sourceIterator.MoveNext())
            {
                TSource candidate = sourceIterator.Current;
                TKey candidateProjected = selector(candidate);
                if (comparer.Compare(candidateProjected, minKey) < 0)
                {
                    min = candidate;
                    minKey = candidateProjected;
                }
            }
            return min;
        }
    }
}

public class TQs
{
    private TimeFrame TF;

    public TQs(TimeFrame TF)
    {
        this.TF = TF;
        CreateTQ();
    }

    void CreateTQ()
    {
        if (TF == TimeFrame.D) return;
        TimeSpan TFSpan = TF2Span(TF);
        DateTime dt = default(DateTime);
        while (dt.Day == 1)
        {
            if (dt.Ticks % TFSpan.Ticks == 0) TimeQuants.Add(dt);
            dt = dt.AddMinutes(1);
        }
    }

    List<DateTime> TimeQuants = new List<DateTime>();

    public DateTime getTimeQuantDT(DateTime inDT)
    {
        if (TF == TimeFrame.D)
            return new DateTime(inDT.Year,
                                inDT.Month,
                                inDT.Day, 0, 0, 0);

        var dt = new DateTime(1, 1, 1, inDT.Hour, inDT.Minute, inDT.Second);

        DateTime dt_tq = TimeQuants[TimeQuants.Count - 1];

        for (int i = 1; i < TimeQuants.Count; i++)
        {
            var tq = TimeQuants[i];
            if (dt < tq)
            {
                dt_tq = TimeQuants[i - 1];
                break;
            }
        }

        return new DateTime(inDT.Year,
                            inDT.Month,
                            inDT.Day,
                            dt_tq.Hour,
                            dt_tq.Minute,
                            dt_tq.Second);


    }

    public static TimeSpan TF2Span(TimeFrame tf)
    {
        if (tf == TimeFrame.M1) return TimeSpan.FromMinutes(1);
        if (tf == TimeFrame.M5) return TimeSpan.FromMinutes(5);
        if (tf == TimeFrame.M10) return TimeSpan.FromMinutes(10);
        if (tf == TimeFrame.M15) return TimeSpan.FromMinutes(15);
        if (tf == TimeFrame.M30) return TimeSpan.FromMinutes(30);
        if (tf == TimeFrame.H1) return TimeSpan.FromHours(1);
        if (tf == TimeFrame.D) return TimeSpan.FromHours(0);
        throw new Exception("No such span for TF");
    }
}

/*public static partial class ext
{
    public static void DoInParrallel(params Action[] actions)
    {
        var tasks = new Task[actions.Length];
        for (int i = 0; i < actions.Length; i++)
            tasks[i] = new Task(actions[i]);

        foreach (var task in tasks)
            task.Start();

        Task.WaitAll(tasks);
    }

    public static TimeSpan TimeFrame2TimeSpan(TimeFrame TF)
    {
        switch (TF)
        {
            case TimeFrame.D:
                return new TimeSpan(1, 0, 0, 0, 0);
            case TimeFrame.H1:
                return new TimeSpan(0, 1, 0, 0);
            case TimeFrame.M30:
                return new TimeSpan(0, 0, 30, 0);
            case TimeFrame.M15:
                return new TimeSpan(0, 0, 15, 0);
            case TimeFrame.M5:
                return new TimeSpan(0, 0, 5, 0);
            case TimeFrame.M1:
                return new TimeSpan(0, 0, 1, 0);
        }

        throw new Exception("TimeFrame - TimeSpan conversion error");

    }

    static double HighestHigh(this IRIndex<double> input, int n)
    {
        double hh = Double.NegativeInfinity;
        for (int i = 0; i < n; i++) hh = Math.Max(hh, input.LookBack(i));
        return hh;
    }

    static double LowestLow(this IRIndex<double> input, int n)
    {
        double ll = Double.PositiveInfinity;
        for (int i = 0; i < n; i++) ll = Math.Min(ll, input.LookBack(i));
        return ll;
    }

    public static bool Jitter(this IRIndex<double> indi)
    {

        return (
                   (
                       indi[-3] > indi[-2] &&
                       indi[-2] < indi[-1] &&
                       indi[-1] > indi[0]
                   )
                   ||
                   (
                       indi[-3] < indi[-2] &&
                       indi[-2] > indi[-1] &&
                       indi[-1] < indi[0]
                   )
               );
    }

    public static bool ShowChart;

    public static void SetValue(this chSeries chSeries, double val)
    {
        if (!ShowChart) return;
        if (chSeries.Type != SeriesType.Normal) return;
        chSeries.Add(val);
    }

    public static void SetRange(this chSeries chSeries, double val, double val2)
    {
        if (!ShowChart) return;
        if (chSeries.Type != SeriesType.Normal) return;
        chSeries.Add(val, val2);
    }

    public static void SetPrev(this chSeries chSeries, double val)
    {
        if (!ShowChart) return;
        if (chSeries.Type != SeriesType.Normal) return;
        chSeries.vals[chSeries.vals.Count - 1] = val;
    }

    public static void SetConst(this chSeries chSeries, double ConstValue)
    {
        if (!ShowChart) return;
        chSeries.Type = SeriesType.Const;
        chSeries.vals.Add(ConstValue);
    }

    public static void SafeAdd<T, V, K>(this Dictionary<T, Dictionary<K, V>> dict, T key, K key2, V val)
    {
        if (!dict.ContainsKey(key)) dict.Add(key, new Dictionary<K, V> { { key2, val } });
        else if (!dict[key].ContainsKey(key2)) dict[key].Add(key2, val);
    }

    public static void SafeAdd<T, V, K>(this SortedDictionary<T, Dictionary<K, V>> dict, T key, K key2, V val)
    {
        if (!dict.ContainsKey(key)) dict.Add(key, new Dictionary<K, V> { { key2, val } });
        else if (!dict[key].ContainsKey(key2)) dict[key].Add(key2, val);
    }

    public static void SafeAdd<T, K>(this SortedDictionary<T, List<K>> dict, T key, K key2)
    {
        if (!dict.ContainsKey(key)) dict.Add(key, seq.Create(key2));
        else dict[key].Add(key2);
    }

    public static void RemoveAll(this Delegate Method)
    {
        //foreach (var handler in Method.GetInvocationList())
        Method = Delegate.RemoveAll(Method, Method);
    }



    public static bool Higher(this IRIndex<double> ind)
    {
        return ind.Count > 1 && ind[0] > ind[-1];
    }

    public static bool Lower(this IRIndex<double> ind)
    {
        return ind.Count > 1 && ind[0] < ind[-1];
    }

    public static bool Rising(this IRIndex<double> ind)
    {
        return ind.Count > 2 && ind[0] > ind[-1] && ind[-1] > ind[-2];
    }

    public static bool Falling(this IRIndex<double> ind)
    {
        return ind.Count > 2 && ind[0] < ind[-1] && ind[-1] < ind[-2];
    }

    public static bool FoundMin(this IRIndex<double> ind)
    {
        return ind.Count > 2 && ind[0] > ind[-1] && ind[-2] > ind[-1];
    }

    public static bool FoundMax(this IRIndex<double> ind)
    {
        return ind.Count > 2 && ind[0] < ind[-1] && ind[-2] < ind[-1];
    }

    public static bool HigherBar(this BarData Bar)
    {
        return Bar.Close > Bar.Open;
    }

    public static bool LowerBar(this BarData Bar)
    {
        return Bar.Close < Bar.Open;
    }

    /*public static BarData LowestLow(BarData FirstBar, BarData LastBar) { return getExtremeBar(FirstBar, LastBar, true); }

    public static BarData HighestHigh(BarData FirstBar, BarData LastBar) { return getExtremeBar(FirstBar, LastBar, false); }

    public static int BarsDistance(BarData FirstBar, BarData LastBar)
    {
        BarData NodeBar = LastBar;
        int BarCounter = 0;
        while (NodeBar != FirstBar.PrevBar)
        {
            NodeBar = NodeBar.PrevBar;
            BarCounter++;
        }
        return BarCounter;


    }


    public static BarData getExtremeBar(BarData FirstBar, BarData LastBar, bool low)
    {
        var NodeBar = LastBar;
        var ExtremeBar = NodeBar;
        while (NodeBar != FirstBar.PrevBar)
        {
            if (low && ExtremeBar.Low > NodeBar.Low) ExtremeBar = NodeBar;
            if (!low && ExtremeBar.High < NodeBar.High) ExtremeBar = NodeBar;
            NodeBar = NodeBar.PrevBar;
        }
        return ExtremeBar;
    }



    public static int Distance(this BarData bar, BarData OlderBar)
    {
        BarData NodeBar = OlderBar;
        int BarCounter = 0;
        while (NodeBar != bar)
        {
            NodeBar = NodeBar.PrevBar;
            BarCounter++;
        }
        return BarCounter;
    }


    public static bool inRange(this BarData bar, double price) { return price > bar.Low && price < bar.High; }
    public static bool inOpenCloseRange(this BarData bar, double price) { return (price > Math.Min(bar.Open, bar.Close) && price < Math.Max(bar.Open, bar.Close)); }

    public static double trueHigh(this BarData bar)
    {
        return Math.Max(bar.PrevBar.Close, bar.High);
    }
    public static double trueLow(this BarData bar)
    {

        return Math.Min(bar.PrevBar.Close, bar.Low);
    }

    public static double trueRange(this BarData bar)
    {
        // max of: (high and low), (previous closing and current maximum;), (previous closing price and the current minimum.)
        return Math.Max(Math.Max(Math.Abs(bar.High - bar.Low), Math.Abs(bar.PrevBar.Close - bar.High)),
            Math.Abs(bar.PrevBar.Close - bar.Low));
    }
 
}   /**/

public static class Service
{
    public static T Clone<T>(T source)
    {
        if (!typeof(T).IsSerializable)
        {
            throw new ArgumentException("The type must be serializable.", "source");
        }

        // Don't serialize a null object, simply return the default for that object
        if (Object.ReferenceEquals(source, null))
        {
            return default(T);
        }

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new MemoryStream();
        using (stream)
        {
            formatter.Serialize(stream, source);
            stream.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(stream);
        }
    }

    public static IEnumerable<FieldInfo> GetAllFields(Type t)
    {
        if (t == null)
            return Enumerable.Empty<FieldInfo>();

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        return t.GetFields(flags).Union(GetAllFields(t.BaseType));
    }
}


public static class csv
{

    public static List<Dictionary<string, string>> ReadDict(string FileName, char delimter = ';')
    {
        var enterFileContent = File.ReadAllLines(FileName).Where(x => x.Trim() != "").ToArray();
        var datatable = new List<Dictionary<string, string>>();

        var header = enterFileContent[0].Split(delimter).Select(x => x.Trim()).ToList();

        string[] tempstr;
        int rows = enterFileContent.Length;
        for (int i = 1; i < rows; i++)
        {
            tempstr = enterFileContent[i].Split(delimter);
            var dict = new Dictionary<string, string>();

            for (int j = 0; j < tempstr.Length; j++)
            {
                if (header[j] == "") continue;
                dict.Add(header[j], tempstr[j].Trim());
            }

            datatable.Add(dict);
        }

        return datatable;
    }

    public static List<string[]> Read(string FileName, char delimter = ';')
    {
        var enterFileContent = File.ReadAllLines(FileName).Where(x => x.Trim() != "").ToArray();
        var datatable = new List<string[]>();

        string[] tempstr;
        int rows = enterFileContent.Length;
        for (int i = 0; i < rows; i++)
        {
            tempstr = enterFileContent[i].Split(delimter);
            datatable.Add(tempstr);
        }

        return datatable;
    }
}