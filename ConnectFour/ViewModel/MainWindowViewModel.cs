// ConnectFour/ViewModel/MainWindowViewModel.cs
using ConnectFour.Logic;
using ConnectFour.Logic.Models;   // Для UserGameData
using ConnectFour.Services;  // Для GameDataService
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq; // Для ToList и других LINQ операций, если понадобятся
using System;
using System.IO;
using System.Text;
using System.Collections.Generic; // Для List<User>
using ConnectFour.Models;
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
        private readonly UserService _userService; // Добавим экземпляр UserService
        public ICommand GenerateReportCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand Column1PinAddClick { get; set; }
        public ICommand Column2PinAddClick { get; set; }
        public ICommand Column3PinAddClick { get; set; }
        public ICommand Column4PinAddClick { get; set; }
        public ICommand Column5PinAddClick { get; set; }
        public ICommand Column6PinAddClick { get; set; }
        public ICommand Column7PinAddClick { get; set; }
        public ICommand ResetButtonClick { get; set; } // Кнопка "New Game"
        public ICommand LogoutCommand { get; private set; }

        // Событие для сигнализации о выходе пользователя
        public event EventHandler UserLoggedOut;

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
            _userService = new UserService(); // Инициализируем UserService

            Column1PinAddClick = new RelayCommand(Column1Click, CanMakeMove);
            Column2PinAddClick = new RelayCommand(Column2Click, CanMakeMove);
            Column3PinAddClick = new RelayCommand(Column3Click, CanMakeMove);
            Column4PinAddClick = new RelayCommand(Column4Click, CanMakeMove);
            Column5PinAddClick = new RelayCommand(Column5Click, CanMakeMove);
            Column6PinAddClick = new RelayCommand(Column6Click, CanMakeMove);
            Column7PinAddClick = new RelayCommand(Column7Click, CanMakeMove);
            ResetButtonClick = new RelayCommand(ResetGameClick, o => true); // Кнопка "New Game"
            LogoutCommand = new RelayCommand(ExecuteLogout, CanLogout);
            GenerateReportCommand = new RelayCommand(ExecuteGenerateReport, CanGenerateReport);

            LoadGameOrCreateNew();
        }
        private bool CanGenerateReport(object parameter)
        {
            return true; // Отчет всегда можно сгенерировать
        }

        private void ExecuteGenerateReport(object parameter)
        {
            try
            {
                // 1. Получаем всех пользователей
                // Нам нужен метод в UserService, который вернет всех пользователей.
                // Если его нет, придется добавить. Предположим, он есть или мы его добавим.
                List<User> allUsers = _userService.GetAllUsers(); // Предполагаемое имя метода

                if (allUsers == null || !allUsers.Any())
                {
                    System.Windows.MessageBox.Show("Нет зарегистрированных пользователей для создания отчета.", "Информация", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    return;
                }

                StringBuilder htmlBuilder = new StringBuilder();

                // Заголовок HTML
                htmlBuilder.AppendLine("<!DOCTYPE html>");
                htmlBuilder.AppendLine("<html lang=\"ru\">");
                htmlBuilder.AppendLine("<head>");
                htmlBuilder.AppendLine("    <meta charset=\"UTF-F-8\">");
                htmlBuilder.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                htmlBuilder.AppendLine("    <title>Отчет по игре ConnectFour</title>");
                htmlBuilder.AppendLine("    <style>");
                htmlBuilder.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; background-color: #f4f4f4; color: #333; }");
                htmlBuilder.AppendLine("        h1 { color: #333; text-align: center; }");
                htmlBuilder.AppendLine("        table { width: 80%; margin: 20px auto; border-collapse: collapse; box-shadow: 0 0 10px rgba(0,0,0,0.1); background-color: #fff; }");
                htmlBuilder.AppendLine("        th, td { padding: 12px 15px; text-align: left; border-bottom: 1px solid #ddd; }");
                htmlBuilder.AppendLine("        th { background-color: #007bff; color: white; }");
                htmlBuilder.AppendLine("        tr:nth-child(even) { background-color: #f2f2f2; }");
                htmlBuilder.AppendLine("        tr:hover { background-color: #e9e9e9; }");
                htmlBuilder.AppendLine("    </style>");
                htmlBuilder.AppendLine("</head>");
                htmlBuilder.AppendLine("<body>");
                htmlBuilder.AppendLine("    <h1>Отчет по игре ConnectFour</h1>");
                htmlBuilder.AppendLine("    <table>");
                htmlBuilder.AppendLine("        <thead>");
                htmlBuilder.AppendLine("            <tr>");
                htmlBuilder.AppendLine("                <th>Пользователь</th>");
                htmlBuilder.AppendLine("                <th>Побед Желтых</th>");
                htmlBuilder.AppendLine("                <th>Побед Красных</th>");
                htmlBuilder.AppendLine("                <th>Всего игр сыграно (победы)</th>");
                htmlBuilder.AppendLine("            </tr>");
                htmlBuilder.AppendLine("        </thead>");
                htmlBuilder.AppendLine("        <tbody>");

                int grandTotalYellowWins = 0;
                int grandTotalRedWins = 0;
                int grandTotalGamesPlayed = 0;

                foreach (var user in allUsers)
                {
                    UserGameData gameData = _gameDataService.LoadGameData(user.Username);
                    int userYellowWins = 0;
                    int userRedWins = 0;
                    int userGamesPlayed = 0;

                    if (gameData != null)
                    {
                        userYellowWins = gameData.TotalScoreYellow;
                        userRedWins = gameData.TotalScoreRed;
                        userGamesPlayed = userYellowWins + userRedWins; // Считаем игры по общему количеству побед

                        grandTotalYellowWins += userYellowWins;
                        grandTotalRedWins += userRedWins;
                        grandTotalGamesPlayed += userGamesPlayed;
                    }

                    htmlBuilder.AppendLine("            <tr>");
                    htmlBuilder.AppendLine($"                <td>{System.Security.SecurityElement.Escape(user.Username)}</td>"); // Экранируем имя пользователя
                    htmlBuilder.AppendLine($"                <td>{userYellowWins}</td>");
                    htmlBuilder.AppendLine($"                <td>{userRedWins}</td>");
                    htmlBuilder.AppendLine($"                <td>{userGamesPlayed}</td>");
                    htmlBuilder.AppendLine("            </tr>");
                }

                htmlBuilder.AppendLine("        </tbody>");
                // Строка с общими итогами
                htmlBuilder.AppendLine("        <tfoot>");
                htmlBuilder.AppendLine("            <tr style=\"font-weight: bold; background-color: #dcdcdc;\">");
                htmlBuilder.AppendLine("                <td>Общий итог:</td>");
                htmlBuilder.AppendLine($"                <td>{grandTotalYellowWins}</td>");
                htmlBuilder.AppendLine($"                <td>{grandTotalRedWins}</td>");
                htmlBuilder.AppendLine($"                <td>{grandTotalGamesPlayed}</td>");
                htmlBuilder.AppendLine("            </tr>");
                htmlBuilder.AppendLine("        </tfoot>");
                htmlBuilder.AppendLine("    </table>");
                htmlBuilder.AppendLine("</body>");
                htmlBuilder.AppendLine("</html>");

                // 3. Сохраняем HTML в файл
                string reportFilePath = "ConnectFour_Report.html"; // Файл будет в папке с exe
                File.WriteAllText(reportFilePath, htmlBuilder.ToString());

                System.Windows.MessageBox.Show($"Отчет успешно создан и сохранен в файле:\n{Path.GetFullPath(reportFilePath)}", "Отчет создан", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                // (Опционально) Попытка открыть файл в браузере по умолчанию
                try
                {
                    // Используем Process.Start с UseShellExecute = true для открытия файла ассоциированным приложением
                    var processStartInfo = new System.Diagnostics.ProcessStartInfo(Path.GetFullPath(reportFilePath))
                    {
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(processStartInfo);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Не удалось открыть отчет: {ex.Message}");
                    // Можно показать сообщение пользователю, что файл не удалось открыть автоматически
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при создании отчета: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        private bool CanLogout(object parameter)
        {
            return true; // Всегда можно выйти
        }
        private void ExecuteLogout(object parameter)
        {
            // Сначала можно сохранить текущее состояние игры, если это еще не сделано
            // (хотя SaveCurrentGameData() вызывается после каждого хода и при завершении)
            // SaveCurrentGameData(); // Раскомментируй, если считаешь нужным дополнительное сохранение перед выходом

            // Вызываем событие, чтобы App.xaml.cs мог на него отреагировать
            UserLoggedOut?.Invoke(this, EventArgs.Empty);
        }
        // MainWindowViewModel.cs
        private bool CanMakeMove(object parameter)
        {
            if (_game.WinnerId != 0)
            {
                return false; // Игра окончена, нельзя ходить
            }

            if (parameter == null)
            {
                // Этого не должно происходить для команд колонок, если CommandParameter задан.
                // Если это какая-то другая команда, использующая этот же CanExecute, то можно вернуть true.
                // Для кнопок колонок, если параметр не пришел, это ошибка.
                // Для отладки можно вернуть true, чтобы кнопки были активны.
                System.Diagnostics.Debug.WriteLine("CanMakeMove: parameter is null!");
                return true; // Временно для отладки, если кнопки неактивны
            }

            if (int.TryParse(parameter.ToString(), out int columnNumber))
            {
                // Проверяем, что номер колонки в допустимых пределах (на всякий случай)
                if (columnNumber < 0 || columnNumber >= Game.GAME_COLUMNS)
                {
                    System.Diagnostics.Debug.WriteLine($"CanMakeMove: Invalid column number {columnNumber}");
                    return false;
                }
                return !_game.FullColumns.Contains(columnNumber);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"CanMakeMove: Failed to parse parameter '{parameter}' to int.");
                return false; // Не смогли распознать параметр как номер колонки
            }
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