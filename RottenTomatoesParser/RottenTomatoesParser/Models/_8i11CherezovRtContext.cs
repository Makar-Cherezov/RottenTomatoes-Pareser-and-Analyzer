using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RottenTomatoesParser.Models;

public partial class _8i11CherezovRtContext : DbContext
{
    public _8i11CherezovRtContext()
    {
    }

    public _8i11CherezovRtContext(DbContextOptions<_8i11CherezovRtContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Director> Directors { get; set; }

    public virtual DbSet<Film> Films { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=sqlvt.main.tpu.ru;Database=8I11_Cherezov_RT;User ID=student3;Password=student3;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Director>(entity =>
        {
            entity.Property(e => e.DirectorId)
                .ValueGeneratedNever()
                .HasColumnName("DirectorID");
            entity.Property(e => e.Birthday).HasColumnType("date");
            entity.Property(e => e.Birthplace).HasMaxLength(50);
            entity.Property(e => e.Name)
                .HasMaxLength(70)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Film>(entity =>
        {
            entity.Property(e => e.FilmId)
                .ValueGeneratedNever()
                .HasColumnName("FilmID");
            entity.Property(e => e.DirectorId).HasColumnName("DirectorID");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Director).WithMany(p => p.Films)
                .HasForeignKey(d => d.DirectorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Films_Directors");

            entity.HasMany(d => d.Genres).WithMany(p => p.Films)
                .UsingEntity<Dictionary<string, object>>(
                    "GenresOfFilm",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_GenresOfFilms_Genres"),
                    l => l.HasOne<Film>().WithMany()
                        .HasForeignKey("FilmId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_GenresOfFilms_Films"),
                    j =>
                    {
                        j.HasKey("FilmId", "GenreId");
                        j.ToTable("GenresOfFilms");
                        j.IndexerProperty<Guid>("FilmId").HasColumnName("FilmID");
                        j.IndexerProperty<int>("GenreId").HasColumnName("GenreID");
                    });
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.Property(e => e.GenreId)
                .ValueGeneratedNever()
                .HasColumnName("GenreID");
            entity.Property(e => e.Genre1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Genre");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
