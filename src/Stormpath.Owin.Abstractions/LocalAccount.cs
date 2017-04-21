using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stormpath.Owin.Abstractions
{
    public class LocalAccount
    {
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Login { get; set; }

        public IDictionary<string, object> CustomData { get; } = new Dictionary<string, object>();
    }
}
