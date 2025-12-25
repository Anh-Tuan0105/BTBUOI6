namespace Lab05.GUI
{
    partial class frmRadarChart
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.chartSkills = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.chartSkills)).BeginInit();
            this.SuspendLayout();
            // 
            // chartSkills
            // 
            chartArea1.Name = "ChartArea1";
            this.chartSkills.ChartAreas.Add(chartArea1);
            this.chartSkills.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.chartSkills.Legends.Add(legend1);
            this.chartSkills.Location = new System.Drawing.Point(0, 0);
            this.chartSkills.Name = "chartSkills";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartSkills.Series.Add(series1);
            this.chartSkills.Size = new System.Drawing.Size(457, 486);
            this.chartSkills.TabIndex = 0;
            this.chartSkills.Text = "chart1";
            // 
            // frmRadarChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 486);
            this.Controls.Add(this.chartSkills);
            this.Name = "frmRadarChart";
            this.Text = "frmRadarChart";
            ((System.ComponentModel.ISupportInitialize)(this.chartSkills)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chartSkills;
    }
}