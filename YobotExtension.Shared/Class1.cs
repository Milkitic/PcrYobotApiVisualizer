using System.Threading.Tasks;

namespace YobotExtension.Shared
{
    public interface IYobotServiceV1<T>
    {
        UriType InitUriType { get; }
        bool IsLogin { get; }

        Task<T> GetApiInfo();
        Task<bool> LoginAsync(string url);
        Task<bool> LogoutAsync();
    }

    public enum UriType
    {
        Login, Api
    }
}
