using System;
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

                    if ((int)msg.StatusCode == 200)
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
                var num = getNumber(luisText);
                int number = 0;
                if (num != null)
                    number = Int32.Parse(num);

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
                var URL = "";
                var failURL = "https://media.tenor.co/images/0a4f3a8c6a64f71e726924746fb5c8ab/raw";

                /*---------------------------------SWITCH ON INTENT----------------------------------------*/

                //Switch on intent of message to get different data from github
                switch (intent)
                {

                    case "usersProfilePic":
                        {
                            if (user == null)
                            {
                                gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user.");
                                URL = failURL;
                                break;
                            }
                            try
                            {
                                gitbotResponse = ($"{user}'s Avatar:");
                                var u = await github.User.Get(user);
                                URL = u.AvatarUrl;
                            }
                            catch
                            {
                                gitbotResponse = ($"Sorry the user \"{user}\" does not exist");
                            }

                        }
                        break;

                    case "lastCommitOnRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            try
                            {
                                var commits = await github.Repository.Commit.Get(repoOwner, repoName, "master");
                                gitbotResponse = ($"The last commit was at {commits.Commit.Committer.Date.TimeOfDay} on {commits.Commit.Committer.Date.Day}/{commits.Commit.Committer.Date.Month}/{commits.Commit.Committer.Date.Year} by {commits.Commit.Author.Name}: \"{commits.Commit.Message}\"");
                            }
                            catch
                            {
                                gitbotResponse = ($"the repository \"{repoOwner}/{repoName}\" does not exist.");
                            }
                        }
                        break;
                    case "timeOfLastCommitOnRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            try
                            {
                                var commits = await github.Repository.Commit.Get(repoOwner, repoName, "master");
                                gitbotResponse = ($"The last commit on {repoOwner}/{repoName}/master was made at {commits.Commit.Committer.Date.TimeOfDay} on {commits.Commit.Committer.Date.Day}/{commits.Commit.Committer.Date.Month}/{commits.Commit.Committer.Date.Year}.");
                            }
                            catch
                            {
                                gitbotResponse = ($"the repository \"{repoOwner}/{repoName}\" does not exist.");
                            }
                        }
                        break;
                    case "totalCommitsOnRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            try
                            {
                                var commits = await github.Repository.Commit.GetAll(repoOwner, repoName);
                                gitbotResponse = ($"There has been {commits.Count} commits on {repoOwner}/{repoName}.");
                            }
                            catch
                            {
                                gitbotResponse = ($"the repository \"{repoOwner}/{repoName}\" does not exist.");
                            }

                        }
                        break;

                    case "lastCommiter":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            try
                            {
                                var commits = await github.Repository.Commit.GetAll(repoOwner, repoName);
                                gitbotResponse = ($"The last commit was made by {commits.ElementAt(0).Commit.Author.Name} \n");
                                gitbotResponse += ($"\nUsername: {commits.ElementAt(0).Author.Login} \n");
                                gitbotResponse += ($"\nEmail: {commits.ElementAt(0).Commit.Author.Email} \n");
                            }
                            catch
                            {
                                gitbotResponse = ("An invalid user or repo was entered.");
                            }
                        }
                        break;

                    // for testing change case to lastCommitOnRepo and comment that out
                    case "lastNCommits":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            try
                            {
                                var commits = await github.Repository.Commit.GetAll(repoOwner, repoName);
                                var noOfCommits = commits.Count;
                                var previousCommits = "";
                                var previousCommiter = "";
                                if (number < commits.Count)
                                {
                                    for (int i = 0; i < number; i++)
                                    {
                                        previousCommits = commits.ElementAt(i).Commit.Message;  // for ElementAt() index 0 = most recent commit
                                        previousCommiter = commits.ElementAt(i).Commit.Author.Name;
                                        if (i == 0)
                                            gitbotResponse += ($"\nCommit #{noOfCommits}, The last commit was by {previousCommiter}. \"{previousCommits}\" \n");
                                        else
                                            gitbotResponse += ($"\nCommit #{noOfCommits - i} was by {previousCommiter}. \"{previousCommits}\" \n");
                                    }
                                }
                                else
                                    gitbotResponse = ($"Ah here, theres not that many commits now!");
                            }
                            catch
                            {
                                gitbotResponse = ("A user or repo was typed incorrectly.");
                            }
                        }
                        break;

                    /* again LUIS should interperate the user, hardcoded for now*/
                    case "usersLastCommit":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            try
                            {
                                var commits = await github.Repository.Commit.GetAll(repoOwner, repoName);
                                int i = 0;
                                while (!String.Equals(commits.ElementAt(i).Author.Login, user, StringComparison.Ordinal) && i < commits.Count)
                                {
                                    i++;
                                }
                                if (i != 0)
                                    gitbotResponse = ($"The last commit by {user} was \"{commits.ElementAt(i).Commit.Message}\".");
                                else
                                    gitbotResponse = ($"{user} has no commits on {repoOwner}/{repoName}.");
                            }
                            catch
                            {
                                gitbotResponse = ("The user or repository entered is not valid.");
                            }
                        }
                        break;

                    /* Could incoorporate this with previous case i.e. last commit message and time */
                    case "timeOfUsersLastCommit":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            try
                            {
                                var commits = await github.Repository.Commit.GetAll(repoOwner, repoName);
                                int i = 0;
                                var u = await github.User.Get(user);
                                var name = u.Name;
                                while (!String.Equals(commits.ElementAt(i).Commit.Author.Name, name, StringComparison.Ordinal))
                                    i++;
                                if (i != 0)
                                    gitbotResponse = ($"The last commit time by {user} was at {commits.ElementAt(i).Commit.Committer.Date.TimeOfDay} on {commits.ElementAt(i).Commit.Committer.Date.Day}/{commits.ElementAt(i).Commit.Committer.Date.Month}/{commits.ElementAt(i).Commit.Committer.Date.Year}.");
                                else
                                    gitbotResponse = ($"The user has no commits on the repo");
                            }
                            catch
                            {
                                gitbotResponse = ("Invalid user or repo entered.");
                            }
                        }
                        break;

                    /* Once again hardcoded until LUIS can handle it*/
                    case "noOfUserCommits":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            try
                            {
                                var commits = await github.Repository.Commit.GetAll(repoOwner, repoName);
                                int noOfCommits = commits.Count;
                                int count = 0;
                                var u = await github.User.Get(user);
                                var name = u.Name;
                                for (int i = 0; i < noOfCommits; i++)
                                    if (String.Equals(commits.ElementAt(i).Commit.Author.Name, name, StringComparison.Ordinal))
                                        count++;

                                if (count == 1)
                                    gitbotResponse = ($"{user} has made {count} commit to {repoOwner}/{repoName}");

                                else
                                    gitbotResponse = ($"{user} has made {count} commits to {repoOwner}/{repoName}");
                            }
                            catch
                            {
                                gitbotResponse = ("FAIL!!!!");
                            }
                        }
                        break;

                    case "usersLastNCommits":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            gitbotResponse = ($"{user}'s last {number} commits were:\n");

                            // check valid repo
                            try
                            {
                                var check = await github.Repository.Commit.GetAll(repoOwner, repoName);
                            }
                            catch
                            {
                                gitbotResponse = ($"The repository \"{repoOwner}/{repoName}\" does not exist");
                                break;
                            }

                            // check valid user
                            try
                            {
                                var check2 = await github.User.Get(user);
                            }
                            catch
                            {
                                gitbotResponse = ($"\"{user}\" is not a user");
                                break;
                            }

                            var commits = await github.Repository.Commit.GetAll(repoOwner, repoName);
                            var u = await github.User.Get(user);
                            int index = 0;
                            int otherIndex = 0;
                            int count = 0; // keep count of commits by user  
                            bool commited = false; // user has commited at all to repo?
                            var name = u.Name;
                            // check if {user} has any commits on given repo
                            while (!commited && otherIndex < commits.Count)
                            {
                                if (String.Equals(commits.ElementAt(otherIndex).Commit.Author.Name, name, StringComparison.Ordinal))
                                    commited = true;
                                otherIndex++;
                            }

                            if (commited)
                            {
                                if (number < commits.Count)
                                {
                                    while (count < number)
                                    {
                                        if (String.Equals(commits.ElementAt(index).Commit.Author.Name, name, StringComparison.Ordinal))
                                        {
                                            count++;
                                            gitbotResponse += ($"\n\"{commits.ElementAt(index).Commit.Message}\" \n");
                                        }
                                        index++;
                                    }
                                }
                                else
                                {
                                    gitbotResponse = ($"There is only {commits.Count} total commits on {repoOwner}/{repoName}.");
                                }
                            }
                            else
                            {
                                gitbotResponse = ($"The user \"{user}\" has not commited to {repoOwner}/{repoName}");
                            }
                        }
                        break;


                    case "numberOfContributorsOnRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            try
                            {
                                var contributors = await github.Repository.GetAllContributors(repoOwner, repoName);
                                if (contributors.Count > 1)
                                    gitbotResponse = ($"There are {contributors.Count} contributors in {repoName} repo.");
                                else
                                    gitbotResponse = ($"There is {contributors.Count} contributor in {repoName} repo.");
                            }
                            catch
                            {
                                gitbotResponse = ($"The repo \"{repoOwner}/{repoName}\" does not exist");
                            }
                        }
                        break;
                    case "numberOfFilesInRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            try
                            {
                                var contents = await github.Repository.Content.GetAllContents(repoOwner, repoName);
                                if (contents.Count > 1)
                                    gitbotResponse = ($"There are {contents.Count} files in {repoName} repo.");
                                else
                                    gitbotResponse = ($"There is {contents.Count} file in {repoName} repo.");
                            }
                            catch
                            {
                                gitbotResponse = ($"The repo \"{repoOwner}/{repoName}\" does not exist");
                            }
                        }
                        break;
                    case "lastPersonToCommitOnRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            try
                            {
                                var commits = await github.Repository.Commit.Get(repoOwner, repoName, "master");
                                gitbotResponse = ($"The last person to commit on {repoOwner}/{repoName} was {commits.Commit.Author.Name}");
                            }
                            catch
                            {
                                gitbotResponse = ($"The repo \"{repoOwner}/{repoName}\" does not exist");
                            }
                        }
                        break;


                    /* are these not already implemented??? */
                    //--------------------------------------------------------------------------------------------------------------------------
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
                    //--------------------------------------------------------------------------------------------------------------------------


                    case "usersBiography":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            try
                            {
                                var u = await github.User.Get(user);
                                if (u.Bio != null)
                                {
                                    gitbotResponse = ($"{user}'s biography is \"{u.Bio}\".");
                                }
                                else
                                    gitbotResponse = ($"{user} has no biography.");
                            }
                            catch
                            {
                                gitbotResponse = ($"Sorry but \"{user}\" is not a user.");
                            }
                        }
                        break;
                    case "usersEmail":
                        {
                            try
                            {
                                if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                                var u = await github.User.Get(user);
                                if (u.Email != null)
                                    gitbotResponse = ($"{user}'s email address is \"{u.Email}\".");
                                else
                                    gitbotResponse = ($"The user : {user} does not display their email.");
                            }
                            catch
                            {
                                gitbotResponse = ($"Sorry but \"{user}\" is not a user.");
                            }
                        }
                        break;
                    case "usersName":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            try
                            {
                                var u = await github.User.Get(user);
                                if (u.Name != null)
                                    gitbotResponse = ($"{user}'s name is \"{u.Name}\".");
                                else
                                    gitbotResponse = ($"\"{user}\" does not display their name.");
                            }
                            catch
                            {
                                gitbotResponse = ($"The user \"{user}\", does not exist.");
                            }
                        }
                        break;
                    case "usersProfileLink":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            try
                            {
                                var u = await github.User.Get(user);
                                gitbotResponse = ($"Here's a link to {user}'s profile: {u.HtmlUrl}");
                            }
                            catch
                            {
                                gitbotResponse = ($"\"{user}\" is not a user.");
                            }
                        }
                        break;
                    case "usersLocation":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }

                            try
                            {
                                var u = await github.User.Get(user);
                                if (u.Location != null)
                                    gitbotResponse = ($"{user}'s location is \"{u.Location}\".");
                                else
                                    gitbotResponse = ($"Sorry but {user} has no location :(");
                            }
                            catch
                            {
                                gitbotResponse = ($"The user \"{user}\" does not exist.");
                            }
                        }
                        break;
                    case "usersFollowerCount":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            try
                            {
                                var u = await github.User.Get(user);
                                gitbotResponse = ($"{user} has {u.Followers} followers.");
                            }
                            catch
                            {
                                gitbotResponse = ($"\"{user}\" is not a valid username.");
                            }
                        }
                        break;
                    case "usersFollowingCount":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            try
                            {
                                var u = await github.User.Get(user);
                                gitbotResponse = ($"{user} is following {u.Following} users.");
                            }
                            catch
                            {
                                gitbotResponse = ($"User:{user} does not exist.");
                            }
                        }
                        break;
                    case "noOfWatchersOfRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            try
                            {
                                var watchers = await github.Activity.Watching.GetAllWatchers(repoOwner, repoName);
                                gitbotResponse = ($"Watchers are {watchers.Count}");
                            }
                            catch
                            {
                                gitbotResponse = ($"User:{user} does not exist.");
                            }
                        }
                        break;
                    case "usersStarsCount":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            try
                            {
                                var repos = await github.Activity.Starring.GetAllForUser(user);
                                gitbotResponse = ($"{user} has starred {repos.Count} repos");
                            }
                            catch
                            {
                                gitbotResponse = ($"User:{user} does not exist.");
                            }
                        }
                        break;
                    case "usersRepositories":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }

                            try
                            {
                                var repos = await github.Repository.GetAllForUser(user);
                                var count = repos.Count;
                                gitbotResponse = ($"Here are {user}'s repositories:  \n");
                                for (int i = 0; i < count; i++)
                                {
                                    gitbotResponse += ($"\"{repos.ElementAt(i).Name}\"  \n");
                                }
                            }
                            catch
                            {
                                gitbotResponse = ($"User:{user} does not exist.");
                            }
                        }
                        break;
                    case "usersRepositoryCount":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            try
                            {
                                var repos = await github.Repository.GetAllForUser(user);
                                gitbotResponse = ($"{user} has {repos.Count} repositories.");
                            }
                            catch
                            {
                                gitbotResponse = ($"User:{user} does not exist.");
                            }
                        }
                        break;
                    case "linkToRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            try
                            {
                                var repo = await github.Repository.Get(repoOwner, repoName);
                                gitbotResponse = ($"Here is the link to {repoOwner}/{repoName}: {repo.HtmlUrl}");
                            }
                            catch
                            {
                                gitbotResponse = ($"The repo \"{repoOwner}/{repoName}\" is not a valid repo");
                            }
                        }
                        break;
                    case "noOfForksOfRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            try
                            {
                                var fork = await github.Repository.Forks.GetAll(repoOwner, repoName);
                                gitbotResponse = ($"There are {fork.Count} defined for the repository");
                            }
                            catch
                            {
                                gitbotResponse = ($"The repo \"{repoOwner}/{repoName}\" does not exist");
                            }
                        }
                        break;
                    case "noOfBranchesOfRepo":
                        {
                            if (repoOwner == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a repoOwner."); break; }
                            try
                            {
                                var branch = await github.Repository.Branch.GetAll(repoOwner, repoName);
                                gitbotResponse = ($"There are {branch.Count} branches in the repository");
                            }
                            catch
                            {
                                gitbotResponse = ($"The repo \"{repoOwner}/{repoName}\" is non-existant");
                            }
                        }
                        break;
                    case "dateAndTimeAccountCreated":
                        {
                            if (user == null) { gitbotResponse = ($"I think you mean \"{intent}\" but I didn't see a user."); break; }
                            try
                            {
                                var u = await github.User.Get(user);
                                gitbotResponse = ($"The account {user} was created at\n");
                                gitbotResponse = ($"{u.CreatedAt}");
                            }
                            catch
                            {
                                gitbotResponse = ($"The user \"{user}\" does not exist.");
                            }
                        }
                        break;
                    case "help":
                        {
                            gitbotResponse = ($"You can ask me anything about information on GitHub!  \nHere's the type of questions that you can ask me: https://github.com/nating/gitbot/wiki/Questions");
                        }
                        break;
                    case "compliment":
                        {
                            gitbotResponse = ("Thank you!");
                        }
                        break;
                    case "thankYou":
                        {
                            gitbotResponse = ("No problem!");
                        }
                        break;
                    default:
                        {
                            gitbotResponse = ("I'm sorry, I don't know what you're asking me for!  \nHere's the type of questions that you can ask me: https://github.com/nating/gitbot/wiki/Questions");
                        }
                        break;

                }

                /*-----------------------------------RESPOND TO CLIENT-------------------------------------*/

                Microsoft.Bot.Connector.Activity reply = activity.CreateReply($"{gitbotResponse}");
                if (URL == failURL)
                {
                    reply.Attachments = new List<Attachment>();  // Initilise attachment arrayList
                    reply.Attachments.Add(new Attachment()
                    {
                        ContentUrl = URL,
                        ContentType = "image/gif",
                        Name = "reply_image.gif"
                    });
                }
                if (URL != "")
                {
                    reply.Attachments = new List<Attachment>();  // Initilise attachment arrayList
                    reply.Attachments.Add(new Attachment()
                    {
                        ContentUrl = URL,
                        ContentType = "image/png",
                        Name = "reply_image.png"
                    });
                }

                // Return our reply to the user
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
                    if (entities[i]["type"].ToString().Equals("num"))
                    {
                        return entities[i]["entity"].ToString();
                    }
                }
            }
            return null;
        }
    }
}
