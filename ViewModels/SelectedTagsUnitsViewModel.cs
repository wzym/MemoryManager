using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ViewModels
{
    public class SelectedTagsUnitsViewModel : INotifyPropertyChanged
    {
        public ICollectionView Units
        {
            get => _units;
            set
            {
                _units = value;
                OnPropertyChanged(nameof(Units));
            }
        }
        private ICollectionView _units = null!;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrEmpty(propertyName)) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}