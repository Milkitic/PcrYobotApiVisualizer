using System;
using System.Threading.Tasks;

namespace YobotExtension.Shared.YobotService
{
    public interface IYobotService<T>
    {
        event Func<Task<string>> InitRequested;
        event Func<Task<bool>> OriginChangeRequested;

        string Version { get; }
        string ApiVersion { get; }
        UriType InitUriType { get; }
        bool IsLogin { get; }

        Task<string> ValidateVersion();
        Task<T> GetApiInfo();
        Task<bool> LoginAsync(string url);
        Task<bool> LogoutAsync();
    }
}