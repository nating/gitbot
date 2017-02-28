using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
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

                /*-----------------------------------ASK LUIS-----------------------------------------------*/

                var luisText = "";

                var query = Uri.EscapeDataString(activity.Text);
                using (HttpClient client = new HttpClient())
                {
                    string RequestURI = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b17d6663-6ed9-40aa-98f1-49a8167fbf31?subscription-key=0b6cd41f07b2459389272439c1ee757a&q=" + query;
                    HttpResponseMessage msg = await client.GetAsync(RequestURI);

                    if ((int)msg.StatusCode==200)
                    {
                        luisText = await msg.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Microsoft.Bot.Connector.Activity r = activity.CreateReply($"Sorry I'm having trouble connecting to my Language Understanding Cortex! :(");
                        await connector.Conversations.ReplyToActivityAsync(r);
                        return Request.CreateResponse(HttpStatusCode.InternalServerError);
                    }
                }

                /*--------------------------------PARSE LUIS------------------------------------------------*/

                //var intent = getIntent(luisText);
                //var user = getUser(luisText);
                //var repo = getRepo(luisText);
                //var number = getNumber(luisText);

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
                    intent = "totalNumberOfCommitsOnRepo";
                }

                //Hardcoded test parameters for GitHub Querying
                var number = 4;
                var username = "nating";
                var repoOwner = "nating";
                var repoName = "gitbot";

                var github = new GitHubClient(new ProductHeaderValue("GitBot"));
                var gitbotResponse = "";

                var theRepo = github.Repository.Get("nating","gitbot");

                /*---------------------------------SWITCH ON INTENT----------------------------------------*/

                
                
                //Switch on intent of message to get different data from github
                switch (intent)
                {
                    case "LastCommitMessage":
                        {
                            var commits = await github.Repository.Commit.Get("nating", "gitbot", "master");
                            gitbotResponse = ($"{commits.Commit.Message}");
                        }
                        break;
                    case "LastCommitTime":
                        {
                            var commits = await github.Repository.Commit.Get("nating", "gitbot", "master");
                            gitbotResponse = ($"Last commit was on {commits.Commit.Committer.Date}");
                        }
                        break;
                    case "NumberOfCommits":
                        {
                            var commits = await github.Repository.Commit.GetAll("nating", "gitbot");
                            gitbotResponse = ($"There has been {commits.Count} commits in this repository");

                        }
                        break;
                    case "numberOfContributors":
                        {
                            var contributors = await github.Repository.GetAllContributors("nating", "gitbot");
                            if (contributors.Count > 1)
                                gitbotResponse = ($"There are {contributors.Count} contributors in {repoName} repo.");
                            else
                                gitbotResponse = ($"There is {contributors.Count} contributor in {repoName} repo.");
                        }
                        break;
                    case "numberOfFiles":
                        {
                            var contents = await github.Repository.Content.GetAllContents("nating", "gitbot");
                            gitbotResponse = ($"{contents.Count} Files in {repoName} repo.");
                        }
                        break;
                    case "lastPersonToCommitOnRepo":
                        {
                            var user = await github.Repository.Commit.Get("nating", "gitbot", "master");
                            gitbotResponse = ($"The last person to commit on {repoOwner}/{repoName} was {user.Commit.Committer.Name}");
                        }
                        break;
                    case "totalNumberOfCommitsOnRepo":
                        {
                            var total = await github.Repository.Commit.GetAll("nating", "gitbot");
                            gitbotResponse = ($"There has been {total.Count} commits on {repoOwner}/{repoName} in total.");
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

        private string getIntent(string luisText)
        {
            JObject luisJson = new JObject(luisText);
            //if ()
            {

            }
            return "";
        }
    }
}