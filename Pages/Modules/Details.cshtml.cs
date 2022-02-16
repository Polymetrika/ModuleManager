using ModuleManager.Authorization;
using ModuleManager.Data;
using ModuleManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ModuleManager.Pages.Modules
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

        public ModuleManager.Models.Module Module { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            ModuleManager.Models.Module? _module = await Context.Modules.FirstOrDefaultAsync(m => m.ModuleId == id);

            if (_module == null)
            {
                return NotFound();
            }
            Module = _module;

            var isAuthorized = User.IsInRole(Constants.ModuleManagersRole) ||
                               User.IsInRole(Constants.ModuleAdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            if (!isAuthorized
                && currentUserId != Module.OwnerID
                && Module.Status != Status.Approved)
            {
                return Forbid();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id, Status status)
        {
            var module = await Context.Modules.FirstOrDefaultAsync(
                                                      m => m.ModuleId == id);

            if (module == null)
            {
                return NotFound();
            }

            var moduleOperation = (status == Status.Approved)
                                                       ? ModuleOperations.Approve
                                                       : ModuleOperations.Reject;

            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, module,
                                        moduleOperation);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            module.Status = status;
            Context.Modules.Update(Module);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
    #endregion
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
