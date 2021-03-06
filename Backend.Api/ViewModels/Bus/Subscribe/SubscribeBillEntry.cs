using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Api.ViewModels.Bus.Subscribe
{
    public class SubscribeBillEntry
    {
        /// <summary>
        /// 
        /// </summary>
        public string cid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 硬中华
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string specification { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int idunit { get; set; }
        /// <summary>
        /// 包
        /// </summary>
        public string unitname { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int idinventoryclass { get; set; }
        /// <summary>
        /// A香烟
        /// </summary>
        public string clsName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int clsId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string remark { get; set; }
    }
}
