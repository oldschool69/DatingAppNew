using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class MessageParams : PaginParams
    {
        public string Container { get; set; } = "Inbox";

        public string MemberId { get; set; } = string.Empty;
    }
}