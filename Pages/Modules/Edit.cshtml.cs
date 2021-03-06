using ModuleManager.Authorization;
using ModuleManager.Data;
using ModuleManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace ModuleManager.Pages.Modules
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
        public ModuleManager.Models.Module Module { get; set; }
        public string Template { get; set; }
        public string ProcessTemplate { get; set; }
        public List<SelectListItem> Templates { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            ModuleManager.Models.Module? module = await Context.Modules.FirstOrDefaultAsync(
                                                             m => m.ModuleId == id);
            if (module == null)
            {
                return NotFound();
            }

            Module = module;
            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                      User, Module,
                                                      ModuleOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            Template = (await Context.Templates.FirstOrDefaultAsync(m => m.TemplateId == Module.TemplateId)).Details;
            var process = (await Context.Processes.FirstOrDefaultAsync(m => m.ProcessId == Module.ProcessId));
            var templates = JsonSerializer.Deserialize<ICollection<string>>(process.RequiredModuleTemplates??"[]");
            ProcessTemplate = ProcessTemplate ?? "";

            var dbtemplates = await Context.Templates.AsNoTracking().Where(a => templates.Contains(a.TemplateId)).ToListAsync();
            Templates=templates.Join(dbtemplates, a => a,b => b.TemplateId, (a, b) => new {b.Name,b.TemplateId}).Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.TemplateId.ToString(),
                                      Text = a.Name
                                  }).ToList();
            //parse required templates IDs from process definition, lookup templates in the DB and provide the list to the UI.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Fetch Module from DB to get OwnerID.
            var module = await Context
                .Modules.AsNoTracking()
                .FirstOrDefaultAsync(m => m.ModuleId == id);

            if (module == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, module,
                                                     ModuleOperations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            Module.OwnerID = module.OwnerID;
            Module.TimeStamp = DateTime.UtcNow;
            Context.Attach(Module).State = EntityState.Modified;

            if (Module.Status == Status.Approved)
            {
                // If the module is updated after approval, 
                // and the user cannot approve,
                // set the status back to submitted so the update can be
                // checked and approved.
                var canApprove = await AuthorizationService.AuthorizeAsync(User,
                                        Module,
                                        ModuleOperations.Approve);

                if (!canApprove.Succeeded)
                {
                    Module.Status = Status.Submitted;
                }
            }

            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
    #endregion
    #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
