using Xunit;

namespace r47.Primitives.EnumT.Test
{
    // Define a collection for tests that need to run sequentially
    [CollectionDefinition("parallel-execution", DisableParallelization = true)]
    public class NonParallelTestCollection
    {
        // This class remains empty.
        // It serves solely as a marker for xUnit Test Collection.        
    }
}