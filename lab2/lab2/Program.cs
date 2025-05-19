using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        int n = 16000;
        int[] testArray = new int[n];
        Console.WriteLine(testArray.Length);

        Stopwatch stopwatch = Stopwatch.StartNew();

        long result = methodToAnalysis(testArray);

        stopwatch.Stop();

        Console.WriteLine(stopwatch.ElapsedMilliseconds);
    }

    public static long methodToAnalysis(int[] arr)
    {
        long n = arr.Length;
        long k = n;

        for (int i = 0; i < n * 2; i++)
        {
            for (int j = 0; j < n / 2; j++)
            {
                k -= 2;
            }
        }

        return k;
    }
}
