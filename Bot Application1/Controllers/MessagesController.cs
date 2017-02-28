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
                // <-- PUT AN ASTERISK IN BETWEEN THESE SLASHES IF TESTING WITHOUT LUIS
              
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

                var intent = getIntent(luisText);
                var user = getUser(luisText);
                var repoOwner = getRepoOwner(luisText);
                var repoName = getRepoName(luisText);
                var number = getNumber(luisText);

                // Sample decoding of message while LUIS not up and running
                /*
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
                    intent = "lastCommitOnRepo";
                }

                //Hardcoded test parameters for GitHub Querying
                var number = 4;
                var user = "nating";
                var repoOwner = "nating";
                var repoName = "gitbot";
                */

                var github = new GitHubClient(new ProductHeaderValue("GitBot"));
                var gitbotResponse = "";

                var theRepo = github.Repository.Get("nating","gitbot");

                /*---------------------------------SWITCH ON INTENT----------------------------------------*/

                
                
                //Switch on intent of message to get different data from github
                switch (intent)
                {
                    case "lastCommitOnRepo":
                        {
                            var commits = await github.Repository.Commit.Get(repoOwner, repoName, "master");
                            gitbotResponse = ($"The last commit by {user} on {repoOwner}/{repoName}/master was \"{commits.Commit.Message}\"");
                        }
                        break;
                    case "timeOfLastCommitOnRepo":
                        {
                            var commits = await github.Repository.Commit.Get(repoOwner, repoName, "master");
                            gitbotResponse = ($"The last commit on {repoOwner}/{repoName}/master was on {commits.Commit.Committer.Date}");
                        }
                        break;
                    case "totalCommitsOnRepo":
                        {
                            var commits = await github.Repository.Commit.GetAll(repoOwner, repoName);
                            gitbotResponse = ($"There has been {commits.Count} commits on {repoOwner}/{repoName}.");

                        }
                        break;
                        //Not Yet Implemented on LUIS
                    case "numberOfContributorsOnRepo":
                        {
                            var contributors = await github.Repository.GetAllContributors(repoOwner, repoName);
                            if (contributors.Count > 1)
                                gitbotResponse = ($"There are {contributors.Count} contributors in {repoName} repo.");
                            else
                                gitbotResponse = ($"There is {contributors.Count} contributor in {repoName} repo.");
                        }
                        break;
                    case "numberOfFilesInRepo":
                        {
                            var contents = await github.Repository.Content.GetAllContents(repoOwner, repoName);
                            if (contents.Count > 1)
                                gitbotResponse = ($"There are {contents.Count} files in {repoName} repo.");
                            else
                                gitbotResponse = ($"There is {contents.Count} file in {repoName} repo.");
                        }
                        break;
                        //Not yet implemented on LUIS
                    case "lastPersonToCommitOnRepo":
                        {
                            var commits = await github.Repository.Commit.Get(repoOwner, repoName, "master");
                            gitbotResponse = ($"The last person to commit on {repoOwner}/{repoName} was {commits.Commit.Committer.Name}");
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
                            gitbotResponse = ($"The last commit made by {user} on {repoOwner}/{repoName} was: {commit}");
                        }
                        break;
                    case "timeOfUsersLastCommitOnRepo":
                        {
                            var time = "";
                            gitbotResponse = ($"{time} is when {user} last commited on {repoOwner}/{repoName}.");
                        }
                        break;
                    case "numberOfCommitsByUserOnRepo":
                        {
                            var total = "";
                            gitbotResponse = ($"{user} has made {total} commits on {repoOwner}/{repoName}.");
                        }
                        break;
                    case "lastNumberOfCommitsByUser":
                        {
                            var commits = "";
                            gitbotResponse = ($"Here are the last {number} commits by {user} on {repoOwner}/{repoName}:{commits}.");
                        }
                        break;
                    case "biographyOfUser":
                        {
                            var u = await github.User.Get(user);
                            gitbotResponse = ($"{user}'s bio is \"{u.Bio}\".");
                        }
                        break;
                    case "usersEmailAddress":
                        {
                            var u = await github.User.Get(user);
                            gitbotResponse = ($"{user}'s email address is \"{u.Email}\".");
                        }
                        break;
                    case "usersName":
                        {
                            var u = await github.User.Get(user);
                            gitbotResponse = ($"{user}'s name is \"{u.Name}\".");
                        }
                        break;
                    case "usersLocation":
                        {
                            var u = await github.User.Get(user);
                            gitbotResponse = ($"{user}'s location is \"{u.Location}\".");
                        }
                        break;
                    case "noOfFollowersForAUser":
                        {
                            var u = await github.User.Get(user);
                            gitbotResponse = ($"{user} has {u.Followers} followers.");
                        }
                        break;
                    case "noOfUsersAUserIsFollowing":
                        {
                            var u = await github.User.Get(user);
                            gitbotResponse = ($"{user} is following {u.Following} users.");
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

        //Takes the text from a LUIS response and returns the value the top scoring intent as a string if present
        //  otherwise returns "doNotKnow"
        private string getIntent(string luisText)
        {
            JObject luisJson = JObject.Parse(luisText);
            if (luisJson["topScoringIntent"] != null)
            {
                //Needs to be updated to include a test to see if score is above a threshold
                return luisJson["topScoringIntent"]["intent"].ToString();
            }
            return "doNotKnow";
        }

        //Takes the text from a LUIS response and returns the value of the first user entity if present
        //  Otherwise returns "doNotKnow"
        private string getUser(string luisText)
        {
            JObject luisJson = JObject.Parse(luisText);
            JArray entities = (JArray)luisJson["entities"];
            if (entities.Count > 0)
            {
                for (var i = 0; i < entities.Count; i++)
                {
                    if (entities[i]["type"].ToString().Equals("user"))
                    {
                        return entities[i]["entity"].ToString();
                    }
                }
            }
            return "doNotKnow";
        }

        //Takes the text from a LUIS response and returns the value of the first repoOwner entity if present
        //  Otherwise returns "doNotKnow"
        private string getRepoOwner(string luisText)
        {
            JObject luisJson = JObject.Parse(luisText);
            JArray entities = (JArray)luisJson["entities"];
            if (entities.Count > 0)
            {
                for (var i = 0; i < entities.Count; i++)
                {
                    if (entities[i]["type"].ToString().Equals("repoOwner"))
                    {
                        return entities[i]["entity"].ToString();
                    }
                }
            }
            return "doNotKnow";
        }

        //Takes the text from a LUIS response and returns the value of the first repoName entity if present
        //  Otherwise returns "doNotKnow"
        private string getRepoName(string luisText)
        {
            JObject luisJson = JObject.Parse(luisText);
            JArray entities = (JArray)luisJson["entities"];
            if (entities.Count > 0)
            {
                for (var i = 0; i < entities.Count; i++)
                {
                    if (entities[i]["type"].ToString().Equals("repoName"))
                    {
                        return entities[i]["entity"].ToString();
                    }
                }
            }
            return "doNotKnow";
        }

        //Takes the text from a LUIS response and returns the value of the first number entity if present
        //  Otherwise returns "doNotKnow"
        private string getNumber(string luisText)
        {
            JObject luisJson = JObject.Parse(luisText);
            JArray entities = (JArray)luisJson["entities"];
            if (entities.Count > 0)
            {
                for (var i = 0; i < entities.Count; i++)
                {
                    if (entities[i]["type"].ToString().Equals("number"))
                    {
                        return entities[i]["entity"].ToString();
                    }
                }
            }
            return "doNotKnow";
        }
    }
}