using Lab05.BUS;
using Lab05.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Lab05.GUI
{
    public partial class Form1 : Form
    {
        private readonly StudentService studentService = new StudentService();
        private readonly FacultyService facultyService = new FacultyService();

        private string avatarFilePath = string.Empty;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                setGridViewStyle(dgvStudent); // Cài đặt giao diện Grid

                var listFacultys = facultyService.GetAll();
                var listStudents = studentService.GetAll();

                FillFalcultyCombobox(listFacultys);
                BindGrid(listStudents);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FillFalcultyCombobox(List<Faculty> listFacultys)
        {
            // Thêm dòng mặc định đầu tiên (tùy chọn, giống trong ảnh hướng dẫn)
            this.cmbFaculty.DataSource = listFacultys;
            this.cmbFaculty.DisplayMember = "FacultyName";
            this.cmbFaculty.ValueMember = "FacultyID";
        }

        private void BindGrid(List<Student> listStudent)
        {
            dgvStudent.Rows.Clear();
            foreach (var item in listStudent)
            {
                int index = dgvStudent.Rows.Add();
                dgvStudent.Rows[index].Cells[0].Value = item.StudentID;
                dgvStudent.Rows[index].Cells[1].Value = item.FullName;

                // Kiểm tra null để tránh lỗi nếu sinh viên không thuộc khoa nào
                if (item.Faculty != null)
                {
                    dgvStudent.Rows[index].Cells[2].Value = item.Faculty.FacultyName;
                }

                dgvStudent.Rows[index].Cells[3].Value = item.AverageScore; // + "" nếu muốn ép kiểu chuỗi

                // Kiểm tra chuyên ngành (Major)
                if (item.MajorID != null && item.Major != null)
                {
                    dgvStudent.Rows[index].Cells[4].Value = item.Major.Name; // + ""
                }

                dgvStudent.Rows[index].Tag = item;
            }
        }
        private void ShowAvatar(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                picAvatar.Image = null;
            }
            else
            {
                try
                {
                    string parentDirectory = Application.StartupPath; // Sửa theo ảnh: dùng StartupPath
                    string imagePath = Path.Combine(parentDirectory, "Images", imageName);

                    if (File.Exists(imagePath))
                    {
                        picAvatar.Image = Image.FromFile(imagePath);
                        picAvatar.Refresh();
                    }
                    else
                    {
                        picAvatar.Image = null;
                    }
                }
                catch
                {
                    picAvatar.Image = null;
                }
            }
        }
        public void setGridViewStyle(DataGridView dgview)
        {
            dgview.BorderStyle = BorderStyle.None;
            dgview.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dgview.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgview.BackgroundColor = Color.White;
            dgview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void chkUnregisterMajor_CheckedChanged(object sender, EventArgs e)
        {
            var listStudents = new List<Student>();

            if (this.chkUnregisterMajor.Checked)
            {
                // Gọi hàm lấy sinh viên chưa có chuyên ngành từ BUS
                listStudents = studentService.GetAllHasNoMajor();
            }
            else
            {
                // Lấy tất cả sinh viên
                listStudents = studentService.GetAll();
            }

            BindGrid(listStudents);
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    avatarFilePath = openFileDialog.FileName; // Lưu đường dẫn vào biến toàn cục
                    picAvatar.Image = Image.FromFile(avatarFilePath);
                }
            }
        }

        private string SaveAvatar(string sourceFilePath, string studentID)
        {
            try
            {
                string folderPath = Path.Combine(Application.StartupPath, "Images");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileExtension = Path.GetExtension(sourceFilePath);
                string targetFilePath = Path.Combine(folderPath, $"{studentID}{fileExtension}");

                // Logic copy file
                if (File.Exists(sourceFilePath))
                {
                    // Nếu copy đè lên chính nó thì bỏ qua lỗi
                    if (sourceFilePath != targetFilePath)
                    {
                        File.Copy(sourceFilePath, targetFilePath, true);
                    }
                }
                else
                {
                    throw new FileNotFoundException($"Không tìm thấy file: {sourceFilePath}");
                    // Có thể handle lỗi tùy ý
                }

                return $"{studentID}{fileExtension}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving avatar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void btnAddUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtID.Text == "" || txtName.Text == "" || txtScore.Text == "")
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                    return;
                }

                // Tìm sinh viên có sẵn hoặc tạo mới (Logic của user cũ, kết hợp logic ảnh)
                var student = studentService.FindById(txtID.Text) ?? new Student();

                // Update thông tin
                student.StudentID = txtID.Text;
                student.FullName = txtName.Text;
                student.AverageScore = float.Parse(txtScore.Text);
                student.FacultyID = int.Parse(cmbFaculty.SelectedValue.ToString());

                // --- Xử lý LƯU ẢNH (Theo Ảnh 3) ---
                if (!string.IsNullOrEmpty(avatarFilePath)) // Nếu có file ảnh mới được chọn
                {
                    string avatarFileName = SaveAvatar(avatarFilePath, txtID.Text);
                    if (!string.IsNullOrEmpty(avatarFileName))
                    {
                        student.Avatar = avatarFileName;
                    }
                }
                // ----------------------------------

                studentService.InsertUpdate(student);

                MessageBox.Show("Lưu dữ liệu thành công!");
                BindGrid(studentService.GetAll());

                // Clear data và reset avatar path (Theo Ảnh 3)
                ResetForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        // Hàm phụ để xóa trắng form sau khi thêm/sửa
        private void ResetForm()
        {
            txtID.Text = "";
            txtName.Text = "";
            txtScore.Text = "";
            picAvatar.Image = null;
            avatarFilePath = string.Empty; // Reset đường dẫn file tạm
            txtID.Focus();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtID.Text == "")
                {
                    MessageBox.Show("Vui lòng chọn sinh viên cần xóa!");
                    return;
                }

                var student = studentService.FindById(txtID.Text);
                if (student == null)
                {
                    MessageBox.Show("Sinh viên không tồn tại!");
                    return;
                }

                DialogResult result = MessageBox.Show($"Bạn có chắc muốn xóa sinh viên {student.FullName}?",
                                                      "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    studentService.Delete(txtID.Text);
                    MessageBox.Show("Xóa thành công!");
                    BindGrid(studentService.GetAll());
                    ResetForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void dgvStudent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;
            DataGridViewRow row = dgvStudent.Rows[e.RowIndex];
            if (row.Tag is Student item)
            {
                txtID.Text = item.StudentID;
                txtName.Text = item.FullName;
                txtScore.Text = item.AverageScore.ToString();
                if (item.Faculty != null)
                    cmbFaculty.SelectedValue = item.FacultyID;

                ShowAvatar(item.Avatar);

                // Reset avatarFilePath khi click chọn sinh viên khác để tránh lưu nhầm ảnh cũ
                // nếu người dùng nhấn Save mà không chọn ảnh mới
                avatarFilePath = string.Empty;
            }
        }
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Lấy từ khóa và chuyển về chữ thường
            string keyword = txtSearch.Text.Trim().ToLower();

            // Lấy danh sách toàn bộ sinh viên (hoặc danh sách hiện tại đang có)
            var listStudents = studentService.GetAll();

            // Lọc theo MSSV hoặc Tên (Contains: chứa từ khóa)
            var searchResult = listStudents.Where(s =>
                s.StudentID.ToLower().Contains(keyword) ||
                s.FullName.ToLower().Contains(keyword)).ToList();

            // Hiển thị kết quả
            BindGrid(searchResult);
        }
        private void đăngKýChuyênNgànhToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var f = new frmRegister();
            f.ShowDialog();
            // 1. Lấy lại danh sách sinh viên mới nhất từ DB (đã có MajorID mới cập nhật)
            var listStudents = studentService.GetAll();

            // 2. Đổ lại dữ liệu vào GridView để cập nhật tên chuyên ngành
            BindGrid(listStudents);
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            var listStudents = studentService.GetAll();

            // Sử dụng LINQ để Group theo tên Khoa và đếm
            var report = listStudents
                .GroupBy(s => s.Faculty.FacultyName)
                .Select(g => new
                {
                    Khoa = g.Key,
                    SoLuong = g.Count()
                })
                .ToList();

            string message = "Thống kê số lượng sinh viên:\n\n";
            foreach (var item in report)
            {
                message += $"- Khoa {item.Khoa}: {item.SoLuong} sinh viên\n";
            }

            MessageBox.Show(message, "Thống kê");
        }

        private void btnPrintCard_Click(object sender, EventArgs e)
        {
            // 0. Kiểm tra dữ liệu
            if (txtID.Text == "" || txtName.Text == "")
            {
                MessageBox.Show("Vui lòng chọn sinh viên cần in thẻ!");
                return;
            }

            // 1. LẤY THÔNG TIN CHUYÊN NGÀNH
            var student = studentService.FindById(txtID.Text);
            string majorName = "Chưa đăng ký";
            if (student != null && student.Major != null)
            {
                majorName = student.Major.Name;
            }

            // 2. THIẾT LẬP KÍCH THƯỚC "KHỦNG" (HD 800x500)
            int cardW = 800;
            int cardH = 500;
            Bitmap card = new Bitmap(cardW, cardH);
            Graphics g = Graphics.FromImage(card);

            // Bật chế độ vẽ chất lượng cao nhất (AntiAlias)
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            // 3. VẼ NỀN & KHUNG
            g.Clear(Color.White); // Nền trắng
                                  // Vẽ viền đôi cho đẹp (Viền xanh đậm 10px)
            //g.DrawRectangle(new Pen(Color.DarkBlue, 10), 0, 0, cardW, cardH);

            // 4. VẼ TIÊU ĐỀ (Font to hơn)
            // Logo trường hoặc tên trường
            using (Font fontSchool = new Font("Arial", 18, FontStyle.Bold))
            {
                g.DrawString("ĐẠI HỌC HUTECH", fontSchool, Brushes.DarkBlue, new PointF(280, 30));
            }

            // Chữ THẺ SINH VIÊN
            using (Font fontTitle = new Font("Arial", 26, FontStyle.Bold))
            {
                g.DrawString("THẺ SINH VIÊN", fontTitle, Brushes.Red, new PointF(300, 70));
            }

            // 5. VẼ ẢNH ĐẠI DIỆN (Size lớn: 200x260)
            // Tọa độ vẽ: Cách trái 40, Cách trên 130
            Rectangle imageRect = new Rectangle(40, 130, 200, 260);

            if (picAvatar.Image != null)
            {
                g.DrawImage(picAvatar.Image, imageRect);
                g.DrawRectangle(new Pen(Color.Black, 2), imageRect); // Viền đen quanh ảnh
            }
            else
            {
                g.DrawRectangle(Pens.Gray, imageRect);
                g.DrawString("No Image", new Font("Arial", 14), Brushes.Gray, new PointF(80, 230));
            }

            // 6. VẼ THÔNG TIN CHI TIẾT
            // Dời tọa độ X sang 270 để né cái ảnh to
            int infoX = 270;
            int topY = 150;   // Dòng đầu tiên bắt đầu từ Y=150
            int gap = 65;     // Khoảng cách giữa các dòng (giãn rộng ra cho thoáng)

            using (Font fontLabel = new Font("Arial", 16, FontStyle.Bold))   // Font nhãn size 16
            using (Font fontContent = new Font("Arial", 16, FontStyle.Regular)) // Font nội dung size 16
            {
                // -- Họ Tên --
                g.DrawString("Họ tên:", fontLabel, Brushes.Black, new PointF(infoX, topY));
                g.DrawString(txtName.Text.ToUpper(), fontContent, Brushes.DarkBlue, new PointF(infoX + 110, topY));

                // -- MSSV --
                g.DrawString("MSSV:", fontLabel, Brushes.Black, new PointF(infoX, topY + gap));
                g.DrawString(txtID.Text, fontContent, Brushes.Red, new PointF(infoX + 110, topY + gap));

                // -- Khoa --
                g.DrawString("Khoa:", fontLabel, Brushes.Black, new PointF(infoX, topY + gap * 2));
                g.DrawString(cmbFaculty.Text, fontContent, Brushes.Black, new PointF(infoX + 110, topY + gap * 2));

                // -- Chuyên Ngành --
                g.DrawString("C.Ngành:", fontLabel, Brushes.Black, new PointF(infoX, topY + gap * 3));
                g.DrawString(majorName, fontContent, Brushes.Black, new PointF(infoX + 110, topY + gap * 3));
            }

            // Trang trí: Mã vạch giả hoặc đường kẻ dưới cùng
            g.FillRectangle(Brushes.DarkBlue, 0, 460, 800, 40); // Một dải màu xanh dưới đáy thẻ

            // 7. LƯU FILE
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg";
            sfd.FileName = txtID.Text + "_TheSV_HD"; // Đặt tên file gợi ý

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(sfd.FileName).ToLower();
                ImageFormat format = ImageFormat.Png;
                if (ext == ".jpg" || ext == ".jpeg") format = ImageFormat.Jpeg;

                card.Save(sfd.FileName, format);

                // Mở ảnh lên xem ngay
                try { System.Diagnostics.Process.Start("explorer.exe", sfd.FileName); } catch { }
            }
        }

        private void btnShowSkills_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra xem người dùng đã chọn sinh viên chưa
            if (txtID.Text == "" || txtName.Text == "" || txtScore.Text == "")
            {
                MessageBox.Show("Vui lòng chọn một sinh viên có đầy đủ điểm số!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 2. Lấy dữ liệu từ các ô nhập liệu
                string name = txtName.Text;
                string faculty = cmbFaculty.Text;
                double score = double.Parse(txtScore.Text);

                // 3. Khởi tạo Form biểu đồ và truyền dữ liệu sang
                // (Đây là lúc Constructor bên kia hoạt động)
                frmRadarChart frm = new frmRadarChart(name, score, faculty);

                // 4. Hiển thị Form lên
                frm.ShowDialog(); // Dùng ShowDialog để người dùng phải tắt biểu đồ mới quay lại được Form chính
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi dữ liệu điểm số: " + ex.Message);
            }
        }
    }
}
