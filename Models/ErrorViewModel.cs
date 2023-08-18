using System;

namespace AdminBlog.Models
{
    public class ErrorViewModel
    {
        // Ýstek kimliðini temsil eden özellik
        public string RequestId { get; set; }

        // Ýstek kimliði varsa, ShowRequestId özelliði true olur
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
