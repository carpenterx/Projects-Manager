using Newtonsoft.Json;
using Projects_Manager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Projects_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string RESPONSE_JSON_PATH = @"C:\Users\jorda\Desktop\projects_response.json";
        public MainWindow()
        {
            InitializeComponent();

            // Rest request
            /*RestCaller restCaller = new();
            string json = restCaller.GetReposResponse().Content;
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
