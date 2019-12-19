using Core;
using System.Collections.Generic;
using System.Linq;
using DevTestMHS.Models;
using NHibernate.Transform;
using System;

namespace Repository
{
    public class MessageRepository
    {
        private readonly IUnitOfWork unitOfWork;

        public MessageRepository(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public int getCount()
        {
            return this.unitOfWork.Session.Query<MHSMessage>()
                .Count();
        }

        public List<MHSMessageContract> getall()
        {

            string sql = "SELECT m.*, left(mc.Content, 2000) as Content from Message m LEFT JOIN MessageContent mc"
                + " ON m.SequenceID = mc.MessageSequenceID order by m.SequenceID";

            var query = this.unitOfWork.Session.CreateSQLQuery(sql);
            //query.SetResultTransformer(Transformers.AliasToBean<MHSMessageAddi>());
            var temp = query.List();
            List<MHSMessageContract> result = new List<MHSMessageContract>();
            foreach (object[] tt in temp)
            {
                         
                result.Add(mapToContract(tt));
                //MHSMessageAddi typed = (MHSMessageAddi)tt;
                //result.Add(typed);
            }

            return result ;

            //return this.unitOfWork.Session.Query<MHSMessage>()
            //    //.Skip(page * pageSize)
            //    //.Take(pageSize)
            //    //.Where(x => x.Destination == "EBS_ASID")
            //    //.Where(x => x.Source == "DEVTEST")
            //    .ToList();
        }


        public List<MHSMessage> getConList(string CID)
        {
            return this.unitOfWork.Session.Query<MHSMessage>()
                .Where(x => x.ConversationID == CID)
                .ToList();
        }
        public void changeState(MHSMessage m)
        {
            this.unitOfWork.Session.Merge(m);
            //this.unitOfWork.Session.SaveOrUpdate(m);
            //this.unitOfWork.Session.Transaction.Commit();
        }

        public MHSMessage getById(string sequenceID)
        {
            return this.unitOfWork.Session.Query<MHSMessage>()
                .Where(x => x.SequenceID == int.Parse(sequenceID))
                .FirstOrDefault();
        }

        private MHSMessageContract mapToContract(object[] tt)
        {
            MHSMessageContract mc = new MHSMessageContract();
            mc.sequenceId = (int)tt.ElementAt(0); //squence id
            mc.state = (string)tt.ElementAt(1); // state
            mc.attempts = (int)tt.ElementAt(2); // attemots 
            mc.actionAt = (int)((long)tt.ElementAt(3)); // actionat
            mc.version = (string)tt.ElementAt(4); //version
            mc.messageType = (string)tt.ElementAt(5);//messageType 
            mc.messageID = (string)tt.ElementAt(6); // MessageID
            mc.referenceID = (string)tt.ElementAt(7); // referenceID
            mc.conversationID = (string)tt.ElementAt(8); //conversationID
            mc.source = (string)tt.ElementAt(9); // source
            mc.destination = (string)tt.ElementAt(10); // destination
            mc.createdAt = ((DateTime)tt.ElementAt(11)).ToLocalTime().ToShortDateString();// createdAt
            mc.transportMessageId = (string)tt.ElementAt(12);// transportmesageId
            mc.shortContent = (string)tt.ElementAt(13); // short content

            return mc;
        }

        public List<MHSMessageContract> getMessagesByKeyword(string keyword)
        {
            string sql = "SELECT m.*, left(mc.Content, 2000) as Content from Message m LEFT JOIN MessageContent mc"
               + " ON m.SequenceID = mc.MessageSequenceID where mc.Content like '%"+ keyword+"%' order by m.SequenceID";

            var query = this.unitOfWork.Session.CreateSQLQuery(sql);
            //query.SetResultTransformer(Transformers.AliasToBean<MHSMessageAddi>());
            var temp = query.List();
            List<MHSMessageContract> result = new List<MHSMessageContract>();
            foreach (object[] tt in temp)
            {
                result.Add(mapToContract(tt));
            }

            return result;

        }

        public string getConnectionString(string identifier)
        {
            identifier = "DEVTEST23"; // for test purpose
            string sql = "Select s.ConnectionString from MHSSystem s where s.Identifier = '" + identifier +"'";
            var query = this.unitOfWork.Session.CreateQuery(sql);
            return query.UniqueResult().ToString();
        }

        public void updateContent(string sequenceId, string newcontent)
        {
            MHSMessageContent mc = this.unitOfWork.Session.Query<MHSMessageContent>()
                .Where(x => x.MessageSequenceId.SequenceID.ToString() == sequenceId)
                .FirstOrDefault();
            mc.Content = newcontent;
            this.unitOfWork.Session.Merge(mc);
        }

        public MHSMessageContent GetMessageContent(int messageId)
        {
            return this.unitOfWork.Session.Query<MHSMessageContent>()
                .Where(x => x.MessageSequenceId.SequenceID == messageId)
                .SingleOrDefault();
        }
    }
    
}
