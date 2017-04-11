using System.Collections.Generic;

namespace Stormpath.Owin.Middleware.Okta
{
    public class ApiError
    {
        public string ErrorCode { get; set; }

        public string ErrorSummary { get; set; }

        public string ErrorLink { get; set; }

        public string ErrorId { get; set; }

        public List<object> ErrorCauses { get; set; }
    }

}
