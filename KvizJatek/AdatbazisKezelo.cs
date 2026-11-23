using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace KvizJatek
{
    public class AdatbazisKezelo
    {
        private readonly string _connectionString =
            @"Server=.\SQLEXPRESS;Database=KvizJatekDb;Trusted_Connection=True;TrustServerCertificate=True;";

        //KÉRDÉSEK
        public List<Kerdes> KerdesekLekerese(bool keverve = false)
        {
            var lista = new List<Kerdes>();
            string sql = keverve
                ? "SELECT Id, KerdesSzoveg, ValaszA, ValaszB, ValaszC, ValaszD, HelyesValasz, KepNev FROM Kerdesek ORDER BY NEWID()"
                : "SELECT Id, KerdesSzoveg, ValaszA, ValaszB, ValaszC, ValaszD, HelyesValasz, KepNev FROM Kerdesek ORDER BY Id DESC";

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Kerdes
                {
                    Id = reader.GetInt32(0),
                    KerdesSzoveg = reader.GetString(1),
                    ValaszA = reader.GetString(2),
                    ValaszB = reader.GetString(3),
                    ValaszC = reader.GetString(4),
                    ValaszD = reader.GetString(5),
                    HelyesValasz = reader.GetString(6),
                    KepNev = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }
            return lista;
        }

        public void KerdesHozzaadasa(Kerdes kerdes)
        {
            const string sql = @"
                INSERT INTO Kerdesek (KerdesSzoveg, ValaszA, ValaszB, ValaszC, ValaszD, HelyesValasz, KepNev)
                VALUES (@szoveg, @a, @b, @c, @d, @helyes, @kep)";

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@szoveg", kerdes.KerdesSzoveg);
            cmd.Parameters.AddWithValue("@a", kerdes.ValaszA);
            cmd.Parameters.AddWithValue("@b", kerdes.ValaszB);
            cmd.Parameters.AddWithValue("@c", kerdes.ValaszC);
            cmd.Parameters.AddWithValue("@d", kerdes.ValaszD);
            cmd.Parameters.AddWithValue("@helyes", kerdes.HelyesValasz);
            cmd.Parameters.AddWithValue("@kep", string.IsNullOrWhiteSpace(kerdes.KepNev) ? DBNull.Value : kerdes.KepNev);
            cmd.ExecuteNonQuery();
        }

        public void KerdesModositasa(Kerdes kerdes)
        {
            const string sql = @"
                UPDATE Kerdesek
                SET KerdesSzoveg = @szoveg,
                    ValaszA = @a,
                    ValaszB = @b,
                    ValaszC = @c,
                    ValaszD = @d,
                    HelyesValasz = @helyes,
                    KepNev = @kep
                WHERE Id = @id";

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", kerdes.Id);
            cmd.Parameters.AddWithValue("@szoveg", kerdes.KerdesSzoveg);
            cmd.Parameters.AddWithValue("@a", kerdes.ValaszA);
            cmd.Parameters.AddWithValue("@b", kerdes.ValaszB);
            cmd.Parameters.AddWithValue("@c", kerdes.ValaszC);
            cmd.Parameters.AddWithValue("@d", kerdes.ValaszD);
            cmd.Parameters.AddWithValue("@helyes", kerdes.HelyesValasz);
            cmd.Parameters.AddWithValue("@kep", string.IsNullOrWhiteSpace(kerdes.KepNev) ? DBNull.Value : kerdes.KepNev);
            cmd.ExecuteNonQuery();
        }

        public void KerdesTorlese(int id)
        {
            const string sql = "DELETE FROM Kerdesek WHERE Id = @id";

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        //EREDMÉNYEK

        public void EredmenyMentese(string jatekosNev, int helyesValaszokSzama)
        {
            const string sql = @"
                INSERT INTO Eredmenyek (JatekosNev, HelyesValaszokSzama)
                VALUES (@nev, @pont)";

            string nev = string.IsNullOrWhiteSpace(jatekosNev) ? "Névtelen" : jatekosNev.Trim();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nev", nev);
            cmd.Parameters.AddWithValue("@pont", helyesValaszokSzama);
            cmd.ExecuteNonQuery();
        }

        public List<Eredmeny> EredmenyekLekerese()
        {
            var lista = new List<Eredmeny>();
            const string sql = @"
                SELECT Id, JatekosNev, HelyesValaszokSzama, Datum
                FROM Eredmenyek
                ORDER BY HelyesValaszokSzama DESC, Datum DESC";

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Eredmeny
                {
                    Id = reader.GetInt32(0),
                    JatekosNev = reader.GetString(1),
                    HelyesValaszokSzama = reader.GetInt32(2),
                    Datum = reader.GetDateTime(3)
                });
            }
            return lista;
        }

        public void EredmenyTorlese(int id)
        {
            const string sql = "DELETE FROM Eredmenyek WHERE Id = @id";

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}