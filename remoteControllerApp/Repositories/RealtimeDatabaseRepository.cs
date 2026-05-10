using remoteControllerApp.Firebase;

namespace remoteControllerApp.Repositories;

public class RealtimeDatabaseRepository : IRealtimeDatabaseRepository
{
    private readonly RealtimeDatabaseClient _client;

    public RealtimeDatabaseRepository(RealtimeDatabaseClient client)
    {
        _client = client;
    }

    public Task SetAsync<T>(string path, T data)
    {
        return _client.SetAsync(path, data);
    }

    public Task PatchAsync<T>(string path, T data)
    {
        return _client.PatchAsync(path, data);
    }

    public Task<T?> GetAsync<T>(string path)
    {
        return _client.GetAsync<T>(path);
    }

    public Task DeleteAsync(string path)
    {
        return _client.DeleteAsync(path);
    }
}