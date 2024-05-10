
using Twilio.Rest.Api.V2010.Account;

namespace src.Services;

public interface ISmsService
{
    Task<MessageResource?> Send(string phone, string? body);
}