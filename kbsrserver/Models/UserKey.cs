using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace kbsrserver.Models
{
    public class UserKey
    {
        public int Id { get; set; }
        
        [MaxLength(100)]
        public string Imei { get; set; }
        public string PublicKey { get; set; }
        public string SessionKey { get; set; }
        public string UserStorageKey { get; set; }
        public string SignHalfKey { get; set; }
        public string PublicSignKey { get; set; }
        public DateTime? SessionKeyGenerated { get; set; }
        
        public virtual ApplicationUser User { get; set; }
    }
}