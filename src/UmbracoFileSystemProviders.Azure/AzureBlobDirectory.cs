using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Our.Umbraco.FileSystemProviders.Azure
{
    internal class AzureBlobDirectory
    {
        public BlobContainerClient Container { get; }

        public AzureBlobDirectory(BlobContainerClient container, string path)
        {
            this.Container = container;
            this.Prefix = path;
        }

        public string Prefix { get; }

        public Pageable<BlobHierarchyItem> ListBlobs()
        {
            if (string.IsNullOrEmpty(Prefix))
            {
                return Container.GetBlobsByHierarchy();
            }
            return Container.GetBlobsByHierarchy(prefix: Prefix);
        }

    }
}
