using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Api.Entities
{
    public class vInventory
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string specification { get; set; }
        public int idunit { get; set; }
        public string unitname { get; set; }
        public string unitname2 { get; set; }
        public int idinventoryclass { get; set; }
        public string chinaname { get; set; }
    }
}
