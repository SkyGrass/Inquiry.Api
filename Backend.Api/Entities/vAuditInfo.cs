using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Api.Entities
{
    public class vAuditInfo
    {
        public Guid UserGuid { get; set; }
        public string UserName { get; set; }
        public DateTime? AuditDate { get; set; }
        public int Flag { get; set; }
        public int No { get; set; }
        public string Remark { get; set; }
    }
}
