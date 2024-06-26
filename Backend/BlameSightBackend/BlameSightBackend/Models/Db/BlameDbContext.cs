﻿using Microsoft.EntityFrameworkCore;

namespace BlameSightBackend.Models.Db;

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

    public virtual DbSet<Repoowner> RepoOwners { get; set; }

    public virtual DbSet<Urgencydescriptor> UrgencyDescriptors { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blame>(entity =>
        {
            entity.HasKey(e => e.BlameId).HasName("blames_pkey");

            entity.Property(e => e.BlameComplete).HasDefaultValue(false);
            entity.Property(e => e.BlameTimestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.BlameViewed).HasDefaultValue(false);

            entity.HasOne(d => d.Blamed).WithMany(p => p.BlameBlameds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_blamed");

            entity.HasOne(d => d.Blamer).WithMany(p => p.BlameBlamers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_blamer");

            entity.HasOne(d => d.Repo).WithMany(p => p.Blames)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_repo");

            entity.HasOne(d => d.UrgencyDescriptor).WithMany(p => p.Blames)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_urgency_descriptor");
        });

        modelBuilder.Entity<Repo>(entity =>
        {
            entity.HasKey(e => e.RepoId).HasName("repos_pkey");

            entity.HasOne(d => d.RepoOwner).WithMany(p => p.Repos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_repo_owner");
        });

        modelBuilder.Entity<Repoowner>(entity =>
        {
            entity.HasKey(e => e.RepoOwnerId).HasName("repoowners_pkey");
        });

        modelBuilder.Entity<Urgencydescriptor>(entity =>
        {
            entity.HasKey(e => e.UrgencyDescriptorId).HasName("urgencydescriptors_pkey");

            entity.Property(e => e.UrgencyDescriptorId).ValueGeneratedNever();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
