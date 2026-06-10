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

    public virtual DbSet<authentication> authentications { get; set; }

    public virtual DbSet<employee_evaluation> employee_evaluations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=APBIVNDB14;Database=SURVEY;User Id=appl_db14;Password=Bivnappl@db14!!#;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<authentication>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__authenti__3214EC27ABA6FF87");

            entity.ToTable("authentication");

            entity.Property(e => e.userAdid)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

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
            entity.Property(e => e.g1_example)
                .HasMaxLength(1000)
                .IsUnicode(true);
            entity.Property(e => e.g1_good_point)
                .HasMaxLength(1000)
                .IsUnicode(true);
            entity.Property(e => e.g1_improve_point)
                .HasMaxLength(1000)
                .IsUnicode(true);
            entity.Property(e => e.g1_improvement_proposal).HasMaxLength(500);
            entity.Property(e => e.g2_example)
                .HasMaxLength(1000)
                .IsUnicode(true);
            entity.Property(e => e.g2_good_point)
                .HasMaxLength(1000)
                .IsUnicode(true);
            entity.Property(e => e.g2_improve_point)
                .HasMaxLength(1000)
                .IsUnicode(true);
            entity.Property(e => e.g2_improvement_proposal).HasMaxLength(500);
            entity.Property(e => e.g3_example)
                .HasMaxLength(1000)
                .IsUnicode(true);
            entity.Property(e => e.g3_good_point)
                .HasMaxLength(1000)
                .IsUnicode(true);
            entity.Property(e => e.g3_improve_point)
                .HasMaxLength(1000)
                .IsUnicode(true);
            entity.Property(e => e.g3_improvement_proposal).HasMaxLength(500);
            entity.Property(e => e.g4_example)
                .HasMaxLength(1000)
                .IsUnicode(true);
            entity.Property(e => e.g4_good_point)
                .HasMaxLength(1000)
                .IsUnicode(true);
            entity.Property(e => e.g4_improve_point)
                .HasMaxLength(1000)
                .IsUnicode(true);
            entity.Property(e => e.g4_improvement_proposal).HasMaxLength(500);
            entity.Property(e => e.g5_example)
                .HasMaxLength(1000)
                .IsUnicode(true);
            entity.Property(e => e.g5_good_point)
                .HasMaxLength(1000)
                .IsUnicode(true);
            entity.Property(e => e.g5_improve_point)
                .HasMaxLength(1000)
                .IsUnicode(true);
            entity.Property(e => e.g5_improvement_proposal).HasMaxLength(500);
            entity.Property(e => e.improvement_proposal).HasColumnType("text");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
