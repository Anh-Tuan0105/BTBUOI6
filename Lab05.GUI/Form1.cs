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
            // Lưu ý: new Faculty() có thể tạo ra dòng trống nếu không gán tên
            listFacultys.Insert(0, new Faculty());

            this.cmbFaculty.DataSource = listFacultys;
            this.cmbFaculty.DisplayMember = "FacultyName";
            this.cmbFaculty.ValueMember = "FacultyID";

            cmbFaculty.SelectedIndex = 1; // Chọn dòng đầu tiên làm mặc định    
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
                // Hiển thị Avatar (gọi hàm ShowAvatar)
                //ShowAvatar(item.Avatar);
            }
        }
        private void ShowAvatar(string ImageName)
        {
            if (string.IsNullOrEmpty(ImageName))
            {
                picAvatar.Image = null;
            }
            else
            {
                try
                {
                    // Lấy đường dẫn gốc của project (để tìm folder Images ngang hàng với code)
                    string parentDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                    string imagePath = Path.Combine(parentDirectory, "Images", ImageName);

                    if (File.Exists(imagePath))
                    {
                        picAvatar.Image = Image.FromFile(imagePath);
                        picAvatar.Refresh();
                    }
                    else
                    {
                        picAvatar.Image = null; // Ảnh không tồn tại trong folder
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
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"; // Chỉ lấy file ảnh
            dlg.Title = "Chọn ảnh đại diện";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // Hiển thị ảnh lên PictureBox
                picAvatar.Image = Image.FromFile(dlg.FileName);

                // QUAN TRỌNG: Lưu đường dẫn file gốc vào Tag để lát nữa nút Save dùng để copy
                picAvatar.Tag = dlg.FileName;
            }
        }

        private void btnAddUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Kiểm tra dữ liệu nhập
                if (txtID.Text == "" || txtName.Text == "" || txtScore.Text == "")
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                    return;
                }

                // 2. Xử lý Avatar (Copy ảnh vào thư mục Images của Project)
                string avatarFileName = null; // Mặc định là null nếu không có ảnh

                // Kiểm tra xem có đường dẫn ảnh trong Tag không (đã chọn ảnh hoặc click từ grid)
                if (picAvatar.Tag != null && !string.IsNullOrEmpty(picAvatar.Tag.ToString()))
                {
                    string sourcePath = picAvatar.Tag.ToString();

                    // Lấy thư mục gốc của Project để lưu ảnh (thư mục Images)
                    string parentDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                    string imageFolder = Path.Combine(parentDirectory, "Images");

                    // Tạo thư mục nếu chưa có
                    if (!Directory.Exists(imageFolder)) Directory.CreateDirectory(imageFolder);

                    // Đặt tên file ảnh theo Mã Sinh Viên (để tránh trùng lặp)
                    string fileExtension = Path.GetExtension(sourcePath);
                    avatarFileName = txtID.Text + fileExtension; // Ví dụ: 123456.jpg

                    string destPath = Path.Combine(imageFolder, avatarFileName);

                    // Copy file (ghi đè nếu file đã tồn tại)
                    // Chỉ copy nếu đường dẫn nguồn KHÁC đường dẫn đích (tránh lỗi copy file vào chính nó)
                    if (sourcePath != destPath)
                    {
                        File.Copy(sourcePath, destPath, true);
                    }
                }

                // 3. Tạo đối tượng Student
                Student s = new Student()
                {
                    StudentID = txtID.Text,
                    FullName = txtName.Text,
                    AverageScore = float.Parse(txtScore.Text),
                    FacultyID = int.Parse(cmbFaculty.SelectedValue.ToString()),
                    Avatar = avatarFileName // Lưu tên file ảnh (ngắn gọn) vào DB
                };

                // Xử lý Chuyên ngành (Major) nếu có
                // s.MajorID = ... (Tùy logic bài toán của bạn nếu có combo Major)

                // 4. Gọi Service để lưu vào DB
                studentService.InsertUpdate(s);

                MessageBox.Show("Lưu dữ liệu thành công!");

                // 5. Load lại dữ liệu lên Grid và xóa form nhập
                BindGrid(studentService.GetAll());
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
            picAvatar.Tag = null;
            txtID.Focus();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra xem đã nhập/chọn MSSV chưa
                if (txtID.Text == "")
                {
                    MessageBox.Show("Vui lòng chọn sinh viên cần xóa!");
                    return;
                }

                // Tìm sinh viên để lấy tên ảnh (phục vụ việc xóa file ảnh - tùy chọn)
                var student = studentService.FindById(txtID.Text);
                if (student == null)
                {
                    MessageBox.Show("Sinh viên không tồn tại!");
                    return;
                }

                // Hỏi xác nhận xóa
                DialogResult result = MessageBox.Show($"Bạn có chắc muốn xóa sinh viên {student.FullName}?",
                                                      "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    // 1. Xóa trong Database
                    studentService.Delete(txtID.Text);

                    // 2. (Tùy chọn) Xóa file ảnh trong thư mục Images để tiết kiệm dung lượng
                    if (!string.IsNullOrEmpty(student.Avatar))
                    {
                        string parentDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                        string imagePath = Path.Combine(parentDirectory, "Images", student.Avatar);
                        if (File.Exists(imagePath))
                        {
                            File.Delete(imagePath);
                        }
                    }

                    MessageBox.Show("Xóa thành công!");

                    // 3. Load lại Grid
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
            // Kiểm tra dòng hợp lệ
            if (e.RowIndex == -1) return;

            // Lấy dòng đang chọn
            DataGridViewRow row = dgvStudent.Rows[e.RowIndex];

            // Lấy đối tượng Student được giấu trong Tag ra
            if (row.Tag is Student item)
            {
                // 1. Đổ dữ liệu lên các ô nhập liệu
                txtID.Text = item.StudentID;
                txtName.Text = item.FullName;
                txtScore.Text = item.AverageScore.ToString();

                // Chọn khoa trên ComboBox
                if (item.Faculty != null)
                    cmbFaculty.SelectedValue = item.FacultyID; // Hoặc gán cmbFaculty.Text = item.Faculty.FacultyName;

                // 2. Hiển thị ảnh (Lấy trực tiếp từ thuộc tính Avatar của đối tượng)
                ShowAvatar(item.Avatar);

                // (Tùy chọn) Lưu đường dẫn ảnh vào Tag của PictureBox để xử lý Lưu sau này
                if (!string.IsNullOrEmpty(item.Avatar))
                {
                    // Code xử lý đường dẫn như cũ...
                    string parentDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                    string imagePath = Path.Combine(parentDirectory, "Images", item.Avatar);
                    picAvatar.Tag = imagePath;
                }
                else
                {
                    picAvatar.Tag = "";
                }
            }
        }
    }
}
