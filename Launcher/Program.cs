using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;

const string Owner = "Marcebooy";
const string Repo = "SurvivorPrototype";

string baseDir = AppContext.BaseDirectory;
string gameDir = Path.Combine(baseDir, "Game");
string versionFile = Path.Combine(baseDir, "version.txt");

Console.WriteLine("SurvivorPrototype Launcher");
Console.WriteLine("==========================");

string? localVersion = File.Exists(versionFile) ? File.ReadAllText(versionFile).Trim() : null;

JsonElement? latestRelease = null;
try
{
    latestRelease = await FetchLatestGameReleaseAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Päivitysten tarkistus epäonnistui ({ex.Message}).");
}

if (latestRelease is { } release)
{
    string remoteVersion = release.GetProperty("tag_name").GetString()!;
    JsonElement zipAsset = release.GetProperty("assets").EnumerateArray()
        .FirstOrDefault(a => a.GetProperty("name").GetString()!.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

    if (zipAsset.ValueKind != JsonValueKind.Undefined)
    {
        bool needsInstall = remoteVersion != localVersion || FindGameExecutable(gameDir) is null;
        if (needsInstall)
        {
            Console.WriteLine($"Uusi versio saatavilla: {remoteVersion} (nykyinen: {localVersion ?? "ei asennettu"})");
            string downloadUrl = zipAsset.GetProperty("browser_download_url").GetString()!;
            await DownloadAndInstallAsync(downloadUrl, gameDir);
            File.WriteAllText(versionFile, remoteVersion);
            Console.WriteLine("Päivitys asennettu.");
        }
        else
        {
            Console.WriteLine($"Peli on ajan tasalla (versio {localVersion}).");
        }
    }
}

string? exePath = FindGameExecutable(gameDir);
if (exePath is null)
{
    Console.WriteLine("Peliä ei löytynyt eikä sitä voitu ladata. Tarkista nettiyhteys ja yritä uudelleen.");
    Console.WriteLine("Paina mitä tahansa näppäintä sulkeaksesi.");
    Console.ReadKey();
    return;
}

Console.WriteLine("Käynnistetään peli...");
Process.Start(new ProcessStartInfo(exePath)
{
    WorkingDirectory = Path.GetDirectoryName(exePath)!,
    UseShellExecute = true
});

static string? FindGameExecutable(string dir)
{
    if (!Directory.Exists(dir)) return null;
    return Directory.EnumerateFiles(dir, "*.exe", SearchOption.AllDirectories)
        .FirstOrDefault(f => !Path.GetFileName(f).Contains("UnityCrashHandler", StringComparison.OrdinalIgnoreCase));
}

static async Task<JsonElement?> FetchLatestGameReleaseAsync()
{
    using var client = new HttpClient();
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SurvivorPrototypeLauncher", "1.0"));
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

    string json = await client.GetStringAsync($"https://api.github.com/repos/{Owner}/{Repo}/releases");
    using JsonDocument doc = JsonDocument.Parse(json);

    var releases = doc.RootElement.EnumerateArray()
        .Where(r => !r.GetProperty("draft").GetBoolean())
        .Where(r => !string.Equals(r.GetProperty("tag_name").GetString(), "launcher", StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(r => r.GetProperty("published_at").GetString())
        .ToList();

    return releases.Count > 0 ? releases[0].Clone() : null;
}

static async Task DownloadAndInstallAsync(string url, string gameDir)
{
    using var client = new HttpClient();
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SurvivorPrototypeLauncher", "1.0"));

    string tempZip = Path.Combine(Path.GetTempPath(), $"survivorprototype_update_{Guid.NewGuid():N}.zip");
    Console.WriteLine("Ladataan päivitystä...");
    await using (Stream stream = await client.GetStreamAsync(url))
    await using (FileStream fileStream = File.Create(tempZip))
    {
        await stream.CopyToAsync(fileStream);
    }

    Console.WriteLine("Puretaan tiedostoja...");
    if (Directory.Exists(gameDir))
        Directory.Delete(gameDir, recursive: true);
    Directory.CreateDirectory(gameDir);
    ZipFile.ExtractToDirectory(tempZip, gameDir);
    File.Delete(tempZip);
}
