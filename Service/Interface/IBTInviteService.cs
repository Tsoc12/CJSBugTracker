using CJSBugTracker.Models;

namespace CJSBugTracker.Service.Interface
{
    public interface IBTInviteService
    {
        Task<bool> AcceptInviteAsync(Guid? token, string UserId, int companyId);
        Task AddNewInviteAsync(Invite invite);
        Task <bool> AnyInviteAsync(Guid token, string email,int companyId);
        Task<Invite?> GetInviteAsync(int inviteId, int companyId);
        Task<Invite?> GetInviteAsync(Guid token, string email, int companyId);
        Task<bool> ValidateInviteCodeAsync(Guid? token);
        Task CancelInviteAsync(int inviteId, int companyId);
        Task UpdateInviteAsync(Invite invite);
    }
}
