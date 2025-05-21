namespace CSharpApp.Core.Interfaces
{
    public interface IJwtAuthService
    {
        Task<string> GetAccessTokenAsync();
    }

}
