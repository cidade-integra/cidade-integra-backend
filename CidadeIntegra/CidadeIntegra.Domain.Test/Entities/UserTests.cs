using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Domain.Validation;
using Google.Cloud.Firestore;
using Xunit;

namespace CidadeIntegra.Domain.Tests.Entities
{
    public class UserTests
    {
        [Fact(DisplayName = "Deve criar um usuário válido")]
        public void Deve_CriarUsuario_Valido()
        {
            var user = new User(
                Guid.NewGuid(),
                "João Silva",
                "joao@teste.com",
                "https://foto.com/jp.png",
                "São Paulo",
                "user",
                "active",
                DateTimeOffset.UtcNow
            );

            user.Validate();

            Assert.Equal("João Silva", user.DisplayName);
            Assert.Equal("joao@teste.com", user.Email);
            Assert.Equal("user", user.Role);
            Assert.Equal("active", user.Status);
            Assert.False(user.Verified);
        }

        [Theory(DisplayName = "Deve falhar ao criar usuário com campos inválidos")]
        [InlineData("", "email@teste.com", "DisplayName is required.")]
        [InlineData("Nome", "", "Email is required and must be valid.")]
        [InlineData("Nome", "email_invalido", "Email is required and must be valid.")]
        public void Deve_Falhar_Usuario_CamposInvalidos(string displayName, string email, string mensagemEsperada)
        {
            var ex = Assert.Throws<DomainExceptionValidation>(() =>
                new User(Guid.NewGuid(), displayName, email, null, null, "user", "active", DateTimeOffset.UtcNow)
            );

            Assert.Contains(mensagemEsperada, ex.Message);
        }

        [Fact(DisplayName = "Deve criar usuário a partir de um dicionário (Firestore simulado)")]
        public void Deve_CriarUsuario_FromDictionary()
        {
            var data = new Dictionary<string, object>
            {
                { "displayName", "Maria Teste" },
                { "email", "maria@teste.com" },
                { "photoURL", "https://img.com/maria.png" },
                { "region", "RJ" },
                { "role", "admin" },
                { "status", "active" },
                { "score", 100 },
                { "reportCount", 3 },
                { "verified", true },
                { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) },
                { "lastLoginAt", Timestamp.FromDateTime(DateTime.UtcNow) }
            };

            var user = User.FromDictionary(data, "firebase123");

            Assert.Equal("Maria Teste", user.DisplayName);
            Assert.Equal("maria@teste.com", user.Email);
            Assert.Equal("admin", user.Role);
            Assert.True(user.Verified);
            Assert.Equal(3, user.ReportCount);
            Assert.Equal("firebase123", user.FirebaseId);
        }

        [Fact(DisplayName = "Deve atualizar usuário a partir de um dicionário (Firestore simulado)")]
        public void Deve_AtualizarUsuario_FromDictionary()
        {
            var user = new User(Guid.NewGuid(), "Antigo", "antigo@teste.com", null, null, "user", "active", DateTimeOffset.UtcNow);

            var data = new Dictionary<string, object>
            {
                { "displayName", "Novo Nome" },
                { "email", "novo@teste.com" },
                { "score", 500 },
                { "verified", true }
            };

            user.UpdateFromDictionary(data);

            Assert.Equal("Novo Nome", user.DisplayName);
            Assert.Equal("novo@teste.com", user.Email);
            Assert.Equal(500, user.Score);
            Assert.True(user.Verified);
        }

        [Fact(DisplayName = "Deve lançar exceção se CreatedAt for no futuro")]
        public void Deve_LancarExcecao_SeCreatedAtFuturo()
        {
            var user = new User(
                Guid.NewGuid(),
                "Lucas Teste",
                "lucas@teste.com",
                null,
                null,
                "user",
                "active",
                DateTimeOffset.UtcNow.AddDays(1)
            );

            var ex = Assert.Throws<ValidationException>(() => user.Validate());
            Assert.Equal("CreatedAt cannot be in the future.", ex.Message);
        }
    }
}
