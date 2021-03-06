﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using OSSFinder.Infrastructure.Exceptions;
using OSSFinder.Models;
using OSSFinder.Services.Interfaces;

namespace OSSFinder.Services
{
    public class CloudReportService : IReportService
    {
        private readonly string _connectionString;

        public CloudReportService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<StatisticsReport> Load(string name)
        {
            //  Always use lowercase names for all blobs in Azure Storage
            name = name.ToLowerInvariant();

            string connectionString = _connectionString;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("stats");
            CloudBlockBlob blob = container.GetBlockBlobReference("popularity/" + name);
            //Check if the report blob is present before processing it.
            if (!blob.Exists())
            {
                throw new StatisticsReportNotFoundException();
            }

            var stream = new MemoryStream();

            await Task.Factory.FromAsync(blob.BeginFetchAttributes(null, null), blob.EndFetchAttributes);
            await Task.Factory.FromAsync(blob.BeginDownloadToStream(stream, null, null), blob.EndDownloadToStream);

            stream.Seek(0, SeekOrigin.Begin);

            string content;
            using (TextReader reader = new StreamReader(stream))
            {
                content = reader.ReadToEnd();
            }

            return new StatisticsReport(content, (blob.Properties.LastModified == null ? (DateTime?)null : blob.Properties.LastModified.Value.UtcDateTime));
        }
    }
}