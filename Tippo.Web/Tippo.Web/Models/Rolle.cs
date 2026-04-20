namespace Tippo.Web.Models
{
    public class Rolle
    {
        public int rollenid { get; set; }
        public string bezeichnung { get; set; }

        public Rolle(int rollenid, string bezeichnung)
        {
            this.rollenid = rollenid;
            this.bezeichnung = bezeichnung;
        }
    }
}
