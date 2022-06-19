using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Api.ViewModels.Bus.Subscribe
{
    public class AuditFlow
    {
        public int BillTypeId { get; set; }
        public string UserGuid { get; set; } 
        public string UserName { get; set; }
        public int No { get; set; }
    }
}
