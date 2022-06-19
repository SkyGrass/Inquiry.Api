using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Api.Entities
{
    public class ZYSoftConfig
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        [Column(TypeName = "nvarchar(50)")]
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string AppId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string AppSecret { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Remark { get; set; }
    }
}
