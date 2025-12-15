namespace HugeTextProcessing.Generating.Tests.Fixtures;

[CollectionDefinition(nameof(TempFilesCollection))]
public class TempFilesCollection : ICollectionFixture<TempDirectoryFixture>;
