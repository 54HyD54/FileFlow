using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Navigation;

namespace FileFlow
{
    internal class TimeAnalyzer
    {
        //Hedef klasördeki dosyaları tarayıp belirlenen tarihteki günden daha eski olanları listeler

        public List<string> EskiDosyalariBul(string klasoryolu, int gunSiniri)
        {
            List<string> eskiDosyalar = new List<string>();

            if (!Directory.Exists(klasoryolu))
            {
                return eskiDosyalar;
            }

            //Klasördeki tüm dosyaları alıyoruz

            string[] dosyalar = Directory.GetFiles(klasoryolu);

            DateTime suAn = DateTime.Now;

            foreach (string dosyaYolu in dosyalar)
            {
                try
                {
                    //Dosya kimliğine erişiyoruz

                    FileInfo fileInfo = new FileInfo(dosyaYolu);

                    //Dosyanın son erişim tarihi

                    DateTime sonErisim = fileInfo.LastAccessTime;

                    //Süre farkı

                    TimeSpan gecenSure = suAn - sonErisim;

                    //Eğer geçen gün sayısı kullanıcının belirlediğinden büyükse dosya ölü demektir

                    if(gecenSure.TotalDays >= gunSiniri)
                    {
                        eskiDosyalar.Add($"{fileInfo.Name} - (Açılmayan Süre: {(int)gecenSure.TotalDays} Gün)");
                    }
                }
                catch (Exception)
                {
                    //Eğer sistem dosyaya erişim izni vermezse dosyanın çökmesini engeller

                    continue;
                }
            }

            return eskiDosyalar;
        }
    }
}

