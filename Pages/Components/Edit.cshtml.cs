using ModuleManager.Authorization;
using ModuleManager.Data;
using ModuleManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ModuleManager.Pages.Components
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
        public ModuleManager.Models.Component Component { get; set; }
        [BindProperty]
        public string Template { get; set; }
        public Module Module { get; set; }

        public async Task<IActionResult> OnGetAsync(string templateid,string moduleid)
        {
            Component = await Context.Components.FirstOrDefaultAsync(
                                                             m => m.TemplateId==templateid && m.ModuleId==moduleid);
            Module= await Context.Modules.FirstOrDefaultAsync(
                                                             m => m.ModuleId == moduleid);
            var template = (await Context.Templates.FirstOrDefaultAsync(m => m.TemplateId == templateid));
            Template = template.Details;
            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                      User, Module,
                                                      ModuleOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            if (Module != null && template !=null && Component == null)
            {
                Component = new Component
                {
                    ComponentId = Guid.NewGuid().ToString("N"),
                    ModuleId = moduleid,
                    TemplateId = templateid,
                    ContentType = template.ContentType,
                    Details="{}"
                };
                Component.TimeStamp = DateTime.UtcNow;
                Context.Components.Add(Component);
                await Context.SaveChangesAsync();
            }


            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Fetch Component from DB to get OwnerID.
            var Component = await Context
                .Components.AsNoTracking()
                .FirstOrDefaultAsync(m => m.ComponentId == id);

            if (Component == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, Component,
                                                     ModuleOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            Component.OwnerID = Component.OwnerID;
            Component.TimeStamp = DateTime.UtcNow;
            Context.Attach(Component).State = EntityState.Modified;

            if (Component.Status == Status.Approved)
            {
                // If the Component is updated after approval, 
                // and the user cannot approve,
                // set the status back to submitted so the update can be
                // checked and approved.
                var canApprove = await AuthorizationService.AuthorizeAsync(User,
                                        Component,
                                        ModuleOperations.Approve);

                if (!canApprove.Succeeded)
                {
                    Component.Status = Status.Submitted;
                }
            }

            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
    #endregion
    #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
