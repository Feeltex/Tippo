namespace Tippo.Web.Models
{
    public class Mannschaft
    {
        public int mannschaftsid { get; set; }
        public string name { get; set; }

        public Mannschaft(int mannschaftsid, string name)
        {
            this.mannschaftsid = mannschaftsid;
            this.name = name;
        }
    }
}
