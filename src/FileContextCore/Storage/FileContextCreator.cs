﻿using FileContextCore.CombinedManager;
using FileContextCore.FileManager;
using FileContextCore.Helper;
using FileContextCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileContextCore.Storage
{
    class FileContextCreator : IDatabaseCreator
    {
        private FileContextCache cache;
        private ICombinedManager manager;

        public FileContextCreator(FileContextCache _cache)
        {
            cache = _cache;
            manager = OptionsHelper.manager;
        }

        public bool EnsureCreated()
        {
            return !manager.Exists();
        }

        public Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(EnsureCreated());
        }

        public bool EnsureDeleted()
        {
            cache.Clear();
            return manager.Clear();
        }

        public Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(EnsureDeleted());
        }
    }
}