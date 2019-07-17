using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluExp_01
{
    public class Graph
    {
        public class Vertex
        {
            public string Id { get; }
            private Graph Graph { get; }

            public Vertex(string id, Graph graph)
            {
                Id = id;
                Graph = graph;
            }

            public HashSet<Vertex> Neighbors = new HashSet<Vertex>();
            public int Degree => Neighbors.Count;
            public int ExcDegree => Degree - 1;
            public double Fi => Degree == 0 ? Double.NegativeInfinity : Neighbors.Average(n => n.Degree) / Degree;
            public double SocialRange => Neighbors.Max(n => n.Degree) / (double) Neighbors.Min(n => n.Degree);

        }

        public Dictionary<string, Vertex> VerticesDictionary = new Dictionary<string, Vertex>();
        public IEnumerable<Vertex> Vertices => VerticesDictionary.Values;
        public HashSet<Tuple<string, string>> Edges = new HashSet<Tuple<string, string>>();

        public void AddEdge(string v1, string v2)
        {
            if (!VerticesDictionary.ContainsKey(v1))
                VerticesDictionary[v1] = new Vertex(v1, this);
            if (!VerticesDictionary.ContainsKey(v2))
                VerticesDictionary[v2] = new Vertex(v2, this);
            if (!VerticesDictionary[v1].Neighbors.Contains(VerticesDictionary[v2]))
            {
                VerticesDictionary[v1].Neighbors.Add(VerticesDictionary[v2]);
                VerticesDictionary[v2].Neighbors.Add(VerticesDictionary[v1]);
                Edges.Add(new Tuple<string, string>(v1.CompareTo(v2) < 0 ? v1 : v2,
                    v1.CompareTo(v2) < 1 ? v2 : v1));
            }
        }

        public double AFI => Vertices.Where(v => v.Degree > 0).Average(v => v.Fi);
        public double GFI => Vertices.Where(v => v.Degree > 0).GMean(v => v.Fi);
        public double LGFI => Vertices.Where(v => v.Degree > 0).LogGMean(v => v.Fi);
        public double AverageSocialRange => Vertices.Where(v => v.Degree > 0).Average(v => v.SocialRange);

        public double Assortativity
        {
            get
            {
                var deg1 = Vertices.First(v => v.Degree > 0).Degree;
                if (Vertices.Where(v => v.Degree > 0).All(v => v.Degree == deg1))
                    return 1.0;

                var EdgeEndpoints = Edges.Select(e =>
                    new Tuple<int, int>(VerticesDictionary[e.Item1].Degree, VerticesDictionary[e.Item2].Degree));

                decimal InvM = 1 / (decimal) Edges.Count;

                decimal sumOfProducts = 0;
                decimal sumOfDegrees = 0;
                decimal sumOfSquares = 0;

                foreach (var edgeEndpoint in EdgeEndpoints)
                {
                    sumOfProducts += (InvM * edgeEndpoint.Item1 * edgeEndpoint.Item2);
                    sumOfDegrees += (InvM * 0.5m) * (edgeEndpoint.Item1 + edgeEndpoint.Item2);
                    sumOfSquares += (InvM * 0.5m) *
                                    (decimal) (Math.Pow(edgeEndpoint.Item1, 2) + Math.Pow(edgeEndpoint.Item2, 2));
                }

                return ((double) sumOfProducts - Math.Pow((double) sumOfDegrees, 2)) /
                       ((double) sumOfSquares - Math.Pow((double) sumOfDegrees, 2));
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(
                $"Vertices: {Vertices.Count()}, Edges: {Edges.Count}, Assort: {Assortativity}, AFI: {AFI}, LGFI: {LGFI}");
            return sb.ToString();
        }

        public string Histogram
        {
            get
            {
                var degreeGroups = Vertices.GroupBy(v => v.Degree).OrderBy(g => g.Key);
                StringBuilder sb = new StringBuilder();
                foreach (var group in degreeGroups)
                    sb.AppendLine($"Degree {group.Key}:\t\t{group.Count()}");
                return sb.ToString();
            }
        }

        private int nextId = 0;

        private static Graph ErdosReyni(int n, double p)
        {
            Graph g = new Graph();
            for (int i = 0; i < n; i++)
                g.AddErdoRenyiVertex(p);
            return g;
        }

        private void AddErdoRenyiVertex(double p)
        {
            AddErdosRenyiVertex(p, (++nextId).ToString());
        }

        public void AddErdosRenyiVertex(double p, string id)
        {
            Random rand = TSRandom.NextRandom();


            Vertex newVertex = new Vertex(id, this);
            VerticesDictionary[nextId.ToString()] = newVertex;
            foreach (var nextVertex in VerticesDictionary.Values)
            {
                if (nextVertex == newVertex)
                    continue;
                if (rand.NextDouble() < p)
                    AddEdge(nextVertex.Id, newVertex.Id);
            }
        }

        public static Graph BarabasiAlbertGraph(int n, int m)
        {
            Graph g = new Graph();
            for (int i = 0; i < m; i++)
                g.AddEdge((++g.nextId).ToString(), (m + 1).ToString());
            g.nextId++;
            for (int i = m + 2; i <= n; i++)
                g.AddBarabasiAlbertVertex(m);
            return g;
        }

        private void AddBarabasiAlbertVertex(int m)
        {
            AddBarabasiAlbertVertex(m, (++nextId).ToString());
        }

        public void AddBarabasiAlbertVertex(int m, string id)
        {
            Vertices.ChooseBiasedSubset(m, v => v.Degree).ToList().ForEach(v => AddEdge(v.Id, id));
        }

        public static Graph Clone(Graph g)
        {
            Graph g2 = new Graph();
            g.Edges.ToList().ForEach(e => g2.AddEdge(e.Item1, e.Item2));
            return g2;
        }

        public Graph Clone() => Clone(this);
    }
}
