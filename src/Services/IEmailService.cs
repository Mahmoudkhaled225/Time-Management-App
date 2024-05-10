namespace src.Services;

public interface IEmailService
{
    // can make email dto or make it entity to be saved in db
    Task<bool> SendEmail(string to, string subject, string body);}