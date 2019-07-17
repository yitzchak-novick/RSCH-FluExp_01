using FluExp_01;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Innoculate2
{
    class Program
    {

        private static readonly int NUM_GRAPHS = 65;


        static object array_lock = new object();
        static double[] maxCompSize_RANDOM = new double[NUM_GRAPHS];
        static double[] maxCompSize_RANDFRIEND = new double[NUM_GRAPHS];
        static double[] maxCompSize_HIGHDEG = new double[NUM_GRAPHS];

        private static readonly double PCT = 0.22;


        static void Main(string[] args)
        {
            try
            {
                _Main(args);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
            Console.WriteLine("DONE. Any key...");
            Console.ReadKey();
        }

        static void _Main(string[] args)
        {
            GenerateGraphs();
            GetGraphStats();
            
            Parallel.ForEach(Enumerable.Range(1, NUM_GRAPHS), i => processGraph(i + ".graph", i-1));
            File.WriteAllLines("results.txt",
                Enumerable.Range(1, NUM_GRAPHS).Select(i =>
                    i.ToString() + "\tRandom:\t" + maxCompSize_RANDOM[i-1] + "\tRandom Friend:\t" + maxCompSize_RANDFRIEND[i-1] + "\tHigh Deg:\t" +
                    maxCompSize_HIGHDEG[i-1]));



        }

        static void GetGraphStats()
        {
            StringBuilder stats = new StringBuilder();
            for (int i = 1; i <= NUM_GRAPHS; i++)
            {
                Graph g = new Graph();
                File.ReadAllLines(i + ".graph").ToList().ForEach(l => g.AddEdge(Regex.Split(l, "\\s+")[0], Regex.Split(l, "\\s+")[1]));
                stats.AppendLine(
                    $"Graph {i}\tAssortativity:{g.Assortativity}\tAFI:\t{g.AFI}\tGFI:\t{g.GFI}\tLGFI:\t{g.LGFI}\tAvg Social Range:\t{g.AverageSocialRange}");
            }
            File.WriteAllText("GraphStats.txt", stats.ToString());
        }

        static void GenerateGraphs()
        {
            int N = 2000;
            int M = 4;

            Parallel.ForEach(Enumerable.Range(1, NUM_GRAPHS), i =>
            {
                Console.WriteLine("Generating graph " + i);
                Graph graph = new Graph();
                for (int j = 1; j < N; j++)
                    graph.AddEdge(j.ToString(), (j + 1).ToString());
                double alpha = TSRandom.DoubleBetween(-2.5, 2.5);
                for (int j = 0; j < (N - M - 1) * (M - 1); j++)
                {
                    var vertex1 = graph.Vertices.Where(v => v.Degree < (N - 1)).ChooseBiasedElement(v => v.Degree);
                    var neighbor = graph.Vertices.Where(v => !v.Neighbors.Contains(vertex1)).GroupBy(v => v.Degree)
                        .ChooseBiasedElement(g =>
                            Math.Pow(Math.Max(vertex1.Degree, g.Key) / (double) Math.Min(vertex1.Degree, g.Key), alpha))
                        .ChooseRandomElement();
                    graph.AddEdge(vertex1.Id, neighbor.Id);
                }

                File.WriteAllLines(i.ToString() + ".graph", graph.Edges.Select(e => e.Item1 + "\t" + e.Item2));
            });

        }

        static void processGraph(string fileName, int indx)
        {
            const int NUM_TRIALS = 50;

            Console.WriteLine("Processing graph " + fileName);
            Graph g = new Graph();
            File.ReadAllLines(fileName)
                .Select(l => new Tuple<string, string>(Regex.Split(l, @"\s+")[0], Regex.Split(l, @"\s+")[1])).ToList()
                .ForEach(e => g.AddEdge(e.Item1, e.Item2));

            List<int> rand = new List<int>();
            List<int> randFriend = new List<int>();
            List<int> highDeg = new List<int>();

            for (int i = 0; i < NUM_TRIALS; i++)
            {
                var g_Rand = g.Clone();
                var g_RandFriend = g.Clone();
                var g_HighDeg = g.Clone();

                for (int j = 0; j < Math.Floor(g.Vertices.Count() * PCT); j++)
                {
                    var gRandVertex = g_Rand.Vertices.Where(v => v.Degree > 0).ChooseRandomElement();
                    Innoculate(g_Rand, gRandVertex);


                    var gRandFriendVertex = g_RandFriend.Vertices.Where(v => v.Degree > 0).ChooseRandomElement()
                        .Neighbors.ChooseRandomElement();
                    Innoculate(g_RandFriend, gRandFriendVertex);


                    var gHighDegVertex = g_HighDeg.Vertices.OrderByDescending(v => v.Degree).First();
                    Innoculate(g_HighDeg, gHighDegVertex);
                }

                var maxCompRand = GetAllConnectedComponents(g_Rand).OrderByDescending(c => c.Count()).First().Count();
                rand.Add(maxCompRand);
                var maxCompRandFriend = GetAllConnectedComponents(g_RandFriend).OrderByDescending(c => c.Count())
                    .First().Count();
                randFriend.Add(maxCompRandFriend);
                var maxCompHighDeg = GetAllConnectedComponents(g_HighDeg).OrderByDescending(c => c.Count()).First()
                    .Count();
                highDeg.Add(maxCompHighDeg);

                File.WriteAllLines($"{fileName}_RANDOM_{i}_{maxCompRand}.graph", g_Rand.Edges.Select(e => e.Item1 + "\t" + e.Item2));
                File.WriteAllLines($"{fileName}_RANDOMFRIEND_{i}_{maxCompRandFriend}.graph", g_Rand.Edges.Select(e => e.Item1 + "\t" + e.Item2));
                File.WriteAllLines($"{fileName}_HIGHDEG_{i}_{maxCompHighDeg}.graph", g_Rand.Edges.Select(e => e.Item1 + "\t" + e.Item2));
            }
            lock(array_lock)
            {
                maxCompSize_RANDOM[indx] = rand.Average();
                maxCompSize_RANDFRIEND[indx] = randFriend.Average();
                maxCompSize_HIGHDEG[indx] = highDeg.Average();
            }
        }

        static void Innoculate(Graph g, Graph.Vertex v)
        {
            foreach (var neighbor in v.Neighbors.ToList())

            {
                g.Edges.RemoveWhere(t => (t.Item1 == v.Id && t.Item2 == neighbor.Id) ||
                                         (t.Item1 == neighbor.Id && t.Item2 == v.Id));
                neighbor.Neighbors.Remove(v);
                v.Neighbors.Remove(neighbor);
            }
        }

        static IEnumerable<IEnumerable<Graph.Vertex>> GetAllConnectedComponents(Graph g)
        {

            var AllVertices = new HashSet<Graph.Vertex>(g.Vertices);

            List<IEnumerable<Graph.Vertex>> components = new List<IEnumerable<Graph.Vertex>>();
            while (AllVertices.Any())
            {
                List<Graph.Vertex> component = new List<Graph.Vertex>();
                var v1 = AllVertices.First();
                Stack<Graph.Vertex> stack = new Stack<Graph.Vertex>();
                stack.Push(v1);
                AllVertices.Remove(v1);

                while (stack.Any())
                {
                    var currentV = stack.Pop();
                    AllVertices.Remove(currentV);
                    component.Add(currentV);
                    foreach (var currentVNeighbor in currentV.Neighbors)
                        if (AllVertices.Contains(currentVNeighbor))
                        {
                            stack.Push(currentVNeighbor);
                            AllVertices.Remove(currentVNeighbor);
                        }
                }

                components.Add(component);
            }

            return components;
        }
    }
}
