

using S3Playground;

await new S3TransferUtility().UploadAsync();
await new S3TransferUtility().DownloadAsync();

await new S3ClientUtility().UploadAsync();
await new S3ClientUtility().DownloadAsync();

await new S3TransferUtilityWithSTS().DownloadAsync();