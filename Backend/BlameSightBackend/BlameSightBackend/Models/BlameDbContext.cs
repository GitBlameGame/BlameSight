using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BlameSightBackend.Models;

public partial class BlameDbContext : DbContext
{
    public BlameDbContext()
    {
    }

    public BlameDbContext(DbContextOptions<BlameDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blame> Blames { get; set; }

    public virtual DbSet<Repo> Repos { get; set; }

    public virtual DbSet<Repoowner> Repoowners { get; set; }

    public virtual DbSet<Urgencydescriptor> Urgencydescriptors { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=BlameGame;Username=postgres;Password=XS4Us7b79Qcu7kodm");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blame>(entity =>
        {
            entity.HasKey(e => e.BlameId).HasName("blames_pkey");

            entity.Property(e => e.BlameAccepted).HasDefaultValue(false);
            entity.Property(e => e.BlameTimestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Blamed).WithMany(p => p.BlameBlameds).HasConstraintName("fk_blamed");

            entity.HasOne(d => d.Blamer).WithMany(p => p.BlameBlamers).HasConstraintName("fk_blamer");

            entity.HasOne(d => d.Repo).WithMany(p => p.Blames).HasConstraintName("fk_repo");

            entity.HasOne(d => d.UrgencyDescriptor).WithMany(p => p.Blames).HasConstraintName("fk_urgency_descriptor");
        });

        modelBuilder.Entity<Repo>(entity =>
        {
            entity.HasKey(e => e.RepoId).HasName("repos_pkey");

            entity.HasOne(d => d.RepoOwner).WithMany(p => p.Repos).HasConstraintName("fk_repo_owner");
        });

        modelBuilder.Entity<Repoowner>(entity =>
        {
            entity.HasKey(e => e.RepoOwnerId).HasName("repoowners_pkey");
        });

        modelBuilder.Entity<Urgencydescriptor>(entity =>
        {
            entity.HasKey(e => e.UrgencyDescriptorId).HasName("urgencydescriptors_pkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
