using FunctionTestsApp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace FunctionsTests
{
    public class FunctionTests
    {
        public static IEnumerable<object[]> Data()
        {
            return new List<object[]>
            {
                new object[] { "Franck" },
                new object[] { "Pedro" },
                new object[] { "Homer" }

            };
        }

        public FunctionTests()
        {
            _now = new DateTime(2019, 1, 1);
            SystemTime.Now = () => _now; ;

        }

        ~FunctionTests()
        {
            SystemTime.Reset();
        }

        private DateTime _now;

        [Fact]
        public void HttpTriggerShouldLogMessage()
        {
            //Arrange
            var logger = new ListLogger();

            //Act
            HttpTrigger.Run(null, logger);

            //Assert
            Assert.Single(logger.Logs, "C# HTTP trigger function processed a request.");
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async void HttpTriggerShouldReturnPassedNameAsQueryString(string queryStringValue)
        {
            var request = Substitute.For<HttpRequest>();
            request.Query["name"].Returns(new StringValues(queryStringValue));
            request.Body.Returns(new MemoryStream());

            var response = await HttpTrigger.Run(request, Substitute.For<ILogger>());

            var okResponse = Assert.IsType<OkObjectResult>(response);
            Assert.Equal($"Hello, {queryStringValue}", okResponse.Value);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async void HttpTriggerShouldReturnPassedNameInBody(string value)
        {
            var request = Substitute.For<HttpRequest>();
            request.Body.Returns(new MemoryStream(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new { name = value }))));

            var response = await HttpTrigger.Run(request, Substitute.For<ILogger>());

            var okResponse = Assert.IsType<OkObjectResult>(response);
            Assert.Equal($"Hello, {value}", okResponse.Value);
        }

        [Fact]
        public async void HttpTriggerShouldReturnBadResponseWhenNoNameWasFound()
        {
            var request = Substitute.For<HttpRequest>();
            request.Body.Returns(new MemoryStream());

            var response = await HttpTrigger.Run(request, Substitute.For<ILogger>());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal("Please pass a name on the query string or in the request body", badRequestResult.Value);
        }



        [Fact]
        public void TimerShouldLogMessage()
        {
            //Arrange
            var logger = new ListLogger();

            //Act
            TimerTrigger.Run(null, logger);

            //Assert

            Assert.Single(logger.Logs,$"C# Timer trigger function executed at: {_now}");
        }

        [Fact]
        public void TimerShouldLogMessage2()
        {
            //Arrange
            var logger = Substitute.For<ILogger>();

            //Act
            TimerTrigger.Run(null, logger);

            //Assert
            logger
                .Received()
                .Log(LogLevel.Information,
                     0, 
                     Arg.Is<FormattedLogValues>(states => states[0].Value.Equals($"C# Timer trigger function executed at: {_now}")),
                     null, 
                     Arg.Any<Func<FormattedLogValues, Exception, string>>());
        }


       

    }
}
