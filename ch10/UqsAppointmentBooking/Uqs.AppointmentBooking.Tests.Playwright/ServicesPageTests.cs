namespace Uqs.AppointmentBooking.Tests.Playwright;

/// <summary>
/// Tests for the Services listing page (Index.razor — route: /).
///
/// Concepts demonstrated:
///   • GotoAsync + WaitForLoadStateAsync  — navigation and Blazor WASM hydration
///   • Locator API                        — resilient element selection
///   • Expect(locator).ToHaveText*()      — auto-retrying assertions (no Thread.Sleep)
///   • First.WaitForAsync()               — explicit wait for dynamic content
///   • AllTextContentsAsync()             — bulk text extraction
///   • ScreenshotAsync()                  — visual artifact capture
/// </summary>
public sealed class ServicesPageTests : PlaywrightTestBase
{
    public ServicesPageTests(PlaywrightFixture fixture) : base(fixture) { }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 1 — Smoke: page loads and shows the correct heading
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(DisplayName = "Services page — heading reads 'Our Services'")]
    public async Task ServicesPage_WhenLoaded_ShowsOurServicesHeading()
    {
        // ── Arrange & Act ──────────────────────────────────────────────────
        // GoToAsync navigates and waits for NetworkIdle — Blazor WASM is fully
        // rendered by the time this returns. No Thread.Sleep required.
        await GoToAsync();

        // ── Assert ─────────────────────────────────────────────────────────
        // Expect() is Playwright's built-in auto-retrying assertion.
        // It polls the locator up to its timeout (default 5 s) before failing,
        // which handles any residual async rendering.
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Our Services");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 2 — Data: at least one service row appears (proves API connectivity)
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(DisplayName = "Services page — at least one service row loaded from API")]
    public async Task ServicesPage_ApiConnected_ShowsAtLeastOneServiceRow()
    {
        await GoToAsync();

        // The table body is populated after an async HTTP call to /services.
        // First.WaitForAsync() auto-waits until a <tr> appears — no sleep loop.
        var rows = Page.Locator("table.table tbody tr");
        await rows.First.WaitForAsync();

        var count = await rows.CountAsync();
        Assert.True(count > 0, $"Expected services from the API but found {count} rows. " +
                               "Is the WebApi running and the Cosmos Emulator started?");

        // Screenshot at this point — saved to screenshots/ for review
        await ScreenshotAsync("01-services-loaded");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 3 — Content: every row has a non-empty name and a GBP price
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(DisplayName = "Services page — every row has a service name and a £ price")]
    public async Task ServicesPage_EachRow_HasServiceNameAndGBPPrice()
    {
        await GoToAsync();
        await Page.Locator("table.table tbody tr").First.WaitForAsync();

        // AllTextContentsAsync() collects text from every matching element in one call.
        var names  = await Page.Locator("table.table tbody tr td:nth-child(1)").AllTextContentsAsync();
        var prices = await Page.Locator("table.table tbody tr td:nth-child(2)").AllTextContentsAsync();

        // Assert all — xUnit will report the specific failing entry if any fail.
        Assert.All(names,  name  => Assert.False(string.IsNullOrWhiteSpace(name),
                                       "Found a service row with a blank name."));
        Assert.All(prices, price => Assert.StartsWith("£", price.Trim(),
                                       StringComparison.Ordinal));
    }
}
