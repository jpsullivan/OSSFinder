﻿using System.IO;

namespace OSSFinder.Services.Interfaces
{
    public interface IFileReference
    {
        /// <summary>
        /// Gets the content ID suitable for use in the ifNoneMatch parameter of <see cref="IFileStorageService.GetFileReferenceAsync"/>
        /// </summary>
        string ContentId { get; }

        Stream OpenRead();
    }
}
