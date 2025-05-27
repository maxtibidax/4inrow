// ConnectFour.Logic/Game.cs
using System;
using System.Collections.Generic;
using ConnectFour.Logic.Models; // Для UserGameData и SerializableKeyValuePair

namespace ConnectFour.Logic
{
    public class Game
    {
        public const int GAME_COLUMNS = 7;
        public const int GAME_ROWS_FOR_EACH_COLUMN = 6;
        public const int DEFAULT_STARTING_PLAYER = 1; // PLAYER 1 - YELLOW, PLAYER 2 - RED

        private List<int> _fullColumns;
        private List<KeyValuePair<int, int>> _fieldsToCheck;
        private int _currentPlayer;
        private int _turnsCount;
        private int _winnerId; // 0 - нет, 1 - желтый, 2 - красный, 3 - ничья
        private KeyValuePair<int, bool>[,] _fieldsMap;
        private KeyValuePair<int, int>[] _winningFieldsCoords;

        public bool WinnerChanged { get; private set; } // Флаг, что победитель изменился в последнем CheckWinningState

        public KeyValuePair<int, bool>[,] FieldsMap { get => _fieldsMap; private set => _fieldsMap = value; } // Сеттер приватный для контроля
        public List<int> FullColumns { get => _fullColumns; private set => _fullColumns = value; }
        public int CurrentPlayer { get => _currentPlayer; private set => _currentPlayer = value; }
        public int TurnsCount { get => _turnsCount; private set => _turnsCount = value; }
        public List<KeyValuePair<int, int>> FieldsToCheck { get => _fieldsToCheck; private set => _fieldsToCheck = value; }

        public int WinnerId
        {
            get => _winnerId;
            private set // Сеттер приватный, управляется из CheckWinningState или SetStateFromLoad
            {
                if (_winnerId != value) // Только если значение действительно меняется
                {
                    WinnerChanged = true; // Устанавливаем флаг, если победитель определен или изменен
                }
                _winnerId = value;
            }
        }

        public KeyValuePair<int, int>[] WinningFieldsCoords { get => _winningFieldsCoords; private set => _winningFieldsCoords = value; }

        public Game()
        {
            InitializeNewGameState();
        }

        private void InitializeNewGameState()
        {
            _turnsCount = 0;
            _currentPlayer = DEFAULT_STARTING_PLAYER;
            _winnerId = 0;
            _fieldsMap = new KeyValuePair<int, bool>[GAME_COLUMNS, GAME_ROWS_FOR_EACH_COLUMN];
            _winningFieldsCoords = null; // Инициализируем как null, массив создается при нахождении победителя
            _fullColumns = new List<int>();
            _fieldsToCheck = new List<KeyValuePair<int, int>>();
            WinnerChanged = false;
        }

        public void AddPin(int column)
        {
            if (WinnerId != 0)
                throw new InvalidOperationException("Cannot add a pin, the game is already over.");
            if (column < 0 || column >= GAME_COLUMNS)
                throw new ArgumentOutOfRangeException(nameof(column), "Bad column id provided.");
            if (FullColumns.Contains(column))
                throw new ArgumentException("You tried to add a pin to a full column!");

            // Проверка на полноту колонки (если вдруг FullColumns не обновился)
            if (FieldsMap[column, 0].Value == true)
            {
                if (!FullColumns.Contains(column)) FullColumns.Add(column); // На всякий случай обновим
                throw new ArgumentException("You tried to add a pin to a full column (checked by FieldsMap)!");
            }

            int placedRow = -1;
            for (int i = GAME_ROWS_FOR_EACH_COLUMN - 1; i >= 0; i--)
            {
                if (FieldsMap[column, i].Value == false) // Находим первую пустую ячейку снизу
                {
                    FieldsMap[column, i] = new KeyValuePair<int, bool>(CurrentPlayer, true);
                    FieldsToCheck.Add(new KeyValuePair<int, int>(column, i));
                    placedRow = i;
                    if (i == 0 && !FullColumns.Contains(column)) // Если фишка поставлена в самый верхний ряд
                    {
                        FullColumns.Add(column);
                    }
                    break;
                }
            }

            if (placedRow == -1) // Этого не должно произойти, если предыдущие проверки отработали
            {
                throw new InvalidOperationException("Failed to place pin, column might be unexpectedly full.");
            }

            TurnsCount++;
            WinnerChanged = false; // Сбрасываем флаг перед проверкой

            if (TurnsCount >= 7) // Проверять победителя есть смысл минимум после 7 ходов (4 фишки одного + 3 другого)
            {
                CheckWinningState();
            }

            if (WinnerId == 0 && TurnsCount == GAME_ROWS_FOR_EACH_COLUMN * GAME_COLUMNS)
            {
                WinnerId = 3; // Ничья, если все ячейки заполнены и победителя нет
            }

            if (WinnerId == 0) // Если игра не окончена, меняем игрока
            {
                SwapPlayer();
            }
        }

