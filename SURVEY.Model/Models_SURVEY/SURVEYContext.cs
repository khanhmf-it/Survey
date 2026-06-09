using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SURVEY.Model.Models_SURVEY;

public partial class SURVEYContext : DbContext
{
    public SURVEYContext()
    {
    }

    public SURVEYContext(DbContextOptions<SURVEYContext> options)
        : base(options)
    {
    }

    public virtual DbSet<employee_evaluation> employee_evaluations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True;Application Name=\"SQL Server Management Studio\";Command Timeout=0");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<employee_evaluation>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__employee__3214EC275C0A7B18");

            entity.ToTable("employee_evaluation");

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.department)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.employee_code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.employee_name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.evaluation_period)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.g1_example).HasColumnType("text");
            entity.Property(e => e.g1_good_point).HasColumnType("text");
            entity.Property(e => e.g1_improve_point).HasColumnType("text");
            entity.Property(e => e.g2_example).HasColumnType("text");
            entity.Property(e => e.g2_good_point).HasColumnType("text");
            entity.Property(e => e.g2_improve_point).HasColumnType("text");
            entity.Property(e => e.g3_example).HasColumnType("text");
            entity.Property(e => e.g3_good_point).HasColumnType("text");
            entity.Property(e => e.g3_improve_point).HasColumnType("text");
            entity.Property(e => e.g4_example).HasColumnType("text");
            entity.Property(e => e.g4_good_point).HasColumnType("text");
            entity.Property(e => e.g4_improve_point).HasColumnType("text");
            entity.Property(e => e.g5_example).HasColumnType("text");
            entity.Property(e => e.g5_good_point).HasColumnType("text");
            entity.Property(e => e.g5_improve_point).HasColumnType("text");
            entity.Property(e => e.improvement_proposal).HasColumnType("text");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
