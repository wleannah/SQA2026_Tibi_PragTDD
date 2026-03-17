namespace Uqs.AppointmentBooking.Tests.Playwright;

/// <summary>
/// xUnit collection definition — tells xUnit to create ONE PlaywrightFixture
/// (browser process) and share it across every test class that carries
/// [Collection("PlaywrightCollection")].
///
/// This class needs no code; xUnit reads the attributes.
/// </summary>
[CollectionDefinition("PlaywrightCollection")]
public sealed class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }
