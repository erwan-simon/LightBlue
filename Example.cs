using System;
using System.Security.Policy;
using System.Collections.Generic;

class GomocupEngine : GomocupInterface
{
	const int MAX_BOARD = 100;
    int[,] board = new int[MAX_BOARD, MAX_BOARD];

	Random rand = new Random();
	int[][] lines = new int[18][];

	public override string brain_about
	{
		get
		{
			return "name=\"ArcadeV42\", author=\"Machin Truc\", version=\"2.3\", country=\"France\", www=\"http://talkaround.io\"";
		}
	}

	private void FillLines()
	{
		// lines
		lines[0] = new[] { 0, 0, 5, 0 };
		lines[1] = new[] { 0, 1, 5, 1 };
		lines[2] = new[] { 0, 2, 5, 2 };
		lines[3] = new[] { 0, 3, 5, 3 };
		lines[4] = new[] { 0, 4, 5, 4 };
        lines[5] = new[] { 0, 5, 5, 5 };
        // collumns
        lines[6] = new[] { 0, 0, 0, 5 };
		lines[7] = new[] { 1, 0, 1, 5 };
		lines[8] = new[] { 2, 0, 2, 5 };
		lines[9] = new[] { 3, 0, 3, 5 };
		lines[10] = new[] { 4, 0, 4, 5 };
        lines[11] = new[] { 5, 0, 5, 5 };
        // diags (top left -> bottom right)
        /*lines[10] = new[] {4, 0, 4, 0}; // une seule case
		lines[11] = new[] {3, 0, 4, 1};
		lines[12] = new[] {2, 0, 4, 2};
		lines[13] = new[] {1, 0, 4, 3};*/
        lines[12] = new[] { 0, 1, 4, 5 };
        lines[13] = new[] { 0, 0, 5, 5 };
        lines[14] = new[] { 1, 0, 5, 4 };
        /*lines[15] = new[] {0, 1, 3, 4};
		lines[16] = new[] {0, 2, 2, 4};
		lines[17] = new[] {0, 3, 1, 4};
		lines[18] = new[] {0, 4, 0, 4}; // une seule case
		// diags (top right -> bottom left)
		lines[19] = new[] {0, 0, 0, 0}; // une seule case
		lines[20] = new[] {0, 1, 1, 0};
		lines[21] = new[] {0, 2, 2, 0};
		lines[22] = new[] {0, 3, 3, 0};*/
        lines[15] = new[] { 0, 4, 4, 0 };
        lines[16] = new[] { 0, 5, 5, 0 };
        lines[17] = new[] { 1, 5, 5, 1 };
        /*lines[24] = new[] {1, 4, 4, 1};
		lines[25] = new[] {2, 4, 4, 2};
		lines[26] = new[] {3, 4, 4, 3};
		lines[27] = new[] {4, 4, 4, 4}; // une seule case*/
    }

	public override void brain_init()
	{
		if (width < 6 || height < 6)
		{
			Console.WriteLine("ERROR size of the board");
			return;
		}
		if (width > MAX_BOARD || height > MAX_BOARD)
		{
			Console.WriteLine("ERROR Maximal board size is " + MAX_BOARD);
			return;
		}
        if (width != height)
        {
            Console.WriteLine("ERROR Board is not a square");
            return;
        }
		FillLines();
		Console.WriteLine("OK");
	}

	public override void brain_restart()
	{
		for (int x = 0; x < width; x++)
			for (int y = 0; y < height; y++)
				board[x, y] = 0;

		Console.WriteLine("OK");
	}

	private bool isFree(int x, int y)
	{
		return x >= 0 && y >= 0 && x < width && y < height && board[x, y] == 0;
	}

	public override void brain_my(int x, int y)
	{
		if (isFree(x, y))
		{
			board[x, y] = -2;
		}
		else
		{
			Console.WriteLine("ERROR my move [{0},{1}]", x, y);
		}
	}

	public override void brain_opponents(int x, int y)
	{
		if (isFree(x, y))
		{
			board[x, y] = -1;
		}
		else
		{
			Console.WriteLine("ERROR opponents's move [{0},{1}]", x, y);
		}
	}

