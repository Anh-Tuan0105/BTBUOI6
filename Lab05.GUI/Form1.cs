using Lab05.BUS;
using Lab05.DAL.Models;
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

        private void đăngKýChuyênNgànhToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var f = new frmRegister();
            f.ShowDialog();
            // 1. Lấy lại danh sách sinh viên mới nhất từ DB (đã có MajorID mới cập nhật)
            var listStudents = studentService.GetAll();

            // 2. Đổ lại dữ liệu vào GridView để cập nhật tên chuyên ngành
            BindGrid(listStudents);
        }
    }
}
