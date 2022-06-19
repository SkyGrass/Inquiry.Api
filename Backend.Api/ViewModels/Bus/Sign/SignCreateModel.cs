using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Api.Entities;

namespace Backend.Api.ViewModels.Bus.Sign
{
    public class SignCreateModel
    {
        public ZYSoftSign sign { get; set; }

        public List<ZYSoftSignEntry> signEntry { get; set; }

        public List<TotalSignEntry> context { get; set; }

    }

    public class TotalSignEntry
    {
        public string name { get; set; }
        public List<SignBillEntry> entry { get; set; }
    }
}
