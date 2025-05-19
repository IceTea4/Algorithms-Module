using System;

class Program
{
    private static int actionCount = 0;

    static void Main()
    {
        int n = 1000;
        long result = Method(n);

        Console.WriteLine(actionCount);
    }

    public static long Method(int n)
    {
        if (n <= 1)
        {
            return 1;
        }

        for (int i = 0; i < (n - 1); i++)
        {
            actionCount++;
        }

        Method(n / 9);
        Method(n / 9);

        return actionCount;
    }
}

