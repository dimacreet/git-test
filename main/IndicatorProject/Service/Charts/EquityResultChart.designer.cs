namespace ResultViewer.Forms
{
    partial class EquityResultChart
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
            Dundas.Charting.WinControl.ChartArea chartArea2 = new Dundas.Charting.WinControl.ChartArea();
            Dundas.Charting.WinControl.Legend legend2 = new Dundas.Charting.WinControl.Legend();
            Dundas.Charting.WinControl.Series series2 = new Dundas.Charting.WinControl.Series();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.BarChart = new Dundas.Charting.WinControl.Chart();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.BarChart)).BeginInit();
            this.SuspendLayout();
            // 
            // BarChart
            // 
            this.BarChart.AlwaysRecreateHotregions = true;
            chartArea2.AxisX.LabelsAutoFit = false;
            chartArea2.AxisX.LabelStyle.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            chartArea2.AxisX.LabelStyle.Format = "d";
            chartArea2.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            chartArea2.AxisX.MajorGrid.LineStyle = Dundas.Charting.WinControl.ChartDashStyle.Dash;
            chartArea2.AxisX.MajorTickMark.Enabled = false;
            chartArea2.AxisX.MajorTickMark.LineColor = System.Drawing.Color.Silver;
            chartArea2.AxisX.ScrollBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            chartArea2.AxisX.ScrollBar.ButtonColor = System.Drawing.Color.Gray;
            chartArea2.AxisX.ScrollBar.LineColor = System.Drawing.Color.Black;
            chartArea2.AxisX.ScrollBar.PositionInside = false;
            chartArea2.AxisX2.Enabled = Dundas.Charting.WinControl.AxisEnabled.False;
            chartArea2.AxisX2.LabelsAutoFit = false;
            chartArea2.AxisX2.LabelStyle.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            chartArea2.AxisX2.MajorGrid.LineColor = System.Drawing.Color.Silver;
            chartArea2.AxisX2.MajorTickMark.Enabled = false;
            chartArea2.AxisX2.MajorTickMark.LineColor = System.Drawing.Color.Silver;
            chartArea2.AxisX2.ScrollBar.Enabled = false;
            chartArea2.AxisY.Enabled = Dundas.Charting.WinControl.AxisEnabled.False;
            chartArea2.AxisY.LabelStyle.Enabled = false;
            chartArea2.AxisY.LineStyle = Dundas.Charting.WinControl.ChartDashStyle.NotSet;
            chartArea2.AxisY.MajorGrid.Enabled = false;
            chartArea2.AxisY.MajorTickMark.Enabled = false;
            chartArea2.AxisY.ScrollBar.Enabled = false;
            chartArea2.AxisY.StartFromZero = false;
            chartArea2.AxisY2.Enabled = Dundas.Charting.WinControl.AxisEnabled.True;
            chartArea2.AxisY2.LabelsAutoFit = false;
            chartArea2.AxisY2.LabelsAutoFitStyle = Dundas.Charting.WinControl.LabelsAutoFitStyle.WordWrap;
            chartArea2.AxisY2.LabelStyle.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            chartArea2.AxisY2.LabelStyle.Format = "N0";
            chartArea2.AxisY2.MajorGrid.LineColor = System.Drawing.Color.Silver;
            chartArea2.AxisY2.MajorGrid.LineStyle = Dundas.Charting.WinControl.ChartDashStyle.Dash;
            chartArea2.AxisY2.MajorTickMark.Enabled = false;
            chartArea2.AxisY2.ScrollBar.Enabled = false;
            chartArea2.AxisY2.StartFromZero = false;
            chartArea2.BackColor = System.Drawing.Color.White;
            chartArea2.BorderColor = System.Drawing.Color.White;
            chartArea2.CursorX.LineWidth = 2;
            chartArea2.CursorX.UserEnabled = true;
            chartArea2.CursorX.UserSelection = true;
            chartArea2.Name = "Default";
            this.BarChart.ChartAreas.Add(chartArea2);
            this.BarChart.Cursor = System.Windows.Forms.Cursors.Cross;
            this.BarChart.Dock = System.Windows.Forms.DockStyle.Fill;
            legend2.BorderStyle = Dundas.Charting.WinControl.ChartDashStyle.NotSet;
            legend2.Enabled = false;
            legend2.Name = "Default";
            this.BarChart.Legends.Add(legend2);
            this.BarChart.Location = new System.Drawing.Point(0, 0);
            this.BarChart.Name = "BarChart";
            this.BarChart.Palette = Dundas.Charting.WinControl.ChartColorPalette.Pastel;
            series2.BackGradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            series2.BackGradientType = Dundas.Charting.WinControl.GradientType.TopBottom;
            series2.BorderWidth = 2;
            series2.ChartType = "Area";
            series2.Color = System.Drawing.Color.RosyBrown;
            series2.Name = "data";
            series2.PaletteCustomColors = new System.Drawing.Color[0];
            this.BarChart.Series.Add(series2);
            this.BarChart.Size = new System.Drawing.Size(1097, 479);
            this.BarChart.TabIndex = 4;
            this.BarChart.Text = "chart1";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(931, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "Draw";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1022, 0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "Clear";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // EquityResultChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1097, 479);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.BarChart);
            this.Name = "EquityResultChart";
            this.Text = "ForTrendDef";
            ((System.ComponentModel.ISupportInitialize)(this.BarChart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private Dundas.Charting.WinControl.Chart BarChart;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}