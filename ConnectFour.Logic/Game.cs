using System;
using System.Collections.Generic;
using System.Windows;

namespace ConnectFour.Logic
{
    public class Game
    {
        public const int GAME_COLUMNS = 7;
        public const int GAME_ROWS_FOR_EACH_COLUMN = 6;
        public const int DEFAULT_STARTING_PLAYER = 1;

        // PLAYER 1 - YELLOW
        // PLAYER 2 - RED

        private List<int> _fullColumns = new List<int>();
        private List<KeyValuePair<int, int>> _fieldsToCheck = new List<KeyValuePair<int, int>>();
        private int _currentPlayer;
        private int _turnsCount;
        private int _winnerId = 0;
        private int[] _score = new int[2];
        private KeyValuePair<int, bool>[,] _fieldsMap = new KeyValuePair<int, bool>[GAME_COLUMNS, GAME_ROWS_FOR_EACH_COLUMN];
        private KeyValuePair<int, int>[] _winningFieldsCoords = null;

        public KeyValuePair<int, bool>[,] FieldsMap { get => _fieldsMap; set => _fieldsMap = value; }

        public List<int> FullColumns { get => _fullColumns; set => _fullColumns = value; }

        public int CurrentPlayer { get => _currentPlayer; set => _currentPlayer = value; }

        public int TurnsCount { get => _turnsCount; set => _turnsCount = value; }

        public List<KeyValuePair<int,int>> FieldsToCheck { get => _fieldsToCheck; set => _fieldsToCheck = value; }

        public bool WinnerChanged = false;

        public int WinnerId
        {
            get => _winnerId;
            set
            {
                WinnerChanged = true;
                _winnerId = value;
                switch(value)
                {
                    case 1:
                        Score[0]++;
                        break;
                    case 2:
                        Score[1]++;
                        break;
                    case 3:
                        break;
                }
            }
        }

        public KeyValuePair<int, int>[] WinningFieldsCoords { get => _winningFieldsCoords; set => _winningFieldsCoords = value; }

        //Score[1] -> Yellow player points
        //Score[2] -> Red player points
        public int[] Score { get => _score; set => _score = value; }

        public Game()
        {
            TurnsCount = 0;
            CurrentPlayer = DEFAULT_STARTING_PLAYER;
        }

        public void AddPin(int column)
        {
            if (column < 0 || column > GAME_COLUMNS - 1)
                throw new ArgumentException("Bad column id provided");

            for(int i = 0; i < GAME_ROWS_FOR_EACH_COLUMN; i++)
            {
                //Checks if column is not full (will be guarded by the user gui, but an Exception needs to be thrown)
                if (FieldsMap[column, 0].Value == true)
                    throw new ArgumentException("You tried to add a pin to a full column!");

                //When there are no pins in current column
                if(FieldsMap[column, GAME_ROWS_FOR_EACH_COLUMN - 1].Value == false)
                {
                    FieldsMap[column, GAME_ROWS_FOR_EACH_COLUMN - 1] = new KeyValuePair<int, bool>(CurrentPlayer, true);
                    FieldsToCheck.Add(new KeyValuePair<int, int>(column, GAME_ROWS_FOR_EACH_COLUMN - 1));
                    break;
                }

                if (FieldsMap[column, i].Value == false)
                {
                    continue;
                }
                else
                {
                    // var na = new KeyValuePair<int, bool>[GAME_COLUMNS, GAME_ROWS_FOR_EACH_COLUMN];
                    FieldsMap[column, i - 1] = new KeyValuePair<int, bool>(CurrentPlayer, true);
                    FieldsToCheck.Add(new KeyValuePair<int, int>(column, i - 1));
                    if (i == 1)
                        FullColumns.Add(column);
                    break;
                }
            }

            SwapPlayerAndNextTurn();

            if(TurnsCount >= 7)
            CheckWinningState();

            if (TurnsCount == GAME_ROWS_FOR_EACH_COLUMN * GAME_COLUMNS)
                WinnerId = 3;
        }       

