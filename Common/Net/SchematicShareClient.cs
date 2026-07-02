using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Git.Common.Data;
using Terraria;

namespace Git.Common.Net
{
    // Client for a schematic sharing website. The site must implement:
    //
    //   POST {base}/api/schematics
    //     body: Schematic JSON (name + commits)
    //     response: 200 with a plain-text share code (e.g. "a3f9k2")
    //
    //   GET {base}/api/schematics/{code}
    //     response: 200 with the Schematic JSON
    //
    // All work happens off-thread; results are marshaled back to the game
    // thread via Main.QueueMainThreadAction before touching game state.
    public static class SchematicShareClient
    {
        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(15) };

        public static void Upload(Schematic schematic, string baseUrl, Action<string> setStatus)
        {
            Task.Run(async () =>
            {
                try
                {
                    string json = JsonSerializer.Serialize(schematic);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var resp = await Http.PostAsync($"{baseUrl.TrimEnd('/')}/api/schematics", content);
                    string body = (await resp.Content.ReadAsStringAsync()).Trim();

                    if (resp.IsSuccessStatusCode)
                        Report(setStatus, $"Uploaded! Share code: {body}");
                    else
                        Report(setStatus, $"Upload failed ({(int)resp.StatusCode})");
                }
                catch (Exception e)
                {
                    Report(setStatus, $"Upload error: {e.Message}");
                }
            });
        }

        public static void Download(string code, string baseUrl, Action<Schematic> onDownloaded, Action<string> setStatus)
        {
            Task.Run(async () =>
            {
                try
                {
                    string url = $"{baseUrl.TrimEnd('/')}/api/schematics/{Uri.EscapeDataString(code)}";
                    var resp = await Http.GetAsync(url);

                    if (!resp.IsSuccessStatusCode)
                    {
                        Report(setStatus, $"Download failed ({(int)resp.StatusCode})");
                        return;
                    }

                    string body = await resp.Content.ReadAsStringAsync();
                    var schematic = JsonSerializer.Deserialize<Schematic>(body);

                    if (schematic == null || schematic.Commits == null || schematic.Commits.Count == 0)
                    {
                        Report(setStatus, "Download failed: invalid schematic data");
                        return;
                    }

                    Main.QueueMainThreadAction(() =>
                    {
                        onDownloaded(schematic);
                        setStatus($"Downloaded \"{schematic.Name}\"");
                    });
                }
                catch (Exception e)
                {
                    Report(setStatus, $"Download error: {e.Message}");
                }
            });
        }

        private static void Report(Action<string> setStatus, string message)
            => Main.QueueMainThreadAction(() => setStatus(message));
    }
}
