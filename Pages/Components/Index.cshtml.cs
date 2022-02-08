using ModuleManager.Authorization;
using ModuleManager.Data;
using ModuleManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ModuleManager.Pages.Components
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

        public IList<ModuleManager.Models.Component> Component { get; set; }

        public async Task OnGetAsync()
        {
            var Components = Context.Components.AsNoTracking().Join(Context.Templates, a => a.TemplateId, b => b.TemplateId, (a, b) => new Component { ComponentId=a.ComponentId,Name=a.Name,TemplateId=b.Name,Status=a.Status,TimeStamp=a.TimeStamp});
            var isAuthorized = User.IsInRole(Constants.ModuleManagersRole) ||
                               User.IsInRole(Constants.ModuleAdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            // Only approved Components are shown UNLESS you're authorized to see them
            // or you are the owner.
            if (!isAuthorized)
            {
                Components = Components.Where(c => c.Status == Status.Approved
                                            || c.OwnerID == currentUserId);
            }

            Component = await Components.ToListAsync();
        }
    }
    #endregion
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
