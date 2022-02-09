#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ModuleManager.Data;
using ModuleManager.Models;

namespace ModuleManager.Pages.Processes
{
    public class EditModel : PageModel
    {
        private readonly ModuleManager.Data.ApplicationDbContext _context;

        public EditModel(ModuleManager.Data.ApplicationDbContext context)
        {
            _context = context;
            Templates = context.Templates.AsNoTracking().Where(a => a.ReleaseStatus==ReleaseStatus.Active).Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.TemplateId.ToString(),
                                      Text = a.Name
                                  }).ToList();
        }

        [BindProperty]
        public Process Process { get; set; }

        public List<SelectListItem> Templates { get; set; }
        public List<SelectListItem> RequiredModuleTemplates { get; set; }
        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Process = await _context.Processes.FirstOrDefaultAsync(m => m.ProcessId == id);
            var selectedTemplateIds = JsonSerializer.Deserialize<ICollection<string>>(Process.RequiredModuleTemplates??"[]").Select(a=>a.ToString());
            RequiredModuleTemplates = new List<SelectListItem>();
            foreach (var template in Templates.Where(a => selectedTemplateIds.Contains(a.Value)))
            {
                RequiredModuleTemplates.Add(new SelectListItem { Text=template.Text,Value=template.Value});
            }
            Templates = Templates.Where(a => !selectedTemplateIds.Contains(a.Value)).ToList();
            if (Process == null)
            {
                return NotFound();
            }
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Process).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProcessExists(Process.ProcessId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ProcessExists(string id)
        {
            return _context.Processes.Any(e => e.ProcessId == id);
        }
    }
}
