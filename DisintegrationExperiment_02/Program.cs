using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluExp_01;
using GraphLibyn;
using GraphLibYN_2019_05;
using Graph = GraphLibYN_2019_05.Graph;

namespace DisintegrationExperiment_02
{
    class Program
    {
        private static int WRITE_AT = 5;
        private static int NODES = 1000;
        private static double ER_P = 0.025;
        private static int BA_M = 3;
        private static int NUM_GRAPHS = 10;
        private static int DATA_POINTS = 20;

        private static Graph Clone(Graph g)
        {
            Graph graph = new Graph();
            g.Edges.ToList().ForEach(e => graph.AddEdge(e.V1.Id, e.V2.Id));
            return graph;
        }

        static ConcurrentBag<Graph> OrigErGraphBag = new ConcurrentBag<Graph>();
        static ConcurrentBag<Graph> OrigBaGraphBag = new ConcurrentBag<Graph>();
    

        static IEnumerable<Graph> ErGraphs => OrigErGraphBag.Select(g => Clone(g));
        static IEnumerable<Graph> BaGraphs => OrigBaGraphBag.Select(g => Clone(g));

        static void Main(string[] args)
        {
            //DisintegrationExperiment_02(args);
            CaptureDisintegrationOfErGraph();
        }

        // A method just to disintegrate an ER graph and capture the animation 
        static void CaptureDisintegrationOfErGraph()
        {

            Graph graph = new Graph();
            GraphLibyn.Graph.NewErdosRenyiGraph(300, 0.025, new Randomizer()).Edges.ToList()
                .ForEach(e => graph.AddEdge(int.Parse(e.Item1.Replace("n", "")).ToString("000#"),
                    int.Parse(e.Item2.Replace("n", "")).ToString("000#")));

            Graph clonedGraph = Clone(graph);

            var originalVertices = graph.Vertices.OrderBy(v => v.Id).ToList();
            var origSize = originalVertices.Count;
            string origVerticesString = String.Join("\r\n", originalVertices.Select(v => v.Id));

            var originalEdges = graph.Edges.OrderBy(e => e.V1.Id).ThenBy(e => e.V2.Id).ToList();
            string origEdgesString =  String.Join("\r\n", originalEdges.Select(e => e.V1.Id + " " + e.V2.Id));
            
            Random rand = new Random();
            var vertexCollection = graph.Vertices.OrderBy(v => rand.NextDouble()).ToList();

            List<Tuple<String, String, String>> removals = new List<Tuple<String, String, String>>();
            foreach (var vertex in vertexCollection)
            {
                Tuple<String, String, String> removal = new Tuple<string, string, string>(vertex.Id, "", "");
                string removalString = "";
                if (vertex.Neighbors.Any())
                {
                    var neighbor = vertex.Neighbors.ChooseRandomElement();
                    removal = new Tuple<string, string, string>(removal.Item1, neighbor.Id, "");
                    removalString = vertex.Id + " -> " + neighbor.Id;
                    neighbor.Neighbors.ToList().ForEach(n =>
                    {
                        removalString += " (" + (neighbor.Id.CompareTo(n.Id) < 0 ? neighbor.Id : n.Id) + ", " +
                                         (neighbor.Id.CompareTo(n.Id) > 0 ? neighbor.Id : n.Id) + ")";
                        graph.RemoveEdge(neighbor.Id, n.Id);
                    });
                }
                removals.Add(new Tuple<string, string, string>(removal.Item1, removal.Item2, removalString));
            }

            var connectedComponents = GetAllConnectedComponents(graph);
            var largest = connectedComponents.OrderByDescending(c => c.Count).First();
            var largestSize = largest.Count;

            var verticesInLargest = largest.Select(v => v.Id)
                .Union(clonedGraph.Vertices.Where(v => largest.Any(ln => v.Neighbors.Select(n => n.Id).Contains(ln.Id)))
                    .Select(v => v.Id)).OrderBy(s => s).ToList();
            var origEdgesInLargest = originalEdges
                .Where(e => largest.Select(v => v.Id).Contains(e.V1.Id) || largest.Select(v => v.Id).Contains(e.V2.Id))
                .OrderBy(e => e.V1.Id).ThenBy(e => e.V2.Id).ToList();

            var verticesInLargestString = String.Join("\r\n", verticesInLargest);
            var origEdgesInLargestString = String.Join("\r\n", origEdgesInLargest.Select(e => e.V1.Id + " " + e.V2.Id));
            var removalsAffectingLargest = String.Join("\r\n",
                removals.Where(t => verticesInLargest.Any(v => t.Item1 == v))
                    .Select(t => t.Item3));


        }

