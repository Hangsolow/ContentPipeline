// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using ContentPipelineSourceGeneratorTests.Benchmarks;

BenchmarkRunner.Run<ContentPipelineServiceBenchmarks>();