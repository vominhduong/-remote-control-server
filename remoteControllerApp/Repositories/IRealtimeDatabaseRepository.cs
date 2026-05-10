namespace remoteControllerApp.Repositories;

public interface IRealtimeDatabaseRepository
{
    Task SetAsync<T>(string path, T data);

    Task PatchAsync<T>(string path, T data);

    Task<T?> GetAsync<T>(string path);

    Task DeleteAsync(string path);
}