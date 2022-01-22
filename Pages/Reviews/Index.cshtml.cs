using ModuleManager.Authorization;
using ModuleManager.Data;
using ModuleManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ModuleManager.Pages.Reviews
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #region snippet
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public IList<ModuleManager.Models.Review> Review { get; set; }

        public async Task OnGetAsync()
        {
            var reviews = Context.Review.AsNoTracking().Join(Context.Templates, a => a.TemplateId, b => b.TemplateID, (a, b) => new Review { ReviewId=a.ReviewId,Name=a.Name,TemplateId=b.Name,Status=a.Status,TimeStamp=a.TimeStamp});
            var isAuthorized = User.IsInRole(Constants.ModuleManagersRole) ||
                               User.IsInRole(Constants.ModuleAdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            // Only approved reviews are shown UNLESS you're authorized to see them
            // or you are the owner.
            if (!isAuthorized)
            {
                reviews = reviews.Where(c => c.Status == ReviewStatus.Approved
                                            || c.OwnerID == currentUserId);
            }

            Review = await reviews.ToListAsync();
        }
    }
    #endregion
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
