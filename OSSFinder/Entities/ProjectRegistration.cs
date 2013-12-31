using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OSSFinder.Entities
{
    public class ProjectRegistration : IEntity
    {
        public ProjectRegistration() 
        {
            Owners = new HashSet<User>();
            Projects = new HashSet<Project>();
        }

        [Required]
        public int Id { get; set; }

        public virtual ICollection<User> Owners { get; set; }
        public virtual ICollection<Project> Projects { get; set; } 
        public int Key { get; set; }
    }
}