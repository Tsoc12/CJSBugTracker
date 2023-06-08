using CJSBugTracker.Models;

namespace CJSBugTracker.Service.Interface
{
    public interface IBTCompanyService
    {
        Task<Company?> GetCompanyInfoAsync(int companyId);
        Task<List<BTUser>> GetCompanyMemberAsync(int companyId);

    }
}
