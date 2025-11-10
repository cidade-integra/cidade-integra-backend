using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CidadeIntegra.Domain.Entities
{
    [Table("Logs")]
    public class Log
    {
        #region Identificação
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        #endregion

        #region Informações
        [Required, MaxLength(50)]
        public string Level { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        public string? Exception { get; set; }
        #endregion

        #region Datas
        public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;
        #endregion

        #region Métodos Auxiliares
        public static Log Create(string level, string message, string? exception = null)
        {
            var log = new Log
            {
                Id = Guid.NewGuid(),
                Level = level,
                Message = message,
                Exception = exception,
                TimeStamp = DateTimeOffset.UtcNow
            };

            log.Validate();
            return log;
        }
        #endregion

        #region Validation
        public void Validate()
        {
            if (Id == Guid.Empty)
                throw new ValidationException("Id cannot be empty.");

            if (string.IsNullOrWhiteSpace(Level))
                throw new ValidationException("Level is required.");

            if (string.IsNullOrWhiteSpace(Message))
                throw new ValidationException("Message is required.");

            if (Level.Length > 50)
                throw new ValidationException("Level exceeds maximum length of 50 characters.");

            if (Message.Length > 2000)
                throw new ValidationException("Message exceeds maximum length of 2000 characters.");

            if (TimeStamp == default)
                throw new ValidationException("Timestamp must be set.");
        }
        #endregion
    }
}
