using Lab05.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab05.BUS
{
    public class FacultyService
    {
        public List<Faculty> GetAll()
        {
            using (Model1 context = new Model1())
            {
                return context.Faculties.ToList();
            }
        }
    }
}
