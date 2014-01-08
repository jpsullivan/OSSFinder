namespace OSSFinder.Entities
{
    public class ProjectDependency : IEntity
    {
        /// <summary>
        /// The project dependency id.
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// The project the dependency is used in.
        /// </summary>
        public Project Project { get; set; }

        /// <summary>
        /// The id of the project this dependency is used in.
        /// </summary>
        public int ProjectKey { get; set; }

        /// <summary>
        /// The dependency itself.
        /// </summary>
        public Dependency Dependency { get; set; }

        /// <summary>
        /// The id of the dependency itself.
        /// </summary>
        public int DependencyKey { get; set; }

        /// <summary>
        /// The dependency version.
        /// </summary>
        public DependencyVersion DependencyVersion { get; set; }

        /// <summary>
        /// The id of the dependency version.
        /// </summary>
        public int DependencyVersionKey { get; set; }
    }
}