using Lab05.BUS;
using Lab05.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab05.GUI
{
    public partial class frmRegister : Form
    {
        private readonly StudentService studentService = new StudentService();
        private readonly FacultyService facultyService = new FacultyService();
        private readonly MajorService majorService = new MajorService(); 
        public frmRegister()
        {
            InitializeComponent();
        }

        private void frmRegister_Load(object sender, EventArgs e)
        {
            try
            {
                var listFacultys = facultyService.GetAll();
                FillFalcultyCombobox(listFacultys);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void FillFalcultyCombobox(List<Faculty> listFacultys)
        {
            this.cmbFaculty.DataSource = listFacultys;
            this.cmbFaculty.DisplayMember = "FacultyName";
            this.cmbFaculty.ValueMember = "FacultyID";
        }

        private void cmbFaculty_SelectedIndexChanged(object sender, EventArgs e)
        {
            Faculty selectedFaculty = cmbFaculty.SelectedItem as Faculty;
            if (selectedFaculty != null)
            {
                // Load danh sách Chuyên ngành tương ứng với Khoa
                var listMajor = majorService.GetAllByFaculty(selectedFaculty.FacultyID);
                FillMajorCombobox(listMajor);

                // Load danh sách Sinh viên thuộc Khoa nhưng chưa có Chuyên ngành (MajorID == null)
                var listStudents = studentService.GetAllHasNoMajor(selectedFaculty.FacultyID);
                BindGrid(listStudents);
            }
        }

        private void FillMajorCombobox(List<Major> listMajor)
        {
            this.cmbMajor.DataSource = listMajor;
            this.cmbMajor.DisplayMember = "Name"; // Tên hiển thị của chuyên ngành
            this.cmbMajor.ValueMember = "MajorID";
        }

        // 3. Hiển thị dữ liệu lên Grid (Theo ảnh fa8aa7)
        private void BindGrid(List<Student> listStudent)
        {
            dgvStudent.Rows.Clear();
            foreach (var item in listStudent)
            {
                int index = dgvStudent.Rows.Add();

                // Cột 0 là Checkbox (đã thêm ở Design), mặc định là false
                dgvStudent.Rows[index].Cells[0].Value = false;

                dgvStudent.Rows[index].Cells[1].Value = item.StudentID;
                dgvStudent.Rows[index].Cells[2].Value = item.FullName;

                if (item.Faculty != null)
                    dgvStudent.Rows[index].Cells[3].Value = item.Faculty.FacultyName;

                dgvStudent.Rows[index].Cells[4].Value = item.AverageScore;
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra xem đã chọn chuyên ngành chưa
                if (cmbMajor.SelectedIndex == -1 || cmbMajor.SelectedValue == null)
                {
                    MessageBox.Show("Vui lòng chọn chuyên ngành!");
                    return;
                }

                // Lấy MajorID từ ComboBox
                int selectedMajorID = (int)cmbMajor.SelectedValue;
                int count = 0;

                // Duyệt qua từng dòng trong DataGridView để tìm SV được check
                foreach (DataGridViewRow row in dgvStudent.Rows)
                {
                    // Kiểm tra ô Checkbox (Cells[0]) có được tick không
                    // Lưu ý: Cần kiểm tra null để tránh lỗi
                    bool isSelected = Convert.ToBoolean(row.Cells[0].Value);

                    if (isSelected)
                    {
                        // Lấy MSSV từ dòng đó (Cells[1] là MSSV theo hàm BindGrid ở trên)
                        string studentID = row.Cells[1].Value.ToString();

                        // Tìm sinh viên trong DB
                        var student = studentService.FindById(studentID);
                        if (student != null)
                        {
                            // Cập nhật MajorID
                            student.MajorID = selectedMajorID;

                            // Lưu vào CSDL (Dùng hàm InsertUpdate có sẵn từ bài trước)
                            studentService.InsertUpdate(student);
                            count++;
                        }
                    }
                }

                if (count > 0)
                {
                    MessageBox.Show($"Đã đăng ký chuyên ngành thành công cho {count} sinh viên!");

                    // Load lại danh sách sinh viên (để những SV đã đăng ký biến mất khỏi danh sách chưa đăng ký)
                    cmbFaculty_SelectedIndexChanged(sender, e);
                }
                else
                {
                    MessageBox.Show("Bạn chưa chọn sinh viên nào!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi trong quá trình đăng ký: " + ex.Message);
            }
        }
    }
}
