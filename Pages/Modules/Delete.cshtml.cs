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
    public class DeleteModel : DI_BasePageModel
    {
        public DeleteModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        [BindProperty]
        public ModuleManager.Models.Module Module { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            ModuleManager.Models.Module? _module = await Context.Modules.FirstOrDefaultAsync(
                                                 m => m.ModuleId == id);

            if (_module == null)
            {
                return NotFound();
            }
            Module = _module;

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, Module,
                                                     ModuleOperations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var module = await Context
                .Modules.AsNoTracking()
                .FirstOrDefaultAsync(m => m.ModuleId == id);

            if (module == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, module,
                                                     ModuleOperations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            Context.Modules.Remove(module);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
    #endregion
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
