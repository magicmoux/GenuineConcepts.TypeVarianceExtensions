using System.TypeVarianceExtensions.Tests.TestTypes;

namespace System.TypeVarianceExtensions.Tests
{
    /// <summary>
    /// Description résumée pour UnitTest1
    /// </summary>
    public class _0_TypeVarianceTests
    {
        [Fact]
        // This is a bit superfluous since all those cases should be solved directly by IsAssgnableFrom
        public void Tests_00_NativeVarianceTests()
        {
            Type? runtimeType = null;

            Assert.True(typeof(Object).IsVariantOf(typeof(Object), out runtimeType));
            Assert.Equal(typeof(Object), runtimeType);

            Assert.True(typeof(EntityType).IsVariantOf(typeof(IEntityType), out runtimeType));
            Assert.Equal(typeof(IEntityType), runtimeType);

            Assert.True(typeof(Range<DateTime>).IsVariantOf(typeof(IComparable<Range<DateTime>>), out runtimeType));
            Assert.Equal(typeof(IComparable<Range<DateTime>>), runtimeType);

            Assert.True(typeof(DateTimeRange).IsVariantOf(typeof(Range<DateTime>), out runtimeType));
            Assert.Equal(typeof(Range<DateTime>), runtimeType);

            Assert.True(typeof(DateTimeRange).IsVariantOf(typeof(IComparable<DateTimeRange>), out runtimeType));
            Assert.Equal(typeof(IComparable<DateTimeRange>), runtimeType);

            Assert.True(typeof(DateTimeRange).IsVariantOf(typeof(IComparable<Range<DateTime>>), out runtimeType));
            Assert.Equal(typeof(IComparable<Range<DateTime>>), runtimeType);

            Assert.True(typeof(List<EntityType>).IsVariantOf(typeof(ICollection<EntityType>), out runtimeType));

            Assert.True(typeof(List<int>).IsVariantOf(typeof(IEnumerable<int>), out runtimeType));
            Assert.Equal(typeof(IEnumerable<int>), runtimeType);

            Assert.False(typeof(ICollection<Exception>).IsVariantOf(typeof(ICollection<IEntityType>), out runtimeType));
        }

        [Fact]
        public void Tests_01_Liskov_GenericTypesVariance()
        {
            Type? runtimeType = null;

            Assert.True(typeof(List<EntityType>).IsVariantOf(typeof(ICollection<IEntityType>), out runtimeType));
            Assert.Equal(typeof(ICollection<IEntityType>), runtimeType);

            Assert.True(typeof(List<SpecificEntityType>).IsVariantOf(typeof(ICollection<IGenericEntityType<EntityType>>), out runtimeType));
            Assert.True(typeof(List<SpecificEntityType>).IsVariantOf(typeof(ICollection<IGenericEntityType<IEntityType>>), out runtimeType));

            Assert.True(typeof(ICollection<SpecificEntityType>).IsVariantOf(typeof(ICollection<IGenericEntityType<EntityType>>), out runtimeType));
            Assert.True(typeof(ICollection<SpecificEntityType>).IsVariantOf(typeof(ICollection<IGenericEntityType<IEntityType>>), out runtimeType));
        }

        [Fact]
        public void Tests_02_Liskov_GenericTypeDefinitionsVarianceTests()
        {
            Type? runtimeType = null;

            Assert.True(typeof(DateTimeRange).IsVariantOf(typeof(Range<>), out runtimeType));
            Assert.Equal(typeof(Range<DateTime>), runtimeType);

            Assert.True(typeof(List<EntityType>).IsVariantOf(typeof(ICollection<>), out runtimeType));
            Assert.Equal(typeof(ICollection<EntityType>), runtimeType);

            Assert.True(typeof(List<EntityType>).IsVariantOf(typeof(ICollection<IEntityType>), out runtimeType));
            Assert.Equal(typeof(ICollection<IEntityType>), runtimeType);

            Assert.True(typeof(List<SpecificEntityType>).IsVariantOf(typeof(ICollection<IGenericEntityType<EntityType>>), out runtimeType));
            Assert.True(typeof(List<SpecificEntityType>).IsVariantOf(typeof(ICollection<IGenericEntityType<IEntityType>>), out runtimeType));

            Assert.True(typeof(ICollection<SpecificEntityType>).IsVariantOf(typeof(ICollection<IGenericEntityType<EntityType>>), out runtimeType));

            //Note in this case wouldn't it be more useful to return a runtime type of ICollection<IGenericEntityType<EntityType>> ?
            Assert.True(typeof(ICollection<SpecificEntityType>).IsVariantOf(typeof(ICollection<IGenericEntityType<IEntityType>>), out runtimeType));

            //Note in this case wouldn't it be more useful to return a runtime type of IComparable<Range<DateTime>> ?
            Assert.True(typeof(DateTimeRange).IsVariantOf(typeof(IComparable<IRange>), out runtimeType));
            Assert.Equal(typeof(IComparable<IRange>), runtimeType);
        }
    }
}