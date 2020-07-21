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
        public void Returns0()
        {
            Action programRun = CloudMicroServices.Program.Main;
            // todo lock in program main move
            for (int i = 0; i < 1; i++)
            {
                programRun.Should().NotThrow();
            }
        }
    }
}
