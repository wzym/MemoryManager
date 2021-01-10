#nullable enable
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NonDomainEntities;

namespace ViewModels
{
    public class ControlButtonsViewModel : INotifyPropertyChanged
    {
        #region F4
        public string F4String 
        { get => _f4String; internal set { _f4String = value; OnPropertyChanged(nameof(F4String)); } }
        private string _f4String = AddingModeF4;
        internal const string AddingModeF4 = "Завершить редактирование";
        internal const string ViewModeF4 = "Добавить карточку";

        public RelayCommand F4Command
        {
            get => _f4Command; 
            internal set
            {
                _f4Command = value; 
                OnPropertyChanged(nameof(F4Command));
            }
        }
        private RelayCommand _f4Command = null!;
        #endregion
        
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrEmpty(propertyName)) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}