﻿using System;
using System.Linq;
using System.Collections.Generic;
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
                    string RequestURI = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/560dcd08-17ab-433c-af87-c1f9790e2df2?subscription-key=d083a35e1b8c47138a0249785069b387&verbose=true&q=" + query;
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
                    
                    /*case "lastCommitOnRepo":
                        {
                            var commits = await github.Repository.Commit.Get(repoOwner, repoName, "master");
                            gitbotResponse = ($"The last commit was at {commits.Commit.Committer.Date.TimeOfDay} on {commits.Commit.Committer.Date.Day}/{commits.Commit.Committer.Date.Month}/{commits.Commit.Committer.Date.Year} by {commits.Commit.Author.Name}: \"{commits.Commit.Message}\"");
                        }
                        break;*/
                    case "timeOfLastCommitOnRepo":
                        {
                            if(repoOwner==null){ gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var commits = await github.Repository.Commit.Get(repoOwner, repoName, "master");
                            gitbotResponse = ($"The last commit on {repoOwner}/{repoName}/master was made at {commits.Commit.Committer.Date.TimeOfDay} on {commits.Commit.Committer.Date.Day}/{commits.Commit.Committer.Date.Month}/{commits.Commit.Committer.Date.Year}.");
                        }
                        break;
                    case "totalCommitsOnRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var commits = await github.Repository.Commit.GetAll(repoOwner, repoName);
                            gitbotResponse = ($"There has been {commits.Count} commits on {repoOwner}/{repoName}.");

                        }
                        break;
                    //Not Yet Implemented on LUIS

                    case "lastCommiter":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var commits = await github.Repository.Commit.GetAll(repoOwner, repoName);
                            gitbotResponse = ($"The last commit was made by {commits.ElementAt(0).Commit.Author.Name} \n");
                            gitbotResponse += ($"\nUsername: {commits.ElementAt(0).Author.Login} \n");
                            gitbotResponse += ($"\nEmail: {commits.ElementAt(0).Commit.Author.Email} \n");
                        }
                        break;

                    // for testing change case to lastCommitOnRepo and comment that out
                    //case "lastNCommits":
                    case "lastCommitOnRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var commits = await github.Repository.Commit.GetAll(repoOwner, repoName);
                            var noOfCommits = commits.Count;
                            var previousCommits = "";
                            var previousCommiter = "";

                            /*int i should instead be the number LUIS found*/
                            for (int i = 0; i < 60; i++)
                            {
                                previousCommits = commits.ElementAt(i).Commit.Message;  // for ElementAt() index 0 = most recent commit
                                previousCommiter = commits.ElementAt(i).Commit.Author.Name;
                                if (i == 0)
                                    gitbotResponse += ($"\nCommit #{noOfCommits}, The last commit was by {previousCommiter}. \"{previousCommits}\" \n");
                                else
                                    gitbotResponse += ($"\nCommit #{noOfCommits - i} was by {previousCommiter}. \"{previousCommits}\" \n");
                            }
                        }
                        break;

                    /* again LUIS should interperate the user, hardcoded for now*/
                    case "usersLastCommit": //test last commit by Shane(SCarmo)
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var commits = await github.Repository.Commit.GetAll(repoOwner, repoName);
                            var User = "Shane Carmody";
                            int i = 0;
                            while (!String.Equals(commits.ElementAt(i).Commit.Author.Name, User, StringComparison.Ordinal))
                            {
                                i++;
                            }
                            gitbotResponse = ($"The last commit by {User} was \"{commits.ElementAt(i).Commit.Message}\" ");
                        }
                        break;

                    /* Could incoorporate this with previous case i.e. last commit message and time */
                    case "timeOfUsersLastCommit":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var commits = await github.Repository.Commit.GetAll(repoOwner, repoName);
                            var User = "Shane Carmody";
                            int i = 0;

                            while (!String.Equals(commits.ElementAt(i).Commit.Author.Name, User, StringComparison.Ordinal))
                                i++;

                            gitbotResponse = ($"The last commit time by {User} was at {commits.ElementAt(i).Commit.Committer.Date.TimeOfDay} on {commits.ElementAt(i).Commit.Committer.Date.Day}/{commits.ElementAt(i).Commit.Committer.Date.Month}/{commits.ElementAt(i).Commit.Committer.Date.Year}.");
                        }
                        break;

                    /* Once again hardcoded until LUIS can handle it*/
                    case "noOfUserCommits":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var commits = await github.Repository.Commit.GetAll(repoOwner, repoName);
                            var User = "Geoffrey Natin";
                            int noOfCommits = commits.Count;
                            int count = 0;
                            for (int i = 0; i < noOfCommits; i++)
                                if (String.Equals(commits.ElementAt(i).Commit.Author.Name, User, StringComparison.Ordinal))
                                    count++;

                            if (count == 1)
                                gitbotResponse = ($"{User} has made {count} commit to {repoOwner}/{repoName}");

                            else
                                gitbotResponse = ($"{User} has made {count} commits to {repoOwner}/{repoName}");
                        }
                        break;

                    case "usersLastNCommits":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var User = "Geoff Natin";
                            var lastNCommits = 2;
                            gitbotResponse = ($"{User}'s last {lastNCommits} commits were:\n");
                            //int[] indexArray = new int[lastNCommits];
                            var commits = await github.Repository.Commit.GetAll(repoOwner, repoName);
                            int index = 0;
                            int count = 0; // keep count of commits by user  
                            while (count < lastNCommits)
                            {
                                if (String.Equals(commits.ElementAt(index).Commit.Author.Name, User, StringComparison.Ordinal))
                                {
                                    count++;
                                    gitbotResponse += ($"\n\"{commits.ElementAt(index).Commit.Message}\" \n");
                                }
                                index++;
                            }


                        }
                        break;
                    case "numberOfContributorsOnRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var contributors = await github.Repository.GetAllContributors(repoOwner, repoName);
                            if (contributors.Count > 1)
                                gitbotResponse = ($"There are {contributors.Count} contributors in {repoName} repo.");
                            else
                                gitbotResponse = ($"There is {contributors.Count} contributor in {repoName} repo.");
                        }
                        break;
                    case "numberOfFilesInRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var contents = await github.Repository.Content.GetAllContents(repoOwner, repoName);
                            if (contents.Count > 1)
                                gitbotResponse = ($"There are {contents.Count} files in {repoName} repo.");
                            else
                                gitbotResponse = ($"There is {contents.Count} file in {repoName} repo.");
                        }
                        break;
                    case "lastPersonToCommitOnRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var commits = await github.Repository.Commit.Get(repoOwner, repoName, "master");
                            gitbotResponse = ($"The last person to commit on {repoOwner}/{repoName} was {commits.Commit.Committer.Name}");
                        }
                        break;
                    case "lastNumberOfCommitsOnRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var commits = "";
                            gitbotResponse = ($"Here are the last {number} commits on {repoOwner}/{repoName}:{commits}");
                        }
                        break;
                    case "usersLastCommitOnRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var commit = "";
                            gitbotResponse = ($"The last commit made by {user} on {repoOwner}/{repoName} was: {commit}");
                        }
                        break;
                    case "timeOfUsersLastCommitOnRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var time = "";
                            gitbotResponse = ($"{time} is when {user} last commited on {repoOwner}/{repoName}.");
                        }
                        break;
                    case "numberOfCommitsByUserOnRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var total = "";
                            gitbotResponse = ($"{user} has made {total} commits on {repoOwner}/{repoName}.");
                        }
                        break;
                    case "lastNumberOfCommitsByUser":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            var commits = "";
                            gitbotResponse = ($"Here are the last {number} commits by {user} on {repoOwner}/{repoName}:{commits}.");
                        }
                        break;
                    case "usersBiography":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            var u = await github.User.Get(user);
                            gitbotResponse = ($"{user}'s biography is \"{u.Bio}\".");
                        }
                        break;
                    case "usersEmail":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            var u = await github.User.Get(user);
                            gitbotResponse = ($"{user}'s email address is \"{u.Email}\".");
                        }
                        break;
                    case "usersName":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            var u = await github.User.Get(user);
                            gitbotResponse = ($"{user}'s name is \"{u.Name}\".");
                        }
                        break;
                    case "usersProfileLink":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            var u = await github.User.Get(user);
                            gitbotResponse = ($"Here's a link to {user}'s profile: {u.HtmlUrl}");
                        }
                        break;
                    case "usersLocation":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            var u = await github.User.Get(user);
                            gitbotResponse = ($"{user}'s location is \"{u.Location}\".");
                        }
                        break;
                    case "usersFollowerCount":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            var u = await github.User.Get(user);
                            gitbotResponse = ($"{user} has {u.Followers} followers.");
                        }
                        break;
                    case "usersFollowingCount":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            var u = await github.User.Get(user);
                            gitbotResponse = ($"{user} is following {u.Following} users.");
                        }
                        break;
                    case "noOfWatchersOfRepo":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            var watchers = await github.Activity.Watching.GetAllWatchers(repoOwner, repoName);
                            gitbotResponse = ($"Watchers are {watchers.Count}");
                        }
                        break;
                    case "usersStarsCount":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            var repos = await github.Activity.Starring.GetAllForUser(user);
                            gitbotResponse = ($"{user} has starred {repos.Count} repos");
                        }
                        break;
                    case "usersRepositories":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            var repos = await github.Repository.GetAllForUser("nating");
                            var count = repos.Count;
                            gitbotResponse = ($"Here are {user}'s repositories:  \n");
                            for (int i = 0; i < count; i++)
                            {
                                gitbotResponse += ($"\"{repos.ElementAt(i).Name}\"  \n");
                            }
                        }
                        break;
                    case "usersRepositoryCount":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            var repos = await github.Repository.GetAllForUser("nating");
                            gitbotResponse = ($"{user} has {repos.Count} repositories.");
                        }
                        break;
                    case "help":
                        {
                            gitbotResponse = ($"You can ask me anything about information on GitHub!  \nHere's the type of questions that you can ask me: https://github.com/nating/gitbot/wiki/Questions");
                        }
                        break;
                    default:
                        {
                            gitbotResponse = ("I'm sorry, I don't know what you're asking me for!  \nHere's the type of questions that you can ask me: https://github.com/nating/gitbot/wiki/Questions");
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
                Console.Write("hi");
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        //Takes the text from a LUIS response and returns the value the top scoring intent as a string if present
        //  otherwise returns null
        private string getIntent(string luisText)
        {
            JObject luisJson = JObject.Parse(luisText);
            if (luisJson["topScoringIntent"] != null)
            {
                //Needs to be updated to include a test to see if score is above a threshold
                return luisJson["topScoringIntent"]["intent"].ToString();
            }
            return null;
        }

        //Takes the text from a LUIS response and returns the value of the first user entity if present
        //  Otherwise returns null
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
            return null;
        }

        //Takes the text from a LUIS response and returns the value of the first repoOwner entity if present
        //  Otherwise returns null
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
            return null;
        }

        //Takes the text from a LUIS response and returns the value of the first repoName entity if present
        //  Otherwise returns null
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
            return null;
        }

        //Takes the text from a LUIS response and returns the value of the first number entity if present
        //  Otherwise returns null
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
            return null;
        }
    }
}