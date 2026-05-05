namespace BTreeDashboard.Web.Models
{
    public class BenchmarkResult
    {
        public string tree { get; set; }
        public long rows { get; set; }
        public double insert_s { get; set; }
        public double search_s { get; set; }
        public double raw_mb { get; set; }
        public double tree_mb { get; set; }
        public double ratio_percent { get; set; }
        public bool found { get; set; }
    }

    public class BenchmarkViewModel
    {
        public List<BenchmarkResult> Data { get; set; }
        public string Log { get; set; }
        public bool IsMultiple { get; set; }
    }
}