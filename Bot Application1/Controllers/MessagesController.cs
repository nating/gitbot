using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Octokit;

namespace Bot_Application1
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Microsoft.Bot.Connector.Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                /*--------------------------------------LUIS-----------------------------------------------*/

                // Send message off to LUIS
                // var json = sendToLUIS(activity.Text);

                /*--------------------------------PARSE LUIS' RESPONSE-------------------------------------*/

                // Create response based on interpretation from LUIS
                // var intent = parseIntent(json);
                // var score = parseScore(json);
                // var params = parseParams(json);

                // Sample decoding of message while LUIS not up and running
                var intent = "";
                if (activity.Text.Contains("biography"))
                {
                    intent = "biography";
                }
                else if (activity.Text.Contains("followers"))
                {
                    intent = "noOfFollowers";
                }

                var github = new GitHubClient(new ProductHeaderValue("GitBot"));
                var gitbotResponse = "";

                /*---------------------------------SWITCH ON INTENT----------------------------------------*/

                //Switch on intent of message to get different data from github
                switch (intent)
                {
                    case "biography":
                        {
                            //var username = params[0];
                            var username = "nating";
                            var user = await github.User.Get(username);
                            gitbotResponse = ($"{username}'s bio is \"{user.Bio}\".");
                        }
                        break;
                    case "noOfFollowers":
                        {
                            //var username = params[0];
                            var username = "nating";
                            var user = await github.User.Get("nating");
                            gitbotResponse = ($"{username} has {user.Followers} followers.");
                        }
                        break;
                    default:
                        {
                            gitbotResponse = ("I'm sorry, I don't know what you're asking me for!");
                        }
                        break;

                }

                /*-----------------------------------RESPOND TO CLIENT-------------------------------------*/

                // Return our reply to the user
                Microsoft.Bot.Connector.Activity reply = activity.CreateReply($"{gitbotResponse}");
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Microsoft.Bot.Connector.Activity HandleSystemMessage(Microsoft.Bot.Connector.Activity message)
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