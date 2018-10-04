using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityProxy;
using Xunit;

namespace UnitProxy.Tests
{
    public class ErrorTests
    {
        [Theory]
        [InlineData(true,
            "Assets/Perfect Parallel/Perfect Engine External/Scripts/CommandLine.cs(88,63): error CS0103: The name `WebUtility' does not exist in the current context")]
        public void IsError(bool error, string line)
        {
            var isError = ErrorFinder.IsError(line);
            Assert.Equal(error, isError);
        }
    }
}
