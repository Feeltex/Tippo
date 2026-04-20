namespace Tippo.Web.Models
{
    public class Tipp
    {
        public int tippid { get; set; }
        public int tippheim { get; set; }
        public int tippgast {  get; set; }
        public int benutzerid { get; set; }
        public int spielid { get; set; }

        public Tipp(int tippid, int tippheim, int tippgast, int benutzerid, int spielid)
        {
            this.tippid = tippid;
            this.tippheim = tippheim;
            this.tippgast = tippgast;
            this.benutzerid = benutzerid;
            this.spielid = spielid;
        }
    }
}
