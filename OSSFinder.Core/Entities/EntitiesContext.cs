using System;
using System.Data.Entity;

namespace OSSFinder.Core.Entities
{
    public class EntitiesContext : DbContext, IEntitiesContext
    {
        /// <summary>
        /// The NuGet Gallery code should usually use this constructor, in order to respect read only mode.
        /// </summary>
        public EntitiesContext(string connectionString, bool readOnly)
            : base(connectionString)
        {
            ReadOnly = readOnly;
        }

        /// <summary>
        /// This constructor is provided mainly for purposes of running migrations from Package Manager console,
        /// or any other scenario where a connection string will be set after the EntitiesContext is created 
        /// (and read only mode is don't care).
        /// </summary>
        public EntitiesContext()
            : base("OSSFinder.SqlServer") // Use the connection string in a web.config (if one is found)
        {
        }

        public bool ReadOnly { get; private set; }
        public IDbSet<Credential> Credentials { get; set; }
        public IDbSet<User> Users { get; set; }

        IDbSet<T> IEntitiesContext.Set<T>()
        {
            return base.Set<T>();
        }

        public override int SaveChanges()
        {
            if (ReadOnly)
            {
                throw new ReadOnlyModeException("Save changes unavailable: the gallery is currently in read only mode, with limited service. Please try again later.");
            }

            return base.SaveChanges();
        }

        public void DeleteOnCommit<T>(T entity) where T : class
        {
            Set<T>().Remove(entity);
        }

#pragma warning disable 618 
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Credential>()
                .HasKey(c => c.Key)
                .HasRequired(c => c.User)
                    .WithMany(u => u.Credentials)
                    .HasForeignKey(c => c.UserKey);

            modelBuilder.Entity<User>()
                .HasKey(u => u.Key);

            modelBuilder.Entity<User>()
                .HasMany<EmailMessage>(u => u.Messages)
                .WithRequired(em => em.ToUser)
                .HasForeignKey(em => em.ToUserKey);

            modelBuilder.Entity<User>()
                .HasMany<Role>(u => u.Roles)
                .WithMany(r => r.Users)
                .Map(c => c.ToTable("UserRoles")
                           .MapLeftKey("UserKey")
                           .MapRightKey("RoleKey"));

            modelBuilder.Entity<Role>()
                .HasKey(u => u.Key);

            modelBuilder.Entity<EmailMessage>()
                .HasKey(em => em.Key);

            modelBuilder.Entity<EmailMessage>()
                .HasOptional<User>(em => em.FromUser)
                .WithMany()
                .HasForeignKey(em => em.FromUserKey);
        }
#pragma warning restore 618
    }
}
