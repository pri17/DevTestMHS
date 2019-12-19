using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevTestMHS.Models
{
    public class MHSMessageContract
    {
         
        public int sequenceId { get; set; }

         
        public int attempts { get; set; }

         
        public string state { get; set; }

         
        public int actionAt { get; set; }

         
        public string version { get; set; }

         
        public string messageType { get; set; } // messagetype, attempts

         
        public string messageID { get; set; }

         
        public string referenceID { get; set; }

         
        public string conversationID { get; set; }

         
        public string source { get; set; }

         
        public string destination { get; set; }

         
        public string createdAt { get; set; }

         
        public string transportMessageId { get; set; }

        public string shortContent { get; set; }
    }
}