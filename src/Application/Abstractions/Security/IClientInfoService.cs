namespace Application.Abstractions.Security;

public interface IClientInfoService
{
    string GetIpAddress();
    string GetUserAgent();
}
