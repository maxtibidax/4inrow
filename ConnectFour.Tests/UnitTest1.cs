using System.Collections.Generic;
using System.Diagnostics;
using ConnectFour.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConnectFour.Tests
{
    [TestClass]
    public class UnitTest1
    {
        private int maxColumns = Game.GAME_COLUMNS;
        private int maxRows = Game.GAME_ROWS_FOR_EACH_COLUMN;
        private int defaultStartingPlayer = Game.DEFAULT_STARTING_PLAYER;

        [TestMethod]
        public void DefTValCheck()
        {
            //Arrange
            var g = new Game();

            //Act
            var fm = g.FieldsMap;

            //Check
            ShowFields(fm);
        }

        [TestMethod]
        public void BoardColorCheck()
        {
            //Arrange
            var g = new Game();

            //Act
            var fm = g.FieldsMap;

            //Check
            ShowFieldsColors(fm);
        }

        [TestMethod]
        public void SimulatePinAddingTest()
        {
            //Arrange
            var gm = new Game();

            //Act
            var fm = gm.FieldsMap;
            gm.AddPin(2); gm.AddPin(0); gm.AddPin(6); gm.AddPin(2);
            gm.AddPin(1); gm.AddPin(3); gm.AddPin(6); gm.AddPin(1);
            gm.AddPin(5); gm.AddPin(0); gm.AddPin(2); gm.AddPin(0);

            //Check
            ShowFieldsColors(fm);
        }

        [TestMethod]
        public void WinningTest()
        {
            //Arrange
            var gm = new Game();
            int expectedWinner = defaultStartingPlayer;

            //Act
            var fm = gm.FieldsMap;
            gm.AddPin(0); gm.AddPin(1); gm.AddPin(0); gm.AddPin(2); gm.AddPin(0);
            gm.AddPin(3); gm.AddPin(0);

            //Assert & Check
            ShowFieldsColors(fm);
            Assert.AreEqual(expectedWinner, gm.WinnerId);
        }

        [TestMethod]
        public void PinAddBlankTest()
        {
            //Arrange
            var gm = new Game();

            //Act
            var fm = gm.FieldsMap;
            gm.AddPin(2);

            //Check
            ShowFields(fm);
        }

        [TestMethod]
        public void MultiplePinsAddTest()
        {
            //Arrange
            var g = new Game();

            //Act
            var fm = g.FieldsMap;
            g.AddPin(2);
            g.AddPin(1);
            g.AddPin(1);
            g.AddPin(6);
            g.AddPin(1);

            //Check
            ShowFields(fm);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException),
        "This test methods expects pin adding failure and throwing an ArgumentException due to bad value provided in the first atgument")]
        public void PinAddingExceptionTest()
        {
            //Arrange
            var g = new Game();

            //Act
            var fm = g.FieldsMap;
            g.AddPin(maxColumns + 1);

            //Assert - ExceptedException     
        }

        [TestMethod]
        public void FillUpEntireColumnTest()
        {
            //Arrange
            var g = new Game();

            //Act
            var fm = g.FieldsMap;
            for (int i = 0; i < maxColumns - 1; i++)
                g.AddPin(2);

            //Check
            ShowFields(fm);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void AddPinToFullColumnTest()
        {
            //Arrange
            var g = new Game();

            //Act
            var fm = g.FieldsMap;
            for (int i = 0; i < maxColumns + 1; i++)
                g.AddPin(2);

            //Check
            ShowFields(fm);
        }

        private void ShowFields(KeyValuePair<int, bool>[,] gameBoard)
        {
            Debug.Write("     ");
            for (int i = 0; i < 7; i++)
            {
                Debug.Write(i + 1 + "              ");
            }
           Debug.Write("\n");

            int row = 0, column = 0;
            for (row = 0; row < 6; row++)
            {
                for (column = 0; column < 7; column++)
                {
                    Debug.Write($"[{gameBoard[column,row].Key} , {gameBoard[column, row].Value}] ");
                }
                Debug.Write("\n");
            }
        }

        private void ShowFieldsColors(KeyValuePair<int, bool>[,] gameBoard)
        {
            Debug.Write("   ");
            for (int i = 0; i < 7; i++)
            {
                Debug.Write(i + 1 + "          ");
            }
            Debug.Write("\n");
            string color = "def";

            int row = 0, column = 0;
            for (row = 0; row < 6; row++)
            {
                for (column = 0; column < 7; column++)
                {
                    if (gameBoard[column, row].Key == 0)
                        color = "none";
                    if (gameBoard[column, row].Key == 1)
                        color = "yellow";
                    if (gameBoard[column, row].Key == 2)
                        color = "red";
                    Debug.Write($"[{color}] ");
                }
                Debug.Write("\n");
            }
        }
    }
}
