namespace ResultViewer.Forms
{
    partial class Histform
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Dundas.Charting.WinControl.ChartArea chartArea1 = new Dundas.Charting.WinControl.ChartArea();
            Dundas.Charting.WinControl.Legend legend1 = new Dundas.Charting.WinControl.Legend();
            Dundas.Charting.WinControl.Series series1 = new Dundas.Charting.WinControl.Series();
            Dundas.Charting.WinControl.Series series2 = new Dundas.Charting.WinControl.Series();
            Dundas.Charting.WinControl.Series series3 = new Dundas.Charting.WinControl.Series();
            Dundas.Charting.WinControl.Series series4 = new Dundas.Charting.WinControl.Series();
            this.HistoChart = new Dundas.Charting.WinControl.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.HistoChart)).BeginInit();
            this.SuspendLayout();
            // 
            // HistoChart
            // 
            this.HistoChart.AlwaysRecreateHotregions = true;
            chartArea1.AxisX.LabelsAutoFitStyle = ((Dundas.Charting.WinControl.LabelsAutoFitStyle)((((Dundas.Charting.WinControl.LabelsAutoFitStyle.IncreaseFont | Dundas.Charting.WinControl.LabelsAutoFitStyle.DecreaseFont)
                        | Dundas.Charting.WinControl.LabelsAutoFitStyle.LabelsAngleStep30)
                        | Dundas.Charting.WinControl.LabelsAutoFitStyle.WordWrap)));
            chartArea1.AxisX.LabelStyle.Format = "N2";
            chartArea1.AxisX.MajorGrid.Enabled = false;
            chartArea1.AxisX.MajorTickMark.Enabled = false;
            chartArea1.AxisY.Enabled = Dundas.Charting.WinControl.AxisEnabled.True;
            chartArea1.AxisY.LabelStyle.Format = "N0";
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            chartArea1.AxisY.MajorTickMark.Enabled = false;
            chartArea1.AxisY2.Enabled = Dundas.Charting.WinControl.AxisEnabled.False;
            chartArea1.CursorX.UserEnabled = true;
            chartArea1.CursorX.UserSelection = true;
            chartArea1.Name = "Chart Area 1";
            this.HistoChart.ChartAreas.Add(chartArea1);
            this.HistoChart.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Enabled = false;
            legend1.Name = "Default";
            this.HistoChart.Legends.Add(legend1);
            this.HistoChart.Location = new System.Drawing.Point(0, 0);
            this.HistoChart.Name = "HistoChart";
            this.HistoChart.Palette = Dundas.Charting.WinControl.ChartColorPalette.Pastel;
            series1.BorderColor = System.Drawing.Color.White;
            series1.Color = System.Drawing.Color.CornflowerBlue;
            series1.LabelBackColor = System.Drawing.Color.White;
            series1.LabelFormat = "N0";
            series1.Name = "data1";
            series1.PaletteCustomColors = new System.Drawing.Color[0];
            series1.ShowLabelAsValue = true;
            series1.SmartLabels.Enabled = true;
            series1.XValueIndexed = true;
            series1.XValueType = Dundas.Charting.WinControl.ChartValueTypes.Double;
            series2.BorderWidth = 2;
            series2.ChartArea = "Chart Area 1";
            series2.ChartType = "Spline";
            series2.Color = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            series2.LabelFormat = "N0";
            series2.MarkerBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            series2.MarkerSize = 6;
            series2.MarkerStyle = Dundas.Charting.WinControl.MarkerStyle.Circle;
            series2.Name = "pareto1";
            series2.PaletteCustomColors = new System.Drawing.Color[0];
            series2.ShowLabelAsValue = true;
            series2.XValueIndexed = true;
            series2.XValueType = Dundas.Charting.WinControl.ChartValueTypes.Double;
            series2.YAxisType = Dundas.Charting.WinControl.AxisType.Secondary;
            series3.ChartArea = "Chart Area 1";
            series3.Color = System.Drawing.Color.IndianRed;
            series3.Name = "data2";
            series3.PaletteCustomColors = new System.Drawing.Color[0];
            series4.BorderWidth = 2;
            series4.ChartArea = "Chart Area 1";
            series4.ChartType = "Line";
            series4.Color = System.Drawing.Color.Navy;
            series4.LabelFormat = "N0";
            series4.MarkerSize = 6;
            series4.MarkerStyle = Dundas.Charting.WinControl.MarkerStyle.Circle;
            series4.Name = "pareto2";
            series4.PaletteCustomColors = new System.Drawing.Color[0];
            series4.ShowLabelAsValue = true;
            series4.XValueIndexed = true;
            series4.XValueType = Dundas.Charting.WinControl.ChartValueTypes.Double;
            this.HistoChart.Series.Add(series1);
            this.HistoChart.Series.Add(series2);
            this.HistoChart.Series.Add(series3);
            this.HistoChart.Series.Add(series4);
            this.HistoChart.Size = new System.Drawing.Size(669, 429);
            this.HistoChart.TabIndex = 5;
            this.HistoChart.Text = "HistoChart";
            // 
            // Histform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 429);
            this.Controls.Add(this.HistoChart);
            this.Name = "Histform";
            this.Text = "Histform";
            ((System.ComponentModel.ISupportInitialize)(this.HistoChart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Dundas.Charting.WinControl.Chart HistoChart;




    }
}