namespace MumbleApi.Services;

public interface IStorage
{
    public Task<string> UploadFile(string filename, string contentType, Stream file);

    public Task DeleteFileIfPossible(string filename);
}
