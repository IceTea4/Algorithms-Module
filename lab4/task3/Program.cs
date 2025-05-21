using System.Diagnostics;
using System.Globalization;

class Program
{
    static List<Place> data1 = new List<Place>();
    static List<Place> data2 = new List<Place>();
    static List<Place> data3 = new List<Place>();

    static Random rand = new Random();

    class Place
    {
        public int Id;
        public double X;
        public double Y;

        public Place(int id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
        }

        public double DistanceTo(Place other)
        {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }
    }

    class BusSolution
    {
        public List<Place>[] Routes = new List<Place>[3];

        public BusSolution()
        {
            for (int i = 0; i < 3; i++)
                Routes[i] = new List<Place>();
        }

        public BusSolution Clone()
        {
            var clone = new BusSolution();
            for (int i = 0; i < 3; i++)
                clone.Routes[i] = new List<Place>(Routes[i]);
            return clone;
        }
    }

    static void Main(string[] args)
    {
        string filePath = @"/Users/IceTea/Documents/Algoritmai/lab4/task3/IP_places_data_2025.csv";
        int startId = 67;

        var allPlaces = ReadPlacesFromCSV(filePath);
        var placeById = allPlaces.ToDictionary(p => p.Id);
        var start = placeById[startId];
        allPlaces.Remove(start);

        Stopwatch stopwatchSeq = Stopwatch.StartNew();
        var best = SimulatedAnnealingNuoseklus(allPlaces, start, Stopwatch.StartNew());
        stopwatchSeq.Stop();

        List<string> lines1 = new List<string>();

        foreach (var place in data1)
        {
            lines1.Add($"{place.X} {place.Y}");
        }

        File.WriteAllLines("data1.txt", lines1);

        List<string> lines2 = new List<string>();

        foreach (var place in data2)
        {
            lines2.Add($"{place.X} {place.Y}");
        }

        File.WriteAllLines("data2.txt", lines2);

        List<string> lines3 = new List<string>();

        foreach (var place in data3)
        {
            lines3.Add($"{place.X} {place.Y}");
        }

        File.WriteAllLines("data3.txt", lines3);

        Console.WriteLine($"Sequential time: {stopwatchSeq.Elapsed:hh\\.mm\\.ss\\.fff}");
        for (int i = 0; i < 3; i++)
        {
            double dist = RouteDistance(best.Routes[i], start);
            Console.WriteLine($"Bus {i + 1} count: {best.Routes[i].Count}; distance: {dist:F2}");
        }

        Console.WriteLine("");

        var stopwatchPar = Stopwatch.StartNew();
        best = SimulatedAnnealingLygiagretus(allPlaces, start, Stopwatch.StartNew());
        stopwatchPar.Stop();

        Console.WriteLine($"Parallel time: {stopwatchPar.Elapsed:hh\\.mm\\.ss\\.fff}");
        for (int i = 0; i < 3; i++)
        {
            double dist = RouteDistance(best.Routes[i], start);
            Console.WriteLine($"Bus {i + 1} count: {best.Routes[i].Count}; distance: {dist:F2}");
        }

        Console.WriteLine("");
        Console.WriteLine($"Islygiagretinimo koeficientas: {stopwatchSeq.Elapsed.TotalMilliseconds / stopwatchPar.Elapsed.TotalMilliseconds}");
        Console.WriteLine($"Islygiagretinimo efektyvumas (proc.): {(stopwatchSeq.Elapsed.TotalMilliseconds / stopwatchPar.Elapsed.TotalMilliseconds) / 10}");
    }

