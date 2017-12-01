using System;
using System.Security.Policy;

class GomocupEngine : GomocupInterface
{
	const int MAX_BOARD = 100;
	int[,] board = new int[MAX_BOARD, MAX_BOARD];
	Random rand = new Random();
	int[][] lines = new int[28][];

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
		lines[10] = new[] {4, 0, 4, 0}; // une seule case
		lines[11] = new[] {3, 0, 4, 1};
		lines[12] = new[] {2, 0, 4, 2};
		lines[13] = new[] {1, 0, 4, 3};
		lines[14] = new[] {0, 0, 4, 4};
		lines[15] = new[] {0, 1, 3, 4};
		lines[16] = new[] {0, 2, 2, 4};
		lines[17] = new[] {0, 3, 1, 4};
		lines[18] = new[] {0, 4, 0, 4}; // une seule case
		// diags (top right -> bottom left)
		lines[19] = new[] {0, 0, 0, 0}; // une seule case
		lines[20] = new[] {0, 1, 1, 0};
		lines[21] = new[] {0, 2, 2, 0};
		lines[22] = new[] {0, 3, 3, 0};
		lines[23] = new[] {0, 4, 4, 0};
		lines[24] = new[] {1, 4, 4, 1};
		lines[25] = new[] {2, 4, 4, 2};
		lines[26] = new[] {3, 4, 4, 3};
		lines[27] = new[] {4, 4, 4, 4}; // une seule case
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

	public override void brain_block(int x, int y)
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
		int i;
		i = -1;
		int[] res = new int[2];
        res[0] = -1;
        res[1] = -1;
		do
		{
            res = Algo();
        } while (!isFree(res[0], res[1]));
		//if (i > 1) Console.WriteLine("DEBUG {0} coordinates didn't hit an empty field", i);
		do_mymove(res[0], res[1]);
	}

	public override void brain_end()
	{
	}

	public override void brain_eval(int x, int y)
	{
	}

    // ALGO

	public int[] Algo()
	{
		int[][,] resTable = new int[(width - 4) * (height - 4)][,];
		for (int id = 0; id != (width - 4) * (height - 4); id++)
		{
			resTable[id] = new int[5, 5];
			ReturnSubTable(id, ref resTable[id]);
			FillSubTable(ref resTable[id]);
		}
		return SetResBoard(resTable);
	}

	private void ReturnSubTable(int id, ref int[,] subTable)
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

	private void SetBack(ref int[,] subTable, int[] line, ref int enNb, ref int alNb, ref int emNb)
	{
		if (emNb == 5 || emNb == 0 || (enNb == 0 && alNb == 0))
			return;
		int y = line[1];
		int x = line[0];
		if (enNb != 0 && alNb != 0)
		{
			while (true)
			{
				if (subTable[x, y] == (enNb == 0 ? -1 : -2) || (y == line[3] && x == line[2]))
					break;
				x += (line[0] < line[2] ? 1 : (line[0] > line[2] ? -1 : 0));
				y += (line[1] < line[3] ? 1 : (line[1] > line[3] ? -1 : 0));
			}
		}
		while (true)
		{
			if (subTable[x, y] != -1 && subTable[x, y] != -2)
				subTable[x, y] += (enNb != 0 ? enNb : alNb) * (enNb != 0 ? enNb : alNb) + (enNb != 0 ? 0 : 1);
			if (y == line[3] && x == line[2])
				break;
			x += (line[0] < line[2] ? 1 : (line[0] > line[2] ? -1 : 0));
			y += (line[1] < line[3] ? 1 : (line[1] > line[3] ? -1 : 0));
		}
		emNb = 0;
	}

	private void FillSubTable(ref int[,] subTable)
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
						if (alNb != 0 && emNb != 0)
							SetBack(ref subTable, line, ref enNb, ref alNb, ref emNb);
						enNb += 1;
						alNb = 0;
						break;
					case -2:
						if (enNb != 0 && emNb != 0)
							SetBack(ref subTable, line, ref enNb, ref alNb, ref emNb);
						alNb += 1;
						enNb = 0;
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
			if (emNb != 0)
				SetBack(ref subTable, line, ref enNb, ref alNb, ref emNb);
		}
	}

	private int[] SetResBoard(int[][,] resTable)
	{
		int[,] finalTable = board;
		int id = 0;
		int max = 0;
		int[] maxSquare = new int[2];
		
		while (id != (width - 4) * (height - 4))
		{
			for (int y = 0; y != 5; y++)
			{
				for (int x = 0; x != 5; x++)
				{
					if (finalTable[(id % (width - 4)) + x, (id / (height - 4)) + y] != -1 &&
						finalTable[(id % (width - 4)) + x, (id / (height - 4)) + y] != -2 && resTable[id][x, y] != 0)
					{
						finalTable[(id % (width - 4)) + x, (id / (height - 4)) + y] += resTable[id][x, y];
					}
					if (finalTable[(id % (width - 4)) + x, (id / (height - 4)) + y] > max)
					{
						max = finalTable[id % (width - 4) + x, id / (height - 4) + y];
						maxSquare[0] = id % (width - 4) + x;
						maxSquare[1] = id / (height - 4) + y;
					}
				}
			}
			id++;
		}
		if (max == 0)
		{
			do
			{
				maxSquare[0] = rand.Next(width);
				maxSquare[1] = rand.Next(height);
			} while (!isFree(maxSquare[0], maxSquare[1]));
		}
		return maxSquare;
	}

	public void PrintBoard(int[,] board)
	{
		int Size = 0;
		if (board.Length == 25)
			Size = 5;
		else if (board.Length == 19 * 19)
			Size = 19;
		for (int y = 0; y != Size; y++)
		{
			for (int x = 0; x != Size; x++)
			{
				Console.Write(board[x, y]);
				if (board[x, y] < 0)
					Console.Write(" ");
				else if (board[x, y] < 10)
					Console.Write("  ");
				else if (board[x, y] < 100)
					Console.Write(" ");
			}
			Console.Write("\n");
		}
	}
}
