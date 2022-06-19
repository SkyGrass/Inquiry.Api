using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Api.RequestPayload.PcBus.Report
{
    public class SubSummeryRequestPayload : RequestPayload
    {
        /// <summary>
        /// 开始日期
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// 科室ID
        /// </summary>
        public string DeptId { get; set; }
    }
}
