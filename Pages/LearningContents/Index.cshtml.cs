using ModuleManager.Authorization;
using ModuleManager.Data;
using ModuleManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ModuleManager.Pages.LearningContents
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #region snippet
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public IList<ModuleManager.Models.LearningContent> LearningContent { get; set; }

        public async Task OnGetAsync()
        {
            var learningContents = Context.LearningContent.AsNoTracking().Join(Context.Templates, a => a.TemplateId, b => b.TemplateID, (a, b) => new LearningContent { LearningContentId=a.LearningContentId,Name=a.Name,TemplateId=b.Name,Status=a.Status,TimeStamp=a.TimeStamp});
            var isAuthorized = User.IsInRole(Constants.ModuleManagersRole) ||
                               User.IsInRole(Constants.ModuleAdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            // Only approved learningContents are shown UNLESS you're authorized to see them
            // or you are the owner.
            if (!isAuthorized)
            {
                learningContents = learningContents.Where(c => c.Status == LearningContentStatus.Approved
                                            || c.OwnerID == currentUserId);
            }

            LearningContent = await learningContents.ToListAsync();
        }
    }
    #endregion
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
