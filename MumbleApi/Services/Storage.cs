using System.Net;

using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

using MumbleApi.Application;
using MumbleApi.Errors;

namespace MumbleApi.Services;

internal class Storage : IStorage
{
    private readonly string _bucketName;
    private readonly StorageClient _gcs;

    public Storage(AppConfig config)
    {
        _bucketName = config.Storage.Bucket;
        _gcs = StorageClient.Create(GoogleCredential.FromJson(config.Storage.ServiceAccountKey));
    }

    public async Task<string> UploadFile(string filename, string contentType, Stream file)
    {
        var obj = await _gcs.UploadObjectAsync(_bucketName, filename, contentType, file);
        if (obj is null)
        {
            throw new StorageException("Could not upload file.");
        }

        return PublicUrl(filename);
    }

    public async Task DeleteFileIfPossible(string filename)
    {
        try
        {
            await _gcs.DeleteObjectAsync(_bucketName, filename);
        }
        catch (GoogleApiException gex) when (gex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            // intentionally.
        }
    }

    private string PublicUrl(string filename) => $"https://storage.googleapis.com/{_bucketName}/{filename}";
}
