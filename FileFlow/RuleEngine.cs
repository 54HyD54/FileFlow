using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;

namespace FileFlow
{
    internal class RuleEngine
    {
        DatabaseManager dbManager = new DatabaseManager();

        public void KuralEkle(string ruleName, string source, string extension, string destination)
        {
            SQLiteConnection connection = dbManager.GetConnection();

            try
            {
                string sorgu = "INSERT INTO Rules (RuleName, SourcePath, ExtensionFilter, DestinationPath) " +
                        "VALUES (@RuleName, @SourcePath, @ExtensionFilter, @DestinationPath)";

                if(ruleName == null)
                {
                    MessageBox.Show("Kural İsmi Boş Bırakılamaz!!!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    SQLiteCommand cmd = new SQLiteCommand(sorgu, connection);

                    cmd.Parameters.AddWithValue("@RuleName", ruleName);
                    cmd.Parameters.AddWithValue("@SourcePath", source);
                    cmd.Parameters.AddWithValue("@ExtensionFilter", extension);
                    cmd.Parameters.AddWithValue("@DestinationPath", destination);
                    cmd.ExecuteNonQuery();
                }

            }
            finally
            {
                connection.Close();
            }

        }

        public DataTable AktifKurallariGetir()
        {
            SQLiteConnection connection = dbManager.GetConnection();

            try
            {
                string sorgu = "SELECT * FROM Rules WHERE IsActive = 1";

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sorgu, connection);

                DataTable dt = new DataTable();

                adapter.Fill(dt);

                return dt;
            }
            finally
            {
                connection.Close();
            }
        }

        public bool KuralDurumGuncelle(int ruleId, int isActive)
        {
            SQLiteConnection connection = dbManager.GetConnection();

            try
            {
                string sorgu = "UPDATE Rules SET isActive = @isActive WHERE ruleId = @ruleID";

                SQLiteCommand cmd = new SQLiteCommand(sorgu, connection);

                cmd.Parameters.AddWithValue("@ruleID", ruleId);
                cmd.Parameters.AddWithValue("@isActive", isActive);
                int eklenenSatir = cmd.ExecuteNonQuery();

                return eklenenSatir > 0;
            }
            finally
            {
                connection.Close();
            }
        }

        public void KuralSil(int ruleId)
        {
            SQLiteConnection connection = dbManager.GetConnection();

            try
            {
                string sorgu = "DELETE FROM Rules WHERE ruleId = @ruleId";

                SQLiteCommand cmd = new SQLiteCommand(sorgu, connection);

                cmd.Parameters.AddWithValue("@ruleId", ruleId);
                cmd.ExecuteNonQuery();

            }
            finally
            {
                connection.Close();
            }
        }

        public bool KuralGuncelle(int ruleId, string ruleName, string source, string extension, string destination)
        {
            SQLiteConnection connection = dbManager.GetConnection();

            try
            {
                string sorgu = "UPDATE Rules SET RuleName = @RuleName, SourcePath = @SourcePath, " +
                               "ExtensionFilter = @ExtensionFilter, DestinationPath = @DestinationPath " +
                               "WHERE RuleID = @RuleID";

                SQLiteCommand cmd = new SQLiteCommand(sorgu, connection);

                cmd.Parameters.AddWithValue("@RuleID", ruleId);
                cmd.Parameters.AddWithValue("@RuleName", ruleName);
                cmd.Parameters.AddWithValue("@SourcePath", source);
                cmd.Parameters.AddWithValue("@ExtensionFilter", extension);
                cmd.Parameters.AddWithValue("@DestinationPath", destination);

                int etkilenen = cmd.ExecuteNonQuery();
                return etkilenen > 0;
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
