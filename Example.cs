using System;
using System.Security.Policy;
using System.Collections.Generic;

class GomocupEngine : GomocupInterface
{
	const int MAX_BOARD = 100;
    int[,] board = new int[MAX_BOARD, MAX_BOARD];

	Random rand = new Random();
	int[][] lines = new int[12][];

	public override string brain_about
	{
		get
		{
			return "name=\"ArcadeV42\", author=\"Machin Truc\", version=\"1.1\", country=\"France\", www=\"http://talkaround.io\"";
		}
	}

	private void FillLines()
	{
		// lines
		lines[0] = new[] {0, 0, 4, 0};
		lines[1] = new[] {0, 1, 4, 1};
		lines[2] = new[] {0, 2, 4, 2};
		lines[3] = new[] {0, 3, 4, 3};
		lines[4] = new[] {0, 4, 4, 4};
		// collumns
		lines[5] = new[] {0, 0, 0, 4};
		lines[6] = new[] {1, 0, 1, 4};
		lines[7] = new[] {2, 0, 2, 4};
		lines[8] = new[] {3, 0, 3, 4};
		lines[9] = new[] {4, 0, 4, 4};
		// diags (top left -> bottom right)
		/*lines[10] = new[] {4, 0, 4, 0}; // une seule case
		lines[11] = new[] {3, 0, 4, 1};
		lines[12] = new[] {2, 0, 4, 2};
		lines[13] = new[] {1, 0, 4, 3};*/
		lines[10] = new[] {0, 0, 4, 4};
        /*lines[15] = new[] {0, 1, 3, 4};
		lines[16] = new[] {0, 2, 2, 4};
		lines[17] = new[] {0, 3, 1, 4};
		lines[18] = new[] {0, 4, 0, 4}; // une seule case
		// diags (top right -> bottom left)
		lines[19] = new[] {0, 0, 0, 0}; // une seule case
		lines[20] = new[] {0, 1, 1, 0};
		lines[21] = new[] {0, 2, 2, 0};
		lines[22] = new[] {0, 3, 3, 0};*/
        lines[11] = new[] {0, 4, 4, 0};
        /*lines[24] = new[] {1, 4, 4, 1};
		lines[25] = new[] {2, 4, 4, 2};
		lines[26] = new[] {3, 4, 4, 3};
		lines[27] = new[] {4, 4, 4, 4}; // une seule case*/
    }