        private void CheckWinningState()
        {
            // Проверяем только вокруг последних добавленных фишек для оптимизации
            // В текущей реализации FieldsToCheck содержит все фишки, что менее оптимально,
            // но для учебного проекта допустимо. Для оптимизации можно проверять только последнюю добавленную фишку.
            // Однако, чтобы не ломать оригинальную логику, оставим как есть, но будем осторожны с WinnerChanged.

            foreach (var item in FieldsToCheck) // В идеале, здесь должен быть только последний ход
            {
                int currentlyCheckedPinColor = FieldsMap[item.Key, item.Value].Key;
                if (currentlyCheckedPinColor == 0) continue; // Пропускаем пустые ячейки (не должно быть в FieldsToCheck)

                // Горизонталь ->
                if (item.Key <= GAME_COLUMNS - 4)
                    if (CheckLine(item.Key, item.Value, 1, 0, currentlyCheckedPinColor)) return;
                // Вертикаль |
                if (item.Value <= GAME_ROWS_FOR_EACH_COLUMN - 4)
                    if (CheckLine(item.Key, item.Value, 0, 1, currentlyCheckedPinColor)) return;
                // Диагональ \ (вниз-вправо)
                if (item.Key <= GAME_COLUMNS - 4 && item.Value <= GAME_ROWS_FOR_EACH_COLUMN - 4)
                    if (CheckLine(item.Key, item.Value, 1, 1, currentlyCheckedPinColor)) return;
                // Диагональ / (вниз-влево)
                if (item.Key >= 3 && item.Value <= GAME_ROWS_FOR_EACH_COLUMN - 4)
                    if (CheckLine(item.Key, item.Value, -1, 1, currentlyCheckedPinColor)) return;
            }
        }

        private bool CheckLine(int startCol, int startRow, int dCol, int dRow, int player)
        {
            for (int i = 1; i < 4; i++)
            {
                if (FieldsMap[startCol + i * dCol, startRow + i * dRow].Key != player)
                    return false;
            }

            WinnerId = player;
            _winningFieldsCoords = new KeyValuePair<int, int>[4];
            for (int t = 0; t < 4; t++)
            {
                _winningFieldsCoords[t] = new KeyValuePair<int, int>(startCol + t * dCol, startRow + t * dRow);
            }
            return true;
        }


        private void SwapPlayer()
        {
            CurrentPlayer = (CurrentPlayer == 1) ? 2 : 1;
        }

        // Сбрасывает текущую доску для новой игры (но не общий счет, который ведется в ViewModel)
        public void Reset()
        {
            InitializeNewGameState();
        }

        // Полный сброс, включая общий счет (если бы он был здесь)
        // Этот метод теперь делает то же, что и Reset, т.к. счет ушел в ViewModel
        public void NewGame()
        {
            InitializeNewGameState();
        }

