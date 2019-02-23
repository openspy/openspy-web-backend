using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CoreWeb.Models;
using Microsoft.Extensions.Logging;

namespace CoreWeb.Database
{
    public class GameTrackerDBContext : DbContext
    {
        public DbSet<Profile> Profile { get; set; }
        public DbSet<User> User { get; set; }
        public virtual DbSet<Block> Block { get; set; }
        public virtual DbSet<Buddy> Buddy { get; set; }
        public virtual DbSet<PersistData> PersistData { get; set; }
        public virtual DbSet<PersistKeyedData> PersistKeyedData { get; set; }

        public static readonly Microsoft.Extensions.Logging.LoggerFactory _myLoggerFactory =
        new LoggerFactory(new[] {
new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider()
        });

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            #warning To protect potentially sensitive information in your connection string, you should move it out of source code.See http://go.microsoft.com/fwlink/?LinkId=723263  for guidance on storing connection strings.
            optionsBuilder.UseMySQL("server=localhost;database=GameTracker;user=CHC;password=123321");
            optionsBuilder.EnableSensitiveDataLogging(true);
            optionsBuilder.UseLoggerFactory(_myLoggerFactory);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Block>(entity =>
            {
                entity.ToTable("blocks");

                entity.HasIndex(e => e.FromProfileid)
                    .HasName("fk_blocks_from_profileid");

                entity.HasIndex(e => e.ToProfileid)
                    .HasName("fk_blocks_to_profileid");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FromProfileid)
                    .HasColumnName("from_profileid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ToProfileid)
                    .HasColumnName("to_profileid")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.FromProfile)
                    .WithMany(p => p.BlocksFromProfile)
                    .HasForeignKey(d => d.FromProfileid)
                    .HasConstraintName("fk_blocks_from_profileid");

                entity.HasOne(d => d.ToProfile)
                    .WithMany(p => p.BlocksToProfile)
                    .HasForeignKey(d => d.ToProfileid)
                    .HasConstraintName("fk_blocks_to_profileid");
            });

            modelBuilder.Entity<Buddy>(entity =>
            {
                entity.ToTable("buddies");

                entity.HasIndex(e => e.FromProfileid)
                    .HasName("fk_buddies_from_profileid");

                entity.HasIndex(e => e.ToProfileid)
                    .HasName("fk_buddies_to_profileid");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FromProfileid)
                    .HasColumnName("from_profileid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ToProfileid)
                    .HasColumnName("to_profileid")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.FromProfile)
                    .WithMany(p => p.BuddiesFromProfile)
                    .HasForeignKey(d => d.FromProfileid)
                    .HasConstraintName("fk_buddies_from_profileid");

                entity.HasOne(d => d.ToProfile)
                    .WithMany(p => p.BuddiesToProfile)
                    .HasForeignKey(d => d.ToProfileid)
                    .HasConstraintName("fk_buddies_to_profileid");
            });

            modelBuilder.Entity<PersistData>(entity =>
            {
                entity.ToTable("persist_data");

                entity.HasIndex(e => e.Profileid)
                    .HasName("fk_pd_profile");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Base64Data)
                    .HasColumnName("base64Data")
                    .HasColumnType("blob");

                entity.Property(e => e.DataIndex)
                    .HasColumnName("data_index")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Gameid)
                    .HasColumnName("gameid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Modified)
                    .HasColumnName("modified")
                    .HasColumnType("timestamp");

                entity.Property(e => e.PersistType)
                    .HasColumnName("persist_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Profileid)
                    .HasColumnName("profileid")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Profile)
                    .WithMany(p => p.PersistData)
                    .HasForeignKey(d => d.Profileid)
                    .HasConstraintName("fk_pd_profile");
            });

            modelBuilder.Entity<PersistKeyedData>(entity =>
            {
                entity.ToTable("persist_keyed_data");

                entity.HasIndex(e => e.Profileid)
                    .HasName("fk_pkd_profile");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DataIndex)
                    .HasColumnName("data_index")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Gameid)
                    .HasColumnName("gameid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.KeyName)
                    .HasColumnName("key_name")
                    .HasColumnType("text");

                entity.Property(e => e.KeyValue)
                    .HasColumnName("key_value")
                    .HasColumnType("blob");

                entity.Property(e => e.Modified)
                    .HasColumnName("modified")
                    .HasColumnType("datetime");

                entity.Property(e => e.PersistType)
                    .HasColumnName("persist_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Profileid)
                    .HasColumnName("profileid")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Profile)
                    .WithMany(p => p.PersistKeyedData)
                    .HasForeignKey(d => d.Profileid)
                    .HasConstraintName("fk_pkd_profile");
            });
            modelBuilder.Entity<Profile>(entity =>
            {
                entity.ToTable("profiles");

                entity.HasIndex(e => e.Userid)
                    .HasName("fk_user");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Admin)
                    .HasColumnName("admin")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Aimname)
                    .HasColumnName("aimname")
                    .HasColumnType("text");

                entity.Property(e => e.Chc)
                    .HasColumnName("chc")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Conn)
                    .HasColumnName("conn")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Countrycode)
                    .HasColumnName("countrycode")
                    .HasColumnType("text");

                entity.Property(e => e.Deleted)
                    .HasColumnName("deleted")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Firstname)
                    .HasColumnName("firstname")
                    .HasMaxLength(31);

                entity.Property(e => e.Homepage)
                    .HasColumnName("homepage")
                    .HasColumnType("text");

                entity.Property(e => e.I1)
                    .HasColumnName("i1")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Icquin)
                    .HasColumnName("icquin")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Inc)
                    .HasColumnName("inc")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Ind)
                    .HasColumnName("ind")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Lastname)
                    .HasColumnName("lastname")
                    .HasMaxLength(31);

                entity.Property(e => e.Lat)
                    .HasColumnName("lat")
                    .HasColumnType("decimal(10,0)");

                entity.Property(e => e.Lon)
                    .HasColumnName("lon")
                    .HasColumnType("decimal(10,0)");

                entity.Property(e => e.Mar)
                    .HasColumnName("mar")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Namespaceid)
                    .HasColumnName("namespaceid")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Nick)
                    .HasColumnName("nick")
                    .HasMaxLength(31);

                entity.Property(e => e.O1)
                    .HasColumnName("o1")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Ooc)
                    .HasColumnName("ooc")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Pic)
                    .HasColumnName("pic")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Sex)
                    .HasColumnName("sex")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Uniquenick)
                    .HasColumnName("uniquenick")
                    .HasMaxLength(21);

                entity.Property(e => e.Userid)
                    .HasColumnName("userid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Zipcode)
                    .HasColumnName("zipcode")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Profiles)
                    .HasForeignKey(d => d.Userid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_user");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Connectionspeed)
                    .HasColumnName("connectionspeed")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Cpubrandid)
                    .HasColumnName("cpubrandid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Cpuspeed)
                    .HasColumnName("cpuspeed")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Deleted)
                    .HasColumnName("deleted")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(51);

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasMaxLength(51);

                entity.Property(e => e.EmailVerified)
                    .HasColumnName("email_verified")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Hasnetwork)
                    .HasColumnName("hasnetwork")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.Partnercode)
                    .HasColumnName("partnercode")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Publicmask)
                    .HasColumnName("publicmask")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Videocard1ram)
                    .HasColumnName("videocard1ram")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Videocard2ram)
                    .HasColumnName("videocard2ram")
                    .HasColumnType("int(11)");
            });


            base.OnModelCreating(modelBuilder);
        }
    }
}