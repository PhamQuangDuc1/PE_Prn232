using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Q1.Models;

public partial class Prn232PeSu2613Context : DbContext
{
    public Prn232PeSu2613Context()
    {
    }

    public Prn232PeSu2613Context(DbContextOptions<Prn232PeSu2613Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingDetail> BookingDetails { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<MembershipPackage> MembershipPackages { get; set; }

    public virtual DbSet<Specialization> Specializations { get; set; }

    public virtual DbSet<Trainer> Trainers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("MyCnn"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Bookings__73951ACD10ABF772");

            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.BookingDate).HasColumnType("datetime");
            entity.Property(e => e.SessionTime).HasColumnType("datetime");
            entity.Property(e => e.TrainerId).HasColumnName("TrainerID");

            entity.HasOne(d => d.Trainer).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TrainerId)
                .HasConstraintName("FK__Bookings__Traine__534D60F1");
        });

        modelBuilder.Entity<BookingDetail>(entity =>
        {
            entity.HasKey(e => new { e.BookingId, e.MemberId }).HasName("PK__BookingD__E35A1E7EFE7BB25E");

            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingDetails)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingDe__Booki__5629CD9C");

            entity.HasOne(d => d.Member).WithMany(p => p.BookingDetails)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingDe__Membe__571DF1D5");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("PK__Members__0CF04B38684CF88E");

            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PackageId).HasColumnName("PackageID");

            entity.HasOne(d => d.Package).WithMany(p => p.Members)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("FK__Members__Package__4CA06362");
        });

        modelBuilder.Entity<MembershipPackage>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("PK__Membersh__322035ECBA7CD091");

            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.PackageName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Specialization>(entity =>
        {
            entity.HasKey(e => e.SpecId).HasName("PK__Speciali__883D519BFC65862A");

            entity.Property(e => e.SpecId).HasColumnName("SpecID");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.SpecName).HasMaxLength(100);
        });

        modelBuilder.Entity<Trainer>(entity =>
        {
            entity.HasKey(e => e.TrainerId).HasName("PK__Trainers__366A1B9C742AC6FB");

            entity.Property(e => e.TrainerId).HasColumnName("TrainerID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.TrainerName).HasMaxLength(100);

            entity.HasMany(d => d.Specs).WithMany(p => p.Trainers)
                .UsingEntity<Dictionary<string, object>>(
                    "TrainerSpec",
                    r => r.HasOne<Specialization>().WithMany()
                        .HasForeignKey("SpecId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__TrainerSp__SpecI__5AEE82B9"),
                    l => l.HasOne<Trainer>().WithMany()
                        .HasForeignKey("TrainerId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__TrainerSp__Train__59FA5E80"),
                    j =>
                    {
                        j.HasKey("TrainerId", "SpecId").HasName("PK__TrainerS__9EE9CE859E157D91");
                        j.ToTable("TrainerSpecs");
                        j.IndexerProperty<int>("TrainerId").HasColumnName("TrainerID");
                        j.IndexerProperty<int>("SpecId").HasColumnName("SpecID");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
