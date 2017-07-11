﻿// <copyright file="GaugeMetricTests.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using System.Linq;
using App.Metrics.Core.Gauge;
using App.Metrics.Core.Meter;
using App.Metrics.Core.ReservoirSampling.Uniform;
using App.Metrics.Core.Timer;
using App.Metrics.FactsCommon;
using FluentAssertions;
using Xunit;

namespace App.Metrics.Facts.Gauge
{
    public class GaugeMetricTests
    {
        [Theory]
        [InlineData(2.0, 4.0, 50.0)]
        [InlineData(0.0, 4.0, 0.0)]
        [InlineData(4.0, 2.0, 100.0)]
        public void Can_calculate_percentage(double numerator, double denominator, double expectedPercentage)
        {
            var hitPercentage = new PercentageGauge(() => numerator, () => denominator);

            hitPercentage.Value.Should().Be(expectedPercentage);
        }

        [Fact]
        public void Can_calculate_the_hit_ratio_as_a_guage()
        {
            var clock = new TestClock();
            var scheduler = new TestTaskScheduler(clock);

            var cacheHitMeter = new DefaultMeterMetric(clock, scheduler);
            var queryTimer = new DefaultTimerMetric(new DefaultAlgorithmRReservoir(1028), clock);

            foreach (var index in Enumerable.Range(0, 1000))
            {
                using (queryTimer.NewContext())
                {
                    clock.Advance(TimeUnit.Milliseconds, 100);
                }

                if (index % 2 == 0)
                {
                    cacheHitMeter.Mark();
                }
            }

            var cacheHitRatioGauge = new HitRatioGauge(cacheHitMeter, queryTimer, value => value.OneMinuteRate);

            cacheHitRatioGauge.Value.Should().BeGreaterThan(0.0);
        }

        [Fact]
        public void Can_calculate_the_hit_ratio_as_a_guage_with_one_min_rate_as_default()
        {
            var clock = new TestClock();
            var scheduler = new TestTaskScheduler(clock);

            var cacheHitMeter = new DefaultMeterMetric(clock, scheduler);
            var queryTimer = new DefaultTimerMetric(new DefaultAlgorithmRReservoir(1028), clock);

            foreach (var index in Enumerable.Range(0, 1000))
            {
                using (queryTimer.NewContext())
                {
                    clock.Advance(TimeUnit.Milliseconds, 100);
                }

                if (index % 2 == 0)
                {
                    cacheHitMeter.Mark();
                }
            }

            var cacheHitRatioGauge = new HitRatioGauge(cacheHitMeter, queryTimer);

            cacheHitRatioGauge.Value.Should().BeGreaterThan(0.0);
        }

        [Fact]
        public void Can_get_and_reset_value_gauge()
        {
            var valueGauge = new ValueGauge();
            valueGauge.SetValue(1.0);

            var value = valueGauge.GetValue(true);

            value.Should().Be(1.0);
            valueGauge.Value.Should().Be(0.0);
        }

        [Fact]
        public void Can_reset_value_gauge()
        {
            var valueGauge = new ValueGauge();
            valueGauge.SetValue(1.0);

            valueGauge.Value.Should().Be(1.0);

            valueGauge.Reset();

            valueGauge.Value.Should().Be(0.0);
        }

        [Fact]
        public void Should_report_nan_on_exception()
        {
            new FunctionGauge(() => throw new InvalidOperationException("test")).Value.Should().Be(double.NaN);

            new DerivedGauge(new FunctionGauge(() => 5.0), (d) => throw new InvalidOperationException("test")).Value.Should().Be(double.NaN);
        }

        [Fact]
        public void When_denomitator_is_zero_returns_NaN()
        {
            var hitPercentage = new PercentageGauge(() => 1, () => 0);

            hitPercentage.Value.Should().Be(double.NaN);
        }
    }
}