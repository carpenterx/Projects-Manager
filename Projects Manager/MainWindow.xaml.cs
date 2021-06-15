using Newtonsoft.Json;
using Projects_Manager.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using YamlDotNet.Serialization;

namespace Projects_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string APPLICATION_FOLDER = "Projects Manager";
        private static readonly string REPO_INFOS_FILE = "repo infos.yml";
        private readonly string repoInfosPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APPLICATION_FOLDER, REPO_INFOS_FILE);

        private static readonly string TOKEN_PATH = @"C:\API\PROJECTS_MANAGER_GITHUB_TOKEN.TXT";
        private static readonly string RESPONSE_JSON_ROOT = @"C:\Users\jorda\Desktop\API Responses\";
        private static readonly string REPOS_FILE_NAME = "repos.json";
        private static readonly string PROJECTS_NAME_ENDING = "-Projects.json";
        private static readonly string HEADERS_PART = "Headers-";
        
        private readonly string token;

        private ObservableCollection<RepoInfo> repoInfos = new();

        public MainWindow()
        {
            InitializeComponent();

            token = File.ReadAllText(TOKEN_PATH);

            string json = GetReposJson(RESPONSE_JSON_ROOT, REPOS_FILE_NAME, HEADERS_PART);
            List<Repo> myRepos = JsonConvert.DeserializeObject<List<Repo>>(json);
            List<RepoInfo> repoInfosList = myRepos.ConvertAll(r => new RepoInfo(r));
            repoInfos = new ObservableCollection<RepoInfo>(repoInfosList);

            reposListView.ItemsSource = repoInfos;
        }

        private string GetReposJson(string rootPath, string fileName, string headersPart)
        {
            string json;
            //string localReposJsonPath = Path.Combine(RESPONSE_JSON_ROOT, REPOS_FILE_NAME);
            string localReposJsonPath = Path.Combine(rootPath, fileName);
            
            if (File.Exists(localReposJsonPath))
            {
                // Load local
                json = File.ReadAllText(localReposJsonPath);
                jsonTypeTxt.Text = "Local";
            }
            else
            {
                RestCaller restCaller = new();
                IRestResponse response = restCaller.GetReposResponse(token);
                json = StoreResponseLocally(response, rootPath, fileName, headersPart);
            }

            return json;
        }

        private string StoreResponseLocally(IRestResponse response, string rootPath, string fileName, string headersPart)
        {
            string json = response.Content;
            Dictionary<string, string> headerDictionary = response.Headers.ToDictionary(h => h.Name, h => h.Value.ToString());
            UpdateRemainingRequestsCount(headerDictionary);
            string responsePath = Path.Combine(rootPath, fileName);
            File.WriteAllText(responsePath, json);
            string headersPath = Path.Combine(rootPath, $"{headersPart}{fileName}");
            StringBuilder headersBuilder = new();
            foreach (var header in response.Headers)
            {
                headersBuilder.AppendLine($"{header.Name}: {header.Value}");
            }
            File.WriteAllText(headersPath, headersBuilder.ToString());
            jsonTypeTxt.Text = "Request";
            return json;
        }

        private string GetRepoProjectsJson(string rootPath, string repoName, string projectsNameEnding, string headersPart)
        {
            string json;

            //string localRepoProjectsJsonPath = Path.Combine(RESPONSE_JSON_ROOT, $"{repoName}{PROJECTS_NAME_ENDING}");
            string localRepoProjectsJsonPath = Path.Combine(rootPath, $"{repoName}{projectsNameEnding}");
            if (File.Exists(localRepoProjectsJsonPath))
            {
                // Load local
                json = File.ReadAllText(localRepoProjectsJsonPath);
                jsonTypeTxt.Text = "Local";
            }
            else
            {
                RestCaller restCaller = new();
                IRestResponse response = restCaller.GetRepoProjectsResponse(repoName, token);
                json = StoreResponseLocally(response, rootPath, $"{repoName}{projectsNameEnding}", headersPart);
                /*json = response.Content;
                Dictionary<string, string> headerDictionary = response.Headers.ToDictionary(h => h.Name, h => h.Value.ToString());
                UpdateRemainingRequestsCount(headerDictionary);
                File.WriteAllText(localRepoProjectsJsonPath, json);
                jsonTypeTxt.Text = "Request";*/
            }

            return json;
        }

        private void UpdateRemainingRequestsCount(Dictionary<string, string> headerDictionary)
        {
            requestsTxt.Text = headerDictionary["X-RateLimit-Remaining"];
        }

        private void OpenIssuesLink(object sender, RoutedEventArgs e)
        {
            Repo repo = (sender as Button).DataContext as Repo;
            OpenInChrome($"{repo.HtmlUrl}/issues");
        }

        private void OpenRepoClick(object sender, RoutedEventArgs e)
        {
            Repo repo = (sender as Button).DataContext as Repo;
            OpenInChrome(repo.HtmlUrl);
        }

        private void OpenInChrome(string url)
        {
            Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", url);
        }

        private void OpenProjectsLink(object sender, RoutedEventArgs e)
        {
            Repo repo = (sender as Button).DataContext as Repo;
            string json = GetRepoProjectsJson(RESPONSE_JSON_ROOT,repo.Name, PROJECTS_NAME_ENDING, HEADERS_PART);
            List<Project> repoProjects = JsonConvert.DeserializeObject<List<Project>>(json);
            if (repoProjects.Count > 0)
            {
                OpenInChrome(repoProjects[^1].HtmlUrl);
            }
            else
            {
                OpenInChrome($"{repo.HtmlUrl}/projects");
            }
        }

        private void SaveData(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string appDirectory = Path.GetDirectoryName(repoInfosPath);
            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }

            SaveCollectionToYamlFile(repoInfos, repoInfosPath);
        }

        private void SaveCollectionToYamlFile<T>(ObservableCollection<T> collection, string filePath)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException($"'{nameof(filePath)}' cannot be null or empty.", nameof(filePath));
            }

            ISerializer serializer = new SerializerBuilder().Build();
            string yaml = serializer.Serialize(collection);
            File.WriteAllText(filePath, yaml);
        }
    }
}
