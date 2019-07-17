using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluExp_01;

namespace Innoculate
{
    class Program
    {
        private static readonly double PCT = .33;

        static void Main(string[] args)
        {
            try { /*_Main(args);*/ AllComponents();}
            catch(Exception ex) { Console.WriteLine(ex.ToString());}
            Console.WriteLine("Any key");
            Console.ReadKey();

        }

        static void AllComponents()
        {
            Enumerable.Range(1, 50).ToList().ForEach(i =>
            {
                Graph g = new Graph();
                File.ReadAllLines(i + ".graph").ToList()
                    .ForEach(l => g.AddEdge(Regex.Split(l, @"\s+")[0], Regex.Split(l, @"\s+")[1]));
                var max = GetAllConnectedComponents(g).Max(grp => grp.Count());
                Console.WriteLine($"{i}.graph:\t{max}");
            });
        }

        static void _Main(string[] args)
        {
            StringBuilder results = new StringBuilder();
            const int TRIALS = 50;
            Enumerable.Range(1, 50).ToList().ForEach(
                i =>
                {
                    int[] totalInnoculations1 = new int[TRIALS];
                    int[] largestComponent1 = new int[TRIALS];
                    
                    int[] totalInnoculations2 = new int[TRIALS];
                    int[] largestComponent2 = new int[TRIALS];

                    Parallel.ForEach(Enumerable.Range(0, TRIALS), j =>
                    {
                        Graph g = new Graph();
                        File.ReadAllLines(i + ".graph").ToList().ForEach(l =>
                            g.AddEdge(Regex.Split(l, @"\s+")[0], Regex.Split(l, @"\s+")[1]));
                        var vertices = g.VerticesDictionary.Values.ChooseRandomSubset((int) (g.VerticesDictionary.Count * PCT));
                        foreach (var vertex in vertices)
                            if (vertex.Degree > 0)
                            {
                                totalInnoculations1[j]++;
                                Innoculate(g, vertex);
                            }
                        largestComponent1[j] = GetAllConnectedComponents(g).Max(grp => grp.Count());
                    });


                   Parallel.ForEach(Enumerable.Range(0, TRIALS), j =>
                        {
                            Graph g = new Graph();
                            File.ReadAllLines(i + ".graph").ToList().ForEach(l =>
                                g.AddEdge(Regex.Split(l, @"\s+")[0], Regex.Split(l, @"\s+")[1]));
                            var vertices = g.VerticesDictionary.Values.ChooseRandomSubset((int)(g.VerticesDictionary.Count * PCT));
                            foreach (var vertex in vertices)
                                if (vertex.Degree > 0)
                                {
                                    var neighbor = vertex.Neighbors.ChooseRandomElement();
                                    if (neighbor.Degree > 0)
                                    {
                                        totalInnoculations2[j]++;
                                        Innoculate(g, neighbor);
                                    }
                                }
                            largestComponent2[j] = GetAllConnectedComponents(g).Max(grp => grp.Count());
                        }
                    );

                    // Experiment 3, just as a sanity test, compare to the case of innoculating the highest degree vertices
                    Graph g3 = new Graph();
                    File.ReadAllLines(i + ".graph").ToList().ForEach(l => g3.AddEdge(Regex.Split(l, @"\s+")[0], Regex.Split(l, @"\s+")[1]));
                    var totalInnocs3 = 0;
                    var verticesToInnoculate = g3.VerticesDictionary.Values.OrderByDescending(v => v.Degree).Take((int)(g3.VerticesDictionary.Count * PCT));
                    foreach (var vertex in verticesToInnoculate)
                    {
                        if (vertex.Degree > 0)
                        {
                            totalInnocs3++;
                            Innoculate(g3, vertex);
                        }
                    }
                    var largestComponent3 = GetAllConnectedComponents(g3).Max(grp => grp.Count());

                    GraphLibyn.Graph grph = new GraphLibyn.Graph();
                    Graph gr = new Graph();
                    File.ReadAllLines(i + ".graph").ToList().ForEach(l => grph.AddEdge(Regex.Split(l, @"\s+")[0], Regex.Split(l, @"\s+")[1]));
                    File.ReadAllLines(i + ".graph").ToList().ForEach(l => gr.AddEdge(Regex.Split(l, @"\s+")[0], Regex.Split(l, @"\s+")[1]));

                    results.Append($"Graph {i} ");
                    results.Append($"Vertices: {gr.VerticesDictionary.Count}/{grph.Nodes.Count()} ");
                    results.Append($"Edges: {gr.Edges.Count}/{grph.Edges.Count()} ");
                    results.Append($"Afi: {gr.AFI}/{grph.FiVector.Average()} ");
                    results.Append($"Assrt: {grph.GraphAssortativity} ");
                    results.Append($"Innocs1: {totalInnoculations1.Average()} ");
                    results.Append($"Innocs2: {totalInnoculations2.Average()} ");
                    results.Append($"Innocs3: {totalInnocs3} ");
                    results.Append($"Comp1: {largestComponent1.Average()} ");
                    results.Append($"Comp2: {largestComponent2.Average()} ");
                    results.Append($"Comp3: {largestComponent3} ");
                    results.AppendLine();
                }
                );
            Console.WriteLine(results.ToString());
            File.WriteAllText("InnoculationResults.txt", results.ToString());
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

            var AllVertices = new HashSet<Graph.Vertex>(g.VerticesDictionary.Values);
            
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
