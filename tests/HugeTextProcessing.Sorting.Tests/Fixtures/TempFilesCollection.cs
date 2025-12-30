using HugeTextProcessing.Tests.Fixtures;

namespace HugeTextProcessing.Sorting.Tests.Fixtures;

[CollectionDefinition(nameof(TempFilesCollection))]
public class TempFilesCollection : ICollectionFixture<TempDirectoryFixture>;
