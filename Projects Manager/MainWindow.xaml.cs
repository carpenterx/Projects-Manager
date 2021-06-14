using Newtonsoft.Json;
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
        private static readonly string RESPONSE_JSON_PATH = @"C:\Users\jorda\Desktop\projects_response.json";
        private string token;

        public MainWindow()
        {
            InitializeComponent();

            token = File.ReadAllText(TOKEN_PATH);

            // Rest request
            /*RestCaller restCaller = new();
            IRestResponse response = restCaller.GetReposResponse(token);
            string json = response.Content;
            Dictionary<string, string> headerDictionary = response.Headers.ToDictionary(h => h.Name, h => h.Value.ToString());
            UpdateRemainingRequestsCount(headerDictionary);
            File.WriteAllText(RESPONSE_JSON_PATH, json);
            ObservableCollection<Repo> myRepos = JsonConvert.DeserializeObject<ObservableCollection<Repo>>(json);*/

            // Load local
            string json = File.ReadAllText(RESPONSE_JSON_PATH);
            ObservableCollection<Repo> myRepos = JsonConvert.DeserializeObject<ObservableCollection<Repo>>(json);

            reposListView.ItemsSource = myRepos;
        }

        private void UpdateRemainingRequestsCount(Dictionary<string, string> headerDictionary)
        {
            requestsTxt.Text = headerDictionary["X-RateLimit-Remaining"];
        }

        private void OpenIssuesLink(object sender, RoutedEventArgs e)
        {
            Repo repo = (sender as Button).DataContext as Repo;
            Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", $"{repo.HtmlUrl}/issues");
        }

        private void OpenRepoClick(object sender, RoutedEventArgs e)
        {
            Repo repo = (sender as Button).DataContext as Repo;
            Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", repo.HtmlUrl);
        }
    }
}
