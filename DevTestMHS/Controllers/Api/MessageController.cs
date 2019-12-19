using Core;
using DevTestMHS.Models;
using Business;
using System.Web.Http;
using Kendo.Mvc.UI;
using System.Collections.Generic;
using Kendo.Mvc.Extensions;
using System.Web.Mvc;
using System.Web.Http.Results;
using System;
using System.IO;
using MessageValidator.BusinessContext;
using System.Net.Http;
using System.Linq;
using MessageValidator.Helper;

namespace DevTestMHS.Controllers
{
 
    public class MessageController : Controller
    {
        
        [System.Web.Http.HttpGet]
        public string Index()
        {
            return "Hello World....";
        }

 
        [System.Web.Http.HttpPost]
        public JsonResult GetLists([DataSourceRequest]DataSourceRequest request)
        {
            //var sourceResult = new DataSourceResult();

            //var result = new JsonResult<DataSourceResult>(sourceResult, new Newtonsoft.Json.JsonSerializerSettings(),
            //                    System.Text.Encoding.UTF8,new MessageController());

            var result = new JsonResult();
            using (var unitofWork = new UnitOfWork())
            {
                List<MHSMessageContract> temp = new MessageBusiness(unitofWork).Getlist();
          
                //foreach (MHSMessage ii in temp)
                //{
                //    string cc = new MessageBusiness(unitofWork).Getlist(ii.SequenceID);
                //    var tt = mapToDC(ii, cc);
                //    cons.Add(tt);
                //}
                IEnumerable<MHSMessageContract> messLists = temp;

                //request.Page = 1; // always set current page to n.o.1
                result = Json(messLists.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                unitofWork.Close();
            }

            return result;
        }

        private MHSMessageContract mapToDC(MHSMessage p, MHSMessageContent mc)
        {
            //StringReader lines = new StringReader(mc.Content);
            //string line20 = null;
            //string temp = null;
            //for (var i = 0; i < 20; i++)
            //{
            //    temp = lines.ReadLine();
            //    line20 += temp + " ";
            //}
        
            return new MHSMessageContract
            {
                sequenceId = p.SequenceID,
                attempts = p.Attempts,
                actionAt = p.ActionAt,
                conversationID = p.ConversationID,
                createdAt = p.CreatedAt.ToLocalTime().ToShortDateString(),
                destination = p.Destination,
                messageID = p.MessageID,
                //messageType = p.MessageType,
                //referenceID = p.ReferenceID,
                source = p.Source,
                state = p.State,
                //transportMessageId = p.TransportMessageId,
                version = p.Version,
                shortContent = mc.Content.Substring(0, mc.Content.Length / 2)
            };
        }

        [System.Web.Http.HttpPost]
        public JsonResult GetWithConID([DataSourceRequest]DataSourceRequest request, [FromBody]string id)
        {
            var result = new JsonResult();
            using (var unitofWork = new UnitOfWork())
            {
                List<MHSMessage> messLists = new MessageBusiness(unitofWork).GetConlist(id);

                List<MHSMessageContract> cons = new List<MHSMessageContract>();

                foreach (MHSMessage ii in messLists)
                {
                    MHSMessageContent cc = new MessageBusiness(unitofWork).GetMessageContent(ii.SequenceID);
                    var tt = mapToDC(ii, cc);
                    cons.Add(tt);
                }
                IEnumerable<MHSMessageContract> templist = cons;

                result = Json(templist.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                unitofWork.Close();
            }

            return result;
        }

        [System.Web.Http.HttpPost]
        public JsonResult ChangeState(string currentState, string squenceID)
        {
            var result = new JsonResult();
            using (var unitofWork = new UnitOfWork())
            {
                ResultMessage rm = new ResultMessage();

                try
                {
                    string tempState = new MessageBusiness(unitofWork).ChangeState(currentState, squenceID);

                    rm.code = "Success";
                    rm.message = "Change State success!";
                    rm.currentState = tempState;
                }
                catch (Exception e)
                {
                    rm.code = "Failed";
                    rm.message = "Sorry, change state fail!";
                    rm.currentState = currentState;
                }
                result = Json(rm, JsonRequestBehavior.AllowGet);
                unitofWork.Close();
            }

            return result;
        }

        [System.Web.Http.HttpPost]
        public JsonResult SearchMessage([DataSourceRequest]DataSourceRequest request, string keyword)
        {
            var result = new JsonResult();
            using (var unitofWork = new UnitOfWork())
            {
               
                List<MHSMessageContract> cons = new MessageBusiness(unitofWork).searchContent(keyword);
                IEnumerable<MHSMessageContract> templist = cons;
                result = Json(templist.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
             
                unitofWork.Close();
            }

            return result;
        }

        [System.Web.Http.HttpGet]
        public JsonResult getCount()
        {
            int result;
            using (var unitOfWork = new UnitOfWork())
            {
                result = new MessageBusiness(unitOfWork).getCount();
                unitOfWork.Close();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Http.HttpGet]
        public JsonResult GetMessageContent(int id)
        {
            MHSMessageContent message;
            using (var unitOfWork = new UnitOfWork())
            {
                message = new MessageBusiness(unitOfWork).GetMessageContent(id);
                unitOfWork.Close();
            }

            var dataContract = MapToDataContract(message);
            return Json(dataContract, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Http.HttpPost]
        public JsonResult ValidateMessage(int id, [FromBody] string messageContent)
        {
            IList<ValidationError> errors = new List<ValidationError>();
            using (var unitOfWork = new UnitOfWork())
            {
                try
                {
                    errors = new MessageBusiness(unitOfWork).ValidateMessage(id, messageContent);
                }
                catch (Exception ex) when (MessageHelper.GetExceptionFilter(ex))
                {
                    var responseException = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(ex.Message)
                    };
                    throw new HttpResponseException(responseException);
                }
                unitOfWork.Close();
            }

            var dataContract = this.MapToDataContract(errors);
            return Json(dataContract, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Http.HttpPost]
        public JsonResult UpdateMessage(int id, [FromBody] string messageContent)
        {
            var result = new JsonResult();
            using (var unitofwork = new UnitOfWork())
            {
                ResultMessage rm = new ResultMessage();
                try
                {
                    new MessageBusiness(unitofwork).updateContent(id.ToString(), messageContent);
                    rm.code = "Success";
                    rm.message = "Update success!";
                }
                catch(Exception e)
                {
                    rm.code = "Error";
                    rm.message = "Sorry, update falied!";
                }
                result = Json(rm, JsonRequestBehavior.AllowGet);
                unitofwork.Close();
            }
            return result;
        }

        private IEnumerable<ValidationErrorContract> MapToDataContract(IList<ValidationError> errors)
        {
            if (errors.Count == 0)
            {
                return Enumerable.Empty<ValidationErrorContract>();
            }

            IList<ValidationErrorContract> errorsDTO = new List<ValidationErrorContract>();
            foreach (var error in errors)
            {
                errorsDTO.Add(new ValidationErrorContract
                {
                    ErrorText = error.ErrorText,
                    ErrorType = error.Error.ToString(),
                    Field = error.Field,
                    FieldNumber = error.FieldNumber,
                    SegmentOrGroup = error.SegmentOrGroup
                });
            }

            return errorsDTO;
        }

        private MHSMessageContentContract MapToDataContract(MHSMessageContent message)
        {
            if (message == null)
            {
                return null;
            }

            return new MHSMessageContentContract
            {
                Content = message.Content,
                SequenceId = message.SequenceId,
                MessageId = message.MessageSequenceId.SequenceID
            };
        }
    }
}
