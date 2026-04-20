using System.Data.Common;
using Npgsql;
using Tippo.Web.Models;

namespace Tippo.Web.Database
{
    public class DatabaseManager
    {
        private readonly string connectionstring;
        public DatabaseManager()
        {
            connectionstring = "Host=localhost;Port=5432;Database=TippoDB;Username=postgres;Password=123db!";
        }

        public void InsertBenutzer(Benutzer benutzer)
        {
            using var conn = new NpgsqlConnection(connectionstring);
            conn.Open();

            string sql = @$"insert into benutzer(benutzername, email, erstellungsdatum, passwort_hash, rollenid) values ('{benutzer.benutzername}', '{benutzer.email}', '{DateTime.Now}', '{benutzer.passwort_hash}', {benutzer.rollenid});";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void CreateGame(Spiel spiel)
        {
            using var conn = new NpgsqlConnection(connectionstring);
            conn.Open();

            string sql = @$"insert into spiel(wettbewerb, saison, spieltag, mannschaftheimid, mannschaftgastid, anstosszeit )
                            values('{spiel.wettbewerb}', {spiel.saison}, '{spiel.spieltag}', {spiel.mannschaftheimid}, {spiel.mannschaftgastid}, '{spiel.anstosszeit}')";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void CreateTipp(Tipp tipp)
        {
            using var conn = new NpgsqlConnection(connectionstring);
            conn.Open();

            string sql = $@"insert into tipp(benutzerid, spielid, tippgast, tippheim)
                            values({tipp.benutzerid}, {tipp.spielid}, {tipp.tippgast}, {tipp.tippheim})";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}
