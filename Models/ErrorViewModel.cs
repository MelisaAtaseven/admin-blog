using System;

namespace AdminBlog.Models
{
    public class ErrorViewModel
    {
        // �stek kimli�ini temsil eden �zellik
        public string RequestId { get; set; }

        // �stek kimli�i varsa, ShowRequestId �zelli�i true olur
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
