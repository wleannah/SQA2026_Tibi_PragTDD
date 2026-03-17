namespace Uqs.AppointmentBooking.Tests.Playwright;

/// <summary>
/// Abstract base class for all Playwright test classes.
///
/// Lifecycle per test class:
///   InitializeAsync  — creates a fresh BrowserContext + IPage + starts Trace recording
///   DisposeAsync     — stops Trace, saves trace.zip, closes context
///
/// Each test class gets its own context (isolated cookies, local storage, etc.)
/// but shares the single browser process owned by PlaywrightFixture.
/// </summary>
[Collection("PlaywrightCollection")]
public abstract class PlaywrightTestBase : IAsyncLifetime
{
    protected readonly PlaywrightFixture Fixture;
    private IBrowserContext? _context;

    /// <summary>The page (tab) tests navigate and assert against.</summary>
    protected IPage Page { get; private set; } = null!;

    protected PlaywrightTestBase(PlaywrightFixture fixture) => Fixture = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // Per-class setup
    // ─────────────────────────────────────────────────────────────────────────
    public async Task InitializeAsync()
    {
        // A BrowserContext is an isolated browser session — like an Incognito window.
        _context = await Fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1400, Height = 900 },
        });

        // Trace Viewer: records every action + screenshot + DOM snapshot + network call.
        // Open the output .zip with:  playwright show-trace traces/<file>.zip
        await _context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,  // Capture a screenshot on every action
            Snapshots   = true,  // Capture DOM snapshots for the timeline
            Sources     = true,  // Embed source files in the trace
            Title       = GetType().Name,
        });

        // IPage = a single tab; this is what all navigation and locators run against.
        Page = await _context.NewPageAsync();

        // Allow up to 30 s for navigation and element waits.
        // Blazor WASM downloads and initialises the .NET runtime before rendering;
        // first load can take 10+ seconds on a cold start.
        Page.SetDefaultTimeout(30_000);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Per-class teardown
    // ─────────────────────────────────────────────────────────────────────────
    public async Task DisposeAsync()
    {
        var tracePath = $"{PlaywrightFixture.TraceDir}/{GetType().Name}.zip";

        await _context!.Tracing.StopAsync(new TracingStopOptions { Path = tracePath });
        await _context.CloseAsync();

        Console.WriteLine($"[Trace] Saved → {Path.GetFullPath(tracePath)}");
        Console.WriteLine($"        View with: playwright show-trace \"{Path.GetFullPath(tracePath)}\"");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers used by all test classes
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Navigate to a website path and wait for the Blazor WASM app to finish
    /// rendering (network settles = all API calls complete, no pending requests).
    /// </summary>
    protected async Task GoToAsync(string relativePath = "")
    {
        await Page.GotoAsync($"{PlaywrightFixture.WebsiteUrl}{relativePath}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Save a full-page screenshot to the screenshots/ folder.
    /// File name is auto-prefixed with the test class name.
    /// </summary>
    protected async Task ScreenshotAsync(string label)
    {
        var path = $"{PlaywrightFixture.ScreenshotDir}/{GetType().Name}-{label}.png";
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
        Console.WriteLine($"[Screenshot] {Path.GetFullPath(path)}");
    }
}
