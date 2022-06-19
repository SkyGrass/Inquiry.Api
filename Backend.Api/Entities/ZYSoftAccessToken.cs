using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Api.Entities
{
    public class ZYSoftAccessToken
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public string Id { get; set; }

        [Required]
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string AccessToken { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public DateTime SaveTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public DateTime ExpireTime { get; set; }
    }
}
