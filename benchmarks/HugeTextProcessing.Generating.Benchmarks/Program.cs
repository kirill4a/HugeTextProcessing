// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using HugeTextProcessing.Generating.Benchmarks;

Console.WriteLine("Hello, World!");

BenchmarkRunner.Run<FileGeneratorBenchmarks>();
