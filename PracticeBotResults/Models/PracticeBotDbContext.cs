using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracticeBotResults.Models
{
    public class PracticeBotDbContext : DbContext
    {
        public PracticeBotDbContext(DbContextOptions<PracticeBotDbContext> options) : base(options)
        {

        }
        public virtual DbSet<Results> Results { get; set; }

        // Generated via Scaffold-DbContext "Server=..." Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Results>(entity =>
            {
                entity.HasKey(e => new { e.CourseId, e.AssessmentId, e.QuestionId, e.UserId });

                entity.Property(e => e.AssessmentId).HasMaxLength(15);

                entity.Property(e => e.UserId).HasMaxLength(50);

                entity.Property(e => e.AssessmentName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.CourseName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Timestamp).HasDefaultValueSql("(sysutcdatetime())");
            });
        }
    }
}
