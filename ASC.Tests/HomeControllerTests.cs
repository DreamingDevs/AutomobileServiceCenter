using ASC.Web.Controllers;
using Xunit;
using Moq;
using Microsoft.Extensions.Options;
using ASC.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ASC.Tests.TestUtilities;
using ASC.Utilities;

namespace ASC.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<IOptions<ApplicationSettings>> optionsMock;
        private readonly Mock<HttpContext> mockHttpContext;
        public HomeControllerTests()
        {
            // Create an instance of Mock IOptions
            optionsMock = new Mock<IOptions<ApplicationSettings>>();
            mockHttpContext = new Mock<HttpContext>();
            // Set FakeSession to HttpContext Session.
            mockHttpContext.Setup(p => p.Session).Returns(new FakeSession());

            // Set IOptions<> Values property to return ApplicationSettings object
            optionsMock.Setup(ap => ap.Value).Returns(new ApplicationSettings { ApplicationTitle = "ASC" });
        }

        [Fact]
        public void HomeController_Index_View_Test()
        {
            // Home controller instantiated with Mock IOptions<> object 
            var controller = new HomeController(optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            Assert.IsType(typeof(ViewResult), controller.Index());
        }

        [Fact]
        public void HomeController_Index_NoModel_Test()
        {
            var controller = new HomeController(optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            // Assert Model for Null
            Assert.Null((controller.Index() as ViewResult).ViewData.Model);
        }

        [Fact]
        public void HomeController_Index_Validation_Test()
        {
            var controller = new HomeController(optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            // Assert ModelState Error Count to 0
            Assert.Equal(0, (controller.Index() as ViewResult).ViewData.ModelState.ErrorCount);
        }

        //[Fact]
        //public void HomeController_Index_Session_Test()
        //{
        //    var controller = new HomeController(optionsMock.Object);
        //    controller.ControllerContext.HttpContext = mockHttpContext.Object;

        //    controller.Index();

        //    // Session value with key "Test" should not be null.
        //    Assert.NotNull(controller.HttpContext.Session.GetSession<ApplicationSettings>("Test"));
        //}
    }
}
