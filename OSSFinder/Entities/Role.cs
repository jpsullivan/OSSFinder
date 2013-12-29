using System.Collections.Generic;

namespace OSSFinder.Entities
{
    public class Role
    {
        public Role()
        {
            Users = new HashSet<User>();
        }

        public int Key { get; set; }
        public string Name { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}