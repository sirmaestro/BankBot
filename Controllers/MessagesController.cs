using Autofac;
using botapplication.DataModel;
using botapplication.Model;
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
using static botapplication.Model.RateObject;

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
                string[] supportedCurrency = new string[] { "AUD", "BGN", "BRL", "CAD", "CHF", "CNY", "CZK", "DKK", "GBP", "HKD", "HRK", "HUF", "IDR", "ILS", "INR", "JPY", "KRW", "MXN", "MYR", "NOK", "NZD", "PHP", "PLN", "RON", "RUB", "SEK", "SGD", "THB", "TRY", "USD", "ZAR" };


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
                    reply = "You can type :\n\n'Signin' to sign into facebook \n\n'Account' to check your account information \n\n'Rate of EXAMPLE' to check the current rates \n\n'Set currency as EXAMPLE' to set a default currency and I will remember you";
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

                if (userMessage.ToLower().Equals("delete"))
                {
                    List<Timeline> timelines = await AzureManager.AzureManagerInstance.GetTimelines();
                    foreach (Timeline t in timelines)
                    {
                        if ((t.firstName == userFirstName) && (t.lastName == userLastName))
                        {
                            reply = "Your account information has been deleted:\n\n First Name: " + t.firstName + "\n\nLast Name: " + t.lastName + "\n\nCurrency: " + t.currency;
                            await AzureManager.AzureManagerInstance.DeleteTimeline(t);
                        }
                    }
                }

                if (userMessage.ToLower().Contains("account"))
                {
                    List<Timeline> timelines = await AzureManager.AzureManagerInstance.GetTimelines();
                    foreach (Timeline t in timelines)
                    {
                        if ((t.firstName == userFirstName) && (t.lastName == userLastName))
                        {
                            reply = "Your account information:\n\n First Name: " + t.firstName + "\n\nLast Name: " + t.lastName + "\n\nCurrency: " + t.currency;
                        }
                    }
                }

                if (userMessage.Length == 19)
                {
                    if (userMessage.ToLower().Substring(0, 7).Equals("set currency as"))
                    {
                        string userCurrency = userMessage.ToUpper().Substring(16);
                        reply = "Sorry, the currency you entered is not supported";
                        foreach (string x in supportedCurrency)
                        {
                            if (userCurrency.Equals(x))
                            {
                                userData.SetProperty<string>("UserCurrency", userCurrency);
                                await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                                reply = "Currency set as " + userCurrency;
                                bool noupdateuser = true;
                                List<Timeline> timelines = await AzureManager.AzureManagerInstance.GetTimelines();
                                foreach (Timeline t in timelines)
                                {
                                    if ((t.firstName == userFirstName) && (t.lastName == userLastName))
                                    {
                                        t.currency = userCurrency;
                                        reply = "Your account information has been updated:\n\n First Name: " + t.firstName + "\n\nLast Name: " + t.lastName + "\n\nCurrency: " + t.currency;
                                        noupdateuser = false;
                                        await AzureManager.AzureManagerInstance.UpdateTimeline(t);
                                    }
                                }

                                if (noupdateuser)
                                {
                                    Timeline userDataSend = new Timeline()
                                    {
                                        firstName = userFirstName,
                                        lastName = userLastName,
                                        currency = userCurrency,
                                        Date = DateTime.Now
                                    };
                                    await AzureManager.AzureManagerInstance.AddTimeline(userDataSend);
                                }
                            }
                        }
                    }
                }
                if (userMessage.Length == 11)
                {
                    if (userMessage.ToLower().Substring(0, 7).Equals("rate of"))
                    {
                        string userCurrency = userMessage.ToUpper().Substring(8);
                        List<Timeline> timelines = await AzureManager.AzureManagerInstance.GetTimelines();
                        reply = "Sorry, I don't think I have your information";
                        foreach (Timeline t in timelines)
                        {
                            if ((t.firstName == userFirstName) && (t.lastName == userLastName))
                            {
                                double r = 0;
                                RateObject.RootObject rateobject;
                                HttpClient client = new HttpClient();
                                string x = await client.GetStringAsync(new Uri("https://api.fixer.io/latest?base=" + t.currency));
                                rateobject = JsonConvert.DeserializeObject<RateObject.RootObject>(x);
                                switch (userCurrency)
                                {
                                    case "AUD":
                                        r = rateobject.rates.AUD;
                                        break;
                                    case "BGN":
                                        r = rateobject.rates.BGN;
                                        break;
                                    case "BRL":
                                        r = rateobject.rates.BRL;
                                        break;
                                    case "CAD":
                                        r = rateobject.rates.CAD;
                                        break;
                                    case "CHF":
                                        r = rateobject.rates.CHF;
                                        break;
                                    case "CNY":
                                        r = rateobject.rates.CNY;
                                        break;
                                    case "CZK":
                                        r = rateobject.rates.CZK;
                                        break;
                                    case "DKK":
                                        r = rateobject.rates.DKK;
                                        break;
                                    case "GBP":
                                        r = rateobject.rates.GBP;
                                        break;
                                    case "HKD":
                                        r = rateobject.rates.HKD;
                                        break;
                                    case "HRK":
                                        r = rateobject.rates.HRK;
                                        break;
                                    case "HUF":
                                        r = rateobject.rates.HUF;
                                        break;
                                    case "IDR":
                                        r = rateobject.rates.IDR;
                                        break;
                                    case "ILS":
                                        r = rateobject.rates.ILS;
                                        break;
                                    case "INR":
                                        r = rateobject.rates.INR;
                                        break;
                                    case "JPY":
                                        r = rateobject.rates.JPY;
                                        break;
                                    case "KRW":
                                        r = rateobject.rates.KRW;
                                        break;
                                    case "MXN":
                                        r = rateobject.rates.MXN;
                                        break;
                                    case "MYR":
                                        r = rateobject.rates.MYR;
                                        break;
                                    case "NOK":
                                        r = rateobject.rates.NOK;
                                        break;
                                    case "NZD":
                                        r = rateobject.rates.NZD;
                                        break;
                                    case "PHP":
                                        r = rateobject.rates.PHP;
                                        break;
                                    case "PLN":
                                        r = rateobject.rates.PLN;
                                        break;
                                    case "RON":
                                        r = rateobject.rates.RON;
                                        break;
                                    case "RUB":
                                        r = rateobject.rates.RUB;
                                        break;
                                    case "SEK":
                                        r = rateobject.rates.SEK;
                                        break;
                                    case "SGD":
                                        r = rateobject.rates.SGD;
                                        break;
                                    case "THB":
                                        r = rateobject.rates.THB;
                                        break;
                                    case "TRY":
                                        r = rateobject.rates.TRY;
                                        break;
                                    case "USD":
                                        r = rateobject.rates.USD;
                                        break;
                                    case "ZAR":
                                        r = rateobject.rates.ZAR;
                                        break;
                                    default:
                                        reply = "Sorry, I can't find your currency";
                                        break;
                                }
                                reply = t.currency + " to " + userCurrency + " is " + r;
                                if (userCurrency.Equals(t.currency))
                                {
                                    reply = t.currency + " to " + userCurrency + " is 1";
                                }
                            }
                        }
                        
                    }
                }
                //if (userMessage.ToLower().Equals("currency rate"))
                //{
                //    List<Timeline> timelines = await AzureManager.AzureManagerInstance.GetTimelines();
                //    foreach (Timeline t in timelines)
                //    {
                //        reply = "Sorry, I don't think I have your information";
                //        if ((t.firstName == userFirstName) && (t.lastName == userLastName))
                //        {
                //            reply = t.currency + " rate is: \n\n";
                //            RateObject.RootObject rateobject;
                //            HttpClient client = new HttpClient();
                //            string x = await client.GetStringAsync(new Uri("https://api.fixer.io/latest?base=" + t.currency));
                //            rateobject = JsonConvert.DeserializeObject<RateObject.RootObject>(x);
                //            foreach (var entry in rateobject.rates)
                //            {
                //                reply += entry.Value + " in " + entry.Key + "\n\n";
                //            }
                //        }
                //    }

                //}

                //if (userMessage.Equals("test"))
                //{
                //    Test userDataSend = new Test()
                //    {
                //        firstName = "bob",
                //        lastName = "smith",
                //        currency = "USD",
                //        Date = DateTime.Now
                //    };
                //    await AzureManager.AzureManagerInstance.AddTimeline(userDataSend);
                //}

                //if (userMessage.ToLower().Equals("new timeline"))
                //{
                //    Timeline timeline = new Timeline()
                //    {
                //        firstName = "bob",
                //        lastName = "smith",
                //        currency = "USD",
                //        Date = DateTime.Now
                //    };

                //    await AzureManager.AzureManagerInstance.AddTimeline(timeline);

                //    reply = "New timeline added [" + timeline.Date + "]";
                //}

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