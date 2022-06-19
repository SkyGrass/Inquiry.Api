using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Api.Entities
{
    public class ZYSoftUserWcMapping
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        /// <summary>
        /// 用户GUID
        /// </summary>
        public Guid UserGuid { get; set; } 

        /// <summary>
        /// 微信OpenId
        /// </summary>
        public string OpenId { get; set; } 
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOn { get; set; }
    }
}
