using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace botapplication
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
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                var userMessage = activity.Text;


                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                // return our reply to the user
                //Activity endReply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");

                //Activity endReply = activity.CreateReply($"Hello there, my name is BankBot");

                string reply = "Hello " + activity.From.Name + ", my name is BankBot. Type 'help' to see what I can do!";

                if (userData.GetProperty<bool>("SentGreeting"))
                {
                    reply = "Hello again" + activity.From.Name;
                }
                else
                {
                    userData.SetProperty<bool>("SentGreeting", true);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                }

                if (userMessage.ToLower().Contains("help"))
                {
                    reply = "You can type 'signin' to sign into facebook";
                }

                if (userMessage.ToLower().Contains("signin"))
                {
                    Activity card1 = activity.CreateReply("");

                    var signinCard = new SigninCard()
                    {
                        Text = "Sign in to Facebook here!",
                        Buttons = new List<CardAction> {
                            new CardAction()
                            {
                                Value = "https://www.facebook.com/v2.8/dialog/oauth?client_id=655730517931284&display=popup&response_type=token&redirect_uri=https://www.facebook.com/connect/login_success.html",
                                Type = "signin",
                                Title = "Facebook OAuth",
                                Image = "https://cdn1.iconfinder.com/data/icons/logotypes/32/square-facebook-128.png"
                            },
                        }
                    };

                    card1.Attachments = new List<Attachment> {
                       signinCard.ToAttachment()
                    };

                    await connector.Conversations.SendToConversationAsync(card1);
                    return Request.CreateResponse(HttpStatusCode.OK);

                }

                //
                //Test emo = new Test()
                //{
                //    Anger = temp.Anger,
                //    Contempt = temp.Contempt,
                //    Disgust = temp.Disgust,
                //    Fear = temp.Fear,
                //    Happiness = temp.Happiness,
                //    Neutral = temp.Neutral,
                //    Sadness = temp.Sadness,
                //    Surprise = temp.Surprise,
                //    Date = DateTime.Now
                //};

                //await AzureManager.AzureManagerInstance.AddTimeline(emo);
                //
                Activity endReply = activity.CreateReply(reply);
                await connector.Conversations.ReplyToActivityAsync(endReply);
            }

            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                IConversationUpdateActivity update = activity;
                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                {
                    var client = scope.Resolve<IConnectorClient>();
                    if (update.MembersAdded.Any())
                    {
                        var reply = activity.CreateReply();
                        var newMembers = update.MembersAdded?.Where(t => t.Id != activity.Recipient.Id);
                        foreach (var newMember in newMembers)
                        {
                            reply.Text = "Welcome";
                            if (!string.IsNullOrEmpty(newMember.Name))
                            {
                                reply.Text += $" {newMember.Name}";
                            }
                            reply.Text += "!";
                            await client.Conversations.ReplyToActivityAsync(reply);
                        }
                    }
                }
            }

            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}