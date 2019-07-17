using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphLibYN_2019_05;

namespace ReplaceGraphWithLargestConnectedComponent
{
    class Program
    {
        static void Main(String[] args)
        {
            Console.WriteLine(args[0]);
            DirectoryInfo dir = new DirectoryInfo(args[0]);
            Parallel.ForEach(dir.GetFiles().Select(f => f.FullName), s => TrimGraph(s));
        }
        static void TrimGraph(string fullFileName)
        {
            var path = Path.GetDirectoryName(fullFileName);
            var fileName = new FileInfo(fullFileName).Name;
            fileName = fileName.Substring(0, fileName.LastIndexOf('.'));

            Graph graph = Graph.ParseFromTSVEdgesFile(fullFileName);

            var maxComponent = GetAllConnectedComponents(graph).OrderByDescending(c => c.Count).First();
            var includedEdges = graph.Edges.Where(e => maxComponent.Contains(e.V1));
            File.WriteAllLines(path + '\\' + fileName + "_MAXCOMP.graph",
                includedEdges.Select(e => e.V1.Id + "\t" + e.V2.Id));

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
