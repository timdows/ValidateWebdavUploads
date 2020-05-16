using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebDav;

namespace ValidateWebdavUploads
{
    class Program
    {
        public static IWebDavClient _client;

        static async Task Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Needed arguments:\r\n1. Webdav base address\r\n2. User\r\n3. Password\r\n4. Localshare");
                return;
            }

            var baseAddress = args[0].ToString();
            var user = args[1].ToString();
            var pass = args[2].ToString();
            var localShare = args[3].ToString();

            Console.WriteLine($"1. Webdav base address: {baseAddress}\r\n2. User: {user}\r\n3. Password: ****\r\n4. Localshare: {localShare}");

            var clientParams = new WebDavClientParams
            {
                BaseAddress = new Uri(baseAddress),
                Credentials = new NetworkCredential(user, pass)
            };
            _client = new WebDavClient(clientParams);

            var files = Directory.GetFiles(localShare, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                //var isUploaded = await CheckIfFileIsUploaded(baseAddress, file.Replace(localShare, string.Empty));

                var size = new System.IO.FileInfo(file).Length;
                var isUploadedAndSameSize = await CheckIfFileIsUploadedAndSameSize(baseAddress, file.Replace(localShare, string.Empty), size);

                if (!isUploadedAndSameSize)
                {
                    Console.WriteLine($"MISSING: {file}");
                }
                else
                {
                    Console.WriteLine($"OKAY: {file}");
                }
            }
        }

        static async Task<bool> CheckIfFileIsUploaded(string baseAddress, string filePath)
        {
            var requestUri = baseAddress + filePath.Replace("\\", "/");
            var result = await _client.Propfind(requestUri);

            return result.IsSuccessful;
        }

        static async Task<bool> CheckIfFileIsUploadedAndSameSize(string baseAddress, string filePath, long size)
        {
            var requestUri = baseAddress + filePath.Replace("\\", "/");
            var result = await _client.Propfind(requestUri);

            if (!result.IsSuccessful)
            {
                return false;
            }

            var remoteSize = result.Resources.First().ContentLength;
            return result.IsSuccessful && size == remoteSize;
        }
    }
}
