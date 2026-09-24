using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Phanmemwar3.Core
{
    public class KkweUpdateInfo
    {
        public string Version { get; set; } = "";
        public string Title { get; set; } = "";
        public string PubDate { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public long ContentLength { get; set; } = 0;
        public string ReleaseNotesUrl { get; set; } = "";
    }

    public class KkweUpdater
    {
        private const string APPCAST_URL = "https://up5.update.netease.com/pl/kk-editor-appcast.txt";
        private readonly HttpClient _http;

        public KkweUpdater()
        {
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            _http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }

        public async Task<KkweUpdateInfo?> CheckForUpdatesAsync()
        {
            try
            {
                var content = await _http.GetStringAsync(APPCAST_URL);
                if (string.IsNullOrWhiteSpace(content)) return null;

                var doc = XDocument.Parse(content);
                XNamespace sparkle = "http://www.andymatuschak.org/xml-namespaces/sparkle";

                var item = doc.Root?.Element("channel")?.Element("item");
                if (item == null) return null;

                var info = new KkweUpdateInfo
                {
                    Title = item.Element("title")?.Value ?? "",
                    PubDate = item.Element("pubDate")?.Value ?? "",
                    ReleaseNotesUrl = item.Element(sparkle + "releaseNotesLink")?.Value ?? ""
                };

                var enclosure = item.Element("enclosure");
                if (enclosure != null)
                {
                    info.DownloadUrl = enclosure.Attribute("url")?.Value ?? "";
                    info.Version = enclosure.Attribute(sparkle + "version")?.Value ?? "";
                    if (long.TryParse(enclosure.Attribute("length")?.Value, out long len))
                    {
                        info.ContentLength = len;
                    }
                }

                if (string.IsNullOrEmpty(info.Version))
                {
                    var m = Regex.Match(info.Title, @"[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+");
                    if (m.Success) info.Version = m.Value;
                }

                return info;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DownloadInstallerAsync(string downloadUrl, string destinationFile, IProgress<int>? progress, CancellationToken ct)
        {
            try
            {
                using var response = await _http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;
                using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var fileStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                    totalRead += bytesRead;

                    if (totalBytes.HasValue && totalBytes.Value > 0)
                    {
                        int percent = (int)((totalRead * 100) / totalBytes.Value);
                        progress?.Report(percent);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
