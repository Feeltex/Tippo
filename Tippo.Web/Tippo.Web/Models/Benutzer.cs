namespace Tippo.Web.Models
{
    public class Benutzer
    {
        public int benutzerid { get; set; }
        public string benutzername { get; set; }
        public string email { get; set; }
        public string passwort_hash { get; set; }
        public DateTime erstellungsdatum { get; set; }
        public int rollenid { get; set; }

        public Benutzer()
        {
        }

        public Benutzer(int benutzerid, string benutzername, string email, string passwort_hash, DateTime erstellungsdatum, int rollenid)
        {
            this.benutzerid = benutzerid;
            this.benutzername = benutzername;
            this.email = email;
            this.passwort_hash = passwort_hash;
            this.erstellungsdatum = erstellungsdatum;
            this.rollenid = rollenid;
        }
        public Benutzer(string benutzername, string email, string passwort_hash)
        {
            this.benutzername = benutzername;
            this.email = email;
            this.passwort_hash = passwort_hash;
        }
    }
}
