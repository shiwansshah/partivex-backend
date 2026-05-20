namespace Partivex.Domain.Entities;

public class SmtpSetting
{
    public int Id { get; set; }

    public string SenderEmail { get; set; } = string.Empty;

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;

    public DateTimeOffset UpdatedAt { get; set; }
}
