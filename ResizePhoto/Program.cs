using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;
using NLog.Internal;

namespace ResizePhoto
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ///////////////////////////// ДЛЯ РОБОТИ ПРОГРАМИ ПОТРІБНО ДОДАТИ СИСТЕМНИЙ ШЛЯХ ВІНДОВС ДО IrfanView x64!!!!!!!!!!!!!!!!!!

            ObservableCollection<PhotoInfo> InformPhotoHigh = new ObservableCollection<PhotoInfo>();
            ObservableCollection<PhotoInfo> InformPhotoMedium = new ObservableCollection<PhotoInfo>();
            ObservableCollection<PhotoInfo> InformPhotoLow = new ObservableCollection<PhotoInfo>();
            Console.WriteLine("Start");
            ////////////////////////  ВИНЕСТИ ЯК ЗМІННІ КУДИСЬ (файлик або конфіг файл)///////////////////////////
            string HightPhotoPath = System.Configuration.ConfigurationManager.AppSettings.Get("PhatHightPhoto");   //шлях де брати
            string MediumPhotoPath = System.Configuration.ConfigurationManager.AppSettings.Get("PathMediumPhoto");//куди класти великої якості
            string LowPhotoPath = System.Configuration.ConfigurationManager.AppSettings.Get("PathLowPhoto");      //куди класти низької якось
            double mediumSize = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings.Get("MediumPhotoSize"));                          //Найменьша сторона до якої зводити для СЕРЕДНЬОї якость
            double lowSize = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings.Get("LowPhotoSize"));                               //Найменьша сторона до якої зводити для НИЗЬКОЇ якость
            string pathLog = System.Configuration.ConfigurationManager.AppSettings.Get("PathLog");        //куди писати лог



            // Перезапис файлу
            //using (StreamWriter writer = new StreamWriter(pathLog, false))
            //{
            //    await writer.WriteLineAsync(text);
            //}
            // добавление в файл
            try
            {
                using (StreamWriter writer = new StreamWriter(pathLog, true))
                {
                    await writer.WriteLineAsync("//////////////////////////////////////////");
                    await writer.WriteLineAsync("Запуск програми: " + DateTime.Now.ToString());
                    await writer.WriteLineAsync("//////////////////////////////////////////");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Не коректно заданий шлях і наздва для логу: " + pathLog);
            }

            _ = GetInfoFilesAsync(HightPhotoPath, InformPhotoHigh);
            _ = GetInfoFilesAsync(MediumPhotoPath, InformPhotoMedium);
            _ = GetInfoFilesAsync(LowPhotoPath, InformPhotoLow);

            foreach (var highPhoto in InformPhotoHigh)
            {
                if (InformPhotoMedium.Count == 0) // Якщо папки взагалі пусті
                {
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(pathLog, true))
                        {
                            await writer.WriteLineAsync("Знайдено відсутнє фото! Запуск обробки: " + highPhoto.Name);
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Не коректно заданий шлях і наздва для логу: " + pathLog);
                    }

                    _ = ResizePhotoAsync(HightPhotoPath, MediumPhotoPath, highPhoto.Name, mediumSize);
                    _ = ResizePhotoAsync(HightPhotoPath, LowPhotoPath, highPhoto.Name, lowSize);
                }
                else
                {
                    var item = InformPhotoMedium.FirstOrDefault(i => i.Name == highPhoto.Name); // вже робоча))) фігня яка мала додати фото якщо такого ще не існує
                    if (item == null)
                    {
                        try
                        {
                            using (StreamWriter writer = new StreamWriter(pathLog, true))
                            {
                                await writer.WriteLineAsync("Знайдено відсутнє фото! Запуск обробки: " + highPhoto.Name);
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Не коректно заданий шлях і наздва для логу: " + pathLog);
                        }

                        _ = ResizePhotoAsync(HightPhotoPath, MediumPhotoPath, highPhoto.Name, mediumSize);
                        _ = ResizePhotoAsync(HightPhotoPath, LowPhotoPath, highPhoto.Name, lowSize);
                        continue;
                    }
                    foreach (var mediumPhoto in InformPhotoMedium)
                    {

                        if (highPhoto.Name == mediumPhoto.Name && highPhoto.CreateDate > mediumPhoto.CreateDate) // Власне сама перевірка на дату та час
                        {
                            try
                            {
                                using (StreamWriter writer = new StreamWriter(pathLog, true))
                                {
                                    await writer.WriteLineAsync("Знайдено новішу картинку великої якості! Запуск оновлення картинки: " + highPhoto.Name);
                                }
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Не коректно заданий шлях і наздва для логу: " + pathLog);
                            }

                            _ = ResizePhotoAsync(HightPhotoPath, MediumPhotoPath, highPhoto.Name, mediumSize);
                            _ = ResizePhotoAsync(HightPhotoPath, LowPhotoPath, highPhoto.Name, lowSize);
                            continue;
                        }
                    }
                }
            }
            foreach (var mediumPhoto in InformPhotoMedium)
            {
                if (InformPhotoLow.Count == 0) // Якщо папка взагалі пуста
                {
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(pathLog, true))
                        {
                            await writer.WriteLineAsync("Знайдено відсутнє МАЛЕНЬКЕ фото! Запуск обробки: " + mediumPhoto.Name);
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Не коректно заданий шлях і наздва для логу: " + pathLog);
                    }

                    _ = ResizePhotoAsync(MediumPhotoPath, LowPhotoPath, mediumPhoto.Name, lowSize);
                }
                else
                {
                    var item = InformPhotoLow.FirstOrDefault(i => i.Name == mediumPhoto.Name); // вже робоча))) фігня яка мала додати фото якщо такого ще не існує
                    if (item == null)
                    {
                        try
                        {
                            using (StreamWriter writer = new StreamWriter(pathLog, true))
                            {
                                await writer.WriteLineAsync("Знайдено відсутнє МАЛЕНЬКЕ фото! Запуск обробки: " + mediumPhoto.Name);
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Не коректно заданий шлях і наздва для логу: " + pathLog);
                        }

                        _ = ResizePhotoAsync(MediumPhotoPath, LowPhotoPath, mediumPhoto.Name, lowSize);
                        continue;
                    }
                    foreach (var lowPhoto in InformPhotoMedium)
                    {

                        if (lowPhoto.Name == mediumPhoto.Name && mediumPhoto.CreateDate > mediumPhoto.CreateDate) // Власне сама перевірка на дату та час
                        {
                            try
                            {
                                using (StreamWriter writer = new StreamWriter(pathLog, true))
                                {
                                    await writer.WriteLineAsync("Знайдено новішу картинку середньої якості! Запуск оновлення картинки: " + mediumPhoto.Name);
                                }
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Не коректно заданий шлях і наздва для логу: " + pathLog);
                            }
                            _ = ResizePhotoAsync(MediumPhotoPath, LowPhotoPath, mediumPhoto.Name, lowSize);
                            continue;
                        }
                    }
                }
            }
            try
            {
                using (StreamWriter writer = new StreamWriter(pathLog, true))
                {
                    await writer.WriteLineAsync("Програма успішно завершила роботу!");
                    await writer.WriteLineAsync("Час завершення роботи: " + DateTime.Now.ToString());
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Не коректно заданий шлях і наздва для логу: " + pathLog);
                Console.ReadKey();
            }
            
            //Інформація про файли
            async Task GetInfoFilesAsync(string path, ObservableCollection<PhotoInfo> InformPhoto)
            {
                try
                {
                    string[] files = System.IO.Directory.GetFiles(path);
                    foreach (string file in files)
                    {
                        PhotoInfo photoInfo = new PhotoInfo();
                        photoInfo.Name = System.IO.Path.GetFileName(file);
                        photoInfo.CreateDate = System.IO.File.GetLastWriteTime(file);
                        InformPhoto.Add(photoInfo);
                    }
                }
                catch (Exception)
                {
                    using (StreamWriter writer = new StreamWriter(pathLog, true))
                    {
                        await writer.WriteLineAsync("Не знайдено вказаний шлях: " + path);
                    }
                    Environment.Exit(0);
                }
            }


            //зміна якості файлу через програмку
            async Task ResizePhotoAsync(string StartPath, string EndPath, string name, double size)
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmd";
                psi.Arguments = @" /c i_view64.exe " + StartPath + name + @" /resize_short=" + size + " /resample /aspectratio /convert=" + EndPath + name + " /silent";
                Process.Start(psi);
                using (StreamWriter writer = new StreamWriter(pathLog, true))
                {
                    await writer.WriteLineAsync("Катринку " + name + " відредаговано та скопійовано: " + EndPath + " Розмір найкоротшої сторони: " + size);
                }
            }
        }

    }

    public class PhotoInfo //клас для збереження даних
    {
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
    }

}