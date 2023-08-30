using Microsoft.EntityFrameworkCore;
using TaskyAPI.Models;


namespace TaskyAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {

        }
        public DbSet<User> User { get; set; } = default!;
        public DbSet<TaskyAPI.Models.Task> Task { get; set; } = default!;
        public DbSet<UserAccount> UserAccount { get; set; } = default!;
        public DbSet<TaskList> TaskList { get; set; } = default!;
        public DbSet<TaskListMeta> TaskListMeta { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserAccount>()
              .HasOne(e => e.User)
              .WithOne(e => e.Account)
              .HasForeignKey<User>(e => e.UserAccountId);


            modelBuilder.Entity<TaskList>()
            .HasMany(e => e.Tasks)
            .WithOne(e => e.TaskList)
            .HasForeignKey(e => e.TaskListId)
            .HasPrincipalKey(e => e.Id);


           // modelBuilder.Entity<TaskListMeta>().Property(e => e.Id).HasComputedColumnSql();


            modelBuilder.Entity<TaskyAPI.Models.Task>()
            .HasOne(e => e.Creator)
            .WithOne()
            .HasForeignKey<TaskyAPI.Models.Task>(e => e.CreatorId);

        }
    }
}