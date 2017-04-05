namespace ResultViewer.Forms
{
    partial class Scatter
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
            this.HistoChart = new Dundas.Charting.WinControl.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.HistoChart)).BeginInit();
            this.SuspendLayout();
            // 
            // HistoChart
            // 
            this.HistoChart.AlwaysRecreateHotregions = true;
            chartArea1.AxisX.MajorGrid.Enabled = false;
            chartArea1.AxisY.MajorGrid.Enabled = false;
            chartArea1.CursorX.UserEnabled = true;
            chartArea1.CursorX.UserSelection = true;
            chartArea1.CursorY.UserEnabled = true;
            chartArea1.CursorY.UserSelection = true;
            chartArea1.Name = "Chart Area 2";
            this.HistoChart.ChartAreas.Add(chartArea1);
            this.HistoChart.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Enabled = false;
            legend1.Name = "Default";
            this.HistoChart.Legends.Add(legend1);
            this.HistoChart.Location = new System.Drawing.Point(0, 0);
            this.HistoChart.Name = "HistoChart";
            this.HistoChart.Palette = Dundas.Charting.WinControl.ChartColorPalette.Pastel;
            series1.BorderColor = System.Drawing.Color.White;
            series1.ChartType = "Point";
            series1.Color = System.Drawing.Color.CornflowerBlue;
            series1.Label = "#LAST";
            series1.LabelBackColor = System.Drawing.Color.White;
            series1.LabelFormat = "N0";
            series1.Name = "data1";
            series1.PaletteCustomColors = new System.Drawing.Color[0];
            series1.ShowLabelAsValue = true;
            series1.SmartLabels.Enabled = true;
            series1.XValueType = Dundas.Charting.WinControl.ChartValueTypes.Double;
            this.HistoChart.Series.Add(series1);
            this.HistoChart.Size = new System.Drawing.Size(669, 429);
            this.HistoChart.TabIndex = 5;
            this.HistoChart.Text = "HistoChart";
            // 
            // Scatter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 429);
            this.Controls.Add(this.HistoChart);
            this.Name = "Scatter";
            this.Text = "Histform";
            ((System.ComponentModel.ISupportInitialize)(this.HistoChart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Dundas.Charting.WinControl.Chart HistoChart;





    }
}