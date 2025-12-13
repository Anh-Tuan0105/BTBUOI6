using Lab05.DAL.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab05.BUS
{
    public class StudentService
    {
        public List<Student> GetAll()
        {
            Model1 context = new Model1();
            return context.Students.ToList();
        }

        // 2. Lấy danh sách sinh viên chưa đăng ký chuyên ngành (MajorID là null)
        public List<Student> GetAllHasNoMajor()
        {
            Model1 context = new Model1();
            return context.Students.Where(p => p.MajorID == null).ToList();

        }

        // 3. Lấy sinh viên chưa có chuyên ngành nhưng thuộc một khoa cụ thể
        public List<Student> GetAllHasNoMajor(int facultyID)
        {
            Model1 context = new Model1();
            return context.Students.Where(p => p.MajorID == null && p.FacultyID == facultyID).ToList();

        }

        // 4. Tìm kiếm sinh viên theo ID
        public Student FindById(string studentId)
        {
            Model1 context = new Model1();
            return context.Students.FirstOrDefault(p => p.StudentID == studentId);

        }

        // 5. Thêm mới hoặc Cập nhật sinh viên
        public void InsertUpdate(Student s)
        {
            Model1 context = new Model1();
                // Hàm AddOrUpdate sẽ kiểm tra khóa chính:
                // Nếu trùng ID -> Update
                // Nếu chưa có ID -> Insert
            context.Students.AddOrUpdate(s);
            context.SaveChanges();
        }

        public void Delete(string studentID)
        {
            Model1 context = new Model1();
            var student = context.Students.FirstOrDefault(p => p.StudentID == studentID);
            if (student != null)
            {
                context.Students.Remove(student);
                context.SaveChanges();
            }
        }
    }
}
