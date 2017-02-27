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
                    intent = "biographyOfUser";
                }
                else if (activity.Text.Contains("followers"))
                {
                    intent = "noOfFollowersForAUser";
                }
                else if (activity.Text.Contains("test"))
                {
                    //For now, put the intent you are testing in here! :)
                    intent = "";
                }

                var github = new GitHubClient(new ProductHeaderValue("GitBot"));
                var gitbotResponse = "";

                // Initialise variables for GitHub Queries
                //var number = hasNumber(json)? getNumber(json) : "";
                //var username = hasUser(json) ? getUser(json) : "";
                //var repoOwner = hasRepo(json) ? getRepoOwner(json) : "";
                //var repoName = hasRepo(json) ? getRepoName) ; "";

                //Hardcoded test parameters for GitHub Querying
                var number = 4;
                var username = "nating";
                var repoOwner = "nating";
                var repoName = "gitbot";

                var theRepo = github.Repository.Get("nating","gitbot");

                /*---------------------------------SWITCH ON INTENT----------------------------------------*/

                //Switch on intent of message to get different data from github
                switch (intent)
                {
                    case "lastPersonToCommitOnRepo":
                        {
                            var user = "";
                            gitbotResponse = ($"The last person to commit on {repoOwner}/{repoName} was {user}");
                        }
                        break;
                    case "totalNumberOfCommitsOnRepo":
                        {
                            var total = "";
                            gitbotResponse = ($"There has been {total} commits on {repoOwner}/{repoName} in total.");
                        }
                        break;
                    case "numberOfFilesOnRepo":
                        {
                            var total = "";
                            gitbotResponse = ($"There are {total} files in {repoOwner}/{repoName}.");
                        }
                        break;
                    case "numberOfContributorsOnRepo":
                        {
                            var total = "";
                            gitbotResponse = ($"{repoOwner}/{repoName} has {total} contributors.");
                        }
                        break;
                    case "timeOfLastCommitOnRepo":
                        {
                            var time = "";
                            gitbotResponse = ($"The last commit on {repoOwner}/{repoName} was made at {time}.");
                        }
                        break;
                    case "lastNumberOfCommitsOnRepo":
                        {
                            var commits = "";
                            gitbotResponse = ($"Here are the last {number} commits on {repoOwner}/{repoName}:{commits}");
                        }
                        break;
                    case "usersLastCommitOnRepo":
                        {
                            var commit = "";
                            gitbotResponse = ($"The last commit made by {username} on {repoOwner}/{repoName} was: {commit}");
                        }
                        break;
                    case "timeOfUsersLastCommitOnRepo":
                        {
                            var time = "";
                            gitbotResponse = ($"{time} is when {username} last commited on {repoOwner}/{repoName}.");
                        }
                        break;
                    case "numberOfCommitsByUserOnRepo":
                        {
                            var total = "";
                            gitbotResponse = ($"{username} has made {total} commits on {repoOwner}/{repoName}.");
                        }
                        break;
                    case "lastNumberOfCommitsByUser":
                        {
                            var commits = "";
                            gitbotResponse = ($"Here are the last {number} commits by {username} on {repoOwner}/{repoName}:{commits}.");
                        }
                        break;
                    case "biographyOfUser":
                        {
                            var user = await github.User.Get(username);
                            gitbotResponse = ($"{username}'s bio is \"{user.Bio}\".");
                        }
                        break;
                    case "usersEmailAddress":
                        {
                            var user = await github.User.Get(username);
                            gitbotResponse = ($"{username}'s email address is \"{user.Email}\".");
                        }
                        break;
                    case "usersName":
                        {
                            var user = await github.User.Get(username);
                            gitbotResponse = ($"{username}'s name is \"{user.Name}\".");
                        }
                        break;
                    case "usersLocation":
                        {
                            var user = await github.User.Get(username);
                            gitbotResponse = ($"{username}'s location is \"{user.Location}\".");
                        }
                        break;
                    case "noOfFollowersForAUser":
                        {
                            var user = await github.User.Get(username);
                            gitbotResponse = ($"{username} has {user.Followers} followers.");
                        }
                        break;
                    case "noOfUsersAUserIsFollowing":
                        {
                            var user = await github.User.Get(username);
                            gitbotResponse = ($"{username} is following {user.Following} users.");
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
                Microsoft.Bot.Connector.Activity reply = activity.CreateReply($"You said to me:\"{activity.Text}\". \n\n My response is:\"{gitbotResponse}\"");
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