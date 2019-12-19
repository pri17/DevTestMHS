using System.Security.Cryptography.X509Certificates;

namespace DomainMappings
{
    using DevTestMHS.Models;
    using FluentNHibernate.Mapping;

    public class NHSMessageMap : ClassMap<MHSMessage>
    {
        public NHSMessageMap()
        {
            this.Id(x => x.SequenceID).Column("SequenceID");
            this.Map(x => x.Attempts).Column("Attempts");
            this.Map(x => x.State);
            this.Map(x => x.ActionAt);
            this.Map(x => x.Version);
            this.Map(x => x.MessageType);
            this.Map(x => x.MessageID);
            this.Map(x => x.ReferenceID);
            this.Map(x => x.ConversationID);
            this.Map(x => x.Source);
            this.Map(x => x.Destination);
            this.Map(x => x.CreatedAt);
            this.Map(x => x.TransportMessageId);
            this.Table("dbo.Message");
        }
    }

}
