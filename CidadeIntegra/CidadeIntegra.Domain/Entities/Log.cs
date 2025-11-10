using CidadeIntegra.Domain.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CidadeIntegra.Domain.Entities
{
    [Table("Logs")]
    public class Log
    {
        #region Atributos
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(50)]
        public string Level { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        public string? Exception { get; set; }

        public DateTimeOffset TimeStamp { get; set; }
        #endregion

        #region Construtor
        public Log() { } // EF

        public Log(Guid id, string level, string message, string? exception, DateTimeOffset timestamp)
        {
            ValidateParameters(id, level, message, timestamp);

            Id = id;
            Level = level.Trim();
            Message = message.Trim();
            Exception = exception?.Trim();
            TimeStamp = timestamp;
        }
        #endregion

        #region Fábrica
        public static Log Create(string level, string message, string? exception = null)
        {
            return new Log(
                Guid.NewGuid(),
                level,
                message,
                exception,
                DateTimeOffset.UtcNow
            );
        }
        #endregion

        #region Validação
        private void ValidateParameters(Guid id, string level, string message, DateTimeOffset timestamp)
        {
            DomainExceptionValidation.When(id == Guid.Empty, "Id cannot be empty.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(level), "Level is required.");
            DomainExceptionValidation.When(level.Length > 50, "Level exceeds maximum length of 50 characters.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(message), "Message is required.");
            DomainExceptionValidation.When(message.Length > 2000, "Message exceeds maximum length of 2000 characters.");
            DomainExceptionValidation.When(timestamp == default, "Timestamp must be set.");
        }
        #endregion
    }
}