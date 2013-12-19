namespace OSSFinder.Core.Auditing
{
    /// <summary>
    /// Represents the actual data stored in an audit entry, an AuditRecord/AuditEnvironment pair
    /// </summary>
    public class AuditEntry
    {
        public AuditRecord Record { get; set; }
        public AuditActor Actor { get; set; }

        public AuditEntry()
        {
        }

        public AuditEntry(AuditRecord record, AuditActor actor)
        {
            Record = record;
            Actor = actor;
        }
    }
}
