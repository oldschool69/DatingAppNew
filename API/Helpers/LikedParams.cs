using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class LikedParams : PaginParams
    {
        public required string Predicate { get; set; } 

        public string CurrentMemberId { get; set; } = string.Empty;
    }
}