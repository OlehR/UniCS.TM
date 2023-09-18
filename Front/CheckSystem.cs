using System;
using System.IO;
using ModelMID;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Front
{
    public class CheckSystem
    {
        const long MinFreeSpace = 1; // мінімальна кількість місця на диску до якого не виникає повідомлення
        const int MaxAgeLogFile = 30;// максимальна кількість днів збереження логів 


        public void CheckDisk()
        {
            // Отримати інформацію про системний диск
            DriveInfo systemDrive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory));

            // Отримати інформацію про диск, з якого запускається програма
            DriveInfo startupDrive = new DriveInfo(Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory));

            // Перевірити вільне місце на дисках
            long systemDriveFreeSpace = systemDrive.AvailableFreeSpace;
            long startupDriveFreeSpace = startupDrive.AvailableFreeSpace;

            // Перевірка на наявність вказаного вільного місця
            long minFreeSpaceInBytes = MinFreeSpace * 1024L * 1024L * 1024L;  // розмір в байтах

            if (systemDriveFreeSpace < minFreeSpaceInBytes)
            {
                DialogResult dialogResult = MessageBox.Show($"На системному диску залишилось {systemDriveFreeSpace / (1024 * 1024)} MB!!! Продовжити роботу програми?", "Увага!", (MessageBoxButtons)MessageBoxButton.YesNo, (MessageBoxIcon)MessageBoxImage.Question);
                if (dialogResult == DialogResult.No)
                    ExitProgram();
            }

            if (startupDriveFreeSpace < minFreeSpaceInBytes)
            {
                DialogResult dialogResult = MessageBox.Show($"На робочому диску залишилось {startupDriveFreeSpace / (1024 * 1024)} MB!!! Продовжити роботу програми?", "Увага!", (MessageBoxButtons)MessageBoxButton.YesNo, (MessageBoxIcon)MessageBoxImage.Question);
                if (dialogResult == DialogResult.No)
                    ExitProgram();
            }
        }

        private void ExitProgram()
        {
            System.Diagnostics.Process.Start("explorer.exe");
            System.Windows.Application.Current.Shutdown();
        }

        
        public async Task DeleteOldLogFilesAsync()
        {
            string logFolderPath = Global.PathLog;
            if (Directory.Exists(logFolderPath))
            {
                try
                {
                    // Отримати поточну дату
                    DateTime currentDate = DateTime.Now;

                    // Отримати всі файли у папці з логами
                    DirectoryInfo logDirectory = new DirectoryInfo(logFolderPath);
                    FileInfo[] logFiles = logDirectory.GetFiles();

                    foreach (FileInfo logFile in logFiles)
                    {
                        // Визначити вік файлу (різниця між поточною датою і датою створення файлу)
                        TimeSpan age = currentDate - logFile.CreationTime;

                        // Перевірка, чи файл старіший за вказане число
                        if (age.TotalDays > MaxAgeLogFile)
                            await Task.Run(() => logFile.Delete());
                        
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка видалення логів! {ex}");
                }
            }
        }
    }
}
