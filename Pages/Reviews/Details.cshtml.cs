using ModuleManager.Authorization;
using ModuleManager.Data;
using ModuleManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ModuleManager.Pages.Reviews
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

        public ModuleManager.Models.Review Review { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ModuleManager.Models.Review? _module = await Context.Review.FirstOrDefaultAsync(m => m.ReviewId == id);

            if (_module == null)
            {
                return NotFound();
            }
            Review = _module;

            var isAuthorized = User.IsInRole(Constants.ModuleManagersRole) ||
                               User.IsInRole(Constants.ModuleAdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            if (!isAuthorized
                && currentUserId != Review.OwnerID
                && Review.Status != ReviewStatus.Approved)
            {
                return Forbid();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id, ReviewStatus status)
        {
            var review = await Context.Review.FirstOrDefaultAsync(
                                                      m => m.ReviewId == id);

            if (review == null)
            {
                return NotFound();
            }

            var moduleOperation = (status == ReviewStatus.Approved)
                                                       ? ModuleOperations.Approve
                                                       : ModuleOperations.Reject;

            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, review,
                                        moduleOperation);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            review.Status = status;
            Context.Review.Update(Review);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
    #endregion
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
