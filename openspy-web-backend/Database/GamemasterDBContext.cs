using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CoreWeb.Models;
using Microsoft.Extensions.Configuration;

namespace CoreWeb.Database
{
    public class GamemasterDBContext : DbContext
    {
        public virtual DbSet<Game> Game { get; set; }
        public virtual DbSet<Group> Group { get; set; }

        private IConfiguration configuration;

        public GamemasterDBContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
               #warning To protect potentially sensitive information in your connection string, you should move it out of source code.See http://go.microsoft.com/fwlink/?LinkId=723263  for guidance on storing connection strings.
               optionsBuilder.UseMySQL(configuration.GetConnectionString("GamemasterDB"));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("games");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Backendflags)
                    .HasColumnName("backendflags")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasColumnType("text");

                entity.Property(e => e.Disabledservices)
                    .HasColumnName("disabledservices")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Gamename)
                    .IsRequired()
                    .HasColumnName("gamename")
                    .HasColumnType("text");

                entity.Property(e => e.Keylist)
                    .IsRequired()
                    .HasColumnName("keylist")
                    .HasColumnType("text");

                entity.Property(e => e.Keytypelist)
                    .IsRequired()
                    .HasColumnName("keytypelist")
                    .HasColumnType("text");

                entity.Property(e => e.Queryport)
                    .HasColumnName("queryport")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'6500'");

                entity.Property(e => e.Secretkey)
                    .IsRequired()
                    .HasColumnName("secretkey")
                    .HasColumnType("text");
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(e => e.Groupid);

                entity.ToTable("grouplist");

                entity.Property(e => e.Groupid)
                    .HasColumnName("groupid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Gameid)
                    .HasColumnName("gameid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Maxwaiting)
                    .HasColumnName("maxwaiting")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("text");

                entity.Property(e => e.Other)
                    .IsRequired()
                    .HasColumnName("other")
                    .HasColumnType("text");
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}