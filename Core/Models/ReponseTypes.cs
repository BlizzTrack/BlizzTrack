using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
    public class ReponseTypes
    {
        public class ErrorMessage
        {
            [Required]
            public string Error { get; set; }
        }

        public class NotFound : ErrorMessage
        {
            public NotFound()
            {
                Error = "Not Found";
            }
        }

        public class BadRequest : ErrorMessage
        {
            public BadRequest()
            {
                Error = "Bad Request";
            }
        }
    }
}
