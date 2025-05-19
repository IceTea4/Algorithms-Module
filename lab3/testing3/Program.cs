using System;
using System.Collections.Generic;

class Program
{
    static int[] groups = { 2, 3, 5, 2, 4, 3 };
    static int[] ferries = { 5, 6, 7 };
    static Dictionary<string, int> memo = new();
    static int count = 0;

    static void Main()
    {
        //RunTest(2, 4, 1, 5);
        //Console.WriteLine("Warmup^");

        RunTest(2, 4, 1, 5);
        RunTest(3, 4, 1, 5);
        RunTest(4, 4, 2, 6);
        RunTest(5, 4, 2, 6);
        RunTest(6, 4, 2, 7);
        RunTest(7, 4, 2, 7);
        RunTest(8, 4, 3, 8);
        RunTest(9, 4, 3, 8);

        //int result = Dp(0, ferries);
        //Console.WriteLine("Maksimaliai perkelta žmonių: " + result);

        //int result2 = IterativeDP();
        //Console.WriteLine("Maksimaliai perkelta žmonių: " + result2);
    }

    static void RunTest(int groupSize, int groupMax, int ferryCount, int ferryMax)
    {
        Random rand = new Random();
        groups = new int[groupSize];
        ferries = new int[ferryCount];
        count = 0;
        memo = new();

        for (int i = 0; i < groupSize; i++)
            groups[i] = rand.Next(1, groupMax + 1);

        for (int i = 0; i < ferryCount; i++)
            ferries[i] = rand.Next(groupMax + 1, ferryMax + 1);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        int result = IterativeDP();
        sw.Stop();

        Console.WriteLine($"Veiksmu skaicius: {count}");
        //Console.WriteLine($"Laikas: {sw.ElapsedTicks}");
    }

    static int Dp(int usedMask, int[] capacities)
    {
        string key = usedMask + "|"
            + string.Join(",", capacities);
        if (memo.ContainsKey(key))
        {
            return memo[key];
        }

        int maxValue = 0;

        for (int i = 0; i < groups.Length; i++)
        {
            if ((usedMask & (1 << i)) != 0)
            {
                continue;
            }

            for (int j = 0; j < capacities.Length; j++)
            {
                if (groups[i] <= capacities[j])
                {
                    int[] newCaps
                        = new int[capacities.Length];
                    Array.Copy(
                        capacities,
                        newCaps,
                        capacities.Length);
                    newCaps[j] -= groups[i];

                    int newMask
                        = usedMask | (1 << i);
                    int value = groups[i]
                        + Dp(newMask, newCaps);

                    if (value > maxValue)
                    {
                        maxValue = value;
                    }
                }
            }
        }

        memo[key] = maxValue;
        return maxValue;
    }

    static int IterativeDP()
    {
        var dp = new Dictionary<string, int>();
        var queue = new Queue<(
            int usedMask, int[] caps)>();

        int[] startCaps = (int[])ferries.Clone();
        string startKey = "0|"
            + string.Join(",", startCaps);
        dp[startKey] = 0;
        queue.Enqueue((0, startCaps));

        int maxPeople = 0;

        while (queue.Count > 0)
        {
            var (usedMask, caps)
                = queue.Dequeue();
            string stateKey = usedMask + "|"
                + string.Join(",", caps);
            int currentValue = dp[stateKey];

            maxPeople = Math.Max(
                maxPeople, currentValue);

            for (int i = 0; i < groups.Length; i++)
            {
                if ((usedMask & (1 << i)) != 0)
                {
                    continue;
                }

                for (int j = 0; j < caps.Length; j++)
                {
                    if (groups[i] > caps[j])
                    {
                        continue;
                    }

                    int[] newCaps
                        = (int[])caps.Clone();
                    newCaps[j] -= groups[i];
                    int newMask = usedMask | (1 << i);
                    string newKey = newMask + "|"
                        + string.Join(",", newCaps);
                    int newValue = currentValue + groups[i];

                    if (!dp.ContainsKey(newKey)
                        || newValue > dp[newKey])
                    {
                        dp[newKey] = newValue;
                        queue.Enqueue(
                            (newMask, newCaps));
                    }
                }
            }
        }

        return maxPeople;
    }
}
