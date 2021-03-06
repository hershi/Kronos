﻿using Kronos.Core.Networking;
using Xunit;

namespace Kronos.Core.Tests.Networking
{
    public class RequestStatusTests
    {
        [Theory]
        [InlineData(RequestStatusCode.Unknown, 0)]
        [InlineData(RequestStatusCode.Ok, 1)]
        [InlineData(RequestStatusCode.Failed, 2)]
        [InlineData(RequestStatusCode.NotFound, 3)]
        public void RequestStatusContainsGoodStatusCodes(RequestStatusCode status, int expectedValue)
        {
            int statusCode = (int)status;

            Assert.Equal(statusCode, expectedValue);
        }
    }
}
