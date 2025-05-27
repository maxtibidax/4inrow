// ConnectFour/ViewModel/MainWindowViewModel.cs
using ConnectFour.Logic;
using ConnectFour.Logic.Models;   // Для UserGameData
using ConnectFour.Services;  // Для GameDataService
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq; // Для ToList и других LINQ операций, если понадобятся

namespace ConnectFour.ViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private Game _game = new Game();
        private GameBoardMapper _mapper = new GameBoardMapper();
        private string[] mappedLocs = new string[Game.GAME_COLUMNS * Game.GAME_ROWS_FOR_EACH_COLUMN];
        private int notifiedWinner = 0; // Для отображения победителя в UI, если нужно

        // Общий счет игры
        private int _totalScoreYellow;
        private int _totalScoreRed;

        private string[] mappedDiscardedArrows = null;
        private string currentTurn;

        private readonly string _username;
        private readonly GameDataService _gameDataService;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand Column1PinAddClick { get; set; }
        public ICommand Column2PinAddClick { get; set; }
        public ICommand Column3PinAddClick { get; set; }
        public ICommand Column4PinAddClick { get; set; }
        public ICommand Column5PinAddClick { get; set; }
        public ICommand Column6PinAddClick { get; set; }
        public ICommand Column7PinAddClick { get; set; }
        public ICommand ResetButtonClick { get; set; } // Кнопка "New Game"

        public string[] MappedLocs
        {
            get => mappedLocs;
            set { mappedLocs = value; NotifyPropertyChanged(nameof(MappedLocs)); }
        }

        public string[] MappedDiscardedArrows
        {
            get => mappedDiscardedArrows;
            set { mappedDiscardedArrows = value; NotifyPropertyChanged(nameof(MappedDiscardedArrows)); }
        }

        public string CurrentTurn
        {
            get => currentTurn;
            set { currentTurn = value; NotifyPropertyChanged(nameof(CurrentTurn)); }
        }

        public int NotifiedWinner // Если используется для отображения сообщения о победителе
        {
            get => notifiedWinner;
            set { notifiedWinner = value; NotifyPropertyChanged(nameof(NotifiedWinner)); }
        }

        public int TotalScoreYellow
        {
            get => _totalScoreYellow;
            set { _totalScoreYellow = value; NotifyPropertyChanged(nameof(TotalScoreYellow)); }
        }

        public int TotalScoreRed
        {
            get => _totalScoreRed;
            set { _totalScoreRed = value; NotifyPropertyChanged(nameof(TotalScoreRed)); }
        }

        // Конструктор принимает имя пользователя
        public MainWindowViewModel(string username)
        {
            _username = username;
            _gameDataService = new GameDataService();

            Column1PinAddClick = new RelayCommand(Column1Click, CanMakeMove);
            Column2PinAddClick = new RelayCommand(Column2Click, CanMakeMove);
            Column3PinAddClick = new RelayCommand(Column3Click, CanMakeMove);
            Column4PinAddClick = new RelayCommand(Column4Click, CanMakeMove);
            Column5PinAddClick = new RelayCommand(Column5Click, CanMakeMove);
            Column6PinAddClick = new RelayCommand(Column6Click, CanMakeMove);
            Column7PinAddClick = new RelayCommand(Column7Click, CanMakeMove);
            ResetButtonClick = new RelayCommand(ResetGameClick, o => true); // Кнопка "New Game"

            LoadGameOrCreateNew();
        }

        private bool CanMakeMove(object parameter)
        {
            // Проверяем не только окончание игры, но и полность конкретной колонки
            if (_game.WinnerId != 0) return false;

            // Получаем номер колонки из команды
            int column = GetColumnFromCommand(parameter);
            if (column == -1) return false;

            return !_game.FullColumns.Contains(column);
        }

        private int GetColumnFromCommand(object parameter)
        {
            // Определяем номер колонки по команде
            if (parameter is ICommand command)
            {
                if (command == Column1PinAddClick) return 0;
                if (command == Column2PinAddClick) return 1;
                if (command == Column3PinAddClick) return 2;
                if (command == Column4PinAddClick) return 3;
                if (command == Column5PinAddClick) return 4;
                if (command == Column6PinAddClick) return 5;
                if (command == Column7PinAddClick) return 6;
            }
            return -1;
        }

        private void LoadGameOrCreateNew()
        {
            UserGameData loadedData = _gameDataService.LoadGameData(_username);
            if (loadedData != null)
            {
                _game.SetStateFromLoad(loadedData);
                TotalScoreYellow = loadedData.TotalScoreYellow;
                TotalScoreRed = loadedData.TotalScoreRed;
                NotifiedWinner = _game.WinnerId; // Если игра была завершена, отобразить это
            }
            else
            {
                // Нет сохраненных данных, начинаем новую игру и инициализируем счет
                _game.NewGame(); // NewGame теперь не сбрасывает общий счет
                TotalScoreYellow = 0;
                TotalScoreRed = 0;
                NotifiedWinner = 0;
            }
            // Обновляем все отображения в UI
            RefreshGameBoardDisplay();
            UpdateCommandStates();
        }

        private void SaveCurrentGameData()
        {
            UserGameData dataToSave = _game.GetStateForSave();
            dataToSave.TotalScoreYellow = this.TotalScoreYellow;
            dataToSave.TotalScoreRed = this.TotalScoreRed;
            _gameDataService.SaveGameData(_username, dataToSave);
        }

        // Кнопка "New Game" сбрасывает текущую доску, но не общий счет
        public void ResetGameClick(object o)
        {
            _game.Reset(); // Сбрасывает доску, текущего игрока, но не общий счет
            _mapper.Reset(); // ВАЖНО: сбрасываем также состояние маппера!
            NotifiedWinner = 0;
            RefreshGameBoardDisplay();
            SaveCurrentGameData(); // Сохраняем состояние (пустая доска, тот же счет)
            UpdateCommandStates();
        }

        public void Column1Click(object o) { MakeMove(0); }
        public void Column2Click(object o) { MakeMove(1); }
        public void Column3Click(object o) { MakeMove(2); }
        public void Column4Click(object o) { MakeMove(3); }
        public void Column5Click(object o) { MakeMove(4); }
        public void Column6Click(object o) { MakeMove(5); }
        public void Column7Click(object o) { MakeMove(6); }

        private void MakeMove(int column)
        {
            if (_game.WinnerId != 0) return; // Нельзя ходить, если игра окончена

            try
            {
                _game.AddPin(column); // Логика игры, включая CheckWinningState
                ProcessGameUpdate();
            }
            catch (System.ArgumentException ex)
            {
                // Например, колонка полна
                System.Windows.MessageBox.Show(ex.Message, "Ошибка хода");
            }
        }

        private async void ProcessGameUpdate()
        {
            // ИСПРАВЛЕНИЕ: проверяем WinnerChanged сразу после AddPin
            if (_game.WinnerChanged && _game.WinnerId != 0) // Если победитель определился ТОЛЬКО ЧТО
            {
                NotifiedWinner = _game.WinnerId; // Устанавливаем для UI

                // ИСПРАВЛЕНИЕ: обновляем счет сразу после определения победителя
                if (_game.WinnerId == 1)
                {
                    TotalScoreYellow++;
                    System.Diagnostics.Debug.WriteLine($"Yellow wins! Score: {TotalScoreYellow}");
                }
                else if (_game.WinnerId == 2)
                {
                    TotalScoreRed++;
                    System.Diagnostics.Debug.WriteLine($"Red wins! Score: {TotalScoreRed}");
                }
                // Если WinnerId == 3 (ничья), счет не меняется

                RefreshGameBoardDisplay(true); // Обновить доску с подсветкой выигрыша
                SaveCurrentGameData(); // Сохраняем после определения победителя и обновления счета
                UpdateCommandStates(); // Обновить доступность кнопок

                await Task.Delay(3000); // Показываем выигрышную комбинацию

                _game.Reset(); // Сбрасываем доску для следующей игры
                _mapper.Reset(); // ИСПРАВЛЕНИЕ: сбрасываем также маппер
                NotifiedWinner = 0;
                RefreshGameBoardDisplay(); // Обновляем доску (теперь она пустая)
                SaveCurrentGameData(); // Сохраняем состояние новой игры
                UpdateCommandStates();
            }
            else if (_game.WinnerId == 0) // Игра продолжается
            {
                RefreshGameBoardDisplay();
                SaveCurrentGameData(); // Сохраняем после каждого хода
                UpdateCommandStates();
            }
            else // Игра уже была завершена, просто обновляем отображение (на всякий случай)
            {
                RefreshGameBoardDisplay(true); // Показать выигрыш, если он есть
                UpdateCommandStates();
            }
        }

        // Обновляет состояние UI на основе текущего состояния _game
        private void RefreshGameBoardDisplay(bool highlightWin = false)
        {
            if (highlightWin && _game.WinnerId != 0 && _game.WinnerId != 3)
            {
                _mapper.MapToFileNameWin(_game); // Отображает выигрышную комбинацию
            }
            else
            {
                _mapper.MapToFileName(_game); // Обычное отображение
            }
            _mapper.DiscardFilledColumnIndicators(_game);
            _mapper.UpdateTurnIndicator(_game);

            MappedLocs = _mapper.FileNameMapper.ToArray(); // Создаем копию для избежания проблем с привязкой

            if (_game.WinnerId != 0) // Если игра окончена, скрыть все стрелки
            {
                MappedDiscardedArrows = _mapper.HideArrowIndicators().ToArray();
            }
            else // Иначе, показать/скрыть нужные
            {
                MappedDiscardedArrows = _mapper.ArrowIndicatorControllers.ToArray();
            }
            CurrentTurn = _mapper.CurrentTurn;

            // Принудительно уведомляем об изменении счета
            NotifyPropertyChanged(nameof(TotalScoreYellow));
            NotifyPropertyChanged(nameof(TotalScoreRed));
        }

        // Обновляем CanExecute для команд
        private void UpdateCommandStates()
        {
            (Column1PinAddClick as RelayCommand)?.RaiseCanExecuteChanged();
            (Column2PinAddClick as RelayCommand)?.RaiseCanExecuteChanged();
            (Column3PinAddClick as RelayCommand)?.RaiseCanExecuteChanged();
            (Column4PinAddClick as RelayCommand)?.RaiseCanExecuteChanged();
            (Column5PinAddClick as RelayCommand)?.RaiseCanExecuteChanged();
            (Column6PinAddClick as RelayCommand)?.RaiseCanExecuteChanged();
            (Column7PinAddClick as RelayCommand)?.RaiseCanExecuteChanged();
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}