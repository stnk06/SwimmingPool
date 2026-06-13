using SwimmingPool.Infrastructure;
using SwimmingPool.Models;
using SwimmingPool.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SwimmingPool.Views;

namespace SwimmingPool.ViewModels
{
    public class TrainersViewModel : BaseViewModel
    {
        private readonly TrainerRepository _repository;
        public ObservableCollection<Trainer> Trainers { get; set; }
        private Trainer _selectedTrainer;
        public Trainer SelectedTrainer
        {
            get => _selectedTrainer;
            set { _selectedTrainer = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ExportExcelCommand { get; }

        public TrainersViewModel()
        {
            _repository = new TrainerRepository();
            Trainers = new ObservableCollection<Trainer>();
            LoadData();
            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, CanExecute);
            DeleteCommand = new RelayCommand(Delete, CanExecute);
            ExportExcelCommand = new RelayCommand(ExportExcel);
        }

        private void LoadData()
        {
            Trainers.Clear();
            var trainers = _repository.GetAllTrainers();
            foreach (var trainer in trainers)
            {
                Trainers.Add(trainer);
            }
        }

        private void ExportExcel(object obj)
        {
            var mapping = new Dictionary<string, string>
            {
                { "FullName", "ФИО Тренера" },
                { "Specialization", "Специализация" },
                { "ExperienceYears", "Опыт (лет)" },
                { "ContactInfo", "Контакты" }
            };
            ExcelExportService.ExportToExcel(Trainers.ToList(), mapping, "Тренеры");
        }

        private void Add(object obj)
        {
            var newTrainer = new Trainer();
            var trainerWindow = new TrainerWindow(newTrainer, "Добавление нового тренера");

            if (trainerWindow.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(newTrainer.FullName))
                {
                    MessageBox.Show("Поле 'ФИО' не может быть пустым.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _repository.AddTrainer(trainerWindow.Trainer);
                LoadData();
            }
        }

        private void Edit(object obj)
        {
            if (SelectedTrainer != null)
            {
                var trainerCopy = new Trainer
                {
                    TrainerId = SelectedTrainer.TrainerId,
                    FullName = SelectedTrainer.FullName,
                    Specialization = SelectedTrainer.Specialization,
                    ExperienceYears = SelectedTrainer.ExperienceYears,
                    ContactInfo = SelectedTrainer.ContactInfo
                };
                var trainerWindow = new TrainerWindow(trainerCopy, "Редактирование данных тренера");

                if (trainerWindow.ShowDialog() == true)
                {
                    if (string.IsNullOrWhiteSpace(trainerCopy.FullName))
                    {
                        MessageBox.Show("Поле 'ФИО' не может быть пустым.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    _repository.UpdateTrainer(trainerWindow.Trainer);
                    LoadData();
                }
            }
        }

        private void Delete(object obj)
        {
            if (SelectedTrainer != null)
            {
                if (MessageBox.Show($"Вы уверены, что хотите удалить тренера '{SelectedTrainer.FullName}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _repository.DeleteTrainer(SelectedTrainer.TrainerId);
                    LoadData();
                }
            }
        }

        private bool CanExecute(object obj) => SelectedTrainer != null;
    }
}