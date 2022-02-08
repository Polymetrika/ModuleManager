using ModuleManager.Authorization;
using ModuleManager.Data;
using ModuleManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ModuleManager.Pages.Assessments
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #region snippet
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public ModuleManager.Models.Component Component { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ModuleManager.Models.Component? _module = await Context.Components.FirstOrDefaultAsync(m => m.ComponentId == id);

            if (_module == null)
            {
                return NotFound();
            }
            Component = _module;

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

        public async Task<IActionResult> OnPostAsync(int id, Status status)
        {
            var Component = await Context.Components.FirstOrDefaultAsync(
                                                      m => m.ComponentId == id);

            if (Component == null)
            {
                return NotFound();
            }

            var moduleOperation = (status == Status.Approved)
                                                       ? ModuleOperations.Approve
                                                       : ModuleOperations.Reject;

            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, Component,
                                        moduleOperation);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            Component.Status = status;
            Context.Components.Update(Component);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
    #endregion
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