	public override void brain_init()
	{
		if (width < 5 || height < 5)
		{
			Console.WriteLine("ERROR size of the board");
			return;
		}
		if (width > MAX_BOARD || height > MAX_BOARD)
		{
			Console.WriteLine("ERROR Maximal board size is " + MAX_BOARD);
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
        int weight = 0;
        double[,] resTable = new double[height, width];
        int[] res = FinalDecision();
        return res;
	}

    private int[] FinalDecision()
    {
        double[,] resTable = new double[height, width];
        for (int y = 0; y != height; y++)
        {
            for(int x = 0; x!= width; x++)
            {
                resTable[x, y] = board[x, y];
            }
        }
        for (int id = 0; id != (width - 4) * (height - 4); id++)
        {
            double[,] subTable = new double[5, 5];
            ReturnSubTable(id, ref subTable);
            FillSubTable(ref subTable);
            FillResTable(ref resTable, subTable, id);
        }
        List<int[]> resPos = new List<int[]>();
        double max = TakeBestOnes(resTable, ref resPos);
        /*if (resPos.Count > 1)
        {
            int max2 = 0;
            int[] res = new int[2];
            foreach (int[] pos in resPos)
            {
                int temp = TreePossibility(resTable, pos, -1, 0);
                if (temp >= max2)
                {
                    max2 = temp;
                    res = pos;
                }
            }
            return res;
        }
        else*/
            return resPos[0];
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
        for (int y = 0; y != 5; y++)
        {
            for (int x = 0; x != 5; x++)
            {
                if (subTable[x, y] != -1 && subTable[x, y] != -2 && resTable[(id % (width - 4)) + x, (id / (height - 4)) + y] < subTable[x, y])
                    resTable[(id % (width - 4)) + x, (id / (height - 4)) + y] = subTable[x, y];
            }
        }
    }

    private int TreePossibility(double[,] resTable, int[] pos, int whoAmI, int level)
    {
        for (int id = 0; id != (width - 4) * (height - 4); id++)
        {
            double[,] subTable = new double[5, 5];
            ReturnSubTable(id, ref subTable);
            FillSubTable(ref subTable);
            FillResTable(ref resTable, subTable, id);
        }
        List<int[]> resPos = new List<int[]>();
        double max = TakeBestOnes(resTable, ref resPos);
        if (resPos.Count > 1)
        {
            int max2 = 0;
            int[] res = new int[2];
            foreach (int[] tempPos in resPos)
            {
                int temp = TreePossibility(resTable, tempPos, -1, 0);
                if (temp >= max2)
                {
                    max2 = temp;
                    res = tempPos;
                }
            }
            //return res;
        }
        //else
        //return resPos[0];
        return 0;
    }

	private void ReturnSubTable(int id, ref double[,] subTable)
	{
		int x = id % (width - 4);
		int y = id / (height - 4);
		for (int a = 0; a != 5; a += 1)
		{
			for (int b = 0; b != 5; b += 1)
			{
				subTable[b, a] = board[x, y];
				x += 1;
			}
			x = id % (width - 4);
			y += 1;
		}
	}

	private void SetBack(ref double[,] subTable, int[] line, ref int enNb, ref int alNb, ref int emNb)
	{
        int y = line[1];
		int x = line[0];
		while (true)
		{
			if (subTable[x, y] != (enNb != 0 ? -1 : -2))
			{
                int x2 = x;
                int y2 = y;
                int before = 0;
                int after = 0;
                //Console.WriteLine("MESSAGE 0) x0 = " + line[0] + " | y0 = " + line[1] + " | x1 = " + line[2] + " | y1 = " + line[3]);
                while (true)
                {
                    if (y2 == line[3] && x2 == line[2])
                        break;
                    //Console.WriteLine("MESSAGE 1) x2 = " + x2 + " | y2 = " + y2);
                    x2 += (line[0] < line[2] ? 1 : (line[0] > line[2] ? -1 : 0));
                    y2 += (line[1] < line[3] ? 1 : (line[1] > line[3] ? -1 : 0));
                    //Console.WriteLine("MESSAGE 2) x2 = " + x2 + " | y2 = " + y2);
                    if (subTable[x2, y2] != (enNb != 0 ? -1 : -2))
                        break;
                    else
                        after++;
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
                    if (subTable[x2, y2] != (enNb != 0 ? -1 : -2))
                        break;
                    else
                        before++;
                }
                if (before + after != 0)
                {
                    subTable[x, y] = before + after + 1;
                    if (before + after > 3)
                    {
                        Console.WriteLine("MESSAGE subtable[" + x + "," + y + "] = " + (before + after + 1));
                        PrintBoard(subTable);
                    }
                }
            }
			if (y == line[3] && x == line[2])
				break;
			x += (line[0] < line[2] ? 1 : (line[0] > line[2] ? -1 : 0));
			y += (line[1] < line[3] ? 1 : (line[1] > line[3] ? -1 : 0));
		}
    }

	private void FillSubTable(ref double[,] subTable)
	{
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
            if (emNb != 5 && (enNb + emNb == 5 || alNb + emNb == 5))
                SetBack(ref subTable, line, ref enNb, ref alNb, ref emNb);
        }
    }

	public void PrintBoard(double[,] board)
	{
        string temp = "";
		int Size = 0;
		if (board.Length == 25)
			Size = 5;
		else if (board.Length == 19 * 19)
			Size = 19;
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
