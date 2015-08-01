using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace ScreenshotUploader
{
    class Program
    {
        static void Main(string[] args)
        {
            string my_documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string poe_subdir = "My Games\\Path of Exile\\Screenshots";
            string path;
            try {
                path = Path.Combine(my_documents, poe_subdir);
            } catch (Exception e) {
                Console.WriteLine("Error combining {0} and {1} because {2}{3}",
                    poe_subdir, my_documents, Environment.NewLine, e.Message);
                Console.ReadKey();
                return;
            }

            watch(path);
            while (true) {
                Thread.Sleep(1000);
            }
        }

        static void watch(string path)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.*";
            watcher.Changed += new FileSystemEventHandler(on_new_screenshot);
            watcher.EnableRaisingEvents = true;
            Console.WriteLine("Waiting for new screenshots in:\n{0}", path);
        }

        static async void upload_image(string path)
        {
            Console.WriteLine("Uploading {0}", path);
            try {
                using (var client = new HttpClient()) {
                    using (var stream = File.OpenRead(path)) {
                        var content = new MultipartFormDataContent();
                        var file_content = new ByteArrayContent(new StreamContent(stream).ReadAsByteArrayAsync().Result);
                        file_content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                        file_content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = "screenshot.png",
                            Name = "foo",
                        };
                        content.Add(file_content);
                        client.BaseAddress = new Uri("https://pajlada.se/poe/imgup/");
                        var response = await client.PostAsync("upload.php", content);
                        response.EnsureSuccessStatusCode();
                        Console.WriteLine("Done");
                    }
                }

            } catch (Exception) {
                Console.WriteLine("Something went wrong while uploading the image");
            }
        }

        static void on_new_screenshot(object sender, FileSystemEventArgs e)
        {
            upload_image(e.FullPath);
        }
    }
}