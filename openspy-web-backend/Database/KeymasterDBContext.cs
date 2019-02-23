using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CoreWeb.Models;

namespace CoreWeb.Database
{
    public class KeymasterDBContext : DbContext
    {
        public virtual DbSet<CdKey> CdKey { get; set; }
        public virtual DbSet<ProfileCdKeys> ProfileCdKey { get; set; }
        public virtual DbSet<CdkeyRules> CdKeyRules { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
               #warning To protect potentially sensitive information in your connection string, you should move it out of source code.See http://go.microsoft.com/fwlink/?LinkId=723263  for guidance on storing connection strings.
               optionsBuilder.UseMySQL("server=localhost;database=Keymaster;user=CHC;password=123321");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CdKey>(entity =>
            {
                entity.ToTable("cdkeys");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Cdkey)
                    .IsRequired()
                    .HasColumnName("cdkey")
                    .HasColumnType("text");

                entity.Property(e => e.UserInserted)
                    .HasColumnName("user_inserted")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.Gameid)
                    .HasColumnName("gameid")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<ProfileCdKeys>(entity =>
            {
                entity.ToTable("profile_cdkeys");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Cdkeyid)
                    .IsRequired()
                    .HasColumnName("cdkey_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Profileid)
                    .HasColumnName("profileid")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<CdkeyRules>(entity =>
            {
                entity.ToTable("profile_cdkeys");

                entity.Property(e => e.Gameid)
                    .HasColumnName("gameid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Failifnotfound)
                    .IsRequired()
                    .HasColumnName("fail_if_not_found")
                    .HasColumnType("tinyint(1)");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}