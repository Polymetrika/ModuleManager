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

namespace ModuleManager.Pages.LearningContents
{
    public class CreateModel : DI_BasePageModel
    {
        public CreateModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
            Templates = context.Templates.AsNoTracking().Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.TemplateId.ToString(),
                                      Text = a.Name
                                  }).ToList();
            Modules = context.Module.AsNoTracking().Select(a =>
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
        public ModuleManager.Models.LearningContent LearningContent { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            LearningContent.OwnerID = UserManager.GetUserId(User);

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                        User, LearningContent,
                                                        ModuleOperations.Create);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            LearningContent.Type = (await Context.Templates.FirstOrDefaultAsync(a => a.TemplateId == LearningContent.TemplateId)).LearningContentType;
            LearningContent.TimeStamp = DateTime.UtcNow;
            Context.LearningContent.Add(LearningContent);
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