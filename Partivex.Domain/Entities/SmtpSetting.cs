namespace Partivex.Domain.Entities;

public class SmtpSetting
{
    public int Id { get; set; }

    public string SenderEmail { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAt { get; set; }
}
