using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Api.ViewModels.Bus.Inquiry
{
    public class PriceCurrentConfirm
    {
        public string BillId { get; set; }
        public string PartnerId { get; set; }
        public string InvId { get; set; }
        public decimal ConfirmPrice { get; set; }
    }
}
