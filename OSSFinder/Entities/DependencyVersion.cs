using System;
using System.ComponentModel.DataAnnotations;
using OSSFinder.Models;

namespace OSSFinder.Entities
{
    public class DependencyVersion : IEntity
    {
        /// <summary>
        /// The dependency version id.
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// The dependency the version applies to.
        /// </summary>
        public Dependency Dependency { get; set; }

        /// <summary>
        /// The id of the dependency this version applies to.
        /// </summary>
        public int DependencyKey { get; set; }

        /// <summary>
        /// The version of the dependency. May or may not make use of SemVer.
        /// </summary>
        [StringLength(64)]
        [Required]
        public string Version { get; set; }

        /// <summary>
        /// The language this dependency was primarily written in.
        /// </summary>
        public Language Language { get; set; }

        /// <summary>
        /// When this dependency version was added to OSSFinder.
        /// </summary>
        public DateTime Created { get; set; }
    }
}