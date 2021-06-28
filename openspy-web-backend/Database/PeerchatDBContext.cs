using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CoreWeb.Models;
using Microsoft.Extensions.Configuration;

namespace CoreWeb.Database
{
    public class PeerchatDBContext : DbContext
    {
        public virtual DbSet<UsermodeRecord> Usermode { get; set; }
        public virtual DbSet<ChanpropsRecord> Chanprops { get; set; }
        public virtual DbSet<GlobalOpersRecord> GlobalOpers { get; set; }

        private IConfiguration configuration;

        public PeerchatDBContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
               optionsBuilder.UseMySQL(configuration.GetConnectionString("PeerchatDB"));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsermodeRecord>(entity =>
            {
                entity.ToTable("usermodes");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.channelmask)
                    .IsRequired()
                    .HasColumnName("channelmask")
                    .HasColumnType("text");

                entity.Property(e => e.hostmask)
                    .HasColumnName("hostmask")
                    .HasColumnType("text");

                entity.Property(e => e.comment)
                    .HasColumnName("comment")
                    .HasColumnType("text");

                entity.Property(e => e.machineid)
                    .HasColumnName("machineid")
                    .HasColumnType("text");

                entity.Property(e => e.profileid)
                    .HasColumnName("profileid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.modeflags)
                    .HasColumnName("modeflags")
                    .HasColumnType("int(11)");

                entity.Property(e => e.gameid)
                    .HasColumnName("gameid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.expiresAt)
                    .HasColumnName("expiresAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.setByNick)
                    .HasColumnName("ircNick")
                    .HasColumnType("text");

                entity.Property(e => e.setByHost)
                    .HasColumnName("setByHost")
                    .HasColumnType("text");

                entity.Property(e => e.setByPid)
                    .HasColumnName("setByPid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.setAt)
                    .HasColumnName("setAt")
                    .HasColumnType("datetime");

                entity.Ignore(e => e.expiresIn);
                entity.Ignore(e => e.isGlobal);
            });
            modelBuilder.Entity<ChanpropsRecord>(entity =>
            {
                entity.ToTable("chanprops");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.channelmask)
                    .IsRequired()
                    .HasColumnName("channelmask")
                    .HasColumnType("text");

                entity.Property(e => e.password)
                    .HasColumnName("password")
                    .HasColumnType("text");

                entity.Property(e => e.entrymsg)
                    .HasColumnName("entrymsg")
                    .HasColumnType("text");

                entity.Property(e => e.comment)
                    .HasColumnName("comment")
                    .HasColumnType("text");

                entity.Property(e => e.topic)
                    .HasColumnName("topic")
                    .HasColumnType("text");

                entity.Property(e => e.expiresAt)
                    .HasColumnName("expiresAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.groupname)
                    .HasColumnName("groupname")
                    .HasColumnType("text");

                entity.Property(e => e.limit)
                    .HasColumnName("limit")
                    .HasColumnType("int(11)");

                entity.Property(e => e.onlyOwner)
                    .HasColumnName("onlyOwner")
                    .HasColumnType("bit");

                entity.Property(e => e.setByNick)
                    .HasColumnName("setByNick")
                    .HasColumnType("text");

                entity.Property(e => e.setByPid)
                    .HasColumnName("setByPid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.setByHost)
                    .HasColumnName("setByHost")
                    .HasColumnType("text");


                entity.Ignore(e => e.kickExisting);

            });

            modelBuilder.Entity<GlobalOpersRecord>(entity =>
            {
                entity.ToTable("globalopers");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.profileid)
                    .HasColumnName("profileid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.operflags)
                    .HasColumnName("operflags")
                    .HasColumnType("int(11)");
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}