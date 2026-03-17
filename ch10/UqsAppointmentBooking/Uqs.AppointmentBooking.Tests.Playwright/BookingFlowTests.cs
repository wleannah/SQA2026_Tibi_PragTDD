namespace Uqs.AppointmentBooking.Tests.Playwright;

/// <summary>
/// End-to-end tests that walk through the full booking flow:
///   Services list  →  Booking form  →  form filled and verified
///
/// Concepts demonstrated:
///   • Multi-page navigation (click → new page)
///   • ClickAsync()             — simulates a real user click
///   • SelectOptionAsync()      — drives &lt;select&gt; dropdowns
///   • FillAsync()              — types into text inputs
///   • ToHaveValueAsync()       — asserts input values after filling
///   • ToBeVisibleAsync()       — verifies an element is on screen
///   • Screenshots at key steps — visual test evidence trail
///   • Trace Viewer output      — full timeline stored in traces/
/// </summary>
public sealed class BookingFlowTests : PlaywrightTestBase
{
    public BookingFlowTests(PlaywrightFixture fixture) : base(fixture) { }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 4 — Navigation: clicking 'Select' opens the booking page
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(DisplayName = "Booking flow — clicking 'Select' navigates to the booking form")]
    public async Task BookingFlow_ClickSelectOnFirstService_NavigatesToBookingPage()
    {
        // ── Arrange: services list ────────────────────────────────────────────
        await GoToAsync();
        await Page.Locator("table.table tbody tr").First.WaitForAsync();
        await ScreenshotAsync("02-services-before-click");

        // ── Act: click the calendar / Select link on the first service row ────
        // a[href^='booking/'] matches every NavLink rendered by Index.razor.
        // .First targets the top-most match.
        await Page.Locator("a[href^='booking/']").First.ClickAsync();

        // Wait for the booking page Blazor component to hydrate
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await ScreenshotAsync("03-booking-page-loaded");

        // ── Assert ────────────────────────────────────────────────────────────
        // Heading must change to "Booking" and the URL must contain /booking/
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Booking");
        Assert.Contains("/booking/", Page.Url,
            StringComparison.OrdinalIgnoreCase);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 5 — Full form: select employee, pick time slot, enter name, verify
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(DisplayName = "Booking flow — fill complete booking form and verify all fields")]
    public async Task BookingFlow_FillCompleteForm_AllFieldsPopulatedCorrectly()
    {
        // ── Step 1: Navigate from the services list to the first booking page ──
        await GoToAsync();
        await Page.Locator("a[href^='booking/']").First.WaitForAsync();
        await Page.Locator("a[href^='booking/']").First.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Booking page is async — wait for the employee dropdown before touching anything
        var employeeDropdown = Page.Locator("select[name='EmployeeId']");
        await employeeDropdown.WaitForAsync();

        // ── Step 2: Select the first employee ─────────────────────────────────
        // SelectOptionAsync drives a <select> element — Index: 0 = first <option>.
        await employeeDropdown.SelectOptionAsync(new SelectOptionValue { Index = 0 });

        // After selecting an employee the slots API is called; wait for it to settle.
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await ScreenshotAsync("04-employee-selected");

        // ── Step 3: Pick the first real time slot ─────────────────────────────
        // #_times is the time dropdown (id="_times" in Booking.razor).
        // options[0] is the "- Time -" placeholder, so we take options[1].
        var timeDropdown = Page.Locator("#_times");
        await timeDropdown.WaitForAsync();

        var timeOptions = await timeDropdown.Locator("option").AllAsync();
        if (timeOptions.Count > 1)
        {
            // Grab the value attribute of the first real slot option
            var firstSlotValue = await timeOptions[1].GetAttributeAsync("value");
            await timeDropdown.SelectOptionAsync(
                new SelectOptionValue { Value = firstSlotValue! });
        }

        await ScreenshotAsync("05-time-slot-selected");

        // ── Step 4: Enter customer details ────────────────────────────────────
        // FillAsync clears any existing value then types character by character.
        await Page.Locator("input[name='FirstName']").FillAsync("Jane");
        await Page.Locator("input[name='LastName']").FillAsync("Student");

        await ScreenshotAsync("06-form-complete");

        // ── Assert: verify field values and Book button is ready ──────────────
        await Expect(Page.Locator("input[name='FirstName']")).ToHaveValueAsync("Jane");
        await Expect(Page.Locator("input[name='LastName']")).ToHaveValueAsync("Student");

        // Service name summary should be visible on the right column
        await Expect(Page.Locator("strong")).ToBeVisibleAsync();

        // Book button must be visible and labelled "Book"
        var bookButton = Page.Locator("button.btn.btn-primary");
        await Expect(bookButton).ToBeVisibleAsync();
        await Expect(bookButton).ToHaveTextAsync("Book");

        // Final screenshot — the fully-filled form before submission
        await ScreenshotAsync("07-ready-to-book");

        // NOTE: We assert but do not click "Book" here because the form POSTs to
        // /book which has no server-side handler in this chapter's code.
        // The Tibi demo intentionally stops here to focus on Playwright mechanics.
    }
}
