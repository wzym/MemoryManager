#nullable enable
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LazyPersonality.Domain;

namespace ViewModels.UnitViewModels
{
    public class EditedUnitViewModel : IUnitViewModel, IDataErrorInfo, INotifyPropertyChanged
    {
        public string Error => this[""];

        public string this[string columnName]
        {
            get
            {
                if (string.IsNullOrEmpty(Content)) return "Контент должен что-то содержать.";
                return string.Empty;
            }
        }

        public string Content { get => _content; set { _content = value; OnPropertyChanged(nameof(Content)); } }
        private string _content = string.Empty;

        public string Link { get => _link; set { _link = value; OnPropertyChanged(nameof(Link)); } }
        private string _link = string.Empty;
        
        public MemoryUnit ExtractUnit()
        {
            return new()
            {
                Content = Content,
                Link = Link,
                CreationDate = DateTime.Now
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrEmpty(propertyName)) return;
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}