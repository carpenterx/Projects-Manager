using Newtonsoft.Json;
using Projects_Manager.Models;
using Projects_Manager.Properties;
using Projects_Manager.Windows;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YamlDotNet.Serialization;

namespace Projects_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string APPLICATION_FOLDER = "Projects Manager";
        private const string REPO_INFOS_FILE = "repo infos.yml";
        private readonly string repoInfosPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APPLICATION_FOLDER, REPO_INFOS_FILE);

        private const string TOKEN_PATH = @"C:\API\PROJECTS_MANAGER_GITHUB_TOKEN.TXT";
        private const string RESPONSE_JSON_ROOT = @"C:\Users\jorda\Desktop\work\API Responses\";
        private const string REPOS_FILE_NAME = "repos.json";
        private const string PROJECTS_NAME_ENDING = "-Projects.json";
        private const string HEADERS_PART = "Headers-";
        
        private readonly string token;

        private ObservableCollection<RepoInfo> allRepoInfos;

        private InternetConnectionChecker internet = new();

        public MainWindow()
        {
            InitializeComponent();

            token = File.ReadAllText(TOKEN_PATH);

            List<string> json = GetReposJson();

            MergeProjectsData(json);

            UpdateProjectsDisplay();
        }

        private void MergeProjectsData(List<string> json)
        {
            ObservableCollection<RepoInfo> localRepoInfos = LoadYamlFileToCollection<RepoInfo>(repoInfosPath);

            List<Repo> myRepos = new();
            allRepoInfos = new();
            foreach (string response in json)
            {
                myRepos.AddRange(JsonConvert.DeserializeObject<List<Repo>>(response));
            }

            foreach (Repo repo in myRepos)
            {
                RepoInfo repoInfo = localRepoInfos.FirstOrDefault(r => r.Repo.Name == repo.Name);
                if (repoInfo == null)
                {
                    allRepoInfos.Add(new RepoInfo(repo));
                }
                else
                {
                    allRepoInfos.Add(new RepoInfo(repo, repoInfo.IsHidden, repoInfo.Notes));
                }
            }
        }

        private ObservableCollection<T> LoadYamlFileToCollection<T>(string filePath)
        {
            ObservableCollection<T> list = new();
            if (File.Exists(filePath))
            {
                var input = new StringReader(File.ReadAllText(filePath));
                var deserializer = new DeserializerBuilder().Build();

                ObservableCollection<T> items = deserializer.Deserialize<ObservableCollection<T>>(input);
                list = new ObservableCollection<T>(items);
            }

            return list;
        }

        private List<string> GetReposJson()
        {
            List<string> json = new();
            string localReposJsonPath = Path.Combine(RESPONSE_JSON_ROOT, REPOS_FILE_NAME);
            
            if ((Settings.Default.UseLocalData && File.Exists(localReposJsonPath)) || !internet.IsConnected())
            {
                // Load local
                refreshBtn.Visibility = Visibility.Visible;
                json = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(localReposJsonPath));
                jsonTypeTxt.Text = "Local";
            }
            else
            {
                json = GetOnlineReposJson();
            }

            return json;
        }

        private List<string> GetReposPageJson()
        {
            string linkHeader;
            string page = "1";
            IRestResponse response;
            Dictionary<string, string> headerDictionary;
            List<string> json = new();
            do
            {
                RestCaller restCaller = new();
                response = restCaller.GetReposResponse(token, page);
                json.Add(response.Content);
                headerDictionary = response.Headers.ToDictionary(h => h.Name, h => h.Value.ToString());
                if (headerDictionary.ContainsKey("Link"))
                {
                    linkHeader = headerDictionary["Link"];
                    page = GetNextPage(linkHeader);
                }
                else
                {
                    page = "";
                }
            }
            while (page != "");

            UpdateRemainingRequestsCount(headerDictionary);
            string responsePath = Path.Combine(RESPONSE_JSON_ROOT, REPOS_FILE_NAME);
            File.WriteAllText(responsePath, JsonConvert.SerializeObject(json));
            string headersPath = Path.Combine(RESPONSE_JSON_ROOT, $"{HEADERS_PART}{REPOS_FILE_NAME}");
            StringBuilder headersBuilder = new();
            foreach (var header in response.Headers)
            {
                headersBuilder.AppendLine($"{header.Name}: {header.Value}");
            }
            File.WriteAllText(headersPath, headersBuilder.ToString());
            jsonTypeTxt.Text = "Request";

            return json;
        }

        private string GetNextPage(string linkHeader)
        {
            Regex pagePattern = new Regex("(\\d+)>; rel=\"next\"");
            Match match = pagePattern.Match(linkHeader);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return "";
            }
        }

        private List<string> GetOnlineReposJson()
        {
            refreshBtn.Visibility = Visibility.Collapsed;

            return GetReposPageJson();
        }

        private void StoreResponseLocally(IRestResponse response, string fileName)
        {
            string json = response.Content;
            Dictionary<string, string> headerDictionary = response.Headers.ToDictionary(h => h.Name, h => h.Value.ToString());
            UpdateRemainingRequestsCount(headerDictionary);
            string responsePath = Path.Combine(RESPONSE_JSON_ROOT, fileName);
            File.WriteAllText(responsePath, json);
            string headersPath = Path.Combine(RESPONSE_JSON_ROOT, $"{HEADERS_PART}{fileName}");
            StringBuilder headersBuilder = new();
            foreach (var header in response.Headers)
            {
                headersBuilder.AppendLine($"{header.Name}: {header.Value}");
            }
            File.WriteAllText(headersPath, headersBuilder.ToString());
            jsonTypeTxt.Text = "Request";
        }

        private string GetRepoProjectsJson(string repoName)
        {
            string json;

            string localRepoProjectsJsonPath = Path.Combine(RESPONSE_JSON_ROOT, $"{repoName}{PROJECTS_NAME_ENDING}");
            if ((Settings.Default.UseLocalData && File.Exists(localRepoProjectsJsonPath)) || !internet.IsConnected())
            {
                // Load local
                refreshBtn.Visibility = Visibility.Visible;
                json = File.ReadAllText(localRepoProjectsJsonPath);
                jsonTypeTxt.Text = "Local";
            }
            else
            {
                refreshBtn.Visibility = Visibility.Collapsed;
                RestCaller restCaller = new();
                IRestResponse response = restCaller.GetRepoProjectsResponse(repoName, token);
                json = response.Content;
                StoreResponseLocally(response, $"{repoName}{PROJECTS_NAME_ENDING}");
            }

            return json;
        }

        private void UpdateRemainingRequestsCount(Dictionary<string, string> headerDictionary)
        {
            requestsTxt.Text = headerDictionary["X-RateLimit-Remaining"];
        }

        private void OpenIssuesLink(object sender, RoutedEventArgs e)
        {
            RepoInfo repoInfo = (sender as Button).DataContext as RepoInfo;
            OpenInChrome($"{repoInfo.Repo.HtmlUrl}/issues");
        }

        private void OpenRepoClick(object sender, RoutedEventArgs e)
        {
            RepoInfo repoInfo = (sender as Button).DataContext as RepoInfo;
            OpenInChrome(repoInfo.Repo.HtmlUrl);
        }

        private void OpenInChrome(string url)
        {
            Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", url);
        }

        private void OpenProjectsLink(object sender, RoutedEventArgs e)
        {
            RepoInfo repoInfo = (sender as Button).DataContext as RepoInfo;
            string json = GetRepoProjectsJson(repoInfo.Repo.Name);
            List<Project> repoProjects = JsonConvert.DeserializeObject<List<Project>>(json);
            if (repoProjects.Count > 0)
            {
                OpenInChrome(repoProjects[^1].HtmlUrl);
            }
            else
            {
                OpenInChrome($"{repoInfo.Repo.HtmlUrl}/projects");
            }
        }

        private void SaveData(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string appDirectory = Path.GetDirectoryName(repoInfosPath);
            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }

            SaveCollectionToYamlFile(allRepoInfos, repoInfosPath);
            Settings.Default.Save();
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

        private void ToggleRepoVisibilityClick(object sender, RoutedEventArgs e)
        {
            RepoInfo repoInfo = (sender as Button).DataContext as RepoInfo;
            repoInfo.IsHidden = !repoInfo.IsHidden;
            if (!Settings.Default.ShowHiddenProjects)
            {
                HideHiddenProjects();
            }
        }

        private void ShowHiddenProjects()
        {
            reposListView.ItemsSource = new ObservableCollection<RepoInfo>(allRepoInfos);
        }

        private void HideHiddenProjects()
        {
            reposListView.ItemsSource = new ObservableCollection<RepoInfo>(allRepoInfos.Where(r => r.IsHidden == false));
        }

        private void OpenSettingsClick(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        private void UpdateProjectsDisplay()
        {
            if (Settings.Default.ShowHiddenProjects)
            {
                ShowHiddenProjects();
            }
            else
            {
                HideHiddenProjects();
            }
        }

        private void FindRepoClick(object sender, RoutedEventArgs e)
        {
            FindRepo();
        }

        private void OnKeyDownHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                FindRepo();
            }
        }

        private void FindRepo()
        {
            string searchTerm = searchTxt.Text.ToLowerInvariant();
            foreach (var item in reposListView.Items)
            {
                RepoInfo repoInfo = item as RepoInfo;
                if (repoInfo.Repo.Name.ToLowerInvariant().Contains(searchTerm))
                {
                    reposListView.ScrollIntoView(item);
                    reposListView.SelectedItem = item;
                    break;
                }
            }
        }

        private void RetryConnectionClick(object sender, RoutedEventArgs e)
        {
            if (internet.IsConnected())
            {
                List<string> json = GetOnlineReposJson();

                MergeProjectsData(json);

                UpdateProjectsDisplay();
            }
        }

        private void ShowSettingsKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ShowSettings();
            }
        }

        private void ShowSettings()
        {
            SettingsWindow settingsWindow = new();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
            UpdateProjectsDisplay();
        }
    }
}