        // --- Методы для сериализации ---
        public UserGameData GetStateForSave()
        {
            var gameData = new UserGameData
            {
                BoardState = new List<int>(GAME_COLUMNS * GAME_ROWS_FOR_EACH_COLUMN),
                CurrentPlayer = this.CurrentPlayer,
                TurnsCount = this.TurnsCount,
                FullColumns = new List<int>(this.FullColumns),
                FieldsToCheck = this.FieldsToCheck.ConvertAll(kvp => new SerializableKeyValuePair<int, int>(kvp.Key, kvp.Value)),
                WinnerId = this.WinnerId
            };

            if (this.WinningFieldsCoords != null)
            {
                gameData.WinningFieldsCoords = new SerializableKeyValuePair<int, int>[this.WinningFieldsCoords.Length];
                for (int i = 0; i < this.WinningFieldsCoords.Length; i++)
                {
                    gameData.WinningFieldsCoords[i] = new SerializableKeyValuePair<int, int>(this.WinningFieldsCoords[i].Key, this.WinningFieldsCoords[i].Value);
                }
            }
            else
            {
                gameData.WinningFieldsCoords = null;
            }

            for (int col = 0; col < GAME_COLUMNS; col++)
            {
                for (int row = 0; row < GAME_ROWS_FOR_EACH_COLUMN; row++)
                {
                    gameData.BoardState.Add(this.FieldsMap[col, row].Key);
                }
            }
            return gameData;
        }

        public void SetStateFromLoad(UserGameData loadedData)
        {
            InitializeNewGameState(); // Сначала сбросим до чистого состояния

            this.CurrentPlayer = loadedData.CurrentPlayer;
            this.TurnsCount = loadedData.TurnsCount;
            this.FullColumns = new List<int>(loadedData.FullColumns ?? new List<int>()); // Защита от null

            if (loadedData.FieldsToCheck != null)
            {
                this.FieldsToCheck = loadedData.FieldsToCheck.ConvertAll(skvp => new KeyValuePair<int, int>(skvp.Key, skvp.Value));
            }
            else
            {
                this.FieldsToCheck = new List<KeyValuePair<int, int>>();
            }

            // Устанавливаем WinnerId и WinningFieldsCoords напрямую
            this.WinnerId = loadedData.WinnerId;
            if (loadedData.WinningFieldsCoords != null && loadedData.WinnerId != 0 && loadedData.WinnerId != 3) // Если есть победитель и координаты
            {
                this.WinningFieldsCoords = new KeyValuePair<int, int>[loadedData.WinningFieldsCoords.Length];
                for (int i = 0; i < loadedData.WinningFieldsCoords.Length; i++)
                {
                    this.WinningFieldsCoords[i] = new KeyValuePair<int, int>(loadedData.WinningFieldsCoords[i].Key, loadedData.WinningFieldsCoords[i].Value);
                }
            }
            else
            {
                this.WinningFieldsCoords = null;
            }

            // Если WinnerId был установлен, WinnerChanged должен это отразить
            this.WinnerChanged = (this.WinnerId != 0);


            if (loadedData.BoardState != null && loadedData.BoardState.Count == GAME_COLUMNS * GAME_ROWS_FOR_EACH_COLUMN)
            {
                int k = 0;
                for (int col = 0; col < GAME_COLUMNS; col++)
                {
                    for (int row = 0; row < GAME_ROWS_FOR_EACH_COLUMN; row++)
                    {
                        int playerId = loadedData.BoardState[k++];
                        this.FieldsMap[col, row] = new KeyValuePair<int, bool>(playerId, playerId != 0);
                    }
                }
            }

            // Важно: После загрузки, если игра не завершена (WinnerId == 0) и были ходы (TurnsCount > 0),
            // а FieldsToCheck пуст (например, если мы решим его не сохранять или он был null),
            // то его нужно было бы перестроить.
            // Сейчас мы загружаем FieldsToCheck как есть. Если он пуст, а доска нет,
            // то CheckWinningState не отработает корректно для уже существующих фишек.
            // Для более надежного восстановления, если FieldsToCheck пуст, а доска нет, можно его перестроить:
            if (this.TurnsCount > 0 && this.WinnerId == 0 && (this.FieldsToCheck == null || this.FieldsToCheck.Count == 0))
            {
                RebuildFieldsToCheckFromBoard();
            }
        }

        private void RebuildFieldsToCheckFromBoard()
        {
            this.FieldsToCheck.Clear();
            for (int col = 0; col < GAME_COLUMNS; col++)
            {
                for (int row = 0; row < GAME_ROWS_FOR_EACH_COLUMN; row++)
                {
                    if (this.FieldsMap[col, row].Value == true) // Если ячейка занята
                    {
                        this.FieldsToCheck.Add(new KeyValuePair<int, int>(col, row));
                    }
                }
            }
        }
    }
}