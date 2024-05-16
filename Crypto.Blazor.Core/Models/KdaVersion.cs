using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Blazor.Core.Models
{
    public class KdaVersion
    {
        public string nv { get; set; }
        public List<string> chainIds { get; set; }
    }
}
