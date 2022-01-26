using ModuleManager.Authorization;
using ModuleManager.Data;
using ModuleManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ModuleManager.Pages.LearningContents
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #region snippet
    public class EditModel : DI_BasePageModel
    {
        public EditModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        [BindProperty]
        public ModuleManager.Models.LearningContent LearningContent { get; set; }
        [BindProperty]
        public string Template { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ModuleManager.Models.LearningContent? learningContent = await Context.LearningContent.FirstOrDefaultAsync(
                                                             m => m.LearningContentId == id);
            if (learningContent == null)
            {
                return NotFound();
            }

            LearningContent = learningContent;
            Template = (await Context.Templates.FirstOrDefaultAsync(m => m.TemplateId == LearningContent.TemplateId)).Details;
            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                      User, LearningContent,
                                                      ModuleOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Fetch LearningContent from DB to get OwnerID.
            var learningContent = await Context
                .LearningContent.AsNoTracking()
                .FirstOrDefaultAsync(m => m.LearningContentId == id);

            if (learningContent == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, learningContent,
                                                     ModuleOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            LearningContent.OwnerID = learningContent.OwnerID;
            LearningContent.TimeStamp = DateTime.UtcNow;
            Context.Attach(LearningContent).State = EntityState.Modified;

            if (LearningContent.Status == LearningContentStatus.Approved)
            {
                // If the learningContent is updated after approval, 
                // and the user cannot approve,
                // set the status back to submitted so the update can be
                // checked and approved.
                var canApprove = await AuthorizationService.AuthorizeAsync(User,
                                        LearningContent,
                                        ModuleOperations.Approve);

                if (!canApprove.Succeeded)
                {
                    LearningContent.Status = LearningContentStatus.Submitted;
                }
            }

            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
    #endregion
    #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
