using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dundas.Charting.WinControl;

public class HistObj
{
    public double Key;
    public double val;
}


namespace ResultViewer.Forms
{
    public partial class Histform : Form
    {

        public static List<double> GetPercentile(IEnumerable<double> values, double down_percent, double up_percent)
        {
            var sorted = values.OrderBy(x => x).ToList();

            var amt_end = (int) (values.Count()*up_percent);
            var amt_start = (int) (values.Count()*down_percent);

            var vals = new List<double>();

            for (int i = amt_start; i < amt_end; i++)
            {
                vals.Add(sorted[i]);
            }

            return vals;

        }

        public Histform(IEnumerable<double> values, int interval = 30, bool Chart = true, string Description = "", string path = "", string XFormat = "N2")
        {
            

            var histInfos = values.Select(x => new HistObj
                {
                    Key = x,
                    val = 1
                }
                ).ToList();

            var KeysToAnalysis = histInfos.Select(x => x.Key).ToList();

            var keys = new List<double>();

            double max = KeysToAnalysis.Max();
            double min = KeysToAnalysis.Min();

            foreach (var h in histInfos){
                if (h.Key > max) h.Key = max;
                if (h.Key < min) h.Key = min;
            }

            var step = (max - min) / interval;

            for (var i = 0; i < interval; i++){
                keys.Add(min + step * i);
            }

            CreateChart(histInfos, keys, Description, 1, true, false, false);

            HistChart.ChartAreas[0].AxisX.LabelStyle.Format = XFormat;

            HistChart.UI.Toolbar.Enabled = true;
            // Enable scale breaks 
            HistChart.ChartAreas["Default"].AxisY.ScaleBreakStyle.Enabled = true;
            // Set the scale break type chart1.ChartAreas["Default"].AxisY.ScaleBreakStyle.BreakLineType = BreakLineType.Wave; 
            // Set the spacing gap between the lines of the scale break (as a percentage of y-axis) 
            HistChart.ChartAreas["Default"].AxisY.ScaleBreakStyle.Spacing = 2;
            // Set the line width of the scale break chart1.ChartAreas["Default"].AxisY.ScaleBreakStyle.LineWidth = 2; 
            // Set the color of the scale break chart1.ChartAreas["Default"].AxisY.ScaleBreakStyle.LineColor = Color.Red; 
            // Show scale break if more than 25% of the chart is empty space chart1.ChartAreas["Default"].AxisY.ScaleBreakStyle.CollapsibleSpaceThreshold = 25; 
            // If all data points are significantly far from zero, // the Chart will calculate the scale minimum value chart1.ChartAreas["Default"].AxisY.ScaleBreakStyle.StartFromZero = AutoBool.Auto;


            if (path != ""){
                HistChart.SaveAsImage(path, ChartImageFormat.Png);
                Close();
            }
            else{
                if (Chart)
                {
                    this.WindowState = FormWindowState.Maximized;
                    
                    Application.Run(this);
                }
            }

        }

        public Chart HistChart
        {
            get { return HistoChart; }
        }

        public Histform(IEnumerable<HistObj> histInfos, int interval = 30, double pct_down = 0, double pct_up = 1, string Description = "", bool ShowPct = true,
                        bool ShowPareto = true, bool ShowTotal = false)
        {

            var KeysToAnalysis = histInfos.Select(x => x.Key).ToList();

            var pct = GetPercentile(KeysToAnalysis, pct_down, pct_up);

            var keys = new List<double>();

            double max = pct.Max();
            double min = pct.Min();

            foreach (var h in histInfos)
            {
                if (h.Key > max) h.Key = max;
                if (h.Key < min) h.Key = min;
            }

            var step = (max - min) / interval;

            for (var i = 0; i < interval; i++)
            {
                keys.Add(min + step * i);
            }

            CreateChart(histInfos, keys, Description, (max - min) / 5, ShowPct, ShowPareto, ShowTotal);
            WindowState = FormWindowState.Maximized;
            Application.Run(this);
            
        }

        public Histform(IEnumerable<HistObj> items, List<double> view_keys, string Description, int interval = 1, bool ShowPct = true, bool ShowPareto = true, bool ShowTotal = false)
        {
            CreateChart(items, view_keys, Description, interval, ShowPct, ShowPareto, ShowTotal);
        }

        public SortedDictionary<double, double> histogram;

        void CreateChart(IEnumerable<HistObj> items, List<double> view_keys, string Description, double interval = 1, bool ShowPct = true, bool ShowPareto = true, bool ShowTotal = false)
        {
            InitializeComponent();
            //WindowState = FormWindowState.Maximized;

            if(Description != "")
                HistoChart.Title = Description;

            histogram = new SortedDictionary<double, double>();
            var histogram_count = new SortedDictionary<double, int>();

            if (ShowPct)
                HistChart.Series[0].LabelFormat = "P1";

            foreach (var i in view_keys)
            {
                histogram.Add(i, 0);
                histogram_count.Add(i, 0);
            }

            var Keys = histogram.Keys.ToList();

            foreach (var item in items)
            {
                if (item.Key <= Keys[0])
                {
                    histogram[Keys[0]] += item.val;
                    histogram_count[Keys[0]]++;
                }
                else
                {
                    if (item.Key >= Keys[Keys.Count - 1])
                    {
                        histogram[Keys[Keys.Count - 1]] += item.val;
                        histogram_count[Keys[Keys.Count - 1]]++;
                    }

                    else
                    {
                        for (int i = 0; i < Keys.Count - 1; i++)
                        {
                            if (item.Key >= Keys[i] && item.Key < Keys[i + 1])
                            {
                                var k = Keys[i];
                                histogram[k] += item.val;
                                histogram_count[k]++;
                                break;
                            }
                        }
                    }
                }
            }

            var total = items.Sum(i => i.val);
            var total_count = items.Count();

            var cum_sum = 0d;
            var cum_count = 0d;

            foreach (var pair in histogram)
            {
                
                var pct_from_total = 100d*(double)pair.Value/(double)total;
                cum_sum += pair.Value;
                cum_count += histogram_count[pair.Key];

                var pareto = 100d * cum_count / total_count;

                if (ShowTotal)
                    pareto = cum_sum;

                int dp;

                if (ShowPct)
                {
                    dp = HistoChart.Series[0].Points.AddXY(pair.Key, pct_from_total);
                    if (RemoveZeros && (int)pct_from_total == 0)
                        HistoChart.Series[0].Points[dp].Label = "";
                }
                else
                {
                    dp = HistoChart.Series[0].Points.AddXY(pair.Key, pair.Value);
                    if (RemoveZeros && pair.Value == 0)
                        HistoChart.Series[0].Points[dp].Label = "";
                }

                    HistoChart.Series[0].Points[dp].Label = " ";

                if(ShowPareto)
                    HistoChart.Series[1].Points.AddXY(pair.Key, pareto);
            }

            HistoChart.ChartAreas[0].AxisX.Interval = 1;

            var sl = new StripLine();
            sl.BorderColor = Color.Black;
            sl.BorderStyle = ChartDashStyle.Dash;


            HistoChart.ChartAreas[0].AxisY.StripLines.Add(sl);


            //HistoChart.Refresh();
        }

        private bool RemoveZeros = true;

        /*public Histform(List<HistObj> items, int bins)
        {
            var keys = new List<double>();

            var max = items.Select(p => p.Key).Max();
            var min = items.Select(p => p.Key).Min();

            for (var i = min; i < max; i += (max - min) / bins)
                keys.Add(i);

            CreateChart(items, keys);
        }/**/
    }
}
