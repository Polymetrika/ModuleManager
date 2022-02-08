using ModuleManager.Authorization;
using ModuleManager.Data;
using ModuleManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ModuleManager.Pages.Modules
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #region snippet
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public IList<ModuleManager.Models.Module> Module { get; set; }

        public async Task OnGetAsync()
        {
            var modules = Context.Module.AsNoTracking();
            var isAuthorized = User.IsInRole(Constants.ModuleManagersRole) ||
                               User.IsInRole(Constants.ModuleAdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            // Only approved modules are shown UNLESS you're authorized to see them
            // or you are the owner.
            if (!isAuthorized)
            {
                modules = modules.Where(c => c.Status == Status.Approved
                                            || c.OwnerID == currentUserId);
            }

            Module = await modules.ToListAsync();
        }
    }
    #endregion
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
