using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CoreWeb.Models;
using Microsoft.Extensions.Configuration;

namespace CoreWeb.Database
{
    public class KeymasterDBContext : DbContext
    {
        public virtual DbSet<CdKey> CdKey { get; set; }
        public virtual DbSet<ProfileCdKey> ProfileCdKey { get; set; }
        public virtual DbSet<CdkeyRule> CdKeyRules { get; set; }
        public KeymasterDBContext(DbContextOptions<KeymasterDBContext> options) : base(options)
        {
            
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

                entity.Property(e => e.CdkeyHash)
                    .IsRequired()
                    .HasColumnName("cdkeyMD5Hash")
                    .HasColumnType("text");

                entity.Property(e => e.InsertedByUser)
                    .HasColumnName("inserted_by_user")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.Gameid)
                    .HasColumnName("gameid")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<ProfileCdKey>(entity =>
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

            modelBuilder.Entity<CdkeyRule>(entity =>
            {
                entity.ToTable("cdkey_rules");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

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