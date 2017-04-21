using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Abstractions
{
    public interface ICompatibleOktaAccount
    {
        string Href { get; }

        string Status { get; }

        DateTimeOffset? CreatedAt { get; }

        DateTimeOffset? ModifiedAt { get; }

        DateTimeOffset? PasswordModifiedAt { get; }

        string FullName { get; }

        string GivenName { get; set; }

        string MiddleName { get; set; }

        string Surname { get; set; }

        string Username { get; set; }

        string Email { get; set; }

        string EmailVerificationStatus { get; }

        string EmailVerificationToken { get; }

        IDictionary<string, object> CustomData { get; }

        [Obsolete("Use the CustomData property.")]
        IDictionary<string, object> GetCustomData();

        [Obsolete("Use the CustomData property.")]
        Task<IDictionary<string, object>> GetCustomDataAsync(CancellationToken _);
    }
}
