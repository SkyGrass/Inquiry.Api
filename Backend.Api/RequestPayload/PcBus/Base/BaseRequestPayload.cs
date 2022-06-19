using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Api.RequestPayload.PcBus.Base
{
    public class BaseRequestPayload : RequestPayload
    {
        /// <summary>
        /// 查询类型
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 是否绑定
        /// </summary>
        public int Notbind { get; set; } = 0;
    }
}
