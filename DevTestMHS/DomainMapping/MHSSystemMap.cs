using DevTestMHS.Models;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevTestMHS.DomainMapping
{
    public class MHSSystemMap : ClassMap<MHSSystem>
    {
        public MHSSystemMap()
        {
            this.Id(x => x.Identifier).GeneratedBy.Assigned();
            this.Map(x => x.LocalName);
            this.Map(x => x.PctCode);
            this.Map(x => x.ConnectionString);
            this.Map(x => x.PartyId);
            this.Table("dbo.System");
        }
    }
}