namespace OSSFinder.Entities
{
    public class DependencyType : IEntity
    {
        /// <summary>
        /// The dependency type id.
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// The name of the dependency type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether or not the dependency type is enabled.
        /// </summary>
        public bool Enabled { get; set; }
    }
}