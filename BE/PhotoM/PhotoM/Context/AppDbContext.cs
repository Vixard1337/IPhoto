using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PhotoM.Entities;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace PhotoM.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<collection> collections { get; set; }

    public virtual DbSet<photo> photos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=mydb;user=user;password=password", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.4.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<collection>(entity =>
        {
            entity.HasKey(e => e.collection_id).HasName("PRIMARY");
        });

        modelBuilder.Entity<photo>(entity =>
        {
            entity.HasKey(e => e.photo_id).HasName("PRIMARY");

            entity.Property(e => e.added_date).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.collection).WithMany(p => p.photos).HasConstraintName("photos_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
