#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using LazyPersonality.Domain;
using LazyPersonality.EntitiesStorage;
using NonDomainEntities;
using ViewModels.UnitViewModels;

namespace ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Storage _storage;

        public ControlButtonsViewModel ButtonsViewModel { get; private set; }
        public SelectedTagsUnitsViewModel SearchResultViewModel { get; private set; }
        public CommonSearchViewModel SearchViewModel { get; private set; }
        public IUnitViewModel UnitViewModel 
        { 
            get => _unitViewModel;
            private set { _unitViewModel = value; OnPropertyChanged(nameof(UnitViewModel)); }
        }
        private IUnitViewModel _unitViewModel = null!;

        private MemoryUnit CurrentUnit { get; set; } = null!;

        public MainViewModel()
        {
            _storage = new Storage();
            Task.Run(() => _storage.InitializeAsync())
                .ContinueWith(_ => SetSelectedUnits(_storage.AllUnits));

            ButtonsViewModel = new ControlButtonsViewModel();
            SearchResultViewModel = new SelectedTagsUnitsViewModel();
            SearchViewModel = new CommonSearchViewModel();
            ActivateCardAddingMode();
        }

        private void ActivateCardAddingMode()
        {
            var editedUnitVm = new EditedUnitViewModel();
            UnitViewModel = editedUnitVm;
            ButtonsViewModel.F4Command = 
                new RelayCommand(FinishEditingAndAdd, () => string.IsNullOrEmpty(editedUnitVm.Error));
            ButtonsViewModel.F4String = ControlButtonsViewModel.AddingModeF4;
            editedUnitVm.PropertyChanged += (_, _) => ButtonsViewModel.F4Command.RaiseCanExecuteChanged();
        }

        private async void FinishEditingAndAdd()
        {
            CurrentUnit = await _storage.AddUnit(UnitViewModel.ExtractUnit());
            SetSelectedUnits(_storage.AllUnits);
            ActivateCardViewMode();
        }

        private void ActivateCardViewMode()
        {
            var unitVm = new UnitViewModel(CurrentUnit);
            UnitViewModel = unitVm;
            ButtonsViewModel.F4Command = new RelayCommand(ActivateCardAddingMode);
            ButtonsViewModel.F4String = ControlButtonsViewModel.ViewModeF4;
        }

        private void SetSelectedUnits(IEnumerable<MemoryUnit> units)
        {
            //SearchResultViewModel.Units = new ObservableCollection<MemoryUnit>(units);
            SearchResultViewModel.Units = CollectionViewSource.GetDefaultView(units);
            SearchResultViewModel.Units.CurrentChanged += (_, _) => ProcessSelectionChanged();
        }

        private void ProcessSelectionChanged()
        {
            CurrentUnit = (MemoryUnit) SearchResultViewModel.Units.CurrentItem;
            ActivateCardViewMode();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrEmpty(propertyName)) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}