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
        public DbSet<Notification> Notification { get; set; } = default!;
        public DbSet<TaskMeta> TaskMeta { get; set; } = default!;
        public DbSet<UserDevice> UserDevice { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserAccount>()
              .HasOne(e => e.User)
              .WithMany(e => e.Accounts);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Accounts)
                .WithOne(e => e.User);

            modelBuilder.Entity<TaskList>()
            .HasMany(e => e.Tasks)
            .WithOne(e => e.TaskList)
            .HasForeignKey(e => e.TaskListId)
            .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<TaskyAPI.Models.TaskList>()
            .HasOne(e => e.Creator)
            .WithOne()
            .HasForeignKey<TaskyAPI.Models.TaskList>(e => e.CreatorId);

            modelBuilder.Entity<TaskyAPI.Models.Task>()
            .HasOne(e => e.Creator)
            .WithOne()
            .HasForeignKey<TaskyAPI.Models.Task>(e => e.CreatorId);

            modelBuilder.Entity<TaskyAPI.Models.Task>()
             .HasMany(e => e.Meta)
             .WithOne();


            modelBuilder.Entity<UserDevice>()
           .HasOne(e => e.Account)
           .WithMany(e => e.Devices).HasForeignKey(e => e.AccountId);


            modelBuilder.Entity<TaskMeta>()
            .HasOne(e => e.File)
            .WithOne();
        }
    }
}