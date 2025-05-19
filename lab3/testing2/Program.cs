using System;
using System.Collections.Generic;


class Program
{
    static int[] groups = { 2, 3, 5, 2, 4, 3 };
    //static int[] groups = { 3, 5, 4, 2, 1 };
    static int[] capacity = { 5, 6, 7 };
    static int[,] dp;
    static int count = 0;

    static void Main()
    {
        RunTest(5, 10, 2, 20);
        Console.WriteLine("Warmup^");

        RunTest(5, 10, 2, 20);
        RunTest(10, 10, 3, 25);
        RunTest(20, 10, 5, 30);
        RunTest(50, 10, 6, 40);
        RunTest(100, 10, 10, 50);
        RunTest(200, 10, 15, 60);
        RunTest(500, 10, 20, 70);
        RunTest(1000, 10, 30, 100);

        //InitializeDP();
        //int recursiveResult = Solve(0, 0);
        //Console.WriteLine($"Maksimaliai perkeltų žmonių (rekursinis sprendimas): {recursiveResult}");
        //Console.WriteLine($"Veiksmu skaicius: {count}");

        //InitializeDP();
        //int dinamicResult = SolveDP();
        //Console.WriteLine($"Maksimaliai perkeltų žmonių (DP): {dinamicResult}");
        //Console.WriteLine($"Veiksmu skaicius: {count}");
    }

    static void RunTest(int groupSize, int groupMax, int ferryCount, int ferryMax)
    {
        Random rand = new Random();
        groups = new int[groupSize];
        capacity = new int[ferryCount];
        count = 0;

        for (int i = 0; i < groupSize; i++)
            groups[i] = rand.Next(1, groupMax + 1);

        for (int i = 0; i < ferryCount; i++)
            capacity[i] = rand.Next(groupMax + 1, ferryMax + 1);

        InitializeDP();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        int result = SolveDP();
        sw.Stop();

        //Console.WriteLine($"Veiksmu skaicius: {count}");
        Console.WriteLine($"Laikas: {sw.ElapsedTicks}");
    }

    static void InitializeDP()
    {
        int n = groups.Length;
        int m = capacity.Length;
        dp = new int[n + 1, m + 1];

        for (int i = 0; i <= n; i++)
        {
            for (int j = 0; j <= m; j++)
            {
                dp[i, j] = -1;
            }
        }
    }

    static int Solve(int i, int j)
    {
        if (i >= groups.Length ||
            j >= capacity.Length)
        {
            return 0;
        }

        if (dp[i, j] != -1)
        {
            return dp[i, j];
        }

        int maxPeople = 0;
        int sum = 0;

        for (int k = i; k < groups.Length; k++)
        {
            sum += groups[k];

            if (sum > capacity[j])
            {
                break;
            }

            int result = Solve(k + 1, j + 1);

            maxPeople = Math.Max(maxPeople,
                sum + result);
        }

        dp[i, j] = maxPeople;

        return maxPeople;
    }

    static int SolveDP()
    {
        int n = groups.Length;
        int m = capacity.Length;
        dp[0, 0] = 0;

        for (int j = 0; j < m; j++)
        {
            for (int i = 0; i <= n; i++)
            {
                if (dp[i, j] == -1)
                {
                    continue;
                }

                int sum = 0;

                for (int k = i; k < n; k++)
                {
                    sum += groups[k];

                    if (sum > capacity[j])
                    {
                        break;
                    }

                    dp[k + 1, j + 1] =
                        Math.Max(
                            dp[k + 1, j + 1],
                            dp[i, j] + sum
                        );
                }
            }
        }

        int max = 0;

        for (int i = 0; i <= n; i++)
        {
            max = Math.Max(max, dp[i, m]);
        }

        return max;
    }
}
