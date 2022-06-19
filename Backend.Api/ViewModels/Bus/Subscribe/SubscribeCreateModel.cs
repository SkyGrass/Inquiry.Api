using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Api.Entities;

namespace Backend.Api.ViewModels.Bus.Subscribe
{
    public class SubscribeCreateModel
    {
        public ZYSoftSubscribe subscribe { get; set; }

        public List<ZYSoftSubscribeEntry> subscribeEntry { get; set; }

        public List<SubscribeBillEntry> context { get; set; }
    }

}
