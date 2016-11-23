using Autofac;
using botapplication.DataModel;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.FormFlow;
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
        internal static IDialog<BankForm> MakeRootDialog()
        {
            return Chain.From(() => FormDialog.FromForm(BankForm.BuildForm));
        }
        //bool enteringFormData = false;
        //private int numberOfFieldsEntered = 0;

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
                var names = activity.From.Name.Split(' ');
                string userFirstName = names[0];
                string userLastName = "";
                if (names.Length > 1)
                {
                    userLastName = names[names.Length - 1];
                }
                
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                // return our reply to the user
                //Activity endReply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");

                //Activity endReply = activity.CreateReply($"Hello there, my name is BankBot");

                string reply = "Hello " + userFirstName + ", my name is BankBot. \n\nType 'help' to see what I can do!";

                //if (userData.GetProperty<bool>("SentGreeting"))
                //{
                //    reply = "Hello again, " + activity.From.Name + "! \nThanks for visting us again!";
                //}
                //else
                //{
                //    userData.SetProperty<bool>("SentGreeting", true);
                //    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                //}

                if (userMessage.ToLower().Contains("help"))
                {
                    reply = "You can type :\n\n'Signin' to sign into facebook \n\n'Account' to check your account information \n\n'Set currency as EXAMPLE' to set a default currency and I will remember you";
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

                //if (userMessage.ToLower().Contains("account"))
                //{

                //}

                //if (userMessage.Length == 19)
                //{
                //    if (userMessage.ToLower().Contains("set currency as"))
                //    {
                //        string userCurrency = userMessage.ToUpper().Substring(16);
                //        userData.SetProperty<string>("UserCurrency", userCurrency);
                //        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                //        reply = "Currency set as " + userCurrency;

                //        Test userDataSend = new Test()
                //        {
                //            firstName = userFirstName,
                //            lastName = userLastName,
                //            currency = userCurrency,
                //            Date = DateTime.Now
                //        };
                //        await AzureManager.AzureManagerInstance.AddTimeline(userDataSend);
                //    }
                //}
                if (userMessage.Equals("test"))
                {
                    Timeline userDataSend = new Timeline()
                    {
                        firstName = "bob",
                        lastName = "smith",
                        currency = "USD",
                        Date = DateTime.Now
                    };
                    await AzureManager.AzureManagerInstance.AddTimeline(userDataSend);
                }

                Activity endReply = activity.CreateReply(reply);
                await connector.Conversations.ReplyToActivityAsync(endReply);
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

        private async Task<Activity> HandleSystemMessage(Activity message)
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