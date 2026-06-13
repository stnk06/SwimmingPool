using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SwimmingPool.Models
{
    public class Class : INotifyPropertyChanged
    {
        private int _classId;
        private int _activityTypeId;
        private int _trainerId;
        private int _poolId;
        private DateTime _startTime;
        private DateTime _endTime;
        private int _maxParticipants = 10; // Значение по умолчанию

        // Поля для отображения
        private string _activityTypeName;
        private string _trainerName;
        private string _poolName;

        public int ClassId
        {
            get => _classId;
            set { _classId = value; OnPropertyChanged(); }
        }

        public int ActivityTypeId
        {
            get => _activityTypeId;
            set { _activityTypeId = value; OnPropertyChanged(); }
        }

        public int TrainerId
        {
            get => _trainerId;
            set { _trainerId = value; OnPropertyChanged(); }
        }

        public int PoolId
        {
            get => _poolId;
            set { _poolId = value; OnPropertyChanged(); }
        }

        public DateTime StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(); }
        }

        public DateTime EndTime
        {
            get => _endTime;
            set { _endTime = value; OnPropertyChanged(); }
        }

        public int MaxParticipants
        {
            get => _maxParticipants;
            set { _maxParticipants = value; OnPropertyChanged(); }
        }

        // --- Свойства только для чтения/отображения в UI ---
        public string ActivityTypeName
        {
            get => _activityTypeName;
            set { _activityTypeName = value; OnPropertyChanged(); }
        }

        public string TrainerName
        {
            get => _trainerName;
            set { _trainerName = value; OnPropertyChanged(); }
        }

        public string PoolName
        {
            get => _poolName;
            set { _poolName = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}