#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#region snippet
using ModuleManager.Authorization;
using ModuleManager.Data;
using ModuleManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ModuleManager.Pages.Modules
{
    public class CreateModel : DI_BasePageModel
    {
        public CreateModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
            Templates = context.Templates.Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.TemplateID.ToString(),
                                      Text = a.Name
                                  }).ToList();
        }
        public List<SelectListItem> Templates { get; set; }
        public IActionResult OnGet()
        {
            Templates = Templates;
            return Page();
        }

        [BindProperty]
        public ModuleManager.Models.Module Module { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Module.OwnerID = UserManager.GetUserId(User);

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                        User, Module,
                                                        ModuleOperations.Create);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            Module.TimeStamp = DateTime.UtcNow;
            Context.Module.Add(Module);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
#endregion
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

/*
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public CreateModel(ApplicationDbContext context)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
*/