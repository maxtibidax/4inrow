// ConnectFour/Models/UserGameData.cs
using System.Collections.Generic;

namespace ConnectFour.Logic.Models
{
    public class UserGameData
    {
        // Для FieldsMap: сохраняем только ID игрока в каждой ячейке.
        // Список из 42 элементов, представляющий доску (7 столбцов * 6 рядов).
        // Индексация может быть: элемент = column * Game.GAME_ROWS_FOR_EACH_COLUMN + row
        // или наоборот, главное - консистентность при сохранении/загрузке.
        // Давай для простоты сделаем как в GameBoardMapper: плоский список, где элементы идут по столбцам.
        public List<int> BoardState { get; set; } // ID игрока (0 - пусто, 1 - желтый, 2 - красный)
        public int CurrentPlayer { get; set; }
        public int TurnsCount { get; set; }
        public List<int> FullColumns { get; set; }

        // FieldsToCheck может быть сложным для прямой сериализации и восстановления без точного контекста игры.
        // При загрузке игры, если доска не пустая, можно его пересчитать или упростить логику.
        // Пока оставим, но возможно, его не придется использовать при загрузке, а пересоздавать.
        public List<SerializableKeyValuePair<int, int>> FieldsToCheck { get; set; }

        public int WinnerId { get; set; } // 0 - нет, 1 - желтый, 2 - красный, 3 - ничья
        public SerializableKeyValuePair<int, int>[] WinningFieldsCoords { get; set; } // Может быть null

        // Общий счет побед
        public int TotalScoreYellow { get; set; }
        public int TotalScoreRed { get; set; }

        public UserGameData()
        {
            BoardState = new List<int>(ConnectFour.Logic.Game.GAME_COLUMNS * ConnectFour.Logic.Game.GAME_ROWS_FOR_EACH_COLUMN);
            FullColumns = new List<int>();
            FieldsToCheck = new List<SerializableKeyValuePair<int, int>>();
        }
    }

    // Вспомогательный класс для сериализации KeyValuePair, если стандартный не сработает с System.Text.Json в некоторых контекстах
    // System.Text.Json обычно хорошо справляется с KeyValuePair<TKey, TValue>, если TKey и TValue простые.
    // Но для большей надежности можно использовать такой:
    public class SerializableKeyValuePair<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }

        public SerializableKeyValuePair() { } // Для десериализации

        public SerializableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator KeyValuePair<TKey, TValue>(SerializableKeyValuePair<TKey, TValue> skvp)
        {
            return new KeyValuePair<TKey, TValue>(skvp.Key, skvp.Value);
        }

        public static implicit operator SerializableKeyValuePair<TKey, TValue>(KeyValuePair<TKey, TValue> kvp)
        {
            return new SerializableKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value);
        }
    }
}