    static BusSolution SimulatedAnnealingLygiagretus(List<Place> allPlaces, Place start, Stopwatch timer)
    {
        double temp = 1000;
        double coolingRate = 0.99999;
        double minTemp = 1e-4;
        int noImprovement = 0;
        int maxNoImprovement = 10000;
        int candidatesPerIter = Environment.ProcessorCount;

        var current = CreateInitialBusSolution(allPlaces);
        var best = current.Clone();
        double bestDist = best.Routes.Max(r => RouteDistance(r, start));

        while (temp > minTemp && noImprovement < maxNoImprovement)
        {
            if (timer.Elapsed.TotalSeconds > 10)
            {
                Console.WriteLine("timeend");
                break;
            }

            var candidates = new BusSolution[candidatesPerIter];
            Parallel.For(0, candidatesPerIter, i =>
            {
                candidates[i] = MutateBusSolution(current);
            });

            BusSolution bestCandidate = null;
            double bestCandidateDist = double.MaxValue;

            foreach (var candidate in candidates)
            {
                double dist = candidate.Routes.Max(r => RouteDistance(r, start));
                if (dist < bestCandidateDist)
                {
                    bestCandidate = candidate;
                    bestCandidateDist = dist;
                }
            }

            double delta = bestCandidateDist - bestDist;
            if (delta < 0 || Math.Exp(-delta / temp) > rand.NextDouble())
            {
                current = bestCandidate;
                if (bestCandidateDist < bestDist)
                {
                    best = bestCandidate.Clone();
                    bestDist = bestCandidateDist;
                    noImprovement = 0;
                }
                else
                {
                    noImprovement++;
                }
            }

            temp *= coolingRate;
        }

        return best;
    }

    static BusSolution SimulatedAnnealingNuoseklus(List<Place> allPlaces, Place start, Stopwatch timer)
    {
        double temp = 1000;
        double coolingRate = 0.99999;
        double minTemp = 1e-4;
        int noImprovement = 0;
        int maxNoImprovement = 10000;

        var current = CreateInitialBusSolution(allPlaces);
        var best = current.Clone();
        double bestDist = best.Routes.Max(r => RouteDistance(r, start));

        int i = 0;

        while (temp > minTemp && noImprovement < maxNoImprovement)
        {
            if (timer.Elapsed.TotalSeconds > 10)
            {
                Console.WriteLine("timeend");
                break;
            }

            var next = MutateBusSolution(current);
            double nextDist = next.Routes.Max(r => RouteDistance(r, start));
            double delta = nextDist - bestDist;
            i++;

            if (delta < 0 || Math.Exp(-delta / temp) > rand.NextDouble())
            {
                current = next;
                if (nextDist < bestDist)
                {
                    best = next.Clone();
                    bestDist = nextDist;
                    noImprovement = 0;
                    //Console.WriteLine($"{i} = {bestDist}");
                }
                else noImprovement++;
            }

            temp *= coolingRate;
        }

        data1 = new List<Place>(best.Routes[0]);
        data2 = new List<Place>(best.Routes[1]);
        data3 = new List<Place>(best.Routes[2]);

        return best;
    }

    static BusSolution CreateInitialBusSolution(List<Place> places)
    {
        var solution = new BusSolution();
        var shuffled = places.OrderBy(x => rand.Next()).ToList();

        for (int i = 0; i < shuffled.Count; i++)
            solution.Routes[i % 3].Add(shuffled[i]);

        return solution;
    }

    static BusSolution MutateBusSolution(BusSolution solution)
    {
        var newSol = solution.Clone();

        if (rand.NextDouble() < 0.5)
        {
            int from = rand.Next(3);
            int to = rand.Next(3);
            if (from != to && newSol.Routes[from].Count > 0)
            {
                int index = rand.Next(newSol.Routes[from].Count);
                var place = newSol.Routes[from][index];
                newSol.Routes[from].RemoveAt(index);
                newSol.Routes[to].Add(place);
            }
        }
        else
        {
            int bus = rand.Next(3);
            if (newSol.Routes[bus].Count > 1)
            {
                int i = rand.Next(newSol.Routes[bus].Count);
                int j = rand.Next(newSol.Routes[bus].Count);
                (newSol.Routes[bus][i], newSol.Routes[bus][j]) = (newSol.Routes[bus][j], newSol.Routes[bus][i]);
            }
        }

        return newSol;
    }

    static double RouteDistance(List<Place> route, Place start)
    {
        double dist = 0.0;
        var current = start;

        foreach (var p in route)
        {
            dist += current.DistanceTo(p);
            current = p;
        }

        dist += current.DistanceTo(start);

        return dist;
    }

    static List<Place> ReadPlacesFromCSV(string filePath)
    {
        var places = new List<Place>();

        foreach (var line in File.ReadLines(filePath))
        {
            var parts = line.Split(';');
            if (parts.Length < 5) continue;

            if (int.TryParse(parts[0], out int id) &&
                double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out double x) &&
                double.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
            {
                places.Add(new Place(id, x, y));
            }
        }

        return places;
    }
}
