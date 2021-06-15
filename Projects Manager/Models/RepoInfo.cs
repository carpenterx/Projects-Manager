namespace Projects_Manager.Models
{
    class RepoInfo
    {
        public Repo Repo { get; set; }
        public string Notes { get; set; }
        public bool IsHidden { get; set; }

        public RepoInfo(Repo repo, bool isHidden = false)
        {
            Repo = repo;
            Notes = "";
            IsHidden = isHidden;
        }
    }
}