        static void DisintegrationExperiment_02(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToUpper())
                {
                    case "-WRITEAT":
                        WRITE_AT = int.Parse(args[++i]);
                        break;
                    case "-NODES":
                        NODES = int.Parse(args[++i]);
                        break;
                    case "-ERP":
                        ER_P = double.Parse(args[++i]);
                        break;
                    case "-BAM":
                        BA_M = int.Parse(args[++i]);
                        break;
                    case "-NUMGRAPHS":
                        NUM_GRAPHS = int.Parse(args[++i]);
                        break;
                    case "-DATAPOINTS":
                        DATA_POINTS = int.Parse(args[++i]);
                        break;
                }
            }

            Console.WriteLine($"Starting, generating graphs {DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()}");
            InitializeGraphs();
            Console.WriteLine($"Graphs Generated, Starting RV {DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()}");

            ConcurrentBag<CostResultDictionary> allRandomVertexErResults = new ConcurrentBag<CostResultDictionary>();
            Parallel.ForEach(ErGraphs, graph => allRandomVertexErResults.Add(InoculateRandomVertices(graph)));
            ConcurrentBag<CostResultDictionary> allRandomVertexBaResults = new ConcurrentBag<CostResultDictionary>();
            Parallel.ForEach(BaGraphs, graph => allRandomVertexBaResults.Add(InoculateRandomVertices(graph)));

