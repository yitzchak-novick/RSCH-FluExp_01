using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FluExp_01
{
    class Program
    {
        private static Random rand = new Random();

        static void Main(string[] args)
        {
            Graph gr = Graph.BarabasiAlbertGraph(30, 3);

            Graph g = new Graph();
            Random r = new Random();

            for (int i = 0; i < 10000; i++)
            for (int j = i + 1; j < 10001; j++)
                if (!(i == 0 && j == 1))
                    g.AddEdge(i.ToString(), j.ToString());

            Console.WriteLine($"Vertices\t{g.Vertices.Count()}");
            Console.WriteLine($"Edges\t{g.Edges.Count}");
            Console.WriteLine($"Assort\t{g.Assortativity}");
            Console.WriteLine($"Afi\t{g.AFI}");
            Console.WriteLine($"LGfi\t{g.LGFI}");
            Console.WriteLine($"Gfi\t{g.GFI}");

            Console.ReadKey();

            g = new Graph();
            File.ReadAllLines(@"c:\temp\hyves.tar\hyves\out.hyves").Where(l => !String.IsNullOrWhiteSpace(l) && l[0] != '%')
                .Select(l => new Tuple<string, string>(Regex.Split(l, @"\s+")[0], Regex.Split(l, @"\s+")[1]))
                .ToList().ForEach(l => g.AddEdge(l.Item1, l.Item2));

            Console.WriteLine($"Vertices\t{g.Vertices.Count()}");
            Console.WriteLine($"Edges\t{g.Edges.Count}");
            Console.WriteLine($"Assort\t{g.Assortativity}");
            Console.WriteLine($"Afi\t{g.AFI}");
            Console.WriteLine($"LGfi\t{g.LGFI}");
            Console.WriteLine($"Gfi\t{g.GFI}");

            Console.ReadKey();
            Console.ReadKey();
        }

        static void _Main(string[] args)
        {
            //const string DIR = "BAISH";
            //if (!Directory.Exists(DIR))
            //    Directory.CreateDirectory(DIR);

            //Parallel.ForEach(Enumerable.Range(1, 25), i =>
            //    {
            //        Graph g = GetRandomGraph();
            //        Console.WriteLine($"Vertices: {g.VerticesDictionary.Count}\tEdges: {g.Edges.Count}\tAFI: {g.AFI}");
            //        File.WriteAllLines(DIR + "\\" + i + ".graph", g.Edges.Select(e => e.Item1 + "\t" + e.Item2));
            //    }
            //);

            //Parallel.ForEach(Enumerable.Range(26, 25), i =>
            //    {
            //        Graph g = GetRandomGraph2();
            //        Console.WriteLine($"Vertices: {g.VerticesDictionary.Count}\tEdges: {g.Edges.Count}\tAFI: {g.AFI}");
            //        File.WriteAllLines(DIR + "\\" + i + ".graph", g.Edges.Select(e => e.Item1 + "\t" + e.Item2));
            //    }
            //);
            //Console.WriteLine("Any key... {0}", DateTime.Now.ToShortTimeString());
            //Console.ReadKey();

        }

        //static Graph GetRandomBaAssortGraph()
        //{
        //    int vertexCount = rand.Next(800, 2000);
        //    int totalPossibleEdges = vertexCount * (vertexCount - 1) / 2;
        //    int additionalEdges = rand.Next((int)Math.Floor(.15 * totalPossibleEdges),
        //        (int)Math.Floor(.25 * totalPossibleEdges));

        //    Graph graph = new Graph();
        //    Enumerable.Range(0, vertexCount).ToList()
        //        .ForEach(i => graph.AddEdge(i.ToString(), ((i + 1) % vertexCount).ToString()));
        //    var alpha = 1.0; // rand.DoubleBetween(0.85, 01.15);
        //    for (int i = 0; i < additionalEdges; i++)
        //    {
        //        Graph.Vertex vertex1 = graph.VerticesDictionary.Values.Where(v => v.Degree < graph.VerticesDictionary.Count - 1)
        //            .ChooseBiasedElement(v => v.Degree);
        //        //.ChooseRandomElement();
        //        var remainingVerticesList = graph.VerticesDictionary.Values
        //            .Where(v => v != vertex1 && !v.Neighbors.Contains(vertex1)).ToList();
        //        var allRatios = remainingVerticesList.Select(v => ratio(vertex1, v));
        //        var maxRatio = allRatios.Max();
        //        var minRatio = allRatios.Min();

        //        var remainingVertices = graph.VerticesDictionary.Values.Where(v => v != vertex1 && !v.Neighbors.Contains(vertex1))
        //            .Select(v => new
        //            {
        //                vertex = v,
        //                weight = Math.Pow(maxRatio - ratio(vertex1, v) + minRatio, alpha)
        //            })
        //            .GroupBy(v => v.weight);
        //        var newVertex = remainingVertices.ChooseBiasedElement(g => g.Key).ChooseRandomElement().vertex;
        //        graph.AddEdge(vertex1.Id, newVertex.Id);
        //    }

        //    return graph;
        //}



        //static Graph GetRandomGraph()
        //{
        //    int vertexCount = rand.Next(200, 800);
        //    int totalPossibleEdges = vertexCount * (vertexCount - 1) / 2;
        //    double initialP = rand.DoubleBetween(0.025, 0.1);
        //    int additionalEdges = rand.Next((int) Math.Floor(.15 * totalPossibleEdges),
        //        (int) Math.Floor(.25 * totalPossibleEdges));

        //    Graph graph = new Graph();
        //    Enumerable.Range(0,vertexCount).ToList().ForEach(i => graph.AddErdosRenyiVertex(initialP));
        //    var alpha = rand.DoubleBetween(0.85, 01.15);
        //    for (int i = 0; i < additionalEdges; i++)
        //    {
        //        Graph.Vertex vertex1 = graph.VerticesDictionary.Values.Where(v => v.Degree < graph.VerticesDictionary.Count - 1)
        //            .ChooseBiasedElement(v => v.Degree);
        //            //.ChooseRandomElement();
        //        var remainingVertices = graph.VerticesDictionary.Values.Where(v => v != vertex1 && !v.Neighbors.Contains(vertex1))
        //            .Select(v => new
        //            {
        //                vertex = v,
        //                weight = Math.Pow(scaledRatio(vertex1, v), alpha)
        //            })
        //            .GroupBy(v => v.weight);
        //        var newVertex = remainingVertices.ChooseBiasedElement(g => g.Key).ChooseRandomElement().vertex;
        //        graph.AddEdge(vertex1.Id, newVertex.Id);
        //    }

        //    return graph;
        //}

        //static Graph GetRandomGraph2()
        //{
        //    int vertexCount = rand.Next(200, 800);
        //    int totalPossibleEdges = vertexCount * (vertexCount - 1) / 2;
        //    double initialP = rand.DoubleBetween(0.025, 0.01);
        //    int additionalEdges = rand.Next((int)Math.Floor(.15 * totalPossibleEdges),
        //        (int)Math.Floor(.25 * totalPossibleEdges));

        //    Graph graph = new Graph();
        //    Enumerable.Range(0, vertexCount).ToList().ForEach(i => graph.AddErdosRenyiVertex(initialP));
        //    var alpha = rand.DoubleBetween(0.085, 01.15);
        //    for (int i = 0; i < additionalEdges; i++)
        //    {
        //        Graph.Vertex vertex1 = graph.VerticesDictionary.Values.Where(v => v.Degree < graph.VerticesDictionary.Count - 1)
        //            .ChooseBiasedElement(v => v.Degree);
        //            //.ChooseRandomElement();
        //        var remainingVerticesList = graph.VerticesDictionary.Values
        //            .Where(v => v != vertex1 && !v.Neighbors.Contains(vertex1)).ToList();
        //        var allRatios = remainingVerticesList.Select(v => scaledRatio(vertex1, v)).ToList();
        //        var maxRatio = allRatios.Max();
        //        var minRatio = allRatios.Min();

        //        var remainingVertices = graph.VerticesDictionary.Values.Where(v => v != vertex1 && !v.Neighbors.Contains(vertex1))
        //            .Select(v => new
        //            {
        //                vertex = v,
        //                weight = Math.Pow(maxRatio - scaledRatio(vertex1, v) + minRatio, alpha)
        //            })
        //            .GroupBy(v => v.weight);
        //        var newVertex = remainingVertices.ChooseBiasedElement(g => g.Key).ChooseRandomElement().vertex;
        //        graph.AddEdge(vertex1.Id, newVertex.Id);
        //    }

        //    return graph;
        //}


        //static double ratio(Graph.Vertex v1, Graph.Vertex v2)
        //{
        //    return Math.Max(v1.Degree, v2.Degree) / (double) Math.Min(v1.Degree, v2.Degree);
        //}

        //static double scaledRatio(Graph.Vertex v1, Graph.Vertex v2)
        //{
        //    return Math.Max(v1.Degree + 1, v2.Degree + 1) / (double) Math.Min(v1.Degree + 1, v2.Degree + 1);
        //}
    }
}
