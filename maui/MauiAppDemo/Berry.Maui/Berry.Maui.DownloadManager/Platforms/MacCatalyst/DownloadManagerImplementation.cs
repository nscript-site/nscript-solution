﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Berry.Maui.Abstractions;

namespace Berry.Maui;

/// <summary>
/// The Mac implementation of the download manager.
/// </summary>
public class DownloadManagerImplementation : IDownloadManager
{
    public IEnumerable<IDownloadFile> Queue
    {
        get { throw new NotImplementedException(); }
    }

    public event EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged;

    public Func<IDownloadFile, string>? PathNameForDownloadedFile { get; set; }

    public IDownloadFile CreateDownloadFile(string url)
    {
        throw new NotImplementedException();
    }

    public IDownloadFile CreateDownloadFile(string url, IDictionary<string, string> headers)
    {
        throw new NotImplementedException();
    }

    public void Start(IDownloadFile i, bool mobileNetworkAllowed = true)
    {
        throw new NotImplementedException();
    }

    public void Abort(IDownloadFile i)
    {
        throw new NotImplementedException();
    }

    public void AbortAll()
    {
        throw new NotImplementedException();
    }
}
