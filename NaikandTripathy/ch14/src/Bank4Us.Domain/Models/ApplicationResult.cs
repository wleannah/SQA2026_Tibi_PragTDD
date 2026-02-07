
namespace Bank4Us.Domain.Models;

public sealed class ApplicationResult
{
    public ApplicationStatus Status { get; init; }
    public List<string> Errors { get; } = new();

    public static ApplicationResult Approved() => new() { Status = ApplicationStatus.Approved };

    public static ApplicationResult Cancelled(params string[] errors)
    {
        var r = new ApplicationResult { Status = ApplicationStatus.Cancelled };
        r.Errors.AddRange(errors);
        return r;
    }

    public static ApplicationResult Incomplete(params string[] errors)
    {
        var r = new ApplicationResult { Status = ApplicationStatus.Incomplete };
        r.Errors.AddRange(errors);
        return r;
    }

    public static ApplicationResult PendingVerification(params string[] errors)
    {
        var r = new ApplicationResult { Status = ApplicationStatus.PendingVerification };
        r.Errors.AddRange(errors);
        return r;
    }

    public static ApplicationResult NeedsExtraVerification(params string[] errors)
    {
        var r = new ApplicationResult { Status = ApplicationStatus.NeedsExtraVerification };
        r.Errors.AddRange(errors);
        return r;
    }
}
