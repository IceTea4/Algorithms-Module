using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        int n = 32;

        Stopwatch stopwatch = Stopwatch.StartNew();

        //int rezultatas1 = SkaiciuotiBudus1(n);

        //Console.WriteLine("Būdų pasiekti " + n + " skersinį: " + rezultatas1);

        int rezultatas2 = SkaiciuotiBudus2(n);

        stopwatch.Stop();

        Console.WriteLine("Būdų pasiekti " + n + " skersinį: " + rezultatas2);
        Console.WriteLine(stopwatch.Elapsed);
    }

    static int SkaiciuotiBudus1(int n)
    {
        if (n < 0)
        {
            return 0;
        }

        if (n == 0)
        {
            return 1;
        }

        return SkaiciuotiBudus1(n - 1) + SkaiciuotiBudus1(n - 3);
    }

    static int SkaiciuotiBudus2(int n)
    {
        if (n < 0)
        {
            return 0;
        }

        int[] budai = new int[n + 1];
        budai[0] = 1;

        for (int i = 1; i <= n; i++)
        {
            budai[i] += budai[i - 1];

            if (i >= 3)
            {
                budai[i] += budai[i - 3];
            }
        }

        return budai[n];
    }
}