	public override void brain_block(int x
        , int y)
	{
		if (isFree(x, y))
		{
			board[x, y] = -3;
		}
		else
		{
			Console.WriteLine("ERROR winning move [{0},{1}]", x, y);
		}
	}

	public override int brain_takeback(int x, int y)
	{
		if (x >= 0 && y >= 0 && x < width && y < height && board[x, y] != 0)
		{
			board[x, y] = 0;
			return 0;
		}
		return 2;
	}

	public override void brain_turn()
	{
		int[] res = new int[2];
        res[0] = -1;
        res[1] = -1;
		do
		{
            res = Algo();
        } while (!isFree(res[0], res[1]));
		do_mymove(res[0], res[1]);
	}

	public override void brain_end()
	{
	}

	public override void brain_eval(int x, int y)
	{
	}

    // ALGO

	private int[] Algo()
	{
        double[,] resTable = new double[height, width];
        int[] res = FinalDecision();
        return res;
	}

    private int[] FinalDecision()
    {
        double[,] enTable = new double[height, width];
        double[,] alTable = new double[height, width];
        for (int y = 0; y != height; y++)
        {
            for(int x = 0; x!= width; x++)
            {
                enTable[x, y] = board[x, y];
                alTable[x, y] = board[x, y];
            }
        }
        for (int id = 0; id != (width - 5) * (height - 5); id++)
        {
            double[,] subTable = new double[6, 6];
            ReturnSubTable(id, ref subTable);
            FillSubTable(ref subTable, -1);
            FillResTable(ref enTable, subTable, id);
        }
        for (int id = 0; id != (width - 5) * (height - 5); id++)
        {
            double[,] subTable = new double[6, 6];
            ReturnSubTable(id, ref subTable);
            FillSubTable(ref subTable, -2);
            FillResTable(ref alTable, subTable, id);
        }
        List<int[]> alPos = new List<int[]>();
        List<int[]> enPos = new List<int[]>();
        double alMax = TakeBestOnes(alTable, ref alPos);
        double enMax = TakeBestOnes(enTable, ref enPos);
        Console.WriteLine("MESSAGE ally:");
        PrintBoard(alTable);
        Console.WriteLine("MESSAGE enemy:");
        PrintBoard(enTable);
        if (alMax >= enMax)
            Console.WriteLine("MESSAGE ally won");
        else
            Console.WriteLine("MESSAGE enemy won");
        if ((alMax >= enMax ? alPos.Count : enPos.Count) > 1)
        {
            double max = 0;
            int[] resPos = new int[2];
            foreach (int[] pos in (alMax >= enMax ? alPos : enPos))
            {
                if ((alMax >= enMax ? enTable[pos[0], pos[1]] : alTable[pos[0], pos[1]]) > max)
                {
                    resPos = pos;
                    max = (alMax >= enMax ? enTable[pos[0], pos[1]] : alTable[pos[0], pos[1]]);
                }
            }
            if (max > 0)
                return resPos;
        }
        if (alMax >= enMax)
            return (alPos[rand.Next(alPos.Count)]);
        return (enPos[rand.Next(enPos.Count)]);
    }

