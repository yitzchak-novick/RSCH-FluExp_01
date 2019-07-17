using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluExp_01;

namespace BFSComplexityTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<int, Tuple<TimeSpan, TimeSpan>> results = new Dictionary<int, Tuple<TimeSpan, TimeSpan>>();

            int incr = 1;

            for (int m = 0; m < 4000000; m += incr++)
            {
                int n = (int)(Math.Sqrt(2.0 * m + (0.25)) + 0.5);

            }
        }

        static void BFS(Graph graph)
        {
            Queue<Graph.Vertex> queue = new Queue<Graph.Vertex>();
            HashSet<Graph.Vertex> colored = new HashSet<Graph.Vertex>();

            queue.Enqueue(graph.Vertices.First());

        }
    }
}
