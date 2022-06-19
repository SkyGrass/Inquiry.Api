using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Api.ViewModels.Bus.Inquiry
{
    public class PartnerAskModel
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int EntryId { get; set; }
        public string PriceCurrent { get; set; }
        public string PriceCurrentConfirm { get; set; }
    }
}
