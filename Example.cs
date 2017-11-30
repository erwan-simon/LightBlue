using System;
using System.Security.Policy;

class GomocupEngine : GomocupInterface
{
	const int MAX_BOARD = 100;
	int[,] board = new int[MAX_BOARD, MAX_BOARD];
	Random rand = new Random();
	int[][] lines = new int[24][];

	public override string brain_about
	{
		get
		{
			return "name=\"Random\", author=\"Petr Lastovicka\", version=\"1.1\", country=\"Czech Republic\", www=\"http://petr.lastovicka.sweb.cz\"";
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
		int x, y, i;

		i = -1;
		do
		{
			x = rand.Next(width);
			y = rand.Next(height);
			i++;
			if (terminate != 0) return;
		} while (!isFree(x, y));

		if (i > 1) Console.WriteLine("DEBUG {0} coordinates didn't hit an empty field", i);
		do_mymove(x, y);
	}

	public override void brain_end()
	{
	}

	public override void brain_eval(int x, int y)
	{
	}

    // ALGO

	void Algo()
	{
		
	}

    int[,] ReturnSubTable(int id)
    {
        int x = (id % (width - 4)) - 2;
        int y = (id / (height - 4)) - 2;
        int[,] res = new int[5, 5];

        for (int a = 0; a != 5; a += 1)
        {
            for (int b = 0; b != 5; b += 1)
            {
                res[a, b] = board[x, y];
                x += 1;
            }
            x = (id % (width - 4)) - 2;
            y += 1;
        }
        return res;
    }

	void MakeSquare(int square, ref int enNb, ref int alNb, ref int emNb)
	{
		switch (square)
		{
			case 1:
				alNb += 1;
				emNb = (enNb == 0 ? emNb : 0);
				enNb = 0;
				break;
			case -1:
				enNb += 1;
				emNb = (alNb == 0 ? emNb : 0);
				alNb = 0;
				break;
			default:
				emNb += 1;
				break;
		}		
	}

	void SetBack(ref int[,] subTable, int[] line, ref int enNb, ref int alNb, ref int emNb)
	{
		int y = line[1];
		int x = line[0];
		while (y != line[3] || x != line[2])
		{
			if (subTable[x, y] == (enNb != 0 ? -1 : -2))
				break;
			x += (line[0] < line[2] ? 1 : (line[0] > line[2] ? -1 : 0));
			y += (line[1] < line[3] ? 1 : (line[1] > line[3] ? -1 : 0));			
		}
		while (y != line[3] || x != line[2])
		{
			if (subTable[x, y] != -1 && subTable[x, y] != -2)
				subTable[x, y] += (enNb != 0 ? enNb : alNb) * (enNb != 0 ? enNb : alNb) + (enNb != 0 ? 0 : 1);
			x += (line[0] < line[2] ? 1 : (line[0] > line[2] ? -1 : 0));
			y += (line[1] < line[3] ? 1 : (line[1] > line[3] ? -1 : 0));
		}
		emNb = 0;
	}

	void FillSubTable(int[,] subTable)
	{
		foreach (int[] line in lines)
		{
			int y = line[1];
			int x = line[0];
			int alNb = 0;
			int enNb = 0;
			int emNb = 0;
			while (y != line[3] || x != line[2])
			{
				switch (subTable[x, y])
				{
					case -1:
						if (alNb != 0 && emNb != 0)
							SetBack(ref subTable, line, ref alNb, ref enNb, ref emNb);
						enNb += 1;
						alNb = 0;
						break;
					case -2:
						if (enNb != 0 && emNb != 0)
							SetBack(ref subTable, line, ref alNb, ref enNb, ref emNb);
						alNb += 1;
						enNb = 0;
						break;
					case -3:
						break;
					default:
						emNb += 1;
						break;
				}
				x += (line[0] < line[2] ? 1 : (line[0] > line[2] ? -1 : 0));
				y += (line[1] < line[3] ? 1 : (line[1] > line[3] ? -1 : 0));
			}
			if (emNb != 0)
				SetBack(ref subTable, line, ref alNb, ref enNb, ref emNb);
		}
	}

/*
	void FillSubTable(int[,] subTable)
	{
		int x;
		int y;
		int enNb = 0; // enemy number
		int alNb = 0; // ally number
		int emNb = 0; // empty number
		
		for (y = 0; y != 5; y += 1)
		{
			for (x = 0; x != 5; x += 1)
			{
				MakeSquare(subTable[x, y], ref enNb, ref alNb, ref emNb);
			}
			for (x = 0; x != 5; x += 1)
			{
				if (subTable[x, y] == 0 || (subTable[x, y] != 1 && subTable[x, y] != -1))
					subTable[x, y] += (alNb + enNb + (emNb / 2)) * (alNb + enNb + (emNb / 2)) + (alNb > 0 ? 1 : 0);
			}
		}
		for (x = 0; x != 5; x += 1)
		{
			for (y = 0; y != 5; y += 1)
			{
				MakeSquare(subTable[x, y], ref enNb, ref alNb, ref emNb);
			}
			for (y = 0; y != 5; y += 1)
			{
				if (subTable[x, y] == 0 || (subTable[x, y] != 1 && subTable[x, y] != -1))
					subTable[x, y] += (alNb + enNb + (emNb / 2)) * (alNb + enNb + (emNb / 2)) + (alNb > 0 ? 1 : 0);
			}
		}
		alNb = 0;
		enNb = 0;
		emNb = 0;
		y = 0;
		x = 0;
		for (int k = 0 ; k < 5 * 2 ; k++ )
		{
			for (y = 0 ; y <= k ; y++ )
			{
				x = k - y;
				if( x < 5 && y < 5 ) {
					MakeSquare(subTable[x, y], ref enNb, ref alNb, ref emNb);
				}
			}
		}
	}
*/
	void SetResBoard()
	{
		
	}
}
