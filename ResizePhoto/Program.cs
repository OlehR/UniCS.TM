using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace ResizePhoto
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ObservableCollection<PhotoInfo> InformPhotoHigh = new ObservableCollection<PhotoInfo>();
            ObservableCollection<PhotoInfo> InformPhotoMedium = new ObservableCollection<PhotoInfo>();
            ObservableCollection<PhotoInfo> InformPhotoLow = new ObservableCollection<PhotoInfo>();
            Console.WriteLine("Start");

            ////////////////////////  ВИНЕСТИ ЯК ЗМІННІ КУДИСЬ (файлик або конфіг файл)///////////////////////////
            string HightPhotoPath = @"d:\Pictures\Test\High\";   //шлях де брати
            string MediumPhotoPath = @"d:\Pictures\Test\Low\";   //куди класти великої якості
            string LowPhotoPath = @"d:\Pictures\Test\Medium\";   //куди класти низької якось
            double mediumSize = 600;                             //Найменьша сторона до якої зводити для СЕРЕДНЬОї якость
            double lowSize = 300;                                //Найменьша сторона до якої зводити для НИЗЬКОЇ якость


            GetInfoFiles(HightPhotoPath, InformPhotoHigh); 
            GetInfoFiles(MediumPhotoPath, InformPhotoMedium);
            GetInfoFiles(LowPhotoPath, InformPhotoLow);

           ////////// Тимчасовий вивід інформаціїї
            Console.WriteLine("High");
            foreach (var photo in InformPhotoHigh)
            {
                Console.WriteLine(photo.Name);
                Console.WriteLine(photo.CreateDate);
            }
            Console.WriteLine("Medium");
            foreach (var photo in InformPhotoMedium)
            {
                Console.WriteLine(photo.Name);
                Console.WriteLine(photo.CreateDate);
            }
            Console.WriteLine("Low");
            foreach (var photo in InformPhotoLow)
            {
                Console.WriteLine(photo.Name);
                Console.WriteLine(photo.CreateDate);
            }
            Console.WriteLine("Тицніть клавішу для запуску основного коду");
            Console.ReadKey();
            foreach (var highPhoto in InformPhotoHigh)
            {
                if (InformPhotoMedium.Count == 0 || InformPhotoLow.Count == 0) // Якщо папки взагалі пусті
                {
                    ResizePhoto(HightPhotoPath, MediumPhotoPath, highPhoto.Name, mediumSize);
                    ResizePhoto(HightPhotoPath, LowPhotoPath, highPhoto.Name, lowSize);
                }
                else
                {
                    foreach (var mediumPhoto in InformPhotoMedium)
                    {
                        var item = InformPhotoMedium.FirstOrDefault(i => i.Name == highPhoto.Name); // неробоча фігня яка мала додати фото якщо такого ще не існує
                        if (item == null)
                        {
                            ResizePhoto(HightPhotoPath, MediumPhotoPath, highPhoto.Name, mediumSize);
                            ResizePhoto(HightPhotoPath, LowPhotoPath, highPhoto.Name, lowSize);
                            continue;
                        }
                        if (highPhoto.Name == mediumPhoto.Name && highPhoto.CreateDate > mediumPhoto.CreateDate) // Власне сама перевірка на дату та час
                        {
                            ResizePhoto(HightPhotoPath, MediumPhotoPath, highPhoto.Name, mediumSize);
                            ResizePhoto(HightPhotoPath, LowPhotoPath, highPhoto.Name, lowSize);
                            continue;
                        }
                    }

                }

            }

            //Інформація про файли
            void GetInfoFiles(string path, ObservableCollection<PhotoInfo> InformPhoto)
            {
                string[] files = System.IO.Directory.GetFiles(path);
                foreach (string file in files)
                {
                    PhotoInfo photoInfo = new PhotoInfo();
                    photoInfo.Name = System.IO.Path.GetFileName(file);
                    photoInfo.CreateDate = System.IO.File.GetLastWriteTime(file);
                    InformPhoto.Add(photoInfo);
                    //Console.WriteLine("дата и время создания файла: " + System.IO.File.GetCreationTime(file).ToShortDateString().ToString());
                }
            }

            //ResizePhoto(@"d:\Pictures\Categories\", @"d:\Pictures\Categories\", "test.png");

            //зміна якості файлу через програмку
            void ResizePhoto(string StartPath, string EndPath, string name, double size)
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmd";
                psi.Arguments = @" /c i_view64.exe " + StartPath + name + @" /resize_short=" + size + " /resample /aspectratio /convert=" + EndPath + name;
                Process.Start(psi);
            }
        }

    }

    public class PhotoInfo //клас для збереження даних
    {
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
    }

}