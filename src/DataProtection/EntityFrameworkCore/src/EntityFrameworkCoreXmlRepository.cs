// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.DataProtection.EntityFrameworkCore
{
    /// <summary>
    /// An <see cref="IXmlRepository"/> backed by an EntityFrameworkCore datastore.
    /// </summary>
    public class EntityFrameworkCoreXmlRepository<TContext> : IXmlRepository
        where TContext : DbContext, IDataProtectionKeyContext
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="EntityFrameworkCoreXmlRepository{TContext}"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public EntityFrameworkCoreXmlRepository(IServiceProvider services, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<EntityFrameworkCoreXmlRepository<TContext>>();
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <inheritdoc />
        public virtual IReadOnlyCollection<XElement> GetAllElements()
        {
            // forces complete enumeration
            return GetAllElementsCore().ToList().AsReadOnly();

            IEnumerable<XElement> GetAllElementsCore()
            {
                using (var scope = _services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<TContext>();

                    foreach (var key in context.DataProtectionKeys.AsNoTracking())
                    {
                        _logger.ReadingXmlFromKey(key.FriendlyName!, key.Xml);

                        if (!string.IsNullOrEmpty(key.Xml))
                        {
                            yield return XElement.Parse(key.Xml);
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public void StoreElement(XElement element, string friendlyName)
        {
            using (var scope = _services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TContext>();
                var newKey = new DataProtectionKey()
                {
                    FriendlyName = friendlyName,
                    Xml = element.ToString(SaveOptions.DisableFormatting)
                };

                context.DataProtectionKeys.Add(newKey);
                _logger.LogSavingKeyToDbContext(friendlyName, typeof(TContext).Name);
                context.SaveChanges();
            }
        }
    }
}
