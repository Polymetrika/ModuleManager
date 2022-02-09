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

namespace ModuleManager.Pages.Components
{
    public class CreateModel : DI_BasePageModel
    {
        public CreateModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
            Templates = context.Templates.AsNoTracking().Where(a => a.ContentType != ContentType.Module && a.ReleaseStatus == ReleaseStatus.Active).Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.TemplateId.ToString(),
                                      Text = a.Name
                                  }).ToList();
            Modules = context.Modules.AsNoTracking().Where(a => a.Status == Status.Draft).Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.ModuleId.ToString(),
                                      Text = a.Name
                                  }).ToList();
        }
        public List<SelectListItem> Templates { get; set; }
        public List<SelectListItem> Modules { get; set; }
        public IActionResult OnGet()
        {
            Templates = Templates;
            return Page();
        }

        [BindProperty]
        public ModuleManager.Models.Component Component { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Component.OwnerID = UserManager.GetUserId(User);

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                        User, Component,
                                                        ModuleOperations.Create);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            Component.ContentType = (await Context.Templates.FirstOrDefaultAsync(a => a.TemplateId == Component.TemplateId)).ContentType;
            Component.TimeStamp = DateTime.UtcNow;
            Context.Components.Add(Component);
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