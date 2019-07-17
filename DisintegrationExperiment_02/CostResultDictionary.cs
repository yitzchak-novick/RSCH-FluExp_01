using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisintegrationExperiment_02
{
    public class CostResultDictionary
    {
        private String method;
        public String Method => method;

        public CostResultDictionary(String method)
        {
            this.method = method;
        }

        public SortedList<int, double> Inoculations = new SortedList<int, double>();
        public SortedList<int, double> TotalInterviews = new SortedList<int, double>();
        public SortedList<int, double> VerticesInterviewed = new SortedList<int, double>();
    }
}
