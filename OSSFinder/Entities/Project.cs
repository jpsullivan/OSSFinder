using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OSSFinder.Entities
{
    [DisplayColumn("Title")]
    public class Project : IEntity
    {
        public Project()
        {
            Authors = new HashSet<ProjectAuthor>();
            Listed = true;
        }

        public ProjectRegistration ProjectRegistration { get; set; }
        public int ProjectRegistrationKey { get; set; }

        [Obsolete("Will be removed in a future iteration, for now is write-only")]
        public virtual ICollection<ProjectAuthor> Authors { get; set; }

        public DateTime Created { get; set; }

        /// <remarks>
        /// Has a max length of 4000. Is not indexed but *IS* used for searches. Db column is nvarchar(max).
        /// </remarks>
        public string Description { get; set; }

        /// <remarks>
        /// Has a max length of 4000. Is not indexed and not used for searches. Db column is nvarchar(max).
        /// </remarks>
        public string ReleaseNotes { get; set; }

        public int DownloadCount { get; set; }

        /// <remarks>
        /// Is not a property that we support. Maintained for legacy reasons.
        /// </remarks>
        [Obsolete]
        public string ExternalPackageUrl { get; set; }

        /// <remarks>
        /// Has a max length of 4000. Is not indexed and not used for searches. Db column is nvarchar(max).
        /// </remarks>
        public string IconUrl { get; set; }

        public bool IsLatest { get; set; }
        public bool IsLatestStable { get; set; }

        /// <summary>
        /// This is when the Package Entity was last touched (so caches can notice changes). In UTC.
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// This is when the Package Metadata was last edited by a user. Or NULL. In UTC.
        /// </summary>
        public DateTime? LastEdited { get; set; }

        /// <remarks>
        /// Has a max length of 4000. Is not indexed and not used for searches. Db column is nvarchar(max).
        /// </remarks>
        public string LicenseUrl { get; set; }

        public bool Listed { get; set; }

        public bool HideLicenseReport { get; set; }

        [StringLength(20)]
        public string Language { get; set; }

        public DateTime Published { get; set; }

        /// <remarks>
        /// Has a max length of 4000. Is not indexed and not used for searches. Db column is nvarchar(max).
        /// </remarks>
        public string ProjectUrl { get; set; }

        public bool RequiresLicenseAcceptance { get; set; }

        /// <remarks>
        /// Has a max length of 4000. Is not indexed and not used for searches. Db column is nvarchar(max).
        /// </remarks>
        public string Summary { get; set; }

        /// <remarks>
        /// Has a max length of 4000. Is not indexed and *IS* used for searches, but is maintained via Lucene. Db column is nvarchar(max).
        /// </remarks>
        public string Tags { get; set; }

        [StringLength(256)]
        public string Title { get; set; }

        public string FlattenedAuthors { get; set; }

        public int Key { get; set; }

        /// <summary>
        /// The logged in user when this package version was created.
        /// NULL for older packages.
        /// </summary>
        public User User { get; set; }
        public int? UserKey { get; set; }
    }
}