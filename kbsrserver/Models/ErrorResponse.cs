using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace kbsrserver.Models
{
    public class ErrorResponse
    {
        public HttpStatusCode Code { get; set; }
        public string Message { get; set; }
    }
}