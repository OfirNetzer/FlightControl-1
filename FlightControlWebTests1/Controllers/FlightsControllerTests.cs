﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlightControlWeb.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlightControlWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Moq;
using Moq.Protected;

namespace FlightControlWeb.Controllers.Tests
{
    [TestClass()]
    public class FlightsControllerTests
    {
        [TestMethod()]
        public async Task GetFlightsInternalTest()
        {

            // context
            var options = new DbContextOptionsBuilder<FlightContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new FlightContext(options);

            // Creating elements to add to the DB
            var testFlight = new Flight()
            {
                Id = 1,
                FlightId = "IL30357629",
                IsExternal = false
            };

            DateTime startTime = DateTime.Parse("2020-05-27T12:05:00Z");
            DateTime endTime = DateTime.Parse("2020-05-27T13:05:00Z");
            var testFlightPlan = new FlightPlan()
            {
                Id = 1,
                FlightId = "IL30357629",
                Passengers = 247,
                CompanyName = "United Airlines",
                IsExternal = false,
                EndTime = endTime
            };

            var testInitialLocation = new InitialLocation()
            {
                Id = 1,
                FlightPlanId = 1,
                Longitude = 33.24,
                Latitude = 19.53,
                DateTime = startTime
            };

            var testSegmentFirst = new Segment()
            {
                Id = 1,
                FlightPlanId = 1,
                Longitude = 23.240702,
                Latitude = 34.921971,
                TimeSpanSeconds = 1800,
                StartTime = startTime,
                EndTime = startTime.AddSeconds(1800)
            };

            var testSegmentSecond = new Segment()
            {
                Id = 2,
                FlightPlanId = 1,
                Longitude = 21.346370,
                Latitude = 39.419221,
                TimeSpanSeconds = 1800,
                StartTime = startTime.AddSeconds(1800),
                EndTime = endTime
            };

            await context.FlightItems.AddAsync(testFlight);
            await context.FlightPlanItems.AddAsync(testFlightPlan);
            await context.InitialLocationItems.AddAsync(testInitialLocation);
            await context.SegmentItems.AddAsync(testSegmentFirst);
            await context.SegmentItems.AddAsync(testSegmentSecond); 
            await context.SaveChangesAsync();

            var mockClientFactory = new Mock<IHttpClientFactory>();

            // controller
            var controller = new FlightsController(context, mockClientFactory.Object);

            var relativeTo = "2020-05-27T15:35:05Z";

            var expectedLatitude = 34.93446336111111; //todo check this is really correct
            var expectedLongitude = 23.235439966666664; //todo check this is really correct
            var expectedResult = "{" +
                                 "'flight_id': 'IL30357629'" +
                                 "'longitude':" + expectedLongitude +
                                 "'latitude':" + expectedLatitude +
                                 "'passengers': 247" +
                                 "'company_name': 'United Airlines'" +
                                 "'date_time': '2020-05-27T15:05:00Z'" +
                                 "'is_external': false";

            DateTime relativeToDate = DateTime.Parse(relativeTo);

            //Act
            // running method and checking results
            var result = (await controller.GetFlights(relativeTo)).FirstOrDefault();

            Assert.IsNotNull(result);
            var resultFlightId = result.FlightID;
            var resultLongitude = result.Longitude;
            var resultLatitude = result.Latitude;
            var resultPassengers = result.Passengers;
            var resultCompanyName = result.CompanyName;
            var resultDateTime = result.CurrDateTime;
            var resultIsExternal = result.IsExternal;
            
            //todo need to change this to check if equal to input+what we added to db - maybe only check number of elements?
            Assert.AreEqual("IL30357629", resultFlightId);
            Assert.AreEqual(expectedLongitude, resultLongitude);
            Assert.AreEqual(expectedLatitude, resultLatitude);
            Assert.AreEqual(247, resultPassengers);
            Assert.AreEqual("United Airlines", resultCompanyName);
            Assert.AreEqual("27-May-20 3:35:05 PM", resultDateTime.ToString());
            Assert.AreEqual(false, resultIsExternal);

        }

        [TestMethod()]
        public async Task GetFlightsExternalTest()
        {

            // creating mocked http client factory
            var mockClientFactory = new Mock<IHttpClientFactory>();

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            // mimics input from external server
            var input = "{" +
                        "{" +
                        "'flight_id': 'HA60377638', " +
                        "'longitude': 34.46026097, " +
                        "'latitude': 29.255859349999998, " +
                        "'passengers': 500, " +
                        "'company_name': 'El-Al', " +
                        "'date_time': '2020-05-27T15:05:00Z', " +
                        "'is_external': false" +
                        "}" +
                        "{" +
                        "'flight_id': 'NC5998837', " +
                        "'longitude': 30.632256631999997, " +
                        "'latitude': 33.13848776, " +
                        "'passengers': 257, " +
                        "'company_name': 'swiss', " +
                        "'date_time': '2020-05-27T15:05:00Z', " +
                        "'is_external': false" +
                        "}" +
                        "}";
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(input),
                });
            var client = new HttpClient(mockHttpMessageHandler.Object);
            mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            // context
            var options = new DbContextOptionsBuilder<FlightContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new FlightContext(options);

            // controller
            var controller = new FlightsController(context, mockClientFactory.Object); // todo ISSUE HERE

            var relativeTo = "2020-05-27T15:05:05Z&sync_all";

            //Act
            // running method and checking results
            var result = await controller.GetFlights(relativeTo);
            Assert.IsNotNull(result);
            //todo need to change this to check if equal to input+what we added to db - maybe only check number of elements?
            //Assert.AreEqual(result, input);
            //Assert.AreEqual(HttpStatusCode.OK, (HttpStatusCode)result.StatusCode);
        }
    }
}