using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace FileFlow
{
    public class TemizlikOturumu
    {
        public int OturumId { get; set; }
        public string Tarih { get; set; } = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        public string IslemTuru { get; set; } = "Genel Temizlik";
        public int Belgeler { get; set; } = 0;
        public int Resimler { get; set; } = 0;
        public int Videolar { get; set; } = 0;
        public int Kopyalar { get; set; } = 0;
        public int Diger { get; set; } = 0;
        public int ToplamDosya => Belgeler + Resimler + Videolar + Kopyalar + Diger;
        public float KazanilanMB => (ToplamDosya * 2.5f);
    }

    internal class DatabaseManager
    {
        // 1. TANIMLAMALAR
        private readonly string connectionString = "Data Source=FileFlowDB.sqlite;Version=3;";

        // Hafızadaki aktif ve son tamamlanan temizlik oturumu
        private static TemizlikOturumu? _aktifOturum = null;
        private static TemizlikOturumu? _sonTamamlananOturum = null;

        // 2. YAPICI METOT (Tabloları Başlatma)
        public DatabaseManager()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // 1. Rules Tablosu
                string kuralSorgu = @"CREATE TABLE IF NOT EXISTS Rules (
                                        RuleID INTEGER PRIMARY KEY AUTOINCREMENT,
                                        RuleName TEXT,
                                        SourcePath TEXT,
                                        ExtensionFilter TEXT,
                                        DestinationPath TEXT,
                                        IsActive INTEGER DEFAULT 1
                                    );";
                using (SQLiteCommand cmd = new SQLiteCommand(kuralSorgu, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // 2. Kümülatif İstatistik Tablosu (Tüm Zamanların Kalıcı Hafızası)
                string istatistikSorgu = @"CREATE TABLE IF NOT EXISTS DosyaIstatistikleri (
                                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            KategoriAdi TEXT,
                                            IslemGorenAdet INTEGER DEFAULT 0
                                        );";
                using (SQLiteCommand cmd2 = new SQLiteCommand(istatistikSorgu, conn))
                {
                    cmd2.ExecuteNonQuery();
                }

                // 3. Temizlik Oturumları Tablosu (Her Temizliğin Ayrı Dökümü)
                string oturumSorgu = @"CREATE TABLE IF NOT EXISTS TemizlikOturumlari (
                                        OturumId INTEGER PRIMARY KEY AUTOINCREMENT,
                                        Tarih TEXT,
                                        IslemTuru TEXT,
                                        Belgeler INTEGER DEFAULT 0,
                                        Resimler INTEGER DEFAULT 0,
                                        Videolar INTEGER DEFAULT 0,
                                        Kopyalar INTEGER DEFAULT 0,
                                        Diger INTEGER DEFAULT 0,
                                        ToplamDosya INTEGER DEFAULT 0,
                                        KazanilanMB REAL DEFAULT 0
                                    );";
                using (SQLiteCommand cmd3 = new SQLiteCommand(oturumSorgu, conn))
                {
                    cmd3.ExecuteNonQuery();
                }
            }
        }

        // 3. YARDIMCI METOTLAR
        public SQLiteConnection GetConnection()
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            connection.Open();
            return connection;
        }

        public int GetAktifKuralSayisi()
        {
            int kuralSayisi = 0;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT COUNT(*) FROM Rules", conn))
                {
                    object? res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value)
                    {
                        kuralSayisi = Convert.ToInt32(res);
                    }
                }
            }
            return kuralSayisi;
        }

        public int GetKopyaSayisi()
        {
            int kopya = 0;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT IslemGorenAdet FROM DosyaIstatistikleri WHERE KategoriAdi = 'Kopyalar'", conn))
                {
                    object? result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        kopya = Convert.ToInt32(result);
                    }
                }
            }
            return kopya;
        }

        // 4. KURAL VE TABLO İŞLEMLERİ
        public DataTable GetRules()
        {
            DataTable dt = new DataTable();
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string sorgu = "SELECT RuleID, RuleName, SourcePath, ExtensionFilter, DestinationPath FROM Rules";
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(sorgu, connection))
                {
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        // 5. OTURUM VE ANLIK TEMİZLİK YÖNETİMİ
        public void YeniTemizlikOturumuBaslat(string islemTuru = "Temizlik İşlemi")
        {
            _aktifOturum = new TemizlikOturumu
            {
                Tarih = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
                IslemTuru = islemTuru
            };
        }

        public void TemizlikOturumunuKapat()
        {
            if (_aktifOturum == null) return;

            _sonTamamlananOturum = _aktifOturum;

            // Eğer işlem gören dosya varsa kalıcı SQLite oturum tablosuna kaydet
            if (_aktifOturum.ToplamDosya > 0)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string insert = @"INSERT INTO TemizlikOturumlari 
                                      (Tarih, IslemTuru, Belgeler, Resimler, Videolar, Kopyalar, Diger, ToplamDosya, KazanilanMB)
                                      VALUES (@Tarih, @IslemTuru, @Belgeler, @Resimler, @Videolar, @Kopyalar, @Diger, @ToplamDosya, @KazanilanMB);";
                    using (SQLiteCommand cmd = new SQLiteCommand(insert, connection))
                    {
                        cmd.Parameters.AddWithValue("@Tarih", _aktifOturum.Tarih);
                        cmd.Parameters.AddWithValue("@IslemTuru", _aktifOturum.IslemTuru);
                        cmd.Parameters.AddWithValue("@Belgeler", _aktifOturum.Belgeler);
                        cmd.Parameters.AddWithValue("@Resimler", _aktifOturum.Resimler);
                        cmd.Parameters.AddWithValue("@Videolar", _aktifOturum.Videolar);
                        cmd.Parameters.AddWithValue("@Kopyalar", _aktifOturum.Kopyalar);
                        cmd.Parameters.AddWithValue("@Diger", _aktifOturum.Diger);
                        cmd.Parameters.AddWithValue("@ToplamDosya", _aktifOturum.ToplamDosya);
                        cmd.Parameters.AddWithValue("@KazanilanMB", _aktifOturum.KazanilanMB);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            _aktifOturum = null;
        }

        // Donut Grafiğin SADECE son temizliği çizmesi için değerler
        public List<float> GetSonTemizlikDegerleri()
        {
            var oturum = _aktifOturum ?? _sonTamamlananOturum;
            if (oturum == null)
            {
                oturum = GetEnSonKayitliOturum();
            }

            if (oturum != null)
            {
                return new List<float> { (float)oturum.Belgeler, (float)oturum.Resimler, (float)oturum.Videolar };
            }

            return new List<float> { 0f, 0f, 0f };
        }

        public int GetSonTemizlikKopyaSayisi()
        {
            var oturum = _aktifOturum ?? _sonTamamlananOturum ?? GetEnSonKayitliOturum();
            return oturum != null ? oturum.Kopyalar : 0;
        }

        public TemizlikOturumu? GetSonTemizlikBilgisi()
        {
            return _aktifOturum ?? _sonTamamlananOturum ?? GetEnSonKayitliOturum();
        }

        public TemizlikOturumu? GetEnSonKayitliOturum()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT OturumId, Tarih, IslemTuru, Belgeler, Resimler, Videolar, Kopyalar, Diger FROM TemizlikOturumlari ORDER BY OturumId DESC LIMIT 1;";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new TemizlikOturumu
                            {
                                OturumId = Convert.ToInt32(reader["OturumId"]),
                                Tarih = reader["Tarih"]?.ToString() ?? "",
                                IslemTuru = reader["IslemTuru"]?.ToString() ?? "",
                                Belgeler = Convert.ToInt32(reader["Belgeler"]),
                                Resimler = Convert.ToInt32(reader["Resimler"]),
                                Videolar = Convert.ToInt32(reader["Videolar"]),
                                Kopyalar = Convert.ToInt32(reader["Kopyalar"]),
                                Diger = Convert.ToInt32(reader["Diger"])
                            };
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        public List<TemizlikOturumu> GetGecmisOturumlar(int limit = 5)
        {
            List<TemizlikOturumu> liste = new List<TemizlikOturumu>();
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string query = $"SELECT OturumId, Tarih, IslemTuru, Belgeler, Resimler, Videolar, Kopyalar, Diger FROM TemizlikOturumlari ORDER BY OturumId DESC LIMIT {limit};";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            liste.Add(new TemizlikOturumu
                            {
                                OturumId = Convert.ToInt32(reader["OturumId"]),
                                Tarih = reader["Tarih"]?.ToString() ?? "",
                                IslemTuru = reader["IslemTuru"]?.ToString() ?? "",
                                Belgeler = Convert.ToInt32(reader["Belgeler"]),
                                Resimler = Convert.ToInt32(reader["Resimler"]),
                                Videolar = Convert.ToInt32(reader["Videolar"]),
                                Kopyalar = Convert.ToInt32(reader["Kopyalar"]),
                                Diger = Convert.ToInt32(reader["Diger"])
                            });
                        }
                    }
                }
            }
            catch { }
            return liste;
        }

        // 6. TÜM ZAMANLARIN KÜMÜLATİF VERİLERİ (Hafıza)
        public List<float> GetIstatistikDegerleri()
        {
            List<float> degerler = new List<float>();

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sorgu = "SELECT IslemGorenAdet FROM DosyaIstatistikleri WHERE KategoriAdi != 'Kopyalar' ORDER BY CASE KategoriAdi WHEN 'Belgeler' THEN 1 WHEN 'Resimler' THEN 2 WHEN 'Videolar' THEN 3 ELSE 4 END";

                using (SQLiteCommand cmd = new SQLiteCommand(sorgu, conn))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader[0] != DBNull.Value)
                        {
                            degerler.Add(Convert.ToSingle(reader[0]));
                        }
                    }
                }
            }

            return degerler;
        }

        // 7. VERİ GÜNCELLEME (Hem oturuma hem kümülatif hafızaya işler)
        public void IstatistikGuncelle(string kategoriAdi)
        {
            // 1. Anlık oturum sayacını güncelle
            if (_aktifOturum != null)
            {
                switch (kategoriAdi)
                {
                    case "Belgeler": _aktifOturum.Belgeler++; break;
                    case "Resimler": _aktifOturum.Resimler++; break;
                    case "Videolar": _aktifOturum.Videolar++; break;
                    case "Kopyalar": _aktifOturum.Kopyalar++; break;
                    default: _aktifOturum.Diger++; break;
                }
            }

            // 2. Kümülatif Veritabanı Tablosunu Güncelle (Kalıcı Hafıza)
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string check = "SELECT COUNT(*) FROM DosyaIstatistikleri WHERE KategoriAdi = @KategoriAdi";
                using (SQLiteCommand cmd = new SQLiteCommand(check, connection))
                {
                    cmd.Parameters.AddWithValue("@KategoriAdi", kategoriAdi);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count == 0)
                    {
                        string insert = "INSERT INTO DosyaIstatistikleri (KategoriAdi, IslemGorenAdet) VALUES (@KategoriAdi, 1)";
                        using (SQLiteCommand cmdInsert = new SQLiteCommand(insert, connection))
                        {
                            cmdInsert.Parameters.AddWithValue("@KategoriAdi", kategoriAdi);
                            cmdInsert.ExecuteNonQuery();
                        }
                        return;
                    }
                }

                string sorgu = "UPDATE DosyaIstatistikleri SET IslemGorenAdet = IslemGorenAdet + 1 WHERE KategoriAdi = @KategoriAdi";
                using (SQLiteCommand cmdUpdate = new SQLiteCommand(sorgu, connection))
                {
                    cmdUpdate.Parameters.AddWithValue("@KategoriAdi", kategoriAdi);
                    cmdUpdate.ExecuteNonQuery();
                }
            }
        }
    }
}
