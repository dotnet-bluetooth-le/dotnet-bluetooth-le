using System;
using Plugin.BLE.Abstractions;
using Xunit;

namespace Plugin.BLE.Tests
{
    public class TraceTests
    {
        [Fact(DisplayName = "Trace should catch exceptions.")]
        public void Trace_should_catch_exceptions()
        {
            var called = false;
            var impl = new Action<string, object[]>((f, p) =>
            {
                called = true;
                throw new Exception();
            });

            Trace.TraceImplementation = impl;

            Trace.Message("Test", 1);
            Assert.True(called);
        }
    }
}