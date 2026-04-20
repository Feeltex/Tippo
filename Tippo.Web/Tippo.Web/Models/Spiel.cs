namespace Tippo.Web.Models
{
    public class Spiel
    {
        public int spielid { get; set; }
        public string wettbewerb { get; set; }
        public string saison {  get; set; }
        public int spieltag { get; set; }
        public DateTime anstosszeit { get; set; }
        public int ergebnisheim { get; set; }
        public int ergebnisgast {  get; set; }
        public int mannschaftheimid { get; set; }
        public int mannschaftgastid { get; set; }

        public Spiel(int spielid, string wettbewerb, string saison, int spieltag, DateTime anstosszeit, int ergebnisheim, int ergebnisgast, int mannschaftheimid, int mannschaftgastid)
        {
            this.spielid = spielid;
            this.wettbewerb = wettbewerb;
            this.saison = saison;
            this.spieltag = spieltag;
            this.anstosszeit = anstosszeit;
            this.ergebnisheim = ergebnisheim;
            this.ergebnisgast = ergebnisgast;
            this.mannschaftheimid = mannschaftheimid;
            this.mannschaftgastid = mannschaftgastid;
        }
    }
}
