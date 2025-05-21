using System.Diagnostics;
using System.Globalization;
using System.Numerics;

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

        public override string ToString()
        {
            return $"Place {Id} ({X}, {Y})";
        }
    }

    static void Main(string[] args)
    {
        int begining = 67;
        string filePath = @"/Users/IceTea/Documents/Algoritmai/lab4/task1/IP_places_data_2025.csv";

        List<Place> allPlaces = ReadPlacesFromCSV(filePath);

        Dictionary<int, Place> placeById = allPlaces.ToDictionary(p => p.Id);
        Place start = placeById.GetValueOrDefault(begining);

        if (start == null)
        {
            Console.WriteLine($"Start place (ID {begining}) not found.");
            return;
        }

        List<Place> unvisited = new List<Place>(allPlaces);
        unvisited.Remove(start);

        List<double> route1 = new List<double>();
        List<double> route2 = new List<double>();
        List<double> route3 = new List<double>();

        Place current1 = start;
        Place current2 = start;
        Place current3 = start;

        Stopwatch stopwatchSeq = Stopwatch.StartNew();
        data1.Add(start);
        data2.Add(start);
        data3.Add(start);

        Nuoseklus(unvisited, current1, current2, current3, route1, route2, route3, start);
        stopwatchSeq.Stop();

        Console.WriteLine($"Sequential time: {stopwatchSeq.Elapsed:hh\\.mm\\.ss\\.fff}");
        Console.WriteLine($"Bus 1 count: {route1.Count}; total distance: {route1.Sum():F2}\n");
        Console.WriteLine($"Bus 2 count: {route2.Count}; total distance: {route2.Sum():F2}\n");
        Console.WriteLine($"Bus 3 count: {route3.Count}; total distance: {route3.Sum():F2}\n");

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

        //////////////////////////////////////////////////////////////////////
        unvisited = new List<Place>(allPlaces);
        unvisited.Remove(start);

        route1 = new List<double>();
        route2 = new List<double>();
        route3 = new List<double>();

        current1 = start;
        current2 = start;
        current3 = start;

        Stopwatch stopwatchPar = Stopwatch.StartNew();
        Islygiagretintas(unvisited, route1, route2, route3, start, current1, current2, current3);
        stopwatchPar.Stop();

        Console.WriteLine($"Parallel time: {stopwatchPar.Elapsed:hh\\.mm\\.ss\\.fff}");
        Console.WriteLine($"Bus 1 count: {route1.Count}; total distance: {route1.Sum():F2}\n");
        Console.WriteLine($"Bus 2 count: {route2.Count}; total distance: {route2.Sum():F2}\n");
        Console.WriteLine($"Bus 3 count: {route3.Count}; total distance: {route3.Sum():F2}\n");

        Console.WriteLine("");
        Console.WriteLine($"Islygiagretinimo koeficientas: {stopwatchSeq.Elapsed.TotalMilliseconds / stopwatchPar.Elapsed.TotalMilliseconds}");
        Console.WriteLine($"Islygiagretinimo efektyvumas (proc.): {(stopwatchSeq.Elapsed.TotalMilliseconds / stopwatchPar.Elapsed.TotalMilliseconds) / 3}");
    }

    static void Islygiagretintas(List<Place> sharedUnvisited, List<double> route1, List<double> route2, List<double> route3, Place start, Place current1, Place current2, Place current3)
    {
        object lockObj = new object();

        Parallel.Invoke(
            () =>
            {
                while (true)
                {
                    Place next = null;

                    lock (lockObj)
                    {
                        if (sharedUnvisited.Count == 0)
                        {
                            break;
                        }

                        next = FindNearest(current1, sharedUnvisited);

                        if (next != null)
                        {
                            sharedUnvisited.Remove(next);
                        }
                    }

                    if (next != null)
                    {
                        route1.Add(current1.DistanceTo(next));
                        current1 = next;
                    }
                }
                route1.Add(current1.DistanceTo(start));
            },
            () =>
            {
                while (true)
                {
                    Place next = null;

                    lock (lockObj)
                    {
                        if (sharedUnvisited.Count == 0)
                        {
                            break;
                        }

                        next = FindNearest(current2, sharedUnvisited);

                        if (next != null)
                        {
                            sharedUnvisited.Remove(next);
                        }
                    }

                    if (next != null)
                    {
                        route2.Add(current2.DistanceTo(next));
                        current2 = next;
                    }
                }
                route2.Add(current2.DistanceTo(start));
            },
            () =>
            {
                while (true)
                {
                    Place next = null;

                    lock (lockObj)
                    {
                        if (sharedUnvisited.Count == 0)
                        {
                            break;
                        }

                        next = FindNearest(current3, sharedUnvisited);

                        if (next != null)
                        {
                            sharedUnvisited.Remove(next);
                        }
                    }

                    if (next != null)
                    {
                        route3.Add(current3.DistanceTo(next));
                        current3 = next;
                    }
                }
                route3.Add(current3.DistanceTo(start));
            }
        );
    }

    static void Nuoseklus(List<Place> unvisited, Place current1,
        Place current2, Place current3, List<double> route1,
        List<double> route2, List<double> route3, Place start)
    {
        while (unvisited.Count > 0)
        {
            Place nearest1 = FindNearest(current1, unvisited);
            if (nearest1 != null)
            {
                data1.Add(nearest1);
                route1.Add(current1.DistanceTo(nearest1));
                current1 = nearest1;
                unvisited.Remove(nearest1);
            }

            if (unvisited.Count == 0)
            {
                break;
            }

            Place nearest2 = FindNearest(current2, unvisited);
            if (nearest2 != null)
            {
                data2.Add(nearest2);
                route2.Add(current2.DistanceTo(nearest2));
                current2 = nearest2;
                unvisited.Remove(nearest2);
            }

            if (unvisited.Count == 0)
            {
                break;
            }

            Place nearest3 = FindNearest(current3, unvisited);
            if (nearest3 != null)
            {
                data3.Add(nearest3);
                route3.Add(current3.DistanceTo(nearest3));
                current3 = nearest3;
                unvisited.Remove(nearest3);
            }
        }

        route1.Add(current1.DistanceTo(start));
        route2.Add(current2.DistanceTo(start));
        route3.Add(current3.DistanceTo(start));
        data1.Add(start);
        data2.Add(start);
        data3.Add(start);
    }

    static Place FindNearest(Place from, List<Place> unvisited)
    {
        Place nearest = null;
        double minDist = double.MaxValue;

        foreach (var place in unvisited)
        {
            double dist = from.DistanceTo(place);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = place;
            }
        }

        return nearest;
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
}
