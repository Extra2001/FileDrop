using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FubarDev.FtpServer.AccountManagement;
using Microsoft.Extensions.DependencyInjection;

namespace FileDrop.Helpers.TransferHelper.Transferer
{
    public class FTPMember : IMembershipProvider
    {
        private string token { get; }

        public FTPMember(string token)
        {
            this.token = token;
        }

        public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
        {
            if (username == "root" && password == token)
            {
                var claim = new Claim(ClaimTypes.Name, "root");
                var claimsIdentity = new ClaimsIdentity(new Claim[] { claim });
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                var result = new MemberValidationResult
                    (MemberValidationStatus.AuthenticatedUser, claimsPrincipal);
                return Task.FromResult(result);
            }

            return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidLogin));
        }
    }

    public static class FTPMembershipExtenstionMethod
    {
        public static void AddFtpToken(this ServiceCollection services, string password)
        {
            services.AddSingleton<IMembershipProvider>(new FTPMember(password));
        }
    }
}
