using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
            string json = restCaller.GetReposResponse(token).Content;
            File.WriteAllText(RESPONSE_JSON_PATH, json);
            ObservableCollection<Repos> myRepos = JsonConvert.DeserializeObject<ObservableCollection<Repos>>(json);*/

            // Load local
            string json = File.ReadAllText(RESPONSE_JSON_PATH);
            ObservableCollection<Repo> myRepos = JsonConvert.DeserializeObject<ObservableCollection<Repo>>(json);

            reposListView.ItemsSource = myRepos;
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
