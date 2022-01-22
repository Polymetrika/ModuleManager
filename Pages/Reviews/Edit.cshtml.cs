using ModuleManager.Authorization;
using ModuleManager.Data;
using ModuleManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ModuleManager.Pages.Reviews
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
        public ModuleManager.Models.Review Review { get; set; }
        [BindProperty]
        public string Template { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ModuleManager.Models.Review? review = await Context.Review.FirstOrDefaultAsync(
                                                             m => m.ReviewId == id);
            if (review == null)
            {
                return NotFound();
            }

            Review = review;
            Template = (await Context.Templates.FirstOrDefaultAsync(m => m.TemplateID == Review.TemplateId)).Details;
            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                      User, Review,
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

            // Fetch Review from DB to get OwnerID.
            var review = await Context
                .Review.AsNoTracking()
                .FirstOrDefaultAsync(m => m.ReviewId == id);

            if (review == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, review,
                                                     ModuleOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            Review.OwnerID = review.OwnerID;
            Review.TimeStamp = DateTime.UtcNow;
            Context.Attach(Review).State = EntityState.Modified;

            if (Review.Status == ReviewStatus.Approved)
            {
                // If the review is updated after approval, 
                // and the user cannot approve,
                // set the status back to submitted so the update can be
                // checked and approved.
                var canApprove = await AuthorizationService.AuthorizeAsync(User,
                                        Review,
                                        ModuleOperations.Approve);

                if (!canApprove.Succeeded)
                {
                    Review.Status = ReviewStatus.Submitted;
                }
            }

            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
    #endregion
    #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
