#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#region snippet
using ModuleManager.Authorization;
using ModuleManager.Data;
using ModuleManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
            Templates = context.Templates.AsNoTracking().Where(a => a.ContentType == ContentType.Module && a.ReleaseStatus == ReleaseStatus.Active).Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.TemplateId.ToString(),
                                      Text = a.Name
                                  }).ToList();
            Processes = context.Processes.AsNoTracking().Where(a => a.ReleaseStatus == ReleaseStatus.Active).Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.ProcessId.ToString(),
                                      Text = a.Name
                                  }).ToList();
        }
        public List<SelectListItem> Templates { get; set; }
        public List<SelectListItem> Processes { get; set; }
        public IActionResult OnGet()
        {
            Templates = Templates;
            Processes = Processes;
            return Page();
        }

        [BindProperty]
        public ModuleManager.Models.Module Module { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            Module.ModuleId = Guid.NewGuid().ToString("N");
            ModelState.ClearValidationState(nameof(Module));
            Module.OwnerID = UserManager.GetUserId(User);
            if (!TryValidateModel(Module, nameof(Module)))
            {
                
                return Page();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                        User, Module,
                                                        ModuleOperations.Create);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            Module.TimeStamp = DateTime.UtcNow;
            Context.Modules.Add(Module);
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