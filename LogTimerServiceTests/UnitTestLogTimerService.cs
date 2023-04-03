using System;
using Xunit;
using Rasputin.LogTimerService;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LogTimerServiceTests;

public class UnitTestLogTimerService
{
    [Fact]
    public async Task UnitTestLogTimerServiceInvalidCommandAsync()
    {
        var sut = new QueueTriggerLogTimerService();
        var message = new Message
        {
            Body = "{\"command\":\"invalid\",\"LogTimer\":{\"ISBN\":\"978-3-16-148410-0\",\"Title\":\"The Hitchhiker's Guide to the Galaxy\",\"Author\":\"Douglas Adams\",\"Price\":12.99}}"
        };
        var loggerMock = new Mock<ILogger>();

        // RunAsync and expect ArgumentNullException
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.RunAsync(message.Body, loggerMock.Object));
    }
}