using GraphLibYN_2019_05;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using FluExp_01;
using Graph = GraphLibYN_2019_05.Graph;

namespace DisintegrationExperiment_01
{
    class Program
    {
        // this program is meant to see what happens to a network when nodes are removed, every time the remaining nodes are broken into
        // more clusters, it will display how many nodes where removed and the number of clusters with their sizes
        static void Main(string[] args)
        {
            Parallel.ForEach(Enumerable.Range(0, 10), i =>
            {
                Graph graph = Graph.ParseFromTSVEdgesFile(args[0]);
                List<Vertex> verticesInOrder;
                Random random = TSRandom.NextRandom();

                if (args[1].ToLower().Contains("random"))
                    verticesInOrder = graph.Vertices.OrderBy(v => random.NextDouble()).ToList();
                else
                    verticesInOrder = graph.Vertices.OrderByDescending(v => v.Degree).ToList();

                String results = RemoveVertices(graph, verticesInOrder);
                File.WriteAllText("Results_" + i, results);
            });
        }

        // This method removes vertices and tracks the size of ALL components as the vertices are 
        // removed. It was used to learn more about the way that graphs are disintegrated, but 
        // probably doesn't serve any purpose at this point (YN 6/26/19)
        static String RemoveVertices_OLD(Graph graph, List<Vertex> verticesToRemove)
        {
            StringBuilder results = new StringBuilder();

            var prevConnectedComponentSizes =
                GetAllConnectedComponents(graph).Select(c => c.Count).OrderByDescending(i => i).ToList();
            Console.WriteLine(String.Join(",", prevConnectedComponentSizes));
            var removingList = new StringBuilder();
            int removed = 0;

            for (int i = 0; i < verticesToRemove.Count; i++)
            {
                var vertex = verticesToRemove[i];
                graph.RemoveVertex(vertex.Id);
                removingList.Append($"{vertex.Degree}, ");
                removed++;
                var nextConnectedComponentSizes = GetAllConnectedComponents(graph).Select(c => c.Count)
                    .OrderByDescending(j => j).ToList();
                if (nextConnectedComponentSizes.Count != prevConnectedComponentSizes.Count)
                {
                    Console.WriteLine($"Removed {removed} of {verticesToRemove.Count}: {removingList}");
                    removingList.Clear();
                    prevConnectedComponentSizes = nextConnectedComponentSizes;
                    Console.WriteLine(String.Join(",", prevConnectedComponentSizes));
                }
            }

            return results.ToString();
        }

        static String RemoveVertices(Graph graph, List<Vertex> verticesToRemove)
        {
            StringBuilder results = new StringBuilder();

            var prevConnectedComponentSize =
                GetAllConnectedComponents(graph).Select(c => c.Count).OrderByDescending(i => i).First();
            var totalVertices = graph.Vertices.Count();
            results.AppendLine(prevConnectedComponentSize + "\t" +
                               (prevConnectedComponentSize / totalVertices).ToString("0.####"));
            for (int i = 0; i < verticesToRemove.Count; i++)
            {
                var vertex = verticesToRemove[i];
                vertex.Edges.ToList().ForEach(e => graph.RemoveEdge(e.V1.Id, e.V2.Id));
                prevConnectedComponentSize =
                    GetAllConnectedComponents(graph).Select(c => c.Count).OrderByDescending(j => j).First();
                results.AppendLine(prevConnectedComponentSize + "\t" +
                                   (prevConnectedComponentSize / totalVertices).ToString("0.####"));
            }


            return results.ToString();
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
