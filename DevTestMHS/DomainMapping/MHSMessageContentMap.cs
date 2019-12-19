using DevTestMHS.Models;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevTestMHS.DomainMapping
{
    public class MHSMessageContentMap : ClassMap<MHSMessageContent>
    {
        public MHSMessageContentMap()
        {
            Id(x => x.SequenceId, "SequenceId");
            References(x => x.MessageSequenceId).Column("MessageSequenceId");
            Map(x => x.Content, "Content");
            Table("MessageContent");
            Schema("dbo");
        }
    }
}