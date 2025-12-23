using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Entities
{
    public class Group(string name)
    {
        [Key]
        public string Name { get; set; } = name;

        // Navigation property for the connections in the group
        public ICollection<Connection> Connections { get; set; } = [];
    }
}