using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Api.Entities;

namespace Backend.Api.ViewModels.Bus.Inquiry
{
    public class InquiryCreateModel
    {
        public ZYSoftInquiry inquiry { get; set; }

        public List<ZYSoftInquiryEntry> inquiryEntry { get; set; }

        public List<TotalInquiryEntry> context { get; set; }

    }

    public class TotalInquiryEntry
    {
        public string name { get; set; }
        public List<InquiryBillEntry> entry { get; set; }
    }
}
