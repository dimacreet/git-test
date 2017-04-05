using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dundas.Charting.WinControl;
using TradeFramework;
using TradeFramework.ChartObjects;

namespace ResultViewer.Forms
{

    public partial class EquityResultChart : Form
    {

        public Chart extChart
        {
            get { return BarChart; }
        }

        public void AddChartArea(string Name)
        {
            var chartArea1 = BarChart.ChartAreas.Add(Name);

            chartArea1.AlignWithChartArea = "Default";
            chartArea1.AxisX.LabelsAutoFit = false;
            chartArea1.AxisX.LabelStyle.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular,
                                                                       System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            chartArea1.AxisX.LabelStyle.Format = "d";
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            chartArea1.AxisX.MajorGrid.LineStyle = Dundas.Charting.WinControl.ChartDashStyle.Dash;
            chartArea1.AxisX.MajorTickMark.Enabled = false;
            chartArea1.AxisX.MajorTickMark.LineColor = System.Drawing.Color.Silver;
            chartArea1.AxisX.ScrollBar.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (224)))),
                                                                                 ((int) (((byte) (224)))),
                                                                                 ((int) (((byte) (224)))));
            chartArea1.AxisX.ScrollBar.ButtonColor = System.Drawing.Color.Gray;
            chartArea1.AxisX.ScrollBar.LineColor = System.Drawing.Color.Black;
            chartArea1.AxisX.ScrollBar.PositionInside = false;
            chartArea1.AxisX2.Enabled = Dundas.Charting.WinControl.AxisEnabled.False;
            chartArea1.AxisX2.LabelsAutoFit = false;
            chartArea1.AxisX2.LabelStyle.Font = new System.Drawing.Font("Tahoma", 8.25F,
                                                                        System.Drawing.FontStyle.Regular,
                                                                        System.Drawing.GraphicsUnit.Point,
                                                                        ((byte) (204)));
            chartArea1.AxisX2.MajorGrid.LineColor = System.Drawing.Color.Silver;
            chartArea1.AxisX2.MajorTickMark.Enabled = false;
            chartArea1.AxisX2.MajorTickMark.LineColor = System.Drawing.Color.Silver;
            chartArea1.AxisX2.ScrollBar.Enabled = false;
            chartArea1.AxisY.Enabled = Dundas.Charting.WinControl.AxisEnabled.False;
            chartArea1.AxisY.LabelStyle.Enabled = false;
            chartArea1.AxisY.LineStyle = Dundas.Charting.WinControl.ChartDashStyle.NotSet;
            chartArea1.AxisY.MajorGrid.Enabled = false;
            chartArea1.AxisY.MajorTickMark.Enabled = false;
            chartArea1.AxisY.ScrollBar.Enabled = false;
            chartArea1.AxisY.StartFromZero = false;
            chartArea1.AxisY2.Enabled = Dundas.Charting.WinControl.AxisEnabled.True;
            chartArea1.AxisY2.Title = Name;
            chartArea1.AxisY2.LabelsAutoFit = false;
            chartArea1.AxisY2.LabelsAutoFitStyle = Dundas.Charting.WinControl.LabelsAutoFitStyle.WordWrap;
            chartArea1.AxisY2.LabelStyle.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular,
                                                                        System.Drawing.GraphicsUnit.Point,
                                                                        ((byte) (204)));
            chartArea1.AxisY2.LabelStyle.Format = "N1";
            chartArea1.AxisY2.MajorGrid.LineColor = System.Drawing.Color.Silver;
            chartArea1.AxisY2.MajorGrid.LineStyle = Dundas.Charting.WinControl.ChartDashStyle.Dash;
            chartArea1.AxisY2.MajorTickMark.Enabled = false;
            chartArea1.AxisY2.ScrollBar.Enabled = false;
            chartArea1.AxisY2.StartFromZero = false;
            chartArea1.BackColor = System.Drawing.Color.White;
            chartArea1.BorderColor = System.Drawing.Color.White;
            chartArea1.CursorX.LineWidth = 2;
            chartArea1.CursorX.UserEnabled = true;
            chartArea1.CursorX.UserSelection = true;

            var sl = new StripLine();
            sl.BorderColor = Color.Black;
            sl.BorderStyle = ChartDashStyle.Dash;

            chartArea1.AxisY2.StripLines.Add(sl);
        }

        public EquityResultChart()
        {
            InitializeComponent();
            

        }

        public void DrawAnnotation()
        {
            PolylineAnnotation polylineAnnotation = new PolylineAnnotation();
            polylineAnnotation.LineColor = Color.Black;
            polylineAnnotation.LineWidth = 4;
            polylineAnnotation.ShadowOffset = 0;
            this.BarChart.Annotations.Add((Annotation)polylineAnnotation);
            polylineAnnotation.FreeDrawPlacement = true;
            polylineAnnotation.BeginPlacement();
        }

        private List<BarData> curBars;




        public void SetPosition(List<BarData> Bars, BotPositionInfo info = null)
        {
            curBars = Bars;

            BarChart.ChartAreas[0].AxisY2.Title = "";
            BarChart.ChartAreas[0].AxisY2.LabelStyle.Format = "N3";

            BarChart.Series.Clear();
            BarChart.Annotations.Clear();


            var clust_ser = new Series("data");
            clust_ser.YValueType = ChartValueTypes.Double;

            BarChart.Series.Add(clust_ser);

            BarChart.Series["data"].Type =
                Global.BarsAsCandles ? SeriesChartType.CandleStick : SeriesChartType.Stock;

            BarChart.Series["data"].Color = Color.Black;

            SetBars(Bars);


            var LabelSerie = BarChart.Series["data"];

            xDT.ForEach((dt, indx) => LabelSerie.Points[indx].AxisLabel = dt.ToString("dd.MM.yyyy"));// HH:MM

            if (info != null)
                SetPositions(new List<BotPositionInfo> { info });

            InitAxis(null, null);
        }

        private int AxisMargin = 25;

        private void InitAxis(object sender, ViewEventArgs e)
        {

            foreach (ChartArea chArea in BarChart.ChartAreas)
            {
                var yAxis = chArea.AxisY;
                var y2Axis = chArea.AxisY2;
                var Max = curBars.Select(x => x.High).Max();
                var Min = curBars.Select(x => x.Low).Min();

                var delta = Max*Min >= 0
                                ? Math.Abs(Max) - Math.Abs(Min)
                                : Math.Abs(Max) + Math.Abs(Min);


                
                

                var margin = AxisMargin*delta/100;
                if (Min != Max)
                {
                    yAxis.Maximum = Max + margin;
                    yAxis.Minimum = Min - margin;

                    y2Axis.Maximum = Max + margin;
                    y2Axis.Minimum = Min - margin;
                }
            }

            var LabelSerie = BarChart.Series["data"];

            var setInterval = curBars.Count/6;
            foreach (ChartArea area in BarChart.ChartAreas)
            {
                area.AxisX.MajorGrid.Interval = setInterval;
            }

            BarChart.ChartAreas["Default"].AxisX.LabelStyle.Interval = setInterval;

            for (int i = 0; i < curBars.Count; i++)
            {
                LabelSerie.Points[i].AxisLabel = xDT[i].ToString("dd.MM.yyyy");// HH:mm
            }

            BarChart.ChartAreas["Default"].AxisY2.Interval = 1;
            //foreach (Annotation ann in BarChart.Annotations){
            //ann.Visible = true;
            //}/**/
            //ann.ResizeToContent();
        }

        private void SetBars(IEnumerable<BarData> Bars)
        {
            xDT.Clear();
            dict_xDT.Clear();
            int p = 0;

            foreach (var bar in Bars)
            {

                BarChart.Series["data"].Points.AddXY(p, bar.High, bar.Low, bar.Open, bar.Close);

                xDT.Add(bar.DateTime);
                dict_xDT.Add(bar.DateTime, p);
                ++p;
            }

        }

        private bool BlackTheme = false;

        private void SetPositions(IEnumerable<BotPositionInfo> Positions)
        {
            var LastDT = xDT.Last();
            var StartDT = xDT.First();

            var BarsStart = curBars[0].Open;
            var BarsEnd = curBars.Last().Close;

            //var arrows = new List<DateTime>();

            foreach (var pos in Positions)
            {
                ArrowAnnotation arr = null;
                var posOpenDT = pos.OpenDT < StartDT ? StartDT : pos.OpenDT;
                var posCloseDT = pos.CloseDT > LastDT ? LastDT : pos.CloseDT;
                //if (!arrows.Contains(pos.OpenDT)){

                arr = AddArrow(pos.Long, pos.OpenDT, pos.OpenPrice, "Default");

                if (arr != null)
                {
                    arr.Tag = pos;
                    //      arrows.Add(pos.OpenDT);
                }
                else continue;
                //} else 
                //continue;


                // TODO Open And closing lines are HERE

                var profit =  pos.Long ? (pos.ClosePrice - pos.OpenPrice) : (pos.OpenPrice - pos.ClosePrice);


                var posL = AddLine(posOpenDT,
                                   pos.OpenDT >= StartDT ? pos.OpenPrice : BarsStart,
                                   posCloseDT,
                                   pos.CloseDT <= LastDT ? pos.ClosePrice : BarsEnd,
                                   3,
                                   profit > 0 ? LineType.Solid : LineType.Dash,
                                   BlackTheme ? Color.White : Color.Black, "Default");


                Series slL = null;
                Series tpL = null;
                if (pos.userData != null && pos.userData["StopLoss"] != null)
                {
                    slL = AddLine(posOpenDT, pos.userData["StopLoss"].To<double>(), posCloseDT,
                                  pos.userData["StopLoss"].To<double>(), 2,
                                  LineType.Dot, Color.Red, "Default");
                }

                if (pos.userData != null && pos.userData["TakeProfit"] != null)
                {
                    tpL = AddLine(posOpenDT, pos.userData["TakeProfit"].To<double>(), posCloseDT,
                                  pos.userData["TakeProfit"].To<double>(), 2,
                                  LineType.Dot, Color.Green, "Default");
                }



                //AddBarsLine(pos.OpenDT, pos.CloseDT, 3, pos.Profit > 0?LineType.Solid:LineType.Dash, Color.White);
            }
        }

        public EquityResultChart(IEnumerable<string> xDT, IEnumerable<List<double>> _in_datas = null, string Name = "", List<Color> colors = null)
        {
            InitializeComponent();

            //var xDT = in_data != null ? Enumerable.Range(0, in_data.Count()).ToList() : Enumerable.Range(0, _in_datas.First().Count()).ToList();

            BarChart.Title = Name;
            BarChart.Titles[0].Font = new Font("Arial", 10F, FontStyle.Bold);


            var sl = new StripLine();
            sl.BorderColor = Color.Black;
            sl.BorderStyle = ChartDashStyle.Dash;

            //BarChart.ChartAreas[0].AxisY2.StripLines.Add(sl);
            //

            BarChart.ChartAreas[0].AxisY2.LabelStyle.Format = "N2";


            if (_in_datas != null)
            {
                var datas = _in_datas.ToList();

                BarChart.Series.Clear();

                for (int i = 0; i < datas.Count(); i++)
                {
                    var data = datas[i];
                    var ser = new Series("ser" + i);

                    ser.Points.DataBindXY(xDT, data.Select(x => x).ToList());
                    ser.Type = SeriesChartType.FastLine;
                    ser.XValueIndexed = false;

                    if (colors != null)
                        ser.Color = colors[i];
                    else 
                        ser.Color = etc.RandomColor;

                    ser.BorderWidth = 2;
                    ser.YValueType = ChartValueTypes.Double;

                    BarChart.Series.Add(ser);
                }
            }

            BarChart.UI.Toolbar.Enabled = true;
            // Enable scale breaks 
            BarChart.ChartAreas["Default"].AxisY.ScaleBreakStyle.Enabled = true;
            // Set the scale break type chart1.ChartAreas["Default"].AxisY.ScaleBreakStyle.BreakLineType = BreakLineType.Wave; 
            // Set the spacing gap between the lines of the scale break (as a percentage of y-axis) 
            BarChart.ChartAreas["Default"].AxisY.ScaleBreakStyle.Spacing = 2;

            /*if (in_data != null)
            {
                var clust_ser = new Series("cluster");
                clust_ser.Points.DataBindXY(xDT, in_data.Select(x => x).ToList());
                clust_ser.Type = SeriesChartType.FastLine;
                clust_ser.Color = Color.Red;
                clust_ser.BorderWidth = 2;
                clust_ser.YValueType = ChartValueTypes.Double;

                BarChart.Series.Add(clust_ser);
            }*/

            BarChart.ChartAreas[0].AxisX.LabelStyle.Interval = _in_datas.ToList()[0].Count/10;
            BarChart.ChartAreas[0].AxisX.MajorGrid.Interval = _in_datas.ToList()[0].Count / 10;

            BarChart.Invalidate();
            BarChart.Refresh();
        }

        public EquityResultChart(IEnumerable<double> in_data = null, IEnumerable<List<double>> _in_datas = null)
        {
            InitializeComponent();

            var xDT = in_data != null ? Enumerable.Range(0, in_data.Count()).ToList() : Enumerable.Range(0, _in_datas.First().Count()).ToList();

            BarChart.Title = Name;

            var sl = new StripLine();
            sl.BorderColor = Color.Black;
            sl.BorderStyle = ChartDashStyle.Dash;

            //BarChart.ChartAreas[0].AxisY2.StripLines.Add(sl);
//

            BarChart.ChartAreas[0].AxisY2.LabelStyle.Format = "N2";
            

            if (_in_datas != null)
            {
                var datas = _in_datas.ToList();

                BarChart.Series.Clear();

                for (int i = 0; i < datas.Count(); i++)
                {
                    var data = datas[i];
                    var ser = new Series("ser" + i);

                    ser.Points.DataBindXY(xDT, data.Select(x => x).ToList());
                    ser.Type = SeriesChartType.FastLine;
                    ser.XValueIndexed = false;
                    ser.Color = Color.RoyalBlue;
                    ser.BorderWidth = 2;
                    ser.YValueType = ChartValueTypes.Double;

                    BarChart.Series.Add(ser);
                }
            }

            BarChart.UI.Toolbar.Enabled = true;
            // Enable scale breaks 
            BarChart.ChartAreas["Default"].AxisY.ScaleBreakStyle.Enabled = true;
            // Set the scale break type chart1.ChartAreas["Default"].AxisY.ScaleBreakStyle.BreakLineType = BreakLineType.Wave; 
            // Set the spacing gap between the lines of the scale break (as a percentage of y-axis) 
            BarChart.ChartAreas["Default"].AxisY.ScaleBreakStyle.Spacing = 2;

            if (in_data != null)
            {
                var clust_ser = new Series("cluster");
                clust_ser.Points.DataBindXY(xDT, in_data.Select(x => x).ToList());
                clust_ser.Type = SeriesChartType.FastLine;
                clust_ser.Color = Color.Red;
                clust_ser.BorderWidth = 2;
                clust_ser.YValueType = ChartValueTypes.Double;

                BarChart.Series.Add(clust_ser);
            }/**/

            BarChart.Invalidate();
            BarChart.Refresh();
        }


        public EquityResultChart(List<DateTime> xDT, List<double> datas, string Name = "")
        {
            InitializeComponent();

            this.xDT = xDT;
            
            BarChart.Title = Name;
            BarChart.Titles[0].Font = new Font("Arial", 10F, FontStyle.Bold);


            var clust_ser = new Series("cluster");
            clust_ser.Points.DataBindXY(xDT, datas);
            clust_ser.XValueIndexed = true;
            clust_ser.Type = SeriesChartType.FastLine;
            //clust_ser.Color = Color.White;
            //Chart.ChartAreas[0].BackHatchStyle  = ChartHatchStyle.Percent70;
            //Chart.ChartAreas[0].BackColor = Color.Gray;

            clust_ser.BackHatchStyle = ChartHatchStyle.None;
            //clust_ser.BorderColor = Color.Black;
            clust_ser.BorderWidth = 2;
            clust_ser.YValueType = ChartValueTypes.Double;
            clust_ser.Color = Color.Red;

            BarChart.Series.Add(clust_ser);

            BarChart.Invalidate();
            BarChart.Refresh();

            var sl = new StripLine();
            sl.BorderColor = Color.Black;
            sl.BorderStyle = ChartDashStyle.Dash;

            BarChart.ChartAreas[0].AxisY2.StripLines.Add(sl);
            ///BarChart.AxisViewChanged = InitAxis(null, null);

            //InitAxis(null, null);

        }

        public Chart Chart
        {
            get { return BarChart; }
        }


        public void AddSerieNoDT(List<double> data, Color color, int width, string Area = "", SeriesChartType type = SeriesChartType.FastLine, ChartDashStyle style = ChartDashStyle.Solid)
        {
            var ser = new Series("ser" + BarChart.Series.Count);

            ser.Points.DataBindXY(Enumerable.Range(0, data.Count).ToList(), data.Select(x => x).ToList());
            ser.Type = type;
            ser.Color = color;
            ser.BorderStyle = style;
            ser.BorderWidth = width;
            ser.YValueType = ChartValueTypes.Double;

            if (Area != "")
                ser.ChartArea = Area;
            
            BarChart.Series.Add(ser);
        }




        public void AddSerie(IEnumerable<double> data_in, Color color, int width, string Area = "", SeriesChartType type = SeriesChartType.FastLine, ChartDashStyle style = ChartDashStyle.Solid)
        {
            var data = data_in.ToList();

            var ser = new Series("ser" + BarChart.Series.Count);

            for (int i = 0; i < data.Count; i++)
            {
                var p_indx = ser.Points.AddXY(xDT[i], data[i]);
                var p = ser.Points[p_indx];
                if (data[i].isNaN()) p.Color = Color.Transparent;
            }

            
            ser.Type = type;
            ser.Color = color;
            ser.BorderWidth = width;
            ser.YValueType = ChartValueTypes.Double;
            ser.XValueIndexed = true;
            ser.BorderStyle = style;
            

            if (Area != "")
                ser.ChartArea = Area;

            BarChart.Series.Add(ser);
        }

        public List<DateTime> xDT = new List<DateTime>();
        private Dictionary<DateTime, int> dict_xDT  = new Dictionary<DateTime, int>();

        public EquityResultChart(List<DateTime> xDT, IEnumerable<double> in_cluster, IEnumerable<List<double>> in_datas,
                                 string Name = "")
        {
            InitializeComponent();
            this.xDT = xDT;

            BarChart.Title = Name;

            var sl = new StripLine();
            sl.BorderColor = Color.Black;
            sl.BorderStyle = ChartDashStyle.Dash;

            BarChart.ChartAreas[0].AxisY2.StripLines.Add(sl);

            var cluster = in_cluster.ToList();
            var datas = in_datas.ToList();

            BarChart.Series.Clear();

            for (int i = 0; i < datas.Count(); i++)
            {
                var data = datas[i];
                var ser = new Series("ser" + i);

                ser.Points.DataBindXY(xDT, data.Select(x => x).ToList());
                ser.Type = SeriesChartType.Line;
                ser.Color = Color.Gray;
                ser.BorderWidth = 1;
                ser.YValueType = ChartValueTypes.Double;
                ser.XValueIndexed = true;

                BarChart.Series.Add(ser);
            }

            var clust_ser = new Series("cluster");
            clust_ser.Points.DataBindXY(xDT, cluster.Select(x => x).ToList());
            clust_ser.Type = SeriesChartType.Line;
            clust_ser.Color = Color.Red;
            clust_ser.BorderWidth = 2;
            clust_ser.YValueType = ChartValueTypes.Double;
            clust_ser.XValueIndexed = true;
            

            BarChart.Series.Add(clust_ser);


            BarChart.Invalidate();
            BarChart.Refresh();

        }



        private void AddBarsLine(DateTime fromDT, DateTime toDT, int LineWidth, LineType LineType, Color color)
        {
            var marker = new FinancialMarker();
            int from = dict_xDT[fromDT];
            int to = dict_xDT[toDT];

            marker.MarkerType = FinancialMarkerType.TrendLine;
            marker.FirstPointIndex = from;
            marker.SecondPointIndex = to;

            marker.FirstYIndex = 2;
            marker.SecondYIndex = 2;

            marker.LineWidth = LineWidth;

            marker.LineStyle = _ext.ConvertLineType(LineType);
            marker.LineColor = color;

            BarChart.Series["Bars"].FinancialMarkers.Add(marker);
        }

        public Series AddLine(DateTime fromDT, double y1, DateTime toDT, double y2, int Width, LineType LType,
                               Color color, string AreaName)
        {
            int x1 = dict_xDT[fromDT];
            int x2 = dict_xDT[toDT];

            /*var ann = new LineAnnotation();

            ann.StartCap = LineAnchorCap.None;
            ann.EndCap = LineAnchorCap.None;
            ann.LineWidth = Width;
            ann.LineColor = color;
            ann.LineStyle = ChartDashStyle.Solid;

            //ann.SizeAlwaysRelative = false;
            ann.Visible = true;

            //ann.AxisX = BarChart.ChartAreas[AreaName].AxisX;
            //ann.AxisY = BarChart.ChartAreas[AreaName].AxisY;

            //ann.ClipToChartArea = AreaName;

            var LineAnnSerie = BarChart.Series["LineAnn"];

            var p1 = LineAnnSerie.Points[LineAnnSerie.Points.AddXY(x1, y1)];
            var p2 = LineAnnSerie.Points[LineAnnSerie.Points.AddXY(x2, y2)];

            ann.AllowMoving = false;
            ann.AllowAnchorMoving = false;
            ann.AllowSelecting = false;
        

            ann.Anchor(p1,p2);

            BarChart.Annotations.Add(ann);

            ann.BringToFront();*/
            var sname = "__Line" + BarChart.Series.Count;

            var s = new Series(sname);
            s.Points.AddXY(x1, y1);
            s.Points.AddXY(x2, y2);
            s.Color = color;
            s.Type = SeriesChartType.FastLine;
            s.BorderStyle = _ext.ConvertLineType(LType);
            s.BorderWidth = Width;
            s.ChartArea = AreaName;
            s.EmptyPointStyle.BorderWidth = 0;
            s.EmptyPointStyle.MarkerStyle = MarkerStyle.None;

            BarChart.Series.Add(s);

            return s;
        }

        private int ArrowSize = 15;

        private LineAnnotation AddEllipse(DateTime dt, double Y, string AreaName)
        {
            var ann = new LineAnnotation();
            ChartArea Area = BarChart.ChartAreas[AreaName];

            int i = dict_xDT[dt];
            ;

            if (i == BarChart.Series["Bars"].Points.Count) return null;

            /*ann.LineWidth = 2;
            ann.Height = 2;
            ann.StartCap = LineAnchorCap.Diamond;
            ann.Width = 2;
            ann.BackColor = Color.Green;
            ann.LineStyle = ChartDashStyle.Solid;

    */

            BarChart.Annotations.Add(ann);
            return ann;
        }

        private ArrowAnnotation AddArrow(bool up, DateTime dt, double Y, string AreaName, string text = "",
                                         Color color = default(Color))
        {
            ChartArea Area = BarChart.ChartAreas[AreaName];
            var ann = new ArrowAnnotation();

            int i = dict_xDT[dt];

            double HeightPct = ((double) ArrowSize/(double) BarChart.Height)*100;

            ann.Height = up ? HeightPct : -HeightPct;
            ann.Width = 0;

            if (text != "") ann.Tag = text;

            ann.SizeAlwaysRelative = true;

            ann.AxisX = Area.AxisX;
            ann.AxisY = Area.AxisY;
            ann.ClipToChartArea = AreaName;

            ann.X = i;

            ann.Y = Y;

            //ann.SizeAlwaysRelative = false;

            ann.LineStyle = ChartDashStyle.Solid;
            ann.ArrowStyle = ArrowStyle.Simple;
            ann.LineWidth = 0;

            if (color == default(Color))
            {
                ann.LineColor = BlackTheme ? Color.White : Color.Black;
                ann.BackColor = BlackTheme ? Color.White : Color.Black;
            }
            else
            {
                ann.LineColor = color;
                ann.BackColor = color;
            }

            ann.ArrowSize = 3;

            ann.SmartLabels.Enabled = false;
            ann.AllowAnchorMoving = false;
            ann.AllowMoving = false;
            ann.AllowPathEditing = false;
            ann.AllowResizing = false;
            ann.AllowSelecting = false;
            ann.AllowTextEditing = false;

            BarChart.Annotations.Add(ann);
            return ann;
        }

        private void AddLabel(DateTime dt, double price, string Text, string AreaName, Color color)
        {
            ChartArea Area = BarChart.ChartAreas[AreaName];
            var ann = new TextAnnotation();

            var i = dict_xDT[dt];

            ann.AxisX = Area.AxisX;
            ann.AxisY = Area.AxisY;
            ann.ClipToChartArea = AreaName;

            ann.X = i;
            ann.Y = price;


            ann.Text = Text;
            ann.TextColor = color;

            ann.SmartLabels.Enabled = false;
            ann.AllowAnchorMoving = false;
            ann.AllowMoving = false;
            ann.AllowPathEditing = false;
            ann.AllowResizing = false;
            ann.AllowSelecting = false;
            ann.AllowTextEditing = false;

            BarChart.Annotations.Add(ann);
        }

        private void AddCalloutAnn(DateTime dt, double price, string Text, string AreaName, Color color)
        {
            ChartArea Area = BarChart.ChartAreas[AreaName];
            var ann = new CalloutAnnotation();

            int i = dict_xDT[dt];


            ann.AxisX = Area.AxisX;
            ann.AxisY = Area.AxisY;
            ann.ClipToChartArea = AreaName;

            ann.X = i;

            var Ys = BarChart.Series["Bars"].Points[i].YValues;
            ann.Y = Ys[0];

            ann.CalloutAnchorCap = LineAnchorCap.Arrow;

            ann.BackColor = Color.FromArgb(255, 255, 128);
            ann.CalloutStyle = CalloutStyle.Rectangle;
            ann.Text = Text;
            ann.SmartLabels.Enabled = false;
            ann.AllowAnchorMoving = false;
            ann.AllowMoving = false;
            ann.AllowPathEditing = false;
            ann.AllowResizing = false;
            ann.AllowSelecting = false;
            ann.AllowTextEditing = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DrawAnnotation();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BarChart.Annotations.Clear();
        }


    }



}