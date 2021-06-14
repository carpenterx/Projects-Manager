﻿using Newtonsoft.Json;
using Projects_Manager.Models;
using RestSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Projects_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string TOKEN_PATH = @"C:\API\PROJECTS_MANAGER_GITHUB_TOKEN.TXT";
        private static readonly string RESPONSE_JSON_ROOT = @"C:\Users\jorda\Desktop\API Responses\";
        private static readonly string REPOS_FILE_NAME = "repos.json";
        private static readonly string PROJECTS_NAME_ENDING = "-Projects.json";
        
        private string token;

        public MainWindow()
        {
            InitializeComponent();

            token = File.ReadAllText(TOKEN_PATH);

            string json = GetReposJson();
            ObservableCollection<Repo> myRepos = JsonConvert.DeserializeObject<ObservableCollection<Repo>>(json);

            reposListView.ItemsSource = myRepos;
        }

        private string GetReposJson()
        {
            string json;
            string localReposJsonPath = Path.Combine(RESPONSE_JSON_ROOT, REPOS_FILE_NAME);
            
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
                json = response.Content;
                Dictionary<string, string> headerDictionary = response.Headers.ToDictionary(h => h.Name, h => h.Value.ToString());
                UpdateRemainingRequestsCount(headerDictionary);
                File.WriteAllText(localReposJsonPath, json);
                jsonTypeTxt.Text = "Request";
            }

            return json;
        }

        private string GetRepoProjectsJson(string repoName)
        {
            string json;

            string localRepoProjectsJsonPath = Path.Combine(RESPONSE_JSON_ROOT, $"{repoName}{PROJECTS_NAME_ENDING}");
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
                json = response.Content;
                Dictionary<string, string> headerDictionary = response.Headers.ToDictionary(h => h.Name, h => h.Value.ToString());
                UpdateRemainingRequestsCount(headerDictionary);
                File.WriteAllText(localRepoProjectsJsonPath, json);
                jsonTypeTxt.Text = "Request";
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
            GetRepoProjectsJson(repo.Name);
        }
    }
}
