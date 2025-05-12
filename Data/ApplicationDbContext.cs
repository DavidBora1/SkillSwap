using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Models;

namespace SkillSwap.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Skill> Skills { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Exchange> Exchanges { get; set; } // Aggiungi questa riga

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configura relazione UserProfile -> User
            builder.Entity<UserProfile>()
                .HasOne(u => u.User)
                .WithOne()
                .HasForeignKey<UserProfile>(u => u.UserId);

            // Configura relazione Skill -> UserProfile
            builder.Entity<Skill>()
                .HasOne(s => s.UserProfile)
                .WithMany(u => u.Skills)
                .HasForeignKey(s => s.UserProfileId);

            // Configura relazione Message -> UserProfile (Sender)
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // Evita eliminazione a cascata circolare

            // Configura relazione Message -> UserProfile (Recipient)
            builder.Entity<Message>()
                .HasOne(m => m.Recipient)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict); // Evita eliminazione a cascata circolare

            // Configurazione delle relazioni per Exchange
            builder.Entity<Exchange>()
                .HasOne(e => e.Requestor)
                .WithMany()
                .HasForeignKey(e => e.RequestorId)
                .OnDelete(DeleteBehavior.Restrict);  // Evita delete cascade conflicts

            builder.Entity<Exchange>()
                .HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);  // Evita delete cascade conflicts

            builder.Entity<Exchange>()
                .HasOne(e => e.RequestedSkill)
                .WithMany()
                .HasForeignKey(e => e.RequestedSkillId)
                .OnDelete(DeleteBehavior.Restrict);  // Evita delete cascade conflicts

            builder.Entity<Exchange>()
                .HasOne(e => e.OfferedSkill)
                .WithMany()
                .HasForeignKey(e => e.OfferedSkillId)
                .OnDelete(DeleteBehavior.Restrict);  // Evita delete cascade conflicts
        }
    }
}