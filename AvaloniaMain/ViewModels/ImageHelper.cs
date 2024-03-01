using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Threading.Tasks;

namespace AvaloniaMain.ViewModels
{
    public static class ImageHelper
    {
        public static Bitmap LoadFromFile(string filePath)
        {
            try
            {
                return new Bitmap(filePath);
            }
            catch (Exception ex)
            {
                // Обробка помилок завантаження зображення
                Console.WriteLine($"Error loading image from file '{filePath}': {ex.Message}");
                return null;
            }
        }

        public static async Task<Bitmap?> LoadFromWeb(Uri url)
        {
            using var httpClient = new HttpClient();
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsByteArrayAsync();
                return new Bitmap(new MemoryStream(data));
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"An error occurred while downloading image '{url}' : {ex.Message}");
                return null;
            }
        }
    }
}
