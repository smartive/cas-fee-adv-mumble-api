using MumbleApi.Services;

namespace MumbleApi.Test.Mocks;

public class MockStorage : IStorage
{
    public Task<string> UploadFile(string filename, string contentType, Stream file)
        => Task.FromResult($"https://mockstorage/{filename}");

    public Task DeleteFileIfPossible(string filename)
    => Task.CompletedTask;
}
