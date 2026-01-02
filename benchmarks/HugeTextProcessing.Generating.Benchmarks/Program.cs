// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using HugeTextProcessing.Benchmarks;

Console.WriteLine("Hello, World!");

//BenchmarkRunner.Run<FileGeneratorBenchmarks>();
BenchmarkRunner.Run<LineWriterBenchmarks>();
