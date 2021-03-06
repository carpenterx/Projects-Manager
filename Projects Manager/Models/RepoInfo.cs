using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Projects_Manager.Models
{
    class RepoInfo : INotifyPropertyChanged
    {
        public Repo Repo { get; set; }

        private string _notes;
        [YamlMember(ScalarStyle = ScalarStyle.Literal)]
        public string Notes
        {
            get => _notes;
            set => SetField(ref _notes, value);
        }

        private bool _isHidden;
        public bool IsHidden
        {
            get => _isHidden;
            set => SetField(ref _isHidden, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public RepoInfo(Repo repo, bool isHidden = false, string notes = "")
        {
            Repo = repo;
            Notes = notes;
            IsHidden = isHidden;
        }

        public RepoInfo()
        {
            Repo = null;
            Notes = "";
            IsHidden = false;
        }
    }
}
