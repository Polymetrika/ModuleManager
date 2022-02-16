using ModuleManager.Authorization;
using ModuleManager.Data;
using ModuleManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ModuleManager.Pages.Components
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #region snippet
    [AllowAnonymous]
    public class Details2Model : DI_BasePageModel
    {
        public Details2Model(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public ModuleManager.Models.Component Component { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            ModuleManager.Models.Component? _module = await Context.Components.FirstOrDefaultAsync(m => m.ComponentId == id);

            if (_module == null)
            {
                return NotFound();
            }
            Component = _module;

            if (!User.Identity!.IsAuthenticated)
            {
                return Challenge();
            }

            var isAuthorized = User.IsInRole(Constants.ModuleManagersRole) ||
                               User.IsInRole(Constants.ModuleAdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            if (!isAuthorized
                && currentUserId != Component.OwnerID
                && Component.Status != Status.Approved)
            {
                return Forbid();
            }

            return Page();
        }
    }
    #endregion
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
