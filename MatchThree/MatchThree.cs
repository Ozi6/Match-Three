class MatchThree
{
    private readonly char[] colors = { 'R', 'B', 'G', 'P', 'Y' };
    private char[][] Table;
    private (int, int) currIndex;
    private (int, int) selectedIndex;
    private int points = 0;
    private const int goalPoints = 3000;
    private bool? gameWon = false;
    private bool selected = false;
    private bool isDrawing = false;
    private int time = 60;
    private DateTime lastTimeUpdate;
    Random random = new Random();

    public MatchThree()
    {
        Table = new char[9][];
        for (int i = 0; i < Table.Length; i++)
        {
            Table[i] = new char[9];
            for (int j = 0; j < 9; j++)
                Table[i][j] = InitCheck(i, j);
        }
        currIndex = (0, 0);
        selectedIndex = (10, 10);
    }

    public char InitCheck(int row, int col)
    {
        char selectedColor;
        do
            selectedColor = colors[random.Next(colors.Length)];
        while (!Acceptable(row, col, selectedColor));

        return selectedColor;
    }

    private bool Acceptable(int row, int col, char color)
    {
        if (col >= 2 && Table[row][col - 1] == color && Table[row][col - 2] == color)
            return false;

        if (row >= 2 && Table[row - 1][col] == color && Table[row - 2][col] == color)
            return false;

        return true;
    }

    public void PrintTable()
    {
        if (isDrawing)
            return;

        isDrawing = true;

        Console.Clear();
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                Console.ForegroundColor = GetConsoleColor(Table[i][j]);
                if ((i == currIndex.Item1 && j == currIndex.Item2) || i == selectedIndex.Item1 && j == selectedIndex.Item2)
                    Console.Write("[" + Table[i][j] + "]");
                else
                    Console.Write(" " + Table[i][j] + " ");
            }
            Console.ResetColor();
            Console.WriteLine();
        }
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Points: {points} / {goalPoints}    Time: {time}");
        Console.ResetColor();

        isDrawing = false;
    }

    private ConsoleColor GetConsoleColor(char c)
    {
        return c switch
        {
            'R' => ConsoleColor.Red,
            'G' => ConsoleColor.Green,
            'B' => ConsoleColor.Blue,
            'Y' => ConsoleColor.Yellow,
            'P' => ConsoleColor.Magenta,
            'D' => ConsoleColor.Gray,
            _ => ConsoleColor.White,
        };
    }

    public static void Main()
    {
        MatchThree game = new MatchThree();
        game.PrintTable();

        Console.WriteLine("Press Enter to start the game.");
        Console.ReadLine();

        Thread controls = new Thread(() =>
        {
            while (game.isGameWon() == false)
            {
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.W: game.MoveIndex(-1, 0); break;
                    case ConsoleKey.S: game.MoveIndex(1, 0); break;
                    case ConsoleKey.A: game.MoveIndex(0, -1); break;
                    case ConsoleKey.D: game.MoveIndex(0, 1); break;
                    case ConsoleKey.UpArrow: game.MoveIndex(-1, 0); break;
                    case ConsoleKey.DownArrow: game.MoveIndex(1, 0); break;
                    case ConsoleKey.LeftArrow: game.MoveIndex(0, -1); break;
                    case ConsoleKey.RightArrow: game.MoveIndex(0, 1); break;
                    case ConsoleKey.Enter: game.Select(); break;
                }
            }
        });
        controls.Start();

        game.PrintTable();

        while (game.isGameWon() == false || game.isDrawing)
        {
            game.Update();
        }

        game.PrintTable();

        if (game.isGameWon() == null)
            Console.WriteLine("You could not meet the Goal :(");
        if (game.isGameWon() == true)
            Console.WriteLine("You won!!! :)");
    }

    private bool? isGameWon()
    {
        return gameWon;
    }

    private void MoveIndex(int upDown, int leftRight)
    {
        if (selected)
        {
            (int, int) tempCurrIndex = (currIndex.Item1 + upDown, currIndex.Item2 + leftRight);
            if (tempCurrIndex.Item1 < 0 || tempCurrIndex.Item1 > 8
                || tempCurrIndex.Item2 < 0 || tempCurrIndex.Item2 > 8)
                return;

            int upDownDist = tempCurrIndex.Item1 - selectedIndex.Item1;
            int leftRightDist = tempCurrIndex.Item2 - selectedIndex.Item2;

            if (MathF.Abs(upDownDist) + MathF.Abs(leftRightDist) > 1)
                return;

            currIndex = tempCurrIndex;
            PrintTable();
        }
        else
        {
            (int, int) tempCurrIndex = (currIndex.Item1 + upDown, currIndex.Item2 + leftRight);
            if (tempCurrIndex.Item1 < 0 || tempCurrIndex.Item1 > 8
                || tempCurrIndex.Item2 < 0 || tempCurrIndex.Item2 > 8)
                return;

            currIndex = tempCurrIndex;
            PrintTable();
        }
    }

    private void Update()
    {
        if ((DateTime.Now - lastTimeUpdate).TotalSeconds >= 1)
        {
            time--;
            lastTimeUpdate = DateTime.Now;
            PrintTable();
        }

        if (time <= 0)
        {

            if (points >= goalPoints)
                gameWon = true;
            else
                gameWon = null;
        }
    }

    private void Select()
    {
        if (selected)
        {
            if (selectedIndex == currIndex)
            {
                selectedIndex = (10, 10);
                selected = !selected;
                return;
            }
            SwitchIndices();
            CheckTime();
            selectedIndex = (10, 10);
            selected = !selected;
            PrintTable();

            Thread.Sleep(500);
            DropD();
        }
        else
        {
            selectedIndex = currIndex;
            selected = !selected;
        }
    }

    private void SwitchIndices()
    {
        char temp = Table[selectedIndex.Item1][selectedIndex.Item2];
        Table[selectedIndex.Item1][selectedIndex.Item2] = Table[currIndex.Item1][currIndex.Item2];
        Table[currIndex.Item1][currIndex.Item2] = temp;

        (int, int) tempIndex;
        tempIndex = selectedIndex;
        selectedIndex = currIndex;
        currIndex = tempIndex;
    }

    private void CheckTime()
    {
        if (Table[selectedIndex.Item1][selectedIndex.Item2] != ' ')
            SelectedIndexCheck();

        if (Table[currIndex.Item1][currIndex.Item2] != ' ')
            CurrentIndexCheck();
    }

    private void SelectedIndexCheck()
    {
        HorizontalHandle(selectedIndex);
        VerticalHandle(selectedIndex);
    }

    private void CurrentIndexCheck()
    {
        HorizontalHandle(currIndex);
        VerticalHandle(currIndex);
    }

    private bool HorizontalHandle((int, int) index)
    {
        int row = index.Item1;
        int col = index.Item2;
        char target = Table[row][col];

        int leftAmt = 0;
        for (int i = col - 1; i >= 0; i--)
        {
            if (Table[row][i] == target)
                leftAmt++;
            else
                break;
        }

        int rightAmt = 0;
        for (int i = col + 1; i < Table[row].Length; i++)
        {
            if (Table[row][i] == target)
                rightAmt++;
            else
                break;
        }

        if (leftAmt + rightAmt < 2)
            return false;

        for (int i = col - leftAmt; i <= col + rightAmt; i++)
            Table[row][i] = ' ';

        if (leftAmt + rightAmt == 2)
            points += 100;
        else if (leftAmt + rightAmt == 3)
            points += 300;
        else
            points += 1000;

        return true;
    }

    private bool VerticalHandle((int, int) index)
    {
        int row = index.Item1;
        int col = index.Item2;
        char target = Table[row][col];

        int upAmt = 0;
        for (int i = row - 1; i >= 0; i--)
        {
            if (Table[i][col] == target)
                upAmt++;
            else
                break;
        }

        int downAmt = 0;
        for (int i = row + 1; i < Table[row].Length; i++)
        {
            if (Table[i][col] == target)
                downAmt++;
            else
                break;
        }

        if (upAmt + downAmt < 2)
            return false;

        for (int i = row - upAmt; i <= row + downAmt; i++)
            Table[i][col] = ' ';

        if (upAmt + downAmt == 2)
            points += 100;
        else if (upAmt + downAmt == 3)
            points += 300;
        else
            points += 1000;

        return true;
    }

    private void DropD()
    {
        bool hasExplosions = true;

        while (hasExplosions)
        {
            hasExplosions = false;
            for (int i = Table.Length - 1; i >= 0; i--)
            {
                for (int j = 0; j < Table.Length; j++)
                {
                    if (Table[i][j] != ' ')
                        continue;

                    (int, int) suitableIndex = (10, 10);
                    for (int k = i; k >= 0; k--)
                    {
                        if (Table[k][j] != ' ')
                        {
                            suitableIndex.Item1 = k;
                            suitableIndex.Item2 = j;
                            break;
                        }
                    }

                    if (suitableIndex.Item1 != 10)
                    {
                        char temp = Table[suitableIndex.Item1][suitableIndex.Item2];
                        Table[suitableIndex.Item1][suitableIndex.Item2] = Table[i][j];
                        Table[i][j] = temp;
                    }
                    else
                        continue;

                    PrintTable();
                    Thread.Sleep(75);
                }
            }

            for (int i = 0; i < Table.Length; i++)
            {
                for (int j = 0; j < Table[i].Length; j++)
                {
                    (int, int) indices = (i, j);
                    if (Table[i][j] != ' ' && (HorizontalHandle(indices) || VerticalHandle(indices)))
                    {
                        hasExplosions = true;
                        Table[i][j] = ' ';
                    }
                }
            }

            for (int i = 0; i < Table.Length; i++)
            {
                for (int j = 0; j < Table[i].Length; j++)
                {
                    if (Table[i][j] == ' ')
                    {
                        Table[i][j] = colors[random.Next(colors.Length)];
                        PrintTable();
                        Thread.Sleep(75);
                    }
                }
            }
        }
    }
}