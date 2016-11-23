using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace kbsrserver.Models
{
    public class Note
    {
        public int Id { get; set; }
        [Index(IsUnique = true)]
        [MaxLength(100)]
        public string Name { get; set; }
        public string Text { get; set; }
    }
}