            WriteMatrixToDisk(GetMatrixForAllResults(allRandomVertexErResults.Select(crd => crd.Inoculations).ToArray()), "Rv_Er_Inoculations.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allRandomVertexErResults.Select(crd => crd.TotalInterviews).ToArray()), "Rv_Er_TotalInterviews.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allRandomVertexErResults.Select(crd => crd.VerticesInterviewed).ToArray()), "Rv_Er_VerticesInterviewed.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allRandomVertexBaResults.Select(crd => crd.Inoculations).ToArray()), "Rv_Ba_Inoculations.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allRandomVertexBaResults.Select(crd => crd.TotalInterviews).ToArray()), "Rv_Ba_TotalInterviews.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allRandomVertexBaResults.Select(crd => crd.VerticesInterviewed).ToArray()), "Rv_Ba_VerticesInterviewed.txt");

            Console.WriteLine($"Finished RV, Starting RF {DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()}");

            ConcurrentBag<CostResultDictionary> allRandomFriendErResults = new ConcurrentBag<CostResultDictionary>();
            Parallel.ForEach(ErGraphs, graph => allRandomFriendErResults.Add(InoculateRandomFriends(graph)));
            ConcurrentBag<CostResultDictionary> allRandomFriendBaResults = new ConcurrentBag<CostResultDictionary>();
            Parallel.ForEach(BaGraphs, graph => allRandomFriendBaResults.Add(InoculateRandomFriends(graph)));

            WriteMatrixToDisk(GetMatrixForAllResults(allRandomFriendErResults.Select(crd => crd.Inoculations).ToArray()), "Rf_Er_Inoculations.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allRandomFriendErResults.Select(crd => crd.TotalInterviews).ToArray()), "Rf_Er_TotalInterviews.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allRandomFriendErResults.Select(crd => crd.VerticesInterviewed).ToArray()), "Rf_Er_VerticesInterviewed.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allRandomFriendBaResults.Select(crd => crd.Inoculations).ToArray()), "Rf_Ba_Inoculations.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allRandomFriendBaResults.Select(crd => crd.TotalInterviews).ToArray()), "Rf_Ba_TotalInterviews.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allRandomFriendBaResults.Select(crd => crd.VerticesInterviewed).ToArray()), "Rf_Ba_VerticesInterviewed.txt");

            Console.WriteLine($"Finished RF, Starting MoKF_2 {DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()}");

            ConcurrentBag<CostResultDictionary> allMaxOf2FriendsErResults = new ConcurrentBag<CostResultDictionary>();
            Parallel.ForEach(ErGraphs, graph => allMaxOf2FriendsErResults.Add(InoculateMaxOfKRandomFriends(graph, 2)));
            ConcurrentBag<CostResultDictionary> allMaxOf2FriendsBaResults = new ConcurrentBag<CostResultDictionary>();
            Parallel.ForEach(BaGraphs, graph => allMaxOf2FriendsBaResults.Add(InoculateMaxOfKRandomFriends(graph, 2)));

            WriteMatrixToDisk(GetMatrixForAllResults(allMaxOf2FriendsErResults.Select(crd => crd.Inoculations).ToArray()), "Mo2F_Er_Inoculations.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allMaxOf2FriendsErResults.Select(crd => crd.TotalInterviews).ToArray()), "Mo2F_Er_TotalInterviews.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allMaxOf2FriendsErResults.Select(crd => crd.VerticesInterviewed).ToArray()), "Mo2F_Er_VerticesInterviewed.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allMaxOf2FriendsBaResults.Select(crd => crd.Inoculations).ToArray()), "Mo2F_Ba_Inoculations.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allMaxOf2FriendsBaResults.Select(crd => crd.TotalInterviews).ToArray()), "Mo2F_Ba_TotalInterviews.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allMaxOf2FriendsBaResults.Select(crd => crd.VerticesInterviewed).ToArray()), "Mo2F_Ba_VerticesInterviewed.txt");

            Console.WriteLine($"Finished MoKF_2, Starting MoKF_3 {DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()}");

            ConcurrentBag<CostResultDictionary> allMaxOf3FriendsErResults = new ConcurrentBag<CostResultDictionary>();
            Parallel.ForEach(ErGraphs, graph => allMaxOf3FriendsErResults.Add(InoculateMaxOfKRandomFriends(graph, 3)));
            ConcurrentBag<CostResultDictionary> allMaxOf3FriendsBaResults = new ConcurrentBag<CostResultDictionary>();
            Parallel.ForEach(BaGraphs, graph => allMaxOf3FriendsBaResults.Add(InoculateMaxOfKRandomFriends(graph, 3)));

            WriteMatrixToDisk(GetMatrixForAllResults(allMaxOf3FriendsErResults.Select(crd => crd.Inoculations).ToArray()), "Mo3F_Er_Inoculations.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allMaxOf3FriendsErResults.Select(crd => crd.TotalInterviews).ToArray()), "Mo3F_Er_TotalInterviews.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allMaxOf3FriendsErResults.Select(crd => crd.VerticesInterviewed).ToArray()), "Mo3F_Er_VerticesInterviewed.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allMaxOf3FriendsBaResults.Select(crd => crd.Inoculations).ToArray()), "Mo3F_Ba_Inoculations.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allMaxOf3FriendsBaResults.Select(crd => crd.TotalInterviews).ToArray()), "Mo3F_Ba_TotalInterviews.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allMaxOf3FriendsBaResults.Select(crd => crd.VerticesInterviewed).ToArray()), "Mo3F_Ba_VerticesInterviewed.txt");

            Console.WriteLine($"Finished MoKF_3, Starting MF {DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()}");

            ConcurrentBag<CostResultDictionary> allMaxFriendsErResults = new ConcurrentBag<CostResultDictionary>();
            Parallel.ForEach(ErGraphs, graph => allMaxFriendsErResults.Add(InoculateMaxFriendOfRandomVertices(graph)));
            ConcurrentBag<CostResultDictionary> allMaxFriendsBaResults = new ConcurrentBag<CostResultDictionary>();
            Parallel.ForEach(BaGraphs, graph => allMaxFriendsBaResults.Add(InoculateMaxFriendOfRandomVertices(graph)));

            WriteMatrixToDisk(GetMatrixForAllResults(allMaxFriendsErResults.Select(crd => crd.Inoculations).ToArray()), "Mf_Er_Inoculations.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allMaxFriendsErResults.Select(crd => crd.TotalInterviews).ToArray()), "Mf_Er_TotalInterviews.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allMaxFriendsErResults.Select(crd => crd.VerticesInterviewed).ToArray()), "Mf_Er_VerticesInterviewed.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allMaxFriendsBaResults.Select(crd => crd.Inoculations).ToArray()), "Mf_Ba_Inoculations.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allMaxFriendsBaResults.Select(crd => crd.TotalInterviews).ToArray()), "Mf_Ba_TotalInterviews.txt");
            WriteMatrixToDisk(GetMatrixForAllResults(allMaxFriendsBaResults.Select(crd => crd.VerticesInterviewed).ToArray()), "Mf_Ba_VerticesInterviewed.txt");

  
            Console.WriteLine("All done");
            Console.ReadKey();
        }

        static void WriteMatrixToDisk(String[,] matrix, String fileName)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (j > 0)
                        sb.Append(',');
                    sb.Append(matrix[i, j]);
                }

                sb.AppendLine();
            }

            File.WriteAllText(fileName, sb.ToString());
        }

        static void InitializeGraphs()
        {
            Parallel.ForEach(Enumerable.Range(0, NUM_GRAPHS), i =>
            {
                GraphLibyn.Graph graph =
                    GraphLibyn.Graph.NewErdosRenyiGraph(NODES, ER_P, new Randomizer(TSRandom.NextRandom()));
                Graph g = new Graph();
                graph.Edges.ToList().ForEach(t => g.AddEdge(t.Item1, t.Item2));
                OrigErGraphBag.Add(g);
            });

            Parallel.ForEach(Enumerable.Range(0, NUM_GRAPHS), i =>
            {
                GraphLibyn.Graph graph =
                    GraphLibyn.Graph.NewBarabasiAlbertGraph(NODES, BA_M, new Randomizer(TSRandom.NextRandom()));
                Graph g = new Graph();
                graph.Edges.ToList().ForEach(t => g.AddEdge(t.Item1, t.Item2));
                OrigBaGraphBag.Add(g);
            });
        }

        static String[,] GetMatrixForAllResults(SortedList<int, double>[] results)
        {
            var maxOfMaxKeys = results.Max(sl => sl.Keys.Last());
            foreach (var sortedList in results)
            {
                if (!sortedList.ContainsKey(maxOfMaxKeys))
                {
                    var val = sortedList[sortedList.Keys.Last()];
                    sortedList[maxOfMaxKeys] = val;
                }
            }
            int[][] keySets = results.Select(sl => sl.Keys.ToArray()).ToArray();
            double[][] valueSets = results.Select(sl => sl.Values.ToArray()).ToArray();

            int MAX_KEY = keySets.Min(arr => arr.Last());
            String[,] matrix = new string[results.Length + 4, DATA_POINTS+1];
            int step = MAX_KEY / DATA_POINTS;
            matrix[0, 0] = "Exp";
            for (int i = 1; i <= DATA_POINTS; i++)
                matrix[0, i] = (step * (i-1)).ToString();
           
            for (int i = 1; i <= results.Length; i++)
                matrix[i, 0] = i.ToString();

            int[] currIndexes = new int[results.Length];

            for (int i = 1; i <= DATA_POINTS; i++)
            {
                var xPoint = step * (i - 1);
                for (int j = 0; j < keySets.Length; j++)
                {
                    var currKeys = keySets[j];
                    var currVals = valueSets[j];

                    while (!(currKeys[currIndexes[j]] <= xPoint && currKeys[currIndexes[j]+1] >= xPoint))
                        currIndexes[j]++;
                    var key1 = currKeys[currIndexes[j]];
                    var key2 = currKeys[currIndexes[j] + 1];
                    var val1 = currVals[currIndexes[j]];
                    var val2 = currVals[currIndexes[j] + 1];
                    double pct = (xPoint - key1) / (double)(key2 - key1);
                    var yVal = val1 - (val1 - val2) * pct;
                    matrix[j+1, i] = yVal.ToString();
                }
            }

            matrix[keySets.Length + 1, 0] = "Avg";
            matrix[keySets.Length + 2, 0] = "StdDev";
            matrix[keySets.Length + 3, 0] = "CV";
            for (int i = 1; i <= DATA_POINTS; i++)
            {
                var total = 0.0;
                for (int j = 1; j <= results.Length; j++)
                    total += double.Parse(matrix[j, i]);

                double avg = total / keySets.Length;
                matrix[keySets.Length + 1, i] = avg.ToString();

                total = 0.0;
                for (int j = 1; j <= results.Length; j++)
                    total += Math.Pow(double.Parse(matrix[j, i]) - avg, 2);
                double stdv = Math.Sqrt(total / keySets.Length);
                matrix[keySets.Length + 2, i] = stdv.ToString();
                matrix[keySets.Length + 3, i] = (stdv / avg).ToString();
            }

            return matrix;
        }

        static void WriteResults(string method, int trialNum, CostResultDictionary results)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Join(",", results.Inoculations.Keys.OrderBy(k => k)));
            sb.AppendLine(String.Join(",",
                results.Inoculations.OrderBy(kvp => kvp.Key)
                    .Select(kvp => kvp.Value.ToString("#.#####"))));
            File.WriteAllText($"{method}_{trialNum}_Inoculations.txt", sb.ToString());

            sb = new StringBuilder();
            sb.AppendLine(String.Join(",", results.TotalInterviews.Keys.OrderBy(k => k)));
            sb.AppendLine(String.Join(",",
                results.TotalInterviews.OrderBy(kvp => kvp.Key)
                    .Select(kvp => kvp.Value.ToString("#.#####"))));
            File.WriteAllText($"{method}_{trialNum}_TotalInterviews.txt", sb.ToString());

            sb = new StringBuilder();
            sb.AppendLine(String.Join(",", results.VerticesInterviewed.Keys.OrderBy(k => k)));
            sb.AppendLine(String.Join(",",
                results.VerticesInterviewed.OrderBy(kvp => kvp.Key)
                    .Select(kvp => kvp.Value.ToString("#.#####"))));
            File.WriteAllText($"{method}_{trialNum}_VerticesInterviewed.txt", sb.ToString());
        }

        static void Test01(string[] args)
        {
            Graph g = new Graph();
            g.AddEdge(2, 8);
            g.AddEdge(2, 7);
            g.AddEdge(4, 7);
            g.AddEdge(3, 4);
            g.AddEdge(2, 3);
            g.AddEdge(2, 4);
            g.AddEdge(4, 5);
            g.AddEdge(1, 3);
            g.AddEdge(1, 5);
            g.AddEdge(5, 6);
            g.AddEdge(7, 8);
            g.AddEdge(6, 7);
            g.AddEdge(4, 6);
            g.AddEdge(10, 5);
            g.AddEdge(10, 6);
            g.AddEdge(10, 11);
            g.AddEdge(10, 12);
            g.AddEdge(11, 12);
            g.AddEdge(10, 13);
            g.AddEdge(11, 13);
            g.AddEdge(12, 13);
            g.AddEdge(12, 15);
            g.AddEdge(14, 15);
            g.AddEdge(14, 7);
            g.AddEdge(16, 18);
            g.AddEdge(16, 17);
            g.AddEdge(17, 18);
            g.AddEdge(12, 18);
            g.AddEdge(16, 19);
            g.AddEdge(17, 19);
            g.AddEdge(18, 20);
            g.AddEdge(19, 20);

            InoculateMaxOfKNeighborhoods(g, 3);

            Console.ReadKey();
        }

        static CostResultDictionary InoculateRandomVertices(Graph graph)
        {
            CostResultDictionary crDictionary = new CostResultDictionary("RV");

            var N = graph.Vertices.Count();
            // Because we are going to take each vertex exactly once, its easier to permute them
            // once and then take them in order
            Random rand = TSRandom.NextRandom();
            List<Vertex> vertexCollection = graph.Vertices.OrderBy(v => rand.NextDouble()).ToList();
            int inoculations = 0, interviews = 0, verticesInterviewed = 0;

            for (int i = 0; i < vertexCollection.Count; i++)
            {
                if (i % WRITE_AT == 0)
                {
                    var currSize = GetAllConnectedComponents(graph).OrderByDescending(c => c.Count).First().Count; // /(double) N;
                    crDictionary.Inoculations[inoculations] = currSize;
                    crDictionary.TotalInterviews[interviews] = currSize;
                    crDictionary.VerticesInterviewed[verticesInterviewed] = currSize;
                }

                Vertex vertex = vertexCollection[i];
                inoculations++;
                interviews++;
                verticesInterviewed++;

                vertex.Edges.ToList().ForEach(e => graph.RemoveEdge(e.V1.Id, e.V2.Id));
            }
            return crDictionary;
        }

        static CostResultDictionary InoculateRandomFriends(Graph graph)
        {
            CostResultDictionary crDictionary = new CostResultDictionary("RF");

            var N = graph.Vertices.Count();
            // We will take the vertices in order and get one random friend from each
            Random rand = TSRandom.NextRandom();
            List<Vertex> vertexCollection = graph.Vertices.OrderBy(v => rand.NextDouble()).ToList();
            int inoculations = 0, interviews = 0;
            HashSet<Vertex> interviewedVertices = new HashSet<Vertex>();

            for (int i = 0; i < vertexCollection.Count; i++)
            {
                if (i % WRITE_AT == 0)
                {
                    var currSize = GetAllConnectedComponents(graph).OrderByDescending(c => c.Count).First().Count; // /(double) N;
                    crDictionary.Inoculations[inoculations] = currSize;
                    crDictionary.TotalInterviews[interviews] = currSize;
                    crDictionary.VerticesInterviewed[interviewedVertices.Count] = currSize;
                }

                Vertex vertex = vertexCollection[i];
                interviews++;
                interviewedVertices.Add(vertex);
                if (vertex.Neighbors.Any())
                {
                    Vertex v2 = vertex.Neighbors.ChooseRandomElement();
                    interviews++;
                    interviewedVertices.Add(v2);
                    v2.Edges.ToList().ForEach(e => graph.RemoveEdge(e.V1.Id, e.V2.Id));
                    inoculations++;
                }
            }
            return crDictionary;
        }

        static CostResultDictionary InoculateMaxOfKRandomFriends(Graph graph, int k = 2)
        {
            CostResultDictionary crDictionary = new CostResultDictionary("Mo" + k + "F");

            var N = graph.Vertices.Count();
            // We will take the vertices in order and get the max of k random friends from each
            Random rand = TSRandom.NextRandom();
            List<Vertex> vertexCollection = graph.Vertices.OrderBy(v => rand.NextDouble()).ToList();
            int inoculations = 0, interviews = 0;
            HashSet<Vertex> interviewedVertices = new HashSet<Vertex>();

            for (int i = 0; i < vertexCollection.Count; i++)
            {
                if (i % WRITE_AT == 0)
                {
                    var currSize = GetAllConnectedComponents(graph).OrderByDescending(c => c.Count).First().Count; // /(double) N;
                    crDictionary.Inoculations[inoculations] = currSize;
                    crDictionary.TotalInterviews[interviews] = currSize;
                    crDictionary.VerticesInterviewed[interviewedVertices.Count] = currSize;
                }

                Vertex vertex = vertexCollection[i];
                interviews++;
                interviewedVertices.Add(vertex);
                if (vertex.Neighbors.Any())
                {
                    var selectedNeighbors = vertex.Neighbors.ChooseRandomSubset(k).ToList();
                    interviews += selectedNeighbors.Count();
                    selectedNeighbors.ForEach(n => interviewedVertices.Add(n));
                    var maxNeighbor =
                        selectedNeighbors.First(n1 => n1.Degree == selectedNeighbors.Max(n2 => n2.Degree));
                    maxNeighbor.Edges.ToList().ForEach(e => graph.RemoveEdge(e.V1.Id, e.V2.Id));
                    inoculations++;
                }
            }
            return crDictionary;
        }

        static CostResultDictionary InoculateMaxFriendOfRandomVertices(Graph graph)
        {
            CostResultDictionary crDictionary = new CostResultDictionary("MF");

            var N = graph.Vertices.Count();
            // We will take the vertices in order and get the max of k random friends from each
            Random rand = TSRandom.NextRandom();
            List<Vertex> vertexCollection = graph.Vertices.OrderBy(v => rand.NextDouble()).ToList();
            int inoculations = 0, interviews = 0;
            HashSet<Vertex> interviewedVertices = new HashSet<Vertex>();

            for (int i = 0; i < vertexCollection.Count; i++)
            {
                if (i % WRITE_AT == 0)
                {
                    var currSize = GetAllConnectedComponents(graph).OrderByDescending(c => c.Count).First().Count; // /(double) N;
                    crDictionary.Inoculations[inoculations] = currSize;
                    crDictionary.TotalInterviews[interviews] = currSize;
                    crDictionary.VerticesInterviewed[interviewedVertices.Count] = currSize;
                }

                Vertex vertex = vertexCollection[i];
                //Console.WriteLine($"{vertex.Id}: Degree: {vertex.Degree}, MaxCC: {GetAllConnectedComponents(graph).Select(c => c.Count).Max()}");
                interviews++;
                interviewedVertices.Add(vertex);
                if (vertex.Neighbors.Any())
                {
                    interviews += vertex.Neighbors.Count;
                    vertex.Neighbors.ToList().ForEach(n => interviewedVertices.Add(n));
                    var maxNeighbor = vertex.Neighbors.First(n1 => n1.Degree == vertex.Neighbors.Max(n2 => n2.Degree));
                    maxNeighbor.Edges.ToList().ForEach(e => graph.RemoveEdge(e.V1.Id, e.V2.Id));
                    inoculations++;
                }
            }
            return crDictionary;
        }

        static CostResultDictionary InoculateMaxOfKNeighborhoods(Graph graph, int k = 2)
        {
            // technically k = 0 should reduce to random vertex but let's avoid
            // having to code that..
            if (k < 1)
                throw new Exception("Must include at least the first neighborhood");

            CostResultDictionary crDictionary = new CostResultDictionary("Mo" + k + "N");

            var N = graph.Vertices.Count();
            // We will take the vertices in order and get the max of k neighborhoods of each
            Random rand = TSRandom.NextRandom();
            List<Vertex> vertexCollection = graph.Vertices.OrderBy(v => rand.NextDouble()).ToList();
            int inoculations = 0, interviews = 0;
            HashSet<Vertex> interviewedVertices = new HashSet<Vertex>();

            for (int i = 0; i < vertexCollection.Count; i++)
            {
                if (i % WRITE_AT == 0)
                {
                    var currSize = GetAllConnectedComponents(graph).OrderByDescending(c => c.Count).First().Count; // /(double) N;
                    crDictionary.Inoculations[inoculations] = currSize;
                    crDictionary.TotalInterviews[interviews] = currSize;
                    crDictionary.VerticesInterviewed[interviewedVertices.Count] = currSize;
                }

                Vertex vertex = vertexCollection[i];
                interviews++;
                interviewedVertices.Add(vertex);
                if (vertex.Neighbors.Any())
                {
                    HashSet<Vertex> neighboringVertices = new HashSet<Vertex>(vertex.Neighbors);
                    interviews += neighboringVertices.Count;
                    neighboringVertices.ToList().ForEach(n => interviewedVertices.Add(n));
                    for (int j = 1; j < k; j++)
                    {
                        List<Vertex> newNeighbors = neighboringVertices.SelectMany(n => n.Neighbors).ToList();
                        interviews += newNeighbors.Count;
                        newNeighbors.ForEach(n => interviewedVertices.Add(n));
                        newNeighbors.ForEach(n => neighboringVertices.Add(n));
                    }

                    var maxOfNeighborhood =
                        neighboringVertices.First(n1 => n1.Degree == neighboringVertices.Max(n2 => n2.Degree));
                    maxOfNeighborhood.Edges.ToList().ForEach(e => graph.RemoveEdge(e.V1.Id, e.V2.Id));
                    inoculations++;
                }
            }
            return crDictionary;
        }

        static List<List<Vertex>> GetAllConnectedComponents(Graph graph)
        {
            var allVertices = new HashSet<Vertex>(graph.Vertices);
            List<List<Vertex>> components = new List<List<Vertex>>();

            while (allVertices.Any())
            {
                List<Vertex> component = new List<Vertex>();

                Stack<Vertex> stack = new Stack<Vertex>();

                var firstVertex = allVertices.First();
                allVertices.Remove(firstVertex);
                stack.Push(firstVertex);

                while (stack.Any())
                {
                    var vertex = stack.Pop();
                    component.Add(vertex);
                    foreach (var neighbor in vertex.Neighbors)
                    {
                        if (allVertices.Contains(neighbor))
                        {
                            allVertices.Remove(neighbor);
                            stack.Push(neighbor);
                        }
                    }

                }

                components.Add(component);
            }
            
            return components;
        }

    }
}
