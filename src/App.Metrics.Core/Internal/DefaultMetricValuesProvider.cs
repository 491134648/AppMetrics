﻿// <copyright file="DefaultMetricValuesProvider.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System.Linq;
using App.Metrics.Core.Filtering;
using App.Metrics.Core.Internal.NoOp;
using App.Metrics.Filters;
using App.Metrics.Registry;

namespace App.Metrics.Core.Internal
{
    public sealed class DefaultMetricValuesProvider : IProvideMetricValues
    {
        private readonly IFilterMetrics _globalFilter;
        private readonly IMetricsRegistry _registry;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultMetricValuesProvider" /> class.
        /// </summary>
        /// <param name="globalFilter">The global filter.</param>
        /// <param name="registry">The registry.</param>
        public DefaultMetricValuesProvider(IFilterMetrics globalFilter, IMetricsRegistry registry)
        {
            _globalFilter = globalFilter ?? new NoOpMetricsFilter();
            _registry = registry ?? new NullMetricsRegistry();
        }

        /// <inheritdoc />
        public MetricsDataValueSource Get() { return _registry.GetData(_globalFilter); }

        /// <inheritdoc />
        public MetricsDataValueSource Get(IFilterMetrics overrideGlobalFilter) { return _registry.GetData(overrideGlobalFilter); }

        /// <inheritdoc />
        public MetricsContextValueSource GetForContext(string context)
        {
            var data = Get();

            var filter = new DefaultMetricsFilter().WhereContext(context);

            var contextData = data.Filter(filter);

            return contextData.Contexts.FirstOrDefault() ?? MetricsContextValueSource.Empty;
        }
    }
}