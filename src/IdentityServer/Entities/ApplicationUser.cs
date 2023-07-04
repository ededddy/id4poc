using IdentityModel;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer.Entities
{
    public class ApplicationUser 
    {
        public string SubjectId { get; set; }

        //
        // Summary:
        //     Gets or sets the username.
        public string Username { get; set; }

        //
        // Summary:
        //     Gets or sets the provider name.
        public string ProviderName { get; set; }

        //
        // Summary:
        //     Gets or sets the provider subject identifier.
        public string ProviderSubjectId { get; set; }

        //
        // Summary:
        //     Gets or sets if the user is active.
        public bool IsActive { get; set; } = true;


        //
        // Summary:
        //     Gets or sets the claims.
        public ICollection<Claim> Claims { get; set; } = new HashSet<Claim>(new ClaimComparer());
    }
}
