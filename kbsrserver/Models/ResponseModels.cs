using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace kbsrserver.Models
{
    public class SessionKeyResponse
    {
        public string EncryptedSessionKey { get; set; }
    }

    public class NoteResponse
    {
        public string Name { get; set; }
        public string EncryptedText { get; set; }
    }
}