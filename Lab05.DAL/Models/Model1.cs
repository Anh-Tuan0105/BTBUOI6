using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace Lab05.DAL.Models
{
    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=StudentDB")
        {
        }

        public virtual DbSet<Faculty> Faculties { get; set; }
        public virtual DbSet<Major> Majors { get; set; }
        public virtual DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Faculty>()
                .HasMany(e => e.Majors)
                .WithRequired(e => e.Faculty)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Major>()
                .HasMany(e => e.Students)
                .WithOptional(e => e.Major)
                .HasForeignKey(e => new { e.FacultyID, e.MajorID });
        }
    }
}
