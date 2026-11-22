using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Web.Components.Client.OrderEntryTabs;

/// <summary>
///     Stores the files of supporting documents in the <see cref="SupportingDocumentStorage.ContainerName"/> blob container.
///     The tab uploads a file before it attaches the document, so the order only ever records a file that exists.
/// </summary>
internal sealed class SupportingDocumentStore(BlobContainerClient container, ILogger<SupportingDocumentStore> logger)
{
    /// <summary>
    ///     Uploads <paramref name="content"/> to <paramref name="blobPath"/>. The upload fails when a file is already there,
    ///     so a reused document ID can never replace (and a failed attach then delete) another document's file.
    /// </summary>
    public async Task UploadAsync(string blobPath, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        await container.GetBlobClient(blobPath).UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
            Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }
        }, cancellationToken);
    }

    /// <summary>
    ///     Deletes the file of a document the order rejected, as a best effort: a failure is only logged, since an orphaned
    ///     file does no harm beyond the space it takes.
    /// </summary>
    public async Task TryDeleteAsync(string blobPath)
    {
        try
        {
            await container.GetBlobClient(blobPath).DeleteIfExistsAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "The file {BlobPath} of a document the order rejected could not be deleted.", blobPath);
        }
    }
}
