using Projects_Manager.Properties;
using RestSharp;
using RestSharp.Authenticators;
using System;

namespace Projects_Manager.Models
{
    class RestCaller
    {
        //private const string USER_NAME = "carpenterx";

        public IRestResponse GetReposResponse(string token, string page = "1")
        {
            RestClient client = new RestClient("https://api.github.com/");
;
            RestRequest request = new RestRequest(ListAuthenticatedUserRepos(), Method.GET);
            //request.AddParameter("state", "all");
            request.AddParameter("per_page", "100");
            request.AddParameter("page", page);

            request.AddHeader("Accept", "application/vnd.github.inertia-preview+json");
            client.Authenticator = new HttpBasicAuthenticator(Settings.Default.UserName, token);
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
            {
                return response;
            }
            else
            {
                return null;
            }
        }

        public IRestResponse GetRepoProjectsResponse(string repoName, string token)
        {
            RestClient client = new RestClient("https://api.github.com/");
            ;
            RestRequest request = new RestRequest(ListRepositoryProjects(Settings.Default.UserName, repoName), Method.GET);
            request.AddParameter("state", "all");

            request.AddHeader("Accept", "application/vnd.github.inertia-preview+json");
            client.Authenticator = new HttpBasicAuthenticator(Settings.Default.UserName, token);
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
            {
                return response;
            }
            else
            {
                return null;
            }
        }

        public static DateTime UnixTimeStampToDateTime(string unixTimeStampString)
        {
            double unixTimeStamp = double.Parse(unixTimeStampString);
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        // /users/{username}/projects
        private string ListProjects(string username)
        {
            return $"/users/{username}/projects";
        }

        // List repository projects
        // /repos/{owner}/{repo}/projects
        private string ListRepositoryProjects(string owner, string repo)
        {
            return $"/repos/{owner}/{repo}/projects";
        }

        // List repository issues

        // Note: GitHub's REST API v3 considers every pull request an issue, but not every issue is a pull request. For this reason, "Issues" endpoints may return both issues and pull requests in the response. You can identify pull requests by the pull_request key. Be aware that the id of a pull request returned from "Issues" endpoints will be an issue id. To find out the pull request id, use the "List pull requests" endpoint.

        // GET /repos/{owner}/{repo}/issues
        private string ListRepositoryIssues(string owner, string repo)
        {
            return $"/repos/{owner}/{repo}/issues";
        }

        // List repositories for a user

        // Lists public repositories for the specified user. Note: For GitHub AE, this endpoint will list internal repositories for the specified user.

        // DEFAULT: 30 repos per page

        // /users/{username}/repos
        private string ListUserRepos(string username)
        {
            //return $"/users/{username}/repos";
            return "/user/repos";
        }

        private string ListAuthenticatedUserRepos()
        {
            return "/user/repos";
        }

        // List project columns
        // /projects/{project_id}/columns
        private string GetProjectColumns(string project_id)
        {
            return $"/projects/{project_id}/columns";
        }

        // List project cards

        // /projects/columns/{column_id}/cards
        private string ListProjectCards(string column_id)
        {
            return $"/projects/columns/{column_id}/cards";
        }

        // Get emojis
        // Lists all the emojis available to use on GitHub.
        // GET /emojis
        private string GetEmojis()
        {
            return "/emojis";
        }
    }
}
