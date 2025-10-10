namespace System.TypeVarianceExtensions.Tests
{
    /// <summary>
    /// Description résumée pour UnitTest1
    /// </summary>
    public class _1_InstanceConversionTests
    {
        [Fact]
        public void Tests_00_NonConvertibleToDefault()
        {
            Exception instance = new Exception("This is an exception");
            var conversion = instance.ConvertAs(typeof(IEnumerable<>));
            Assert.Null(conversion);
        }

        [Fact]
        public void Tests_02_GenericTypeDefinitionConversions()
        {
            String instance = "This is a string";
            var conversion = instance.ConvertAs(typeof(IEnumerable<>));
            Assert.IsAssignableFrom<IEnumerable<char>>(conversion);
        }
    }
}