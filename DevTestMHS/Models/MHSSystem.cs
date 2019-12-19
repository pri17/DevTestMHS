using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevTestMHS.Models
{
    public class MHSSystem
    {
        public virtual string Identifier { get; set; }
        public virtual string LocalName { get; set; }
        public virtual string ConnectionString { get; set; }
        public virtual string PctCode { get; set; }
        public virtual string PartyId { get; set; }
    }
}