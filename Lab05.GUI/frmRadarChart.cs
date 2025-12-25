using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Lab05.GUI
{
    public partial class frmRadarChart : Form
    {
        // Constructor nhận tham số truyền vào
        public frmRadarChart(string studentName, double score, string faculty)
        {
            InitializeComponent();

            // Gọi hàm vẽ ngay khi Form khởi tạo
            DrawRadarChart(studentName, score, faculty);
        }

        private void DrawRadarChart(string name, double gpa, string faculty)
        {
            // 1. Dọn dẹp biểu đồ mặc định
            chartSkills.Series.Clear();
            chartSkills.Titles.Clear();

            // 2. Thêm Tiêu đề
            Title title = chartSkills.Titles.Add($"HỒ SƠ NĂNG LỰC: {name.ToUpper()}");
            title.Font = new Font("Arial", 14, FontStyle.Bold);
            title.ForeColor = Color.DarkBlue;

            // 3. Tạo Series dạng Radar
            Series series = chartSkills.Series.Add("Kỹ năng");
            series.ChartType = SeriesChartType.Radar;
            series.BorderWidth = 3;
            series.Color = Color.Red;
            series.BackGradientStyle = GradientStyle.Center;
            series.BackSecondaryColor = Color.Cyan;
            series.Color = Color.FromArgb(100, Color.LightBlue);
            series.BorderColor = Color.Blue;// Màu nền bán trong suốt

            // 4. Logic sinh điểm kỹ năng giả lập dựa theo Khoa
            Random rand = new Random();

            if (faculty.Contains("Công nghệ thông tin"))
            {
                series.Points.AddXY("Tư duy Logic", gpa);
                series.Points.AddXY("Lập trình", gpa * (0.9 + rand.NextDouble() * 0.1));
                series.Points.AddXY("Giải thuật", gpa * 0.9);
                series.Points.AddXY("Tiếng Anh", gpa * 0.8);
                series.Points.AddXY("Teamwork", gpa * 0.7);
            }
            else if (faculty.Contains("Ngôn Ngữ Anh"))
            {
                series.Points.AddXY("Giao tiếp", gpa);
                series.Points.AddXY("Ngữ pháp", gpa * 0.9);
                series.Points.AddXY("Thuyết trình", gpa);
                series.Points.AddXY("Tư duy Logic", gpa * 0.6);
                series.Points.AddXY("Dịch thuật", gpa * 0.9);
            }
            else // Các khoa khác
            {
                series.Points.AddXY("Kỹ năng A", gpa * 0.8);
                series.Points.AddXY("Kỹ năng B", gpa * 0.9);
                series.Points.AddXY("Kỹ năng C", gpa * 0.7);
                series.Points.AddXY("Kỹ năng D", gpa * 0.8);
                series.Points.AddXY("Kỹ năng E", gpa * 0.9);
            }

            // 5. Cấu hình trục (Axis) cho đẹp
            // Lấy ChartArea đầu tiên
            if (chartSkills.ChartAreas.Count > 0)
            {
                var area = chartSkills.ChartAreas[0];
                area.AxisY.Maximum = 10; // Thang điểm 10
                area.AxisY.Minimum = 0;
                area.AxisY.Interval = 2; // Chia vạch
                area.AxisY.LabelStyle.Enabled = false; // Ẩn số trên trục cho đỡ rối
            }
        }
    }
}
