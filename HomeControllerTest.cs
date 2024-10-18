using Microsoft.EntityFrameworkCore;
using ST10384311PROG6212POE.Controllers;
using ST10384311PROG6212POE.Data;
using ST10384311PROG6212POE.Models.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ST10384311PROG6212POE_TEST
{
    [TestClass]
    public class HomeControllerTest
    {
        private ApplicationDbContext _context = null!;
        private HomeController _controller = null!;
        private ILogger<HomeController> _logger = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new ApplicationDbContext(options);

            _logger = new LoggerFactory().CreateLogger<HomeController>();
            _controller = new HomeController(_context, _logger);
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------//
        [TestMethod]
        public void Index_ReturnsViewResult()
        {
            // Act (Call the method you want to test)
            var result = _controller.Index();

            // Assert (Compare your expected result to your actual result)
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------//
        [TestMethod]
        public void SubmitClaims_Get_ReturnsViewResult()
        {
            // Act (Call the method you want to test)
            var result = _controller.SubmitClaims();

            // Assert (Compare your expected result to your actual result)
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------//
        [TestMethod]
        public async Task SubmitClaims_Post_ValidModel_ReturnsRedirect()
        {
            // Arrange (Set up your data)
            var mockClaim = new Claims
            {
                LecturerName = "Zimkhitha Sasanti",
                LecturerEmail = "zim@gmail.com",
                ClaimPeriod = "2024-08",
                TotalHours = 10
            };

            var mockFile = CreateMockFile("file.pdf", "Dummy content");

            _controller.ModelState.Clear();

            // Act (Call the method you want to test)
            var result = await _controller.SubmitClaims(mockClaim, mockFile);

            // Assert (Compare your expected result to your actual result)
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("ClaimsStatus", redirectResult.ActionName);

            var savedClaim = await _context.Claims.FirstOrDefaultAsync();
            Assert.IsNotNull(savedClaim);
            Assert.AreEqual(10, savedClaim.TotalHours);
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------//
        [TestMethod]
        public async Task ProcessClaims_ReturnsPendingClaims()
        {
            // Arrange (Set up your data)
            _context.Claims.Add(new Claims
            {
                ClaimId = 1,
                Status = "Pending",
                ClaimPeriod = "2024-08",
                LecturerEmail = "lecturer1@example.com",
                LecturerName = "Lecturer One",
                TotalHours = 5
            });
            _context.Claims.Add(new Claims
            {
                ClaimId = 2,
                Status = "Approved",
                ClaimPeriod = "2024-08",
                LecturerEmail = "lecturer2@example.com",
                LecturerName = "Lecturer Two",
                TotalHours = 10
            });
            await _context.SaveChangesAsync();

            // Act (Call the method you want to test)
            var result = await _controller.ProcessClaims();

            // Assert (Compare your expected result to your actual result)
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as List<Claims>;
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Count);
            Assert.AreEqual("Pending", model.First().Status);
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------//
        [TestMethod]
        public async Task ClaimsStatus_ReturnsAllClaims()
        {
            // Arrange (Set up your data)
            var claims = new List<Claims>
            {
                new Claims { ClaimId = 1, ClaimPeriod = "2023-01", LecturerEmail = "lecturer1@example.com", LecturerName = "Lecturer One", Status = "Pending" },
                new Claims { ClaimId = 2, ClaimPeriod = "2023-02", LecturerEmail = "lecturer2@example.com", LecturerName = "Lecturer Two", Status = "Approved" }
            };

            _context.Claims.AddRange(claims);
            await _context.SaveChangesAsync();

            // Act (Call the method you want to test)
            var result = await _controller.ClaimsStatus();

            // Assert (Compare your expected result to your actual result)
            Assert.IsNotNull(result);
            // Additional assertions to verify the result
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------//
        private IFormFile CreateMockFile(string fileName, string content)
        {
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            writer.Write(content);
            writer.Flush();
            memoryStream.Position = 0;

            var file = new FormFile(memoryStream, 0, memoryStream.Length, "id_from_form", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            return file;
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
//-------------------------------------------------------------------------------------------End Of File--------------------------------------------------------------------//