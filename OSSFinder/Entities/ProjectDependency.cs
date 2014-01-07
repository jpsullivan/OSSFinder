using OSSFinder.Models;

namespace OSSFinder.Entities
{
    public class ProjectDependency : IEntity
    {
        public int Key { get; set; }

        public Project Project { get; set; }

        public int ProjectKey { get; set; }

        public int DependencyKey { get; set; }

        public int DependencyVersionKey { get; set; }

        public Language Language { get; set; }
    }
}