// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using ContentPipelineSourceGeneratorTests.Benchmarks;

Console.WriteLine("Hello, World!");
BenchmarkRunner.Run<ContentPipelineServiceBenchmarks>();