using Core;
using DevTestMHS.Models;
using MessageValidator.BusinessContext;
using MessageValidator.Helper;
using Repository;
using System.Collections.Generic;
using System.IO;
using MessageValidator;
using System;
using System.Data.SqlClient;
using System.Data;

namespace Business
{
    public class MessageBusiness
    {
        private readonly IUnitOfWork unitOfWork;
        private UnitOfWork unitOfWork1;

        public MessageBusiness(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public int getCount()
        {
            return new MessageRepository(this.unitOfWork).getCount();
        }

        public List<MHSMessageContract> Getlist()
        {
           return new MessageRepository(this.unitOfWork).getall();
        }

        public MHSMessage GetMessage(int id)
        {
            var message = new MessageRepository(this.unitOfWork).getById(id.ToString());
            if (message == null)
            {
                throw new NullReferenceException(string.Format("Message with the ID {0} doesn't exist in the database", id));
            }

            return message;
        }
        
        public IList<ValidationError> ValidateMessage(int id, string messageContent)
        {
            try
            {
                this.GetMessage(id);
            }
            catch (NullReferenceException)
            {
                throw;
            }

            if (string.IsNullOrWhiteSpace(messageContent))
            {
                throw new ArgumentNullException("messageContent");
            }

            var messageToValidate = MessageHelper.GetMessage(messageContent);
            if (messageToValidate == null)
            {
                return new List<ValidationError>() { new ValidationError(null, ErrorTypes.UnexpectedError, "Unable to validate the message") };       
            }

            try
            {
                return new Validator().Validate(messageToValidate, messageToValidate.template);
            }
            catch (ConfigurationException)
            {
                throw;
            }
        }

        public List<MHSMessage> GetConlist(string cID)
        {
            return new MessageRepository(this.unitOfWork).getConList(cID);
        }

        public string ChangeState(string currentState, string sequenceID)
        {
            MessageRepository mr = new MessageRepository(this.unitOfWork);
            MHSMessage currentMsg = mr.getById(sequenceID);

            MHSMessage temp = new MHSMessage();
            temp.ActionAt = currentMsg.ActionAt;
            //temp.Attempts = currentMsg.Attempts;
            temp.ConversationID = currentMsg.ConversationID;
            temp.CreatedAt = currentMsg.CreatedAt;
            temp.Destination = currentMsg.Destination;
            temp.MessageID = currentMsg.MessageID;
            temp.MessageType = currentMsg.MessageType;
            temp.ReferenceID = currentMsg.ReferenceID;
            temp.SequenceID = currentMsg.SequenceID;
            temp.Source = currentMsg.Source;
            temp.Version = currentMsg.Version;
            temp.TransportMessageId = currentMsg.TransportMessageId;
            if (currentState == "Send")
            {
                temp.State = "Failed";
            }
            else // when current state is "failed" or "sent"
            {
                temp.State = "Send";
                temp.Attempts = 0;
            }

            new MessageRepository(this.unitOfWork).changeState(temp);

            //Open a connection to the source database and execute the auditing stored procedure
            auditTrailSetRow("AAHP",temp.SequenceID,"Message","Update", temp.Source);

            return temp.State;
        }

        private string getConnectionString(string identifier)
        {
            return new MessageRepository(this.unitOfWork).getConnectionString(identifier);
        }

        private List<SqlParameter> GenerateSQLParameters(string userId,int sequenceId, string tablename, string actiondesc)
        {
            var paramList = new List<SqlParameter>();

            paramList.Add(new SqlParameter("@UserID", userId));
            paramList.Add(new SqlParameter("@ActionDesc", actiondesc));
            paramList.Add(new SqlParameter("@RowID", sequenceId));
            paramList.Add(new SqlParameter("@TableName", tablename));
            //paramList.Add(new SqlParameter("@ClientID", DBNull.Value));
            //paramList.Add(new SqlParameter("@ReturnID", DBNull.Value));
            //paramList.Add(new SqlParameter("@Trust", DBNull.Value));
            //paramList.Add(new SqlParameter("@DataSetType", DBNull.Value));
            //paramList.Add(new SqlParameter("@UserSDSID", DBNull.Value));
            return paramList;

        }

        private void auditTrailSetRow(string userId, int sequenceId, 
            string tablename, string actiondesc, string source)
        {
            // get connect string for the source database
            string connectString = getConnectionString(source);

            // Open a connection to the source database

            SqlConnection sqlConnObj = new SqlConnection(connectString);

            //Todo: get the user id
            var parameters = GenerateSQLParameters(userId, sequenceId, tablename, actiondesc); 
            SqlCommand sqlCmd = new SqlCommand("dbo.usp_AuditTrailSetRow", sqlConnObj);

            sqlCmd.CommandType = CommandType.StoredProcedure;

            foreach (var param in parameters)
            {
                sqlCmd.Parameters.Add(param);
            }

            sqlConnObj.Open();
            sqlCmd.ExecuteNonQuery();
            sqlConnObj.Close();
        }

        public List<MHSMessageContract> searchContent(string keyword)
        {
            return new MessageRepository(this.unitOfWork).getMessagesByKeyword(keyword);
        }

        //public List<MHSMessage> GetById(int page, int pageSize)
        //{
        //    return new MessageRepository(this.unitOfWork).getById(page, pageSize);
        //}

        public MHSMessageContent GetMessageContent(int messageId)
        {
            return new MessageRepository(this.unitOfWork).GetMessageContent(messageId);
        }

        public void updateContent(string sequenceId, string newcontent)
        {
            MessageRepository mr = new MessageRepository(this.unitOfWork);
            mr.updateContent(sequenceId, newcontent);

            MHSMessage mm = mr.getById(sequenceId);
            auditTrailSetRow( "AAHP", int.Parse(sequenceId), "MessageContent", "Update", mm.Source);
        }
    }
}
