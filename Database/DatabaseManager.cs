using System.Data.Common;
using Npgsql;
using TippoDiscord;
using TippoDiscord.Models;

namespace Tippo.Web.Database
{
    public class DatabaseManager
    {
        private readonly string connectionstring;
        public DatabaseManager()
        {
            connectionstring = "Host=localhost;Port=5432;Database=TippoDB;Username=postgres;Password=123db!";
        }

        public bool InsertUser(string username, string password, string email, string discord_user_id)
        {
            using var conn = new NpgsqlConnection(connectionstring);
            conn.Open();

            string sql = @"INSERT INTO benutzer
                  (benutzername, email, passwort_hash, erstellungsdatum, rollenid, discord_user_id, ist_discord_verifiziert)
                  VALUES
                  (@username, @email, @password, @createdAt, @rollenid, @discord_user_id, @ist_discord_verifiziert)";

            using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("email", $"{email}");
            cmd.Parameters.AddWithValue("password", password);
            cmd.Parameters.AddWithValue("createdAt", DateTime.Now);
            cmd.Parameters.AddWithValue("rollenid", 1);
            cmd.Parameters.AddWithValue("discord_user_id", discord_user_id);
            cmd.Parameters.AddWithValue("ist_discord_verifiziert", 1);

            try
            {
                cmd.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                conn.Close();
                return false;
            }
        }

        public UserCheck SearchUser(string email, string password_hash)
        {
            using var conn = new NpgsqlConnection(connectionstring);
            conn.Open();

            string sql = @"select ist_discord_verifiziert, passwort_hash  from benutzer where email = @email";

            using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("email", email);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                UserCheck new_user = new UserCheck();

                int ist_discord_verifiziert = reader.GetInt32(0);

                string passwordHash = reader.GetString(1);

                new_user.password_hash = passwordHash;
                new_user.ist_discord_verifiziert = ist_discord_verifiziert;
                conn.Close();
                return new_user;
            }
            conn.Close();
            return null;
        }

        public bool LinkDiscord(string email, string discordId)
        {
            using var conn = new NpgsqlConnection(connectionstring);
            conn.Open();

            string sql = $@"update benutzer set ist_discord_verifiziert = 1, discord_user_id = '{discordId}' where email = '{email}';";

            using var cmd = new NpgsqlCommand(sql, conn);

            try
            {
                cmd.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (PostgresException ex)
            {
                conn.Close();
                return false;
            }
        }

        public bool SaveTipp(string discordId, int spielId, int tippHeim, int tippGast)
        {
            using var conn = new NpgsqlConnection(connectionstring);
            conn.Open();

            string sql = @"
                        INSERT INTO tipp (tippheim, tippgast, benutzerid, spielid)
                        SELECT
                            @tippheim,
                            @tippgast,
                            b.benutzerid,
                            @spielid
                        FROM benutzer b
                        WHERE b.discord_user_id = @discordid
                        ON CONFLICT (benutzerid, spielid)
                        DO UPDATE SET
                            tippheim = EXCLUDED.tippheim,
                            tippgast = EXCLUDED.tippgast;
                        ";

            using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("tippheim", tippHeim);
            cmd.Parameters.AddWithValue("tippgast", tippGast);
            cmd.Parameters.AddWithValue("spielid", spielId);
            cmd.Parameters.AddWithValue("discordid", discordId);

            cmd.ExecuteNonQuery();

            try
            {
                cmd.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (PostgresException ex)
            {
                conn.Close();
                return false;
            }
        }

        public List<Spiel> GetMatchday(string wettbewerb, int spieltag)
        {
            List<Spiel> spiele = new List<Spiel>();

            using var conn = new NpgsqlConnection(connectionstring);
            conn.Open();

            string sql = @"
                            SELECT
                                s.spielid,
                                heim.name,
                                gast.name,
                                s.anstosszeit,
                                s.spieltag
                            FROM spiel s
                            INNER JOIN wettbewerb w
                                ON w.wettbewerbid = s.wettbewerbid
                            INNER JOIN mannschaft heim
                                ON heim.mannschaftsid = s.mannschaftheimid
                            INNER JOIN mannschaft gast
                                ON gast.mannschaftsid = s.mannschaftgastid
                                    WHERE LOWER(w.name) = LOWER(@wettbewerb)
                                      AND s.spieltag = @spieltag
                                    ORDER BY s.anstosszeit;
                        ";

            using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("wettbewerb", wettbewerb);
            cmd.Parameters.AddWithValue("spieltag", spieltag);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Spiel spiel = new Spiel();

                spiel.spielId = reader.GetInt32(0);
                spiel.heimteam = reader.GetString(1);
                spiel.gastteam = reader.GetString(2);
                spiel.anstosszeit = reader.GetDateTime(3);
                spiel.spieltag = reader.GetInt32(4);

                spiele.Add(spiel);
            }

            return spiele;
        }

        public List<TippAnzeige> GetMeineTipps(string discordId)
        {
            List<TippAnzeige> tipps = new List<TippAnzeige>();

            using var conn = new NpgsqlConnection(connectionstring);
            conn.Open();

            string sql = @"
        SELECT
            heim.name,
            gast.name,
            t.tippheim,
            t.tippgast,
            s.anstosszeit
        FROM tipp t
        INNER JOIN benutzer b
            ON b.benutzerid = t.benutzerid
        INNER JOIN spiel s
            ON s.spielid = t.spielid
        INNER JOIN mannschaft heim
            ON heim.mannschaftsid = s.mannschaftheimid
        INNER JOIN mannschaft gast
            ON gast.mannschaftsid = s.mannschaftgastid
        WHERE b.discord_user_id = @discordid
        ORDER BY s.anstosszeit;
    ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("discordid", discordId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                tipps.Add(new TippAnzeige
                {
                    Heimteam = reader.GetString(0),
                    Gastteam = reader.GetString(1),
                    TippHeim = reader.GetInt32(2),
                    TippGast = reader.GetInt32(3),
                    Anstosszeit = reader.GetDateTime(4)
                });
            }

            return tipps;
        }
    }
}
