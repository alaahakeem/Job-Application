using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace JobApplication.Infrastructure.Persistence
{
    // IdentityDbContext<ApplicationUser> = DbContext + the ASP.NET Core Identity tables
    // (AspNetUsers, AspNetRoles, ...). Your own DbSets stay exactly as they were.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<JobCandidateApplication> JobCandidateApplications { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // required for the Identity tables

            // Job owner (Recruiter). Restrict: deleting a user never cascades to jobs.
            builder.Entity<Job>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(j => j.RecruiterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Candidate profile <-> user account (one profile per user).
            builder.Entity<Candidate>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Candidate>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            // Blocks duplicate applications (same candidate + same job) at DB level.
            builder.Entity<JobCandidateApplication>()
                .HasIndex(a => new { a.CandidateId, a.JobId })
                .IsUnique();
        }
    }
}
