// <copyright file="GaugeSerializationTests.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using System.Linq;
using App.Metrics.Formatters.Json.Facts.Helpers;
using App.Metrics.Formatters.Json.Facts.TestFixtures;
using App.Metrics.Formatters.Json.Serialization;
using App.Metrics.Gauge;
using FluentAssertions;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace App.Metrics.Formatters.Json.Facts
{
    public class GaugeSerializationTests : IClassFixture<MetricProviderTestFixture>
    {
        private readonly GaugeValueSource _gauge;
        private readonly ITestOutputHelper _output;
        private readonly MetricDataSerializer _serializer;

        public GaugeSerializationTests(ITestOutputHelper output, MetricProviderTestFixture fixture)
        {
            _output = output;
            _serializer = new MetricDataSerializer();
            _gauge = fixture.Gauges.First(x => x.Name == fixture.GaugeNameDefault);
        }

        [Fact]
        public void Can_create_gauge_from_value_source()
        {
            var valueSource = new GaugeValueSource("test", new FunctionGauge(() => 2.0), Unit.Bytes, MetricTags.Empty);

            var gauge = GaugeMetric.FromGauge(valueSource);

            gauge.Value.Should().Be(2.0);
            gauge.Name.Should().Be("test");
            gauge.Tags.Should().BeEmpty();
            Assert.True(gauge.Unit == Unit.Bytes);
        }

        [Fact]
        public void Can_deserialize()
        {
            var result = _serializer.Deserialize<GaugeValueSource>(MetricType.Gauge.SampleJson().ToString());

            result.Name.Should().BeEquivalentTo(_gauge.Name);
            result.Unit.Should().Be(_gauge.Unit);
            result.Value.Should().Be(_gauge.Value);
            result.Tags.Keys.Should().Contain(_gauge.Tags.Keys.ToArray());
            result.Tags.Values.Should().Contain(_gauge.Tags.Values.ToArray());
        }

        [Fact]
        public void Produces_expected_json()
        {
            var expected = MetricType.Gauge.SampleJson();

            var result = _serializer.Serialize(_gauge).ParseAsJson();

            result.Should().Be(expected);
        }

        [Fact]
        public void Produces_valid_json()
        {
            var json = _serializer.Serialize(_gauge);
            _output.WriteLine("Json Gauge: {0}", json);

            Action action = () => JToken.Parse(json);
            action.ShouldNotThrow<Exception>();
        }
    }
}