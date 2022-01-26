using ModuleManager.Authorization;
using ModuleManager.Data;
using ModuleManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ModuleManager.Pages.LearningContents
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

        public ModuleManager.Models.LearningContent LearningContent { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ModuleManager.Models.LearningContent? _module = await Context.LearningContent.FirstOrDefaultAsync(m => m.LearningContentId == id);

            if (_module == null)
            {
                return NotFound();
            }
            LearningContent = _module;

            var isAuthorized = User.IsInRole(Constants.ModuleManagersRole) ||
                               User.IsInRole(Constants.ModuleAdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            if (!isAuthorized
                && currentUserId != LearningContent.OwnerID
                && LearningContent.Status != LearningContentStatus.Approved)
            {
                return Forbid();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id, LearningContentStatus status)
        {
            var learningContent = await Context.LearningContent.FirstOrDefaultAsync(
                                                      m => m.LearningContentId == id);

            if (learningContent == null)
            {
                return NotFound();
            }

            var moduleOperation = (status == LearningContentStatus.Approved)
                                                       ? ModuleOperations.Approve
                                                       : ModuleOperations.Reject;

            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, learningContent,
                                        moduleOperation);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            learningContent.Status = status;
            Context.LearningContent.Update(LearningContent);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
    #endregion
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
