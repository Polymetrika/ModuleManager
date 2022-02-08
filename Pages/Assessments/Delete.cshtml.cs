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
        public ModuleManager.Models.Component Component { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ModuleManager.Models.Component? _module = await Context.Components.FirstOrDefaultAsync(
                                                 m => m.ComponentId == id);

            if (_module == null)
            {
                return NotFound();
            }
            Component = _module;

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, Component,
                                                     ModuleOperations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var Component = await Context
                .Components.AsNoTracking()
                .FirstOrDefaultAsync(m => m.ComponentId == id);

            if (Component == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, Component,
                                                     ModuleOperations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            Context.Components.Remove(Component);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
    #endregion
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
