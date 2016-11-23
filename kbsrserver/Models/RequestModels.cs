using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace kbsrserver.Models
{
    public class RequestModel
    {
        public string PublicKey { get; set; }
        public string Name { get; set; }
    }

    public class SignatureRequestModel
    {
        public string HalfPrivateSignKey { get; set; }
        public string PublicSignKey { get; set; }
    }
}