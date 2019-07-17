using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GraphLibyn;

namespace GetGraphStats
{
    class Program
    {
        static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= 50; i++)
            {
                List<Tuple<string, string>> allEdges = File.ReadAllLines(i + ".graph")
                    .Select(l => new Tuple<string, string>(Regex.Split(l, @"\s+")[0], Regex.Split(l, @"\s+")[1]))
                    .ToList();
                GraphLibyn.Graph g = new Graph();
                allEdges.ForEach(e => g.AddEdge(e.Item1, e.Item2));
                sb.AppendLine($"{i + ".graph"} Vertices: {g.Nodes.Count()} + Edges: {g.Edges.Count()} AFI: {g.FiVector.Average()} Assort: {g.GraphAssortativity} ");
               
            }

            File.WriteAllText("GraphStats.txt", sb.ToString());
            Console.WriteLine(sb.ToString());

            Console.WriteLine("Any key...");
            Console.ReadKey();
        } 
    }
}
