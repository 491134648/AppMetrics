﻿// <copyright file="MetricCoreTestFixture.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using App.Metrics.Core.Configuration;
using App.Metrics.Core.Filtering;
using App.Metrics.Core.Internal;
using App.Metrics.FactsCommon;
using App.Metrics.Registry;
using Microsoft.Extensions.Logging;

namespace App.Metrics.Facts.Fixtures
{
    public class MetricCoreTestFixture : IDisposable
    {
        public MetricCoreTestFixture()
        {
            var loggerFactory = new LoggerFactory();
            var options = new AppMetricsOptions();

            Clock = new TestClock();
            Builder = new DefaultMetricsBuilderFactory();

            IMetricContextRegistry ContextRegistrySetup(string context) => new DefaultMetricContextRegistry(context);

            var registry = new DefaultMetricsRegistry(loggerFactory, options, Clock, ContextRegistrySetup);

            Registry = registry;
            Providers = new DefaultMetricsProvider(Registry, Builder, Clock);
            Snapshot = new DefaultMetricValuesProvider(new NoOpMetricsFilter(), Registry);
            Managers = new DefaultMeasureMetricsProvider(Registry, Builder, Clock);
            Context = options.DefaultContextLabel;
        }

        public IBuildMetrics Builder { get; }

        public IClock Clock { get; }

        public string Context { get; }

        public IMeasureMetrics Managers { get; }

        public IProvideMetrics Providers { get; }

        public IMetricsRegistry Registry { get; }

        public IProvideMetricValues Snapshot { get; }

        public MetricTags[] Tags => new[]
                                    {
                                        new MetricTags("key1", "key2"),
                                        new MetricTags(new[] { "key1", "key2" }, new[] { "value1", "value2" })
                                    };

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}