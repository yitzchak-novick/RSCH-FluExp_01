using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluExp_01;

namespace GenerateBAishGraphs
{
    class Program
    {
        public static void Main(String[] args)
        {
            StringBuilder results = new StringBuilder();

            object lockObject = new object();

            const int NUM_GRAPHS = 100;
            const int N = 2200;
            const int M = 4;

            double CountVertices = 0;
            double CountEdges = 0;
            double Assort = 0;
            double Afi = 0;
            double Gfi = 0;
            double Lgfi = 0;
            Dictionary<int, int> DegreeCounts = new Dictionary<int, int>();

            Parallel.ForEach(Enumerable.Range(1, NUM_GRAPHS), i =>
            {
                Graph baGraph = Graph.BarabasiAlbertGraph(N, M);
                lock (lockObject)
                {
                    CountVertices += baGraph.Vertices.Count();
                    CountEdges += baGraph.Edges.Count;
                    Assort += baGraph.Assortativity;
                    Afi += baGraph.AFI;
                    Gfi += baGraph.GFI;
                    Lgfi += baGraph.LGFI;
                    var groups = baGraph.Vertices.GroupBy(v => v.Degree);
                    foreach (var group in groups)
                    {
                        if (!DegreeCounts.ContainsKey(group.Key))
                            DegreeCounts[group.Key] = 0;
                        DegreeCounts[group.Key] += group.Count();
                    }
                }
            });

            results.AppendLine("REAL BA GRAPH:\n");
            results.AppendLine($"Vertices:\t{CountVertices / NUM_GRAPHS}");
            results.AppendLine($"Edges:\t{CountEdges / NUM_GRAPHS}");
            results.AppendLine($"Assortativity:\t{(Assort / NUM_GRAPHS).ToString("#.###")}");
            results.AppendLine($"Afi:\t{(Afi / NUM_GRAPHS).ToString("#.###")}");
            results.AppendLine($"Gfi:\t{(Gfi / NUM_GRAPHS).ToString("#.###")}");
            results.AppendLine($"LGfi:\t{(Lgfi / NUM_GRAPHS).ToString("#.###")}");
            results.AppendLine();
            DegreeCounts.OrderBy(kvp => kvp.Key).ToList()
                .ForEach(kvp =>
                    results.AppendLine("Degree " + kvp.Key + ":\tCount:\t" +
                                       (kvp.Value / (double) NUM_GRAPHS).ToString("#.###")));
            results.AppendLine();

            Console.Write(results.ToString());

            foreach (var alpha in new List<double>
                {-2.5, -2.0, -1.5, -1.25, -1.0, -0.85, -0.75, -0.5, 0.5, 0.75, 0.85, 1.0, 1.25, 1.5, 2.0, 2.5})
            {
                CountVertices = 0;
                CountEdges = 0;
                Assort = 0;
                Afi = 0;
                Gfi = 0;
                Lgfi = 0;
                DegreeCounts = new Dictionary<int, int>();
                Parallel.ForEach(Enumerable.Range(1, NUM_GRAPHS), i =>
                {
                    Graph graph = new Graph();
                    for (int j = 1; j < N; j++)
                        graph.AddEdge(j.ToString(), (j + 1).ToString());
                    for (int j = 0; j < (N - M - 1) * (M - 1); j++)
                    {
                        var vertex1 = graph.Vertices.Where(v => v.Degree < (N - 1)).ChooseBiasedElement(v => v.Degree);
                        var neighbor = graph.Vertices.Where(v => !v.Neighbors.Contains(vertex1)).GroupBy(v => v.Degree)
                            .ChooseBiasedElement(g =>
                                Math.Pow(
                                    Math.Max(vertex1.Degree, g.Key) / (double) Math.Min(vertex1.Degree, g.Key),
                                    alpha)).ChooseRandomElement();
                        graph.AddEdge(vertex1.Id, neighbor.Id);
                    }

                    lock (lockObject)
                    {
                        CountVertices += graph.Vertices.Count();
                        CountEdges += graph.Edges.Count;
                        Assort += graph.Assortativity;
                        Afi += graph.AFI;
                        Gfi += graph.GFI;
                        Lgfi += graph.LGFI;
                        var groups = graph.Vertices.GroupBy(v => v.Degree);
                        foreach (var group in groups)
                        {
                            if (!DegreeCounts.ContainsKey(group.Key))
                                DegreeCounts[group.Key] = 0;
                            DegreeCounts[group.Key] += group.Count();
                        }
                    }
                });

                results.AppendLine("Alpha:\t" + alpha + "\n");
                results.AppendLine($"Vertices:\t{CountVertices / NUM_GRAPHS}");
                results.AppendLine($"Edges:\t{CountEdges / NUM_GRAPHS}");
                results.AppendLine($"Assortativity:\t{(Assort / NUM_GRAPHS).ToString("#.###")}");
                results.AppendLine($"Afi:\t{(Afi / NUM_GRAPHS).ToString("#.###")}");
                results.AppendLine($"Gfi:\t{(Gfi / NUM_GRAPHS).ToString("#.###")}");
                results.AppendLine($"LGfi:\t{(Lgfi / NUM_GRAPHS).ToString("#.###")}");
                results.AppendLine();
                DegreeCounts.OrderBy(kvp => kvp.Key).ToList()
                    .ForEach(kvp =>
                        results.AppendLine("Degree " + kvp.Key + ":\tCount:\t" +
                                           (kvp.Value / (double) NUM_GRAPHS).ToString("#.###")));
                results.AppendLine();

                Console.Write(results.ToString());
            }

            File.WriteAllText("results.txt", results.ToString());
            Console.WriteLine("DONE. Any key...");
            Console.ReadKey();
        }

        /*
        static void Main(string[] args)
        {
    
            StringBuilder stats = new StringBuilder();

            Graph[] allGraphs = new Graph[50];

            Parallel.ForEach(Enumerable.Range(1, 25), i => allGraphs[i-1] = getGraph(true, TSRandom.DoubleBetween(0.5, 2.5)));
            Parallel.ForEach(Enumerable.Range(26, 25), i => allGraphs[i-1] = getGraph(false, TSRandom.DoubleBetween(0.5, 2.5)));

            for (var i = 0; i < allGraphs.Length; i++)
            {
                var g = allGraphs[i];
                stats.AppendLine($"Graph {i+1}");
                stats.AppendLine(g.ToString());
                stats.AppendLine(g.Histogram);
                stats.AppendLine();

                File.WriteAllLines((i+1) + ".graph", g.Edges.Select(e => e.Item1 + "\t" + e.Item2));
            }

            File.WriteAllText("stats.txt", stats.ToString());
            

            /*
            Graph graph = getGraph();

            Console.WriteLine(graph);
            Console.WriteLine(graph.Histogram);
            

            Console.WriteLine("Done");
            Console.ReadKey();
        }

        static Graph getGraph(bool biasedToSimilar = true, double alpha = 1.0)
        {
            const int N = 1200;
            const int M = 3;

            Graph graph = new Graph();

            for (int i = 0; i < N; i++)
                graph.AddEdge(i.ToString(), (i + 1).ToString());

            for (int i = 0; i < M * N; i++)
            {
                var vertex1 = graph.Vertices.Where(v => v.Degree <= 1000).ChooseBiasedElement(v => v.Degree);
                var remainingVertexGroups =
                    graph.Vertices.Where(v => !v.Neighbors.Contains(vertex1)).GroupBy(v => v.Degree).ToList();
                var maxRatio = remainingVertexGroups.Max(g =>
                    (double)Math.Max(g.Key, vertex1.Degree) / Math.Min(g.Key, vertex1.Degree));
                var minRatio = remainingVertexGroups.Min(g =>
                    (double)Math.Max(g.Key, vertex1.Degree) / Math.Min(g.Key, vertex1.Degree));
                IGrouping<int, Graph.Vertex> selectedGroup;
                if (biasedToSimilar)
                    selectedGroup = remainingVertexGroups.ChooseBiasedElement(g =>
                        Math.Pow(
                                maxRatio - ((double) Math.Max(g.Key, vertex1.Degree) / Math.Min(g.Key, vertex1.Degree)) + minRatio
                            , alpha)
                    );
                else
                    selectedGroup = remainingVertexGroups.ChooseBiasedElement(g =>
                        Math.Pow(
                                ((double)Math.Max(g.Key, vertex1.Degree) / Math.Min(g.Key, vertex1.Degree))
                            , alpha)
                    );
                var neighbor = selectedGroup.ChooseRandomElement();
                graph.AddEdge(vertex1.Id, neighbor.Id);
            }

            return graph;
        }*/
    }
}
