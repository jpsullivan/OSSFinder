using System;

namespace OSSFinder.Entities
{
    public class Dependency : IEntity
    {
        /// <summary>
        /// The dependency Id.
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// Name of the dependency.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The repository url that this dependency is stored.
        /// </summary>
        public string RepositoryUrl { get; set; }

        /// <summary>
        /// The "package" url that this dependency can be used from.
        /// A package url can be something like a nuget url, or a nexus url.
        /// </summary>
        public string PackageUrl { get; set; }

        /// <summary>
        /// The dependencies type, or category.
        /// </summary>
        public DependencyType Type { get; set; }

        /// <summary>
        /// When the dependency was added to OSSFinder.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// When the dependeny was last modified on OSSFinder.
        /// </summary>
        public DateTime LastModified { get; set; }
    }
}