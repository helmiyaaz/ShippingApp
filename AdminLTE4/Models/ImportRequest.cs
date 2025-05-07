namespace FILog.Models
{
    public class ImportRequest
    {
        public List<Dictionary<string, string>> Data { get; set; }
        public bool Overwrite { get; set; }
    }
}
