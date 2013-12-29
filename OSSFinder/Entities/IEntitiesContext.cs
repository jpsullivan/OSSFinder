using System.Data.Entity;

namespace OSSFinder.Entities
{
    public interface IEntitiesContext
    {
        IDbSet<Credential> Credentials { get; set; }
        IDbSet<User> Users { get; set; }
        int SaveChanges();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set", Justification = "This is to match the EF terminology.")]
        IDbSet<T> Set<T>() where T : class;
        void DeleteOnCommit<T>(T entity) where T : class;
    }
}