    private int TreePossibilities(int[,] myBoard, int[] pawnPos, int whoAmI)
    {
        int[,] myBoard2 = new int[height, width];
        for (int y = 0; y != height; y++)
        {
            for (int x = 0; x != width; x++)
            {
                myBoard2[x, y] = myBoard[x, y];
                if (pawnPos[0] == x && pawnPos[1] == y)
                    myBoard2[x, y] = whoAmI;
            }
        }
        double[,] enTable = new double[height, width];
        double[,] alTable = new double[height, width];
        for (int y = 0; y != height; y++)
        {
            for(int x = 0; x!= width; x++)
            {
                enTable[x, y] = myBoard[x, y];
                alTable[x, y] = myBoard[x, y];
            }
        }
        for (int id = 0; id != (width - 5) * (height - 5); id++)
        {
            double[,] subTable = new double[6, 6];
            ReturnSubTable(id, ref subTable);
            FillSubTable(ref subTable, -1);
            FillResTable(ref enTable, subTable, id);
        }
        for (int id = 0; id != (width - 5) * (height - 5); id++)
        {
            double[,] subTable = new double[6, 6];
            ReturnSubTable(id, ref subTable);
            FillSubTable(ref subTable, -2);
            FillResTable(ref alTable, subTable, id);
        }
        List<int[]> alPos = new List<int[]>();
        List<int[]> enPos = new List<int[]>();
        double alMax = TakeBestOnes(alTable, ref alPos);
        if (alMax >= 1000)
            return 1;
        double enMax = TakeBestOnes(enTable, ref enPos);
        if (enMax >= 1000)
            return -1;
        if ((alMax >= enMax ? alPos.Count : enPos.Count) > 1)
        {
            int max = 0;
            foreach (int[] pos in (alMax >= enMax ? alPos : enPos))
            {
                max += TreePossibilities(myBoard2, pos, whoAmI == -1 ? -2 : -1);
            }
            return max;
        }
        return ((alMax >= enMax ? 1 : -1));
    }

    private double TakeBestOnes(double[,] resTable, ref List<int[]> resPos)
    {
        double max = 0;
        
        for (int y = 0; y != height; y++)
        {
            for (int x = 0; x != width; x++)
            {
                if (resTable[x, y] != -1 && resTable[x, y] != -2)
                {
                    if (resTable[x, y] > max)
                    {
                        int[] pos = new int[2];
                        pos[0] = x;
                        pos[1] = y;
                        resPos.Clear();
                        resPos.Add(pos);
                        max = resTable[x, y];
                    }
                    else if (resTable[x, y] == max)
                    {
                        int[] pos = new int[2];
                        pos[0] = x;
                        pos[1] = y;
                        resPos.Add(pos);
                    }
                }
            }
        }
        if (max == 0)
        {
            int[] pos = new int[2];
            do
            {
                pos[0] = rand.Next(width);
                pos[1] = rand.Next(height);
            } while (!isFree(pos[0], pos[1]));
            resPos.Clear();
            resPos.Add(pos);
        }
        return max;
    }

    private void FillResTable(ref double[,] resTable, double[,] subTable, int id)
    {
        for (int y = 0; y != 6; y++)
        {
            for (int x = 0; x != 6; x++)
            {
                if (subTable[x, y] != -1 && subTable[x, y] != -2 && resTable[(id % (width - 5)) + x, (id / (height - 5)) + y] < subTable[x, y])
                    resTable[(id % (width - 5)) + x, (id / (height - 5)) + y] = subTable[x, y];
            }
        }
    }

	private void ReturnSubTable(int id, ref double[,] subTable)
	{
		int x = id % (width - 5);
		int y = id / (height - 5);
		for (int a = 0; a != 6; a += 1)
		{
			for (int b = 0; b != 6; b += 1)
			{
				subTable[b, a] = board[x, y];
				x += 1;
			}
			x = id % (width - 5);
			y += 1;
		}
	}

