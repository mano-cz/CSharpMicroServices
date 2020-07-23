using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NUnit.Framework;

namespace CloudMicroservices.Test
{
    [TestFixture]
    public class ParallelTest
    {
        [Test]
        public void ClassicTest()
        {
            Action programRun = CloudMicroServices.Program.Main;
            // todo lock in program main move
            for (int i = 0; i < 1; i++)
            {
                programRun.Should().NotThrow();
            }
        }

        [Test]
        public void RxTest()
        {
            Action programRxRun = CloudMicroServices.Btdb.Rx.Core.Program.Main;
            for (int i = 0; i < 20; i++)
            {
                programRxRun.Should().NotThrow();
            }
        }
    }
}
