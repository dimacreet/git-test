using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dundas.Charting.WinControl;

namespace ResultViewer.Forms
{
    public partial class Scatter : Form
    {
        public Chart HistChart
        {
            get { return HistoChart; }
        }

        public Scatter(IEnumerable<double> _x, IEnumerable<double> _y, bool Lines = false, string XFormat = "N2", List<string> names = null, List<double> weights = null, string Description = "", bool run = true)
        {

            var x = _x.ToList();
            var y = _y.ToList();

            InitializeComponent();
            HistChart.ChartAreas[0].AxisX.LabelStyle.Format = XFormat;
            HistChart.ChartAreas[0].AxisY.LabelStyle.Format = "N2";
            HistChart.ChartAreas[0].AxisX.LabelStyle.Format = "N2";
            //HistChart.ChartAreas[0].AxisX.Title = "Day";
            //HistChart.ChartAreas[0].AxisY.Title = "Change, %";

            HistChart.Title = Description;

            HistChart.Series[0].Type = SeriesChartType.Point;

            if (Lines)
                HistChart.Series[0].Type = SeriesChartType.Line;

            HistChart.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
            HistChart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightBlue;
            HistChart.ChartAreas[0].AxisX.MajorGrid.LineStyle = ChartDashStyle.Dash;

            HistChart.ChartAreas[0].AxisY.MinorGrid.Enabled = true;
            HistChart.ChartAreas[0].AxisY.MinorGrid.LineColor = Color.LightBlue;
            HistChart.ChartAreas[0].AxisY.MinorGrid.LineStyle = ChartDashStyle.Dash;

            HistChart.Series[0].ShowLabelAsValue = false;

            HistChart.ChartAreas[0].AxisX.Interval = (x.Max() - x.Min()) / 50;

            //HistChart.ChartAreas[0].AxisX.Interval = 1;

            HistChart.ChartAreas[0].AxisY.Maximum = y.Max();
            HistChart.ChartAreas[0].AxisY.Minimum = y.Min();

            HistChart.ChartAreas[0].AxisY.Interval = (y.Max() - y.Min())/10;

            HistChart.Series[0].BorderWidth = 1;
            HistChart.Series[0].BorderColor = Color.LightCyan;

            /*if (weights != null)
            {
                HistChart.Series[0].Type = SeriesChartType.Point;
                HistChart.Series[0].BorderWidth = 1;
            }*/

            WindowState = FormWindowState.Maximized;


            var sl = new StripLine();
            sl.BorderColor = Color.Black;
            sl.BorderStyle = ChartDashStyle.Dash;


            HistoChart.ChartAreas[0].AxisY.StripLines.Add(sl);
            

            for (int i = 0; i < x.Count; i++)
            {

                if(weights != null && weights[i] == 0) continue;

                var p = HistChart.Series[0].Points[HistChart.Series[0].Points.AddXY(x[i],y[i])];
                p.Label = "";
                p.Color = Color.DarkRed;
                p.BorderWidth = 3;

                if (weights != null)
                {
                    p.MarkerSize = (int)(100*(weights[i]/100d)+5);
                    p.MarkerStyle = MarkerStyle.Square;
//
                    //if(weights[i] > 5)
                        //p.Label = weights[i].ToString("N0") + "%";
                    //p.YValues[1] = weights[i];
                }

                /*if (names != null)
                {
                    p.Label = names[i];
                    p.ToolTip = names[i];
                    p.LabelToolTip = names[i];
                }
                else
                {
                    
                    p.ToolTip = " ";
                    p.LabelToolTip = " ";
                }*/

            }

            if(run)
                Application.Run(this);

        }

    }
}