	private void SetBack(ref double[,] subTable, int[] line, int whoAmI)
	{
        int y = line[1];
		int x = line[0];
		while (true)
		{
            if (subTable[x, y] >= 0)
            {
                int x2 = x;
                int y2 = y;
                bool before = false;
                bool after = false;
                int count = 0;
                //Console.WriteLine("MESSAGE 0) x0 = " + line[0] + " | y0 = " + line[1] + " | x1 = " + line[2] + " | y1 = " + line[3]);
                while (true)
                {
                    if (y2 == line[3] && x2 == line[2])
                        break;
                    //Console.WriteLine("MESSAGE 1) x2 = " + x2 + " | y2 = " + y2);
                    x2 += (line[0] < line[2] ? 1 : (line[0] > line[2] ? -1 : 0));
                    y2 += (line[1] < line[3] ? 1 : (line[1] > line[3] ? -1 : 0));
                    //Console.WriteLine("MESSAGE 2) x2 = " + x2 + " | y2 = " + y2);
                    if (subTable[x2, y2] != whoAmI)
                    {
                        after = subTable[x2, y2] >= 0 ? true : false;
                        break;
                    }
                    else
                        count++;
                }
                x2 = x;
                y2 = y;
                while (true)
                {
                    if (y2 == line[1] && x2 == line[0])
                        break;
                   //Console.WriteLine("MESSAGE 3) x2 = " + x2 + " | y2 = " + y2);
                    x2 -= (line[0] < line[2] ? 1 : (line[0] > line[2] ? -1 : 0));
                    y2 -= (line[1] < line[3] ? 1 : (line[1] > line[3] ? -1 : 0));
                    //Console.WriteLine("MESSAGE 4) x2 = " + x2 + " | y2 = " + y2);
                    if (subTable[x2, y2] != whoAmI)
                    {
                        before = subTable[x2, y2] >= 0 ? true : false;
                        break;
                    }
                    else
                        count++;
                }
                if (count != 0)
                {
                    if (count == 4)
                        subTable[x, y] += 10000; // winning move has top priority
                    else if (count == 3 && (before == true ? 0.5 : 0) + (after == true ? 0.5 : 0) == 1)
                        subTable[x, y] += 1000; // decisive move has priority
                    /*else if (count == 3)
                        subTable[x, y] += 100;
                    else if (count == 2 && (before == true ? 0.5 : 0) + (after == true ? 0.5 : 0) == 1)
                        subTable[x,  y] += 10; // almost decisive move has more or less priority
                    else*/
                    else
                    {
                        subTable[x, y] += MyPower(count + (before == true ? 0.5 : 0) + (after == true ? 0.5 : 0), 2) + (whoAmI == -1 ? 0 : 1);
                    }
                }
            }
			if (y == line[3] && x == line[2])
				break;
			x += (line[0] < line[2] ? 1 : (line[0] > line[2] ? -1 : 0));
			y += (line[1] < line[3] ? 1 : (line[1] > line[3] ? -1 : 0));
		}
    }

	private void FillSubTable(ref double[,] subTable, int whoAmI)
	{
        bool print = false;
        foreach (int[] line in lines)
		{
            int y = line[1];
			int x = line[0];
            int alNb = 0;
			int enNb = 0;
			int emNb = 0;
            while (true)
			{
                switch (subTable[x, y])
                {
                    case -1:
                        enNb += 1;
                        break;
                    case -2:
                        alNb += 1;
                        break;
                    case -3:
                        break;
                    default:
                        emNb += 1;
                        break;  
                }
				if (y == line[3] && x == line[2])
					break;
				x += (line[0] < line[2] ? 1 : (line[0] > line[2] ? -1 : 0));
				y += (line[1] < line[3] ? 1 : (line[1] > line[3] ? -1 : 0));
			}
            if (emNb < 5 && (whoAmI == -1 ? enNb : alNb) + emNb >= 5 && ((whoAmI == -1 ? alNb : enNb) == 0 || subTable[line[0], line[1]] == (whoAmI == -1 ? -2 : -1) || subTable[line[2], line[3]] == (whoAmI == -1 ? -2 : -1)))
            {
                SetBack(ref subTable, line, whoAmI);
                print = true;
            }
        }
        if (print)
            PrintBoard(subTable);
    }

	public void PrintBoard(double[,] board)
	{
        string temp = "";   
		int Size = 0;
		if (board.Length == 6 * 6)
			Size = 6;
		else if (board.Length == 20 * 20)
			Size = 20;
		for (int y = 0; y != Size; y++)
		{
			for (int x = 0; x != Size; x++)
			{
				temp += board[x, y];
                temp += " ";
			}
            Console.WriteLine("MESSAGE " + temp);
            temp = "";
        }
        Console.WriteLine("MESSAGE ");
    }

	private double MyPower(double nb, double power)
	{
		nb = power < 0 ? 0 : nb;
		power = power - 1;
		if (power != 0 && nb != 0)
			nb = nb * MyPower(nb, power);
		return nb;
	}
}
