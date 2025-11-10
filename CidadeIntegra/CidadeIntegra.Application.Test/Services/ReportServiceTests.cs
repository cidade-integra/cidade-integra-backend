using CidadeIntegra.Application.Interfaces.Repositories;
using CidadeIntegra.Application.Services;
using CidadeIntegra.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CidadeIntegra.Application.Test.Services
{
    public class ReportServiceTests
    {
        private readonly Mock<IReportRepository> _reportRepositoryMock;
        private readonly Mock<ILogger<ReportService>> _loggerMock;
        private readonly ReportService _reportService;

        public ReportServiceTests()
        {
            _reportRepositoryMock = new Mock<IReportRepository>();
            _loggerMock = new Mock<ILogger<ReportService>>();
            _reportService = new ReportService(_reportRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetPendingReportsAsync_ShouldReturnReports()
        {
            // Arrange
            var reports = new List<Report>
            {
                new Report(Guid.NewGuid(), "Category1", "Title1", "Description1", false),
                new Report(Guid.NewGuid(), "Category2", "Title2", "Description2", true)
            };
            _reportRepositoryMock.Setup(x => x.GetPendingReportsAsync()).ReturnsAsync(reports);

            // Act
            var result = await _reportService.GetPendingReportsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            _reportRepositoryMock.Verify(x => x.GetPendingReportsAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldCallRepositoryAddAndSave()
        {
            // Arrange
            var report = new Report(Guid.NewGuid(), "Category", "Title", "Description", false);
            // Ajuste do FirebaseId, pois é private set
            report.GetType().GetProperty("FirebaseId")!.SetValue(report, "firebase123");

            // Act
            await _reportService.CreateAsync(report);

            // Assert
            _reportRepositoryMock.Verify(x => x.AddAsync(report), Times.Once);
            _reportRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByFirebaseIdAsync_ShouldReturnReport_WhenExists()
        {
            // Arrange
            var firebaseId = "firebase123";
            var report = new Report(Guid.NewGuid(), "Category", "Title", "Description", false);
            report.GetType().GetProperty("FirebaseId")!.SetValue(report, firebaseId);

            _reportRepositoryMock.Setup(x => x.GetByFirebaseIdAsync(firebaseId)).ReturnsAsync(report);

            // Act
            var result = await _reportService.GetByFirebaseIdAsync(firebaseId);

            // Assert
            result.Should().NotBeNull();
            result!.FirebaseId.Should().Be(firebaseId);
            _reportRepositoryMock.Verify(x => x.GetByFirebaseIdAsync(firebaseId), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallRepositoryUpdate()
        {
            // Arrange
            var report = new Report(Guid.NewGuid(), "Category", "Title", "Description", false);

            // Act
            await _reportService.UpdateAsync(report);

            // Assert
            _reportRepositoryMock.Verify(x => x.UpdateAsync(report), Times.Once);
        }
    }
}