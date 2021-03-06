// Copyright (c) morrisjdev. All rights reserved.
// Original copyright (c) .NET Foundation. All rights reserved.
// Modified version by morrisjdev
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using FileContextCore.Diagnostics;
using FileContextCore.Infrastructure.Internal;
using FileContextCore.Infrastructure;
using FileContextCore.Storage;
using FileContextCore.Utilities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace FileContextCore
{
    /// <summary>
    ///     In-memory specific extension methods for <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    public static class FileContextDbContextOptionsExtensions
    {
        /// <summary>
        ///     Configures the context to connect to an in-memory database.
        ///     The in-memory database is shared anywhere the same name is used, but only for a given
        ///     service provider.
        /// </summary>
        /// <typeparam name="TContext"> The type of context being configured. </typeparam>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="connectionString">A connection string that is used to build the options</param>
        /// <param name="databaseRoot">
        ///     All in-memory databases will be rooted in this object, allowing the application
        ///     to control their lifetime. This is useful when sometimes the context instance
        ///     is created explicitly with <c>new</c> while at other times it is resolved using dependency injection.
        /// </param>
        /// <param name="inMemoryOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseFileContextDatabaseConnectionString<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            string connectionString,
            [CanBeNull] FileContextDatabaseRoot databaseRoot = null,
            [CanBeNull] Action<FileContextDbContextOptionsBuilder> inMemoryOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseFileContextDatabaseConnectionString(
                (DbContextOptionsBuilder)optionsBuilder, connectionString, databaseRoot, inMemoryOptionsAction);

        /// <summary>
        ///     Configures the context to connect to an in-memory database.
        ///     The in-memory database is shared anywhere the same name is used, but only for a given
        ///     service provider.
        /// </summary>
        /// <typeparam name="TContext"> The type of context being configured. </typeparam>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="databaseName">
        ///     The name of the in-memory database. This allows the scope of the in-memory database to be controlled
        ///     independently of the context. The in-memory database is shared anywhere the same name is used.
        /// </param>
        /// <param name="location">An optional parameter to define the location where the files are stored</param>
        /// <param name="databaseRoot">
        ///     All in-memory databases will be rooted in this object, allowing the application
        ///     to control their lifetime. This is useful when sometimes the context instance
        ///     is created explicitly with <c>new</c> while at other times it is resolved using dependency injection.
        /// </param>
        /// <param name="inMemoryOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
        /// <param name="serializer">The serializer to be used. Defaults to json</param>
        /// <param name="filemanager">The file manager to be used.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseFileContextDatabase<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            string serializer = "json",
            string filemanager = "default",
            string databaseName = "",
            string location = null,
            [CanBeNull] FileContextDatabaseRoot databaseRoot = null,
            [CanBeNull] Action<FileContextDbContextOptionsBuilder> inMemoryOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseFileContextDatabase(
                (DbContextOptionsBuilder)optionsBuilder, serializer, filemanager, databaseName, location, databaseRoot, inMemoryOptionsAction);

        /// <summary>
        ///     Configures the context to connect to a named in-memory database.
        ///     The in-memory database is shared anywhere the same name is used, but only for a given
        ///     service provider.
        /// </summary>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="connectionString">A connection string that is used to build the options</param>
        /// <param name="databaseRoot">
        ///     All in-memory databases will be rooted in this object, allowing the application
        ///     to control their lifetime. This is useful when sometimes the context instance
        ///     is created explicitly with <c>new</c> while at other times it is resolved using dependency injection.
        /// </param>
        /// <param name="inMemoryOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseFileContextDatabaseConnectionString(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            string connectionString,
            [CanBeNull] FileContextDatabaseRoot databaseRoot = null,
            [CanBeNull] Action<FileContextDbContextOptionsBuilder> inMemoryOptionsAction = null)
        {
            string[] connectionStringParts = connectionString.Split(';');
            Dictionary<string, string> connectionStringSplitted = connectionStringParts
                .Select(segment => segment.Split('='))
                .ToDictionary(parts => parts[0].Trim().ToLowerInvariant(), parts => parts[1].Trim());

            return UseFileContextDatabase(optionsBuilder, 
                connectionStringSplitted.GetValueOrDefault("serializer"),
                connectionStringSplitted.GetValueOrDefault("filemanager"),
                connectionStringSplitted.GetValueOrDefault("databasename"),
                connectionStringSplitted.GetValueOrDefault("location"), databaseRoot, inMemoryOptionsAction);
        }

        /// <summary>
            ///     Configures the context to connect to a named in-memory database.
            ///     The in-memory database is shared anywhere the same name is used, but only for a given
            ///     service provider.
            /// </summary>
            /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
            /// <param name="databaseName">
            ///     The name of the in-memory database. This allows the scope of the in-memory database to be controlled
            ///     independently of the context. The in-memory database is shared anywhere the same name is used.
            /// </param>
            /// <param name="location">An optional parameter to define the location where the files are stored</param>
            /// <param name="databaseRoot">
            ///     All in-memory databases will be rooted in this object, allowing the application
            ///     to control their lifetime. This is useful when sometimes the context instance
            ///     is created explicitly with <c>new</c> while at other times it is resolved using dependency injection.
            /// </param>
            /// <param name="inMemoryOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
            /// <param name="serializer">The serializer to be used. Defaults to json</param>
            /// <param name="filemanager">The file manager to be used.</param>
            /// <returns> The options builder so that further configuration can be chained. </returns>
            public static DbContextOptionsBuilder UseFileContextDatabase(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            string serializer = "json",
            string filemanager = "default",
            string databaseName = "",
            string location = null,
            [CanBeNull] FileContextDatabaseRoot databaseRoot = null,
            [CanBeNull] Action<FileContextDbContextOptionsBuilder> inMemoryOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            var extension = optionsBuilder.Options.FindExtension<FileContextOptionsExtension>()
                            ?? new FileContextOptionsExtension();

            extension = extension.WithCustomOptions(databaseName, serializer, filemanager, location);

            if (databaseRoot != null)
            {
                extension = extension.WithDatabaseRoot(databaseRoot);
            }

            ConfigureWarnings(optionsBuilder);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            inMemoryOptionsAction?.Invoke(new FileContextDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
        {
            // Set warnings defaults
            var coreOptionsExtension
                = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()
                  ?? new CoreOptionsExtension();

            coreOptionsExtension = coreOptionsExtension.WithWarningsConfiguration(
                coreOptionsExtension.WarningsConfiguration.TryWithExplicit(
                    FileContextEventId.TransactionIgnoredWarning, WarningBehavior.Throw));

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(coreOptionsExtension);
        }
    }
}