        private void CheckWinningState()
        {
            //StringBuilder s = new StringBuilder();
            foreach (var item in FieldsToCheck)
            {
                int currentlyCheckedPinColor = FieldsMap[item.Key, item.Value].Key;
               // s.Append("PinColor: " + currentlyCheckedPinColor + " Column: " + item.Key + " Row: " + item.Value);
                // ->
                if(item.Key < GAME_COLUMNS - 3)
                    for (int i = 1; i < 4; i++)
                    {
                        if ((FieldsMap[item.Key + i, item.Value].Value == true && FieldsMap[item.Key + i, item.Value].Key == currentlyCheckedPinColor))
                        {
                            if (i == 3)
                            {
                                WinnerId = currentlyCheckedPinColor;
                                var wfc = new KeyValuePair<int, int>[4];
                                for (int t = 0; t < 4; t++)
                                    wfc[t] = new KeyValuePair<int, int>(item.Key + t, item.Value);
                                WinningFieldsCoords = wfc;
                                return;
                            }
                            continue;
                        }
                        else
                            break;
                    }
                // \|/
                if (item.Value < GAME_ROWS_FOR_EACH_COLUMN - 3)
                    for(int i = 1; i < 4; i++)
                    {
                        if ((FieldsMap[item.Key, item.Value + i].Value == true && FieldsMap[item.Key, item.Value + i].Key == currentlyCheckedPinColor))
                        {
                            if (i == 3)
                            {
                                WinnerId = currentlyCheckedPinColor;
                                var wfc = new KeyValuePair<int, int>[4];
                                for (int t = 0; t < 4; t++)
                                    wfc[t] = new KeyValuePair<int, int>(item.Key, item.Value + t);
                                WinningFieldsCoords = wfc;
                                return;
                            }
                            continue;
                        }
                        else
                            break;
                    }

                // \|\
                if(item.Key < GAME_COLUMNS - 3 && item.Value < GAME_ROWS_FOR_EACH_COLUMN - 3)
                    for (int i = 1; i < 4; i++)
                    {
                        if ((FieldsMap[item.Key + i, item.Value + i].Value == true && FieldsMap[item.Key + i, item.Value + i].Key == currentlyCheckedPinColor))
                        {
                            if (i == 3)
                            {
                                WinnerId = currentlyCheckedPinColor;
                                var wfc = new KeyValuePair<int, int>[4];
                                for (int t = 0; t < 4; t++)
                                    wfc[t] = new KeyValuePair<int, int>(item.Key + t, item.Value + t);
                                WinningFieldsCoords = wfc;
                                return;
                            }
                            continue;
                        }
                        else
                            break;
                    }

                // / bottom left
                if (item.Key > 2 && item.Value < GAME_ROWS_FOR_EACH_COLUMN - 3)
                    for (int i = 1; i < 4; i++)
                    {
                        if ((FieldsMap[item.Key - i, item.Value + i].Value == true && FieldsMap[item.Key - i, item.Value + i].Key == currentlyCheckedPinColor))
                        {
                            if (i == 3)
                            {
                                WinnerId = currentlyCheckedPinColor;
                                var wfc = new KeyValuePair<int, int>[4];
                                for (int t = 0; t < 4; t++)
                                    wfc[t] = new KeyValuePair<int, int>(item.Key - t, item.Value + t);
                                WinningFieldsCoords = wfc;
                                return;
                            }
                            continue;
                        }
                        else
                            break;
                    }
            }
           // MessageBox.Show(s.ToString());
        }

        private void SwapPlayerAndNextTurn()
        {
            TurnsCount++;
            switch(CurrentPlayer)
            {
                case 1:
                    CurrentPlayer = 2;
                    break;
                case 2:
                    CurrentPlayer = 1;
                    break;
            }
            if (TurnsCount == 42)
                NewGame();
            //skip
        }

        //Resets the game
        public void Reset()
        {
            _turnsCount = 0;
            _currentPlayer = DEFAULT_STARTING_PLAYER;
            _winnerId = 0;
            _fieldsMap = new KeyValuePair<int, bool>[GAME_COLUMNS, GAME_ROWS_FOR_EACH_COLUMN];
            _winningFieldsCoords = new KeyValuePair<int, int>[4];
            _fullColumns = new List<int>();
            _fieldsToCheck = new List<KeyValuePair<int, int>>();
            WinnerChanged = false;
        }

        public void NewGame()
        {
            _turnsCount = 0;
            _currentPlayer = DEFAULT_STARTING_PLAYER;
            _winnerId = 0;
            _fieldsMap = new KeyValuePair<int, bool>[GAME_COLUMNS, GAME_ROWS_FOR_EACH_COLUMN];
            _winningFieldsCoords = new KeyValuePair<int, int>[4];
            _fullColumns = new List<int>();
            _fieldsToCheck = new List<KeyValuePair<int, int>>();
            WinnerChanged = false;
            _score = new int[2];
        }
    }
}
