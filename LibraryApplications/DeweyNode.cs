using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApplications
{
    public class DeweyNode
    {
        public string CallNumber { get; set; }
        public string Description { get; set; }
        public List<DeweyNode> Children { get; set; }

        public DeweyNode()
        {
            Children = new List<DeweyNode>();
        }
    }
}
