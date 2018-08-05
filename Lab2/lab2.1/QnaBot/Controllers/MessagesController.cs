using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

using Autofac;

namespace QnaBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                // await Conversation.SendAsync(activity, () => new Dialogs.QnaDialog());
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());

            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                IConversationUpdateActivity update = message;
                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    var client = scope.Resolve<IConnectorClient>();
                    if (update.MembersAdded.Count > 0)
                    {
                        var reply = message.CreateReply();
                        foreach (var newMember in update.MembersAdded)
                        {
                            if (newMember.Name.ToLower() != "bot")
                            {
                                try
                                {
                                    string json = File.ReadAllText(HttpContext.Current.Request.MapPath("~\\AdaptiveCards\\MyCard.json"));
                                    AdaptiveCards.AdaptiveCard card = JsonConvert.DeserializeObject<AdaptiveCards.AdaptiveCard>(json);
                                    reply.Attachments.Add(new Attachment
                                    {
                                        ContentType = AdaptiveCard.ContentType,
                                        Content = card
                                    });
                                }
                                catch (Exception e)
                                {
                                    reply.Text = e.Message;
                                }
                                await client.Conversations.ReplyToActivityAsync(reply);
                            }
                        }
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
            }
            else if (message.Type == ActivityTypes.Typing)
            {
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }
        }
    }
}