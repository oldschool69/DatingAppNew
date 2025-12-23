using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Entities
{
    public class Connection(string connectionId, string userId)
    {
        public string ConnectionId { get; set; } = connectionId;
        public string UserId { get; set; } = userId;

        // Navigation property to the associated group
        public Group Group { get; set; } = null!;
    }
}