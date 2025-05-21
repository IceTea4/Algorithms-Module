using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
    static List<Place> data1 = new List<Place>();
    static List<Place> data2 = new List<Place>();
    static List<Place> data3 = new List<Place>();

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

    class State
    {
        public List<Place>[] Routes;
        public HashSet<int> Visited;
        public double[] Distances;

        public State(List<Place>[] routes, HashSet<int> visited, double[] distances)
        {
            Routes = routes;
            Visited = visited;
            Distances = distances;
        }

        public State Clone()
        {
            return new State(
                Routes.Select(r => new List<Place>(r)).ToArray(),
                new HashSet<int>(Visited),
                (double[])Distances.Clone()
            );
        }

        public double Bound()
        {
            return Distances.Max();
        }
    }

    static List<Place> ReadPlacesFromCSV(string filePath)
    {
        var places = new List<Place>();
        foreach (var line in File.ReadLines(filePath))
        {
            var parts = line.Split(';');
            if (parts.Length < 5)
            {
                continue;
            }

            if (int.TryParse(parts[0], out int id) &&
                double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out double x) &&
                double.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
            {
                places.Add(new Place(id, x, y));
            }
        }

        return places;
    }

    static void Main()
    {
        string path = @"/Users/IceTea/Documents/Algoritmai/lab4/task2/IP_places_data_2025.csv";
        int startId = 67;
        List<Place> allPlaces = ReadPlacesFromCSV(path);
        var start = allPlaces.FirstOrDefault(p => p.Id == startId);

        if (start == null)
        {
            Console.WriteLine("Start location not found.");
            return;
        }

        var places = allPlaces.Where(p => p.Id != startId).ToList();

        State bestState = null;

        Stopwatch stopwatchSeq = Nuoseklus(start, bestState, places);

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

        Console.WriteLine("");

        places = allPlaces.Where(p => p.Id != startId).ToList();
        bestState = null;

        Stopwatch stopwatchPar = Lygiagretus(start, bestState, places);

        Console.WriteLine("");
        Console.WriteLine($"Islygiagretinimo koeficientas: {stopwatchSeq.Elapsed.TotalMilliseconds / stopwatchPar.Elapsed.TotalMilliseconds}");
        Console.WriteLine($"Islygiagretinimo efektyvumas (proc.): {(stopwatchSeq.Elapsed.TotalMilliseconds / stopwatchPar.Elapsed.TotalMilliseconds) / 10}");
    }

    static Stopwatch Lygiagretus(Place start, State bestState, List<Place> places)
    {
        object lockObj = new object();

        double bestTime = double.MaxValue;

        var initialRoutes = new[] {
            new List<Place> { start },
            new List<Place> { start },
            new List<Place> { start }
        };

        var initial = new State(initialRoutes, new HashSet<int>(), new double[] { 0, 0, 0 });

        var stopwatchPar = Stopwatch.StartNew();
        Parallel.ForEach(places, place =>
        {
            for (int bus = 0; bus < 3; bus++)
            {
                var newState = initial.Clone();
                var lastPlace = newState.Routes[bus].Last();
                newState.Routes[bus].Add(place);
                newState.Distances[bus] += lastPlace.DistanceTo(place);
                newState.Visited.Add(place.Id);

                var (localBestTime, localBestState) = BranchParallel(newState, places, start, Stopwatch.StartNew(), 1);

                if (localBestState != null && localBestTime < bestTime)
                {
                    lock (lockObj)
                    {
                        if (localBestTime < bestTime)
                        {
                            bestTime = localBestTime;
                            bestState = localBestState;
                        }
                    }
                }
            }
        });
        stopwatchPar.Stop();

        Console.WriteLine($"Parallel time: {stopwatchPar.Elapsed:hh\\.mm\\.ss\\.fff}");

        if (bestState != null)
        {
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"Bus {i + 1} count: {bestState.Routes[i].Count}; route distance: {bestState.Distances[i]:F2}");
            }
        }
        else
        {
            Console.WriteLine("No solution found within 10 seconds.");
        }

        return stopwatchPar;
    }

    static (double bestTime, State bestState) BranchParallel(State state, List<Place> remaining, Place start, Stopwatch timer, int depth = 0)
    {
        if (timer.Elapsed.TotalSeconds > 10)
            return (double.MaxValue, null);

        if (state.Visited.Count == remaining.Count)
        {
            var cloned = state.Clone();
            for (int i = 0; i < 3; i++)
            {
                var last = cloned.Routes[i].Last();
                cloned.Distances[i] += last.DistanceTo(start);
                cloned.Routes[i].Add(start);
            }

            double maxTime = cloned.Distances.Max();
            return (maxTime, cloned);
        }

        double bestTime = double.MaxValue;
        State bestState = null;

        var tasks = new List<Task<(double, State)>>();

        foreach (var place in remaining)
        {
            if (state.Visited.Contains(place.Id))
                continue;

            for (int bus = 0; bus < 3; bus++)
            {
                var newState = state.Clone();
                var lastPlace = newState.Routes[bus].Last();
                newState.Routes[bus].Add(place);
                newState.Distances[bus] += lastPlace.DistanceTo(place);
                newState.Visited.Add(place.Id);

                if (newState.Bound() >= bestTime)
                    continue;

                if (depth < 2)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        return BranchParallel(newState, remaining, start, timer, depth + 1);
                    }));
                }
                else
                {
                    var (bTime, bState) = BranchParallel(newState, remaining, start, timer, depth + 1);
                    if (bState != null && bTime < bestTime)
                    {
                        bestTime = bTime;
                        bestState = bState;
                    }
                }
            }
        }

        if (tasks.Count > 0)
        {
            Task.WaitAll(tasks.ToArray());

            foreach (var t in tasks)
            {
                var (tTime, tState) = t.Result;
                if (tState != null && tTime < bestTime)
                {
                    bestTime = tTime;
                    bestState = tState;
                }
            }
        }

        return (bestTime, bestState);
    }

    static Stopwatch Nuoseklus(Place start, State bestState, List<Place> places)
    {
        double bestTime = double.MaxValue;

        var initialRoutes = new[] {
            new List<Place> { start },
            new List<Place> { start },
            new List<Place> { start }
        };

        var initial = new State(initialRoutes, new HashSet<int>(), new double[] { 0, 0, 0 });

        var stopwatchSeq = Stopwatch.StartNew();
        Branch(initial, places, start, ref bestTime, ref bestState, Stopwatch.StartNew());
        stopwatchSeq.Stop();

        Console.WriteLine($"Sequential time: {stopwatchSeq.Elapsed:hh\\.mm\\.ss\\.fff}");

        if (bestState != null)
        {
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"Bus {i + 1} count: {bestState.Routes[i].Count}; route distance: {bestState.Distances[i]:F2}");
            }
        }
        else
        {
            Console.WriteLine("No solution found within 10 seconds.");
        }

        return stopwatchSeq;
    }

    static void Branch(State state, List<Place> remaining, Place start, ref double bestTime, ref State bestState, Stopwatch timer)
    {
        if (timer.Elapsed.TotalSeconds > 10)
        {
            Console.WriteLine("timeend");
            return;
        }

        if (state.Visited.Count == remaining.Count)
        {
            var cloned = state.Clone();
            for (int i = 0; i < 3; i++)
            {
                var last = cloned.Routes[i].Last();
                cloned.Distances[i] += last.DistanceTo(start);
                cloned.Routes[i].Add(start);
            }

            double maxTime = cloned.Distances.Max();
            if (maxTime < bestTime)
            {
                bestTime = maxTime;
                bestState = cloned;

                data1 = new List<Place>(cloned.Routes[0]);
                data2 = new List<Place>(cloned.Routes[1]);
                data3 = new List<Place>(cloned.Routes[2]);
            }
            return;
        }

        foreach (var place in remaining)
        {
            if (state.Visited.Contains(place.Id))
            {
                continue;
            }

            for (int bus = 0; bus < 3; bus++)
            {
                var newState = state.Clone();
                var lastPlace = newState.Routes[bus].Last();
                newState.Routes[bus].Add(place);
                newState.Distances[bus] += lastPlace.DistanceTo(place);
                newState.Visited.Add(place.Id);

                if (newState.Bound() < bestTime)
                    Branch(newState, remaining, start, ref bestTime, ref bestState, timer);
            }
        }
    }
}
