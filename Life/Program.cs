using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.IO;

namespace cli_life
{
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }

    public class BoardSettings
    {
        public int Width
        { 
            get; 
            set;
        }
        public int Height
        { 
            get;
            set;
        }
        public int CellSize
        { 
            get;
            set;
        }
        public double LiveDensity
        {
            get;
            set;
        }

        public BoardSettings(int width, int height, int cellSize, double liveDensity)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
            LiveDensity = liveDensity;
        }

        public BoardSettings()
        {
            Width = 0;
            Height = 0;
            CellSize = 0;
            LiveDensity = 0;
        }

        public BoardSettings(BoardSettings boardSettings)
        {
            Width = boardSettings.Width;
            Height = boardSettings.Height;
            CellSize = boardSettings.CellSize;
            LiveDensity = boardSettings.LiveDensity;
        }

        public static void writeToFile(string filename, BoardSettings boardSettings)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(boardSettings, options);
            File.WriteAllText(filename, jsonString);
        }
    }

    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(liveDensity);
        }

        public Board(BoardSettings boardSettings) : this(boardSettings.Width, boardSettings.Height, boardSettings.CellSize, boardSettings.LiveDensity)
        {
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
    }
    class Program
    {
        static Board board;
        static private void Reset()
        {
            string filename = "BoardSettings.json";
            BoardSettings boardSettings = new BoardSettings(50, 20, 1, 0.5);

            if (File.Exists(filename))
                boardSettings = new BoardSettings(JsonSerializer.Deserialize<BoardSettings>(File.ReadAllText(filename)));
            else
                BoardSettings.writeToFile(filename, boardSettings);

            board = new Board(boardSettings);
        }
        static void Render()
        {
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)   
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        Console.Write('*');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
        }
        static void Main(string[] args)
        {
            Reset();
            while(true)
            {
                Console.Clear();
                Render();
                board.Advance();
                Thread.Sleep(500);
            }
        }
    }
}