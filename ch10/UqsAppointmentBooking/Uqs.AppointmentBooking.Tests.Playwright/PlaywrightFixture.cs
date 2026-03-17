namespace Uqs.AppointmentBooking.Tests.Playwright;

/// <summary>
/// Shared fixture that owns the Playwright engine and one browser process.
/// Lives for the lifetime of the [Collection("PlaywrightCollection")] — created
/// once before the first test and disposed after the last.
///
/// Demo settings:
///   Headless = false  → visible browser window on the projector
///   SlowMo   = 700    → 700 ms between each action — easy to follow live
/// </summary>
public sealed class PlaywrightFixture : IAsyncLifetime
{
    // ── Local URLs ────────────────────────────────────────────────────────────
    // HTTP to avoid SSL certificate trust issues during local development.
    // Values come from launchSettings.json in each project.
    public const string WebsiteUrl = "http://localhost:5260";
    public const string ApiUrl     = "http://localhost:5259";

    // ── Output directories (created in InitializeAsync) ───────────────────────
    public const string ScreenshotDir = "screenshots";
    public const string TraceDir      = "traces";

    // ── Playwright handles ────────────────────────────────────────────────────
    private IPlaywright? _playwright;
    public  IBrowser     Browser { get; private set; } = null!;

    // ─────────────────────────────────────────────────────────────────────────
    // Setup — called once before ANY test in the collection runs.
    // ─────────────────────────────────────────────────────────────────────────
    public async Task InitializeAsync()
    {
        // Ensure output folders exist so screenshot/trace saves never throw.
        Directory.CreateDirectory(ScreenshotDir);
        Directory.CreateDirectory(TraceDir);

        // Playwright.CreateAsync() returns the root entry-point to the API.
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        // Raise the default assertion timeout from 5 s → 20 s.
        // Blazor WASM initialises the .NET 10 runtime client-side AFTER the network
        // goes idle, so h1/table elements can take several seconds to appear even
        // though GotoAsync + NetworkIdle has already returned.
        SetDefaultExpectTimeout(20_000);

        // Launch Chromium.
        // TIP: run once before tests to download browser binaries:
        //   powershell bin\Debug\net10.0\playwright.ps1 install chromium
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,   // Show the browser window — key for classroom demos
            SlowMo   = 700,     // Milliseconds between each Playwright action
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Teardown — called once after ALL tests in the collection have run.
    // ─────────────────────────────────────────────────────────────────────────
    public async Task DisposeAsync()
    {
        await Browser.CloseAsync();
        _playwright?.Dispose();
    }
}
