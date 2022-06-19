using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Api.Entities;

namespace Backend.Api.ViewModels.Bus.Po
{
    public class PoCreateModel
    {
        public ZYSoftPo po { get; set; }

        public List<ZYSoftPoEntry> poEntry { get; set; }

        public List<TotalPoEntry> context { get; set; }

    }

    public class TotalPoEntry
    {
        public string name { get; set; }
        public List<PoBillEntry> entry { get; set; }
    }
}
