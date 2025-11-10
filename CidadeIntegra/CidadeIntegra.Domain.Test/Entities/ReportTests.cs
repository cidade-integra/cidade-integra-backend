using System;
using System.ComponentModel.DataAnnotations;
using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Domain.Validation;
using Xunit;

namespace CidadeIntegra.Domain.Tests.Entities
{
    public class ReportTests
    {
        [Fact]
        public void Should_Create_Report_When_Valid_Data()
        {
            var report = new Report(
                Guid.NewGuid(),
                "Infraestrutura",
                "Buraco na rua",
                "Existe um grande buraco na rua principal.",
                false,
                "pending"
            );

            Assert.NotEqual(Guid.Empty, report.Id);
            Assert.Equal("Infraestrutura", report.Category);
            Assert.Equal("Buraco na rua", report.Title);
            Assert.Equal("pending", report.Status);
            Assert.True(report.CreatedAt <= DateTimeOffset.UtcNow);
        }

        [Fact]
        public void Should_Throw_When_UserId_Empty()
        {
            Assert.Throws<DomainExceptionValidation>(() =>
                new Report(Guid.Empty, "Categoria", "Título", "Descrição", false));
        }

        [Theory]
        [InlineData("", "Título", "Descrição", "pending")]
        [InlineData(" ", "Título", "Descrição", "pending")]
        [InlineData(null, "Título", "Descrição", "pending")]
        public void Should_Throw_When_Category_Invalid(string category, string title, string desc, string status)
        {
            Assert.Throws<DomainExceptionValidation>(() =>
                new Report(Guid.NewGuid(), category, title, desc, false, status));
        }

        [Fact]
        public void Should_Throw_When_Title_Too_Long()
        {
            var longTitle = new string('x', 121);
            Assert.Throws<DomainExceptionValidation>(() =>
                new Report(Guid.NewGuid(), "Cat", longTitle, "Descrição", false));
        }

        [Fact]
        public void Should_Throw_When_Status_Invalid()
        {
            Assert.Throws<DomainExceptionValidation>(() =>
                new Report(Guid.NewGuid(), "Cat", "Título", "Descrição", false, "invalid_status"));
        }

        [Fact]
        public void Should_Throw_When_ResolvedAt_Before_CreatedAt()
        {
            var report = new Report(Guid.NewGuid(), "Cat", "Título", "Descrição", false);
            report.ResolvedAt = report.CreatedAt.AddDays(-1);
            Assert.Throws<ValidationException>(() => report.Validate());
        }

        [Fact]
        public void Should_Pass_When_ResolvedAt_After_CreatedAt()
        {
            var report = new Report(Guid.NewGuid(), "Cat", "Título", "Descrição", false);
            report.ResolvedAt = report.CreatedAt.AddDays(1);
            report.Validate(); // Não deve lançar exceção
        }
    }
}
