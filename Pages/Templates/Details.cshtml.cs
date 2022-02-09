#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ModuleManager.Data;
using ModuleManager.Models;

namespace ModuleManager.Pages.Templates
{
    public class DetailsModel : PageModel
    {
        private readonly ModuleManager.Data.ApplicationDbContext _context;

        public DetailsModel(ModuleManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Template Template { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Template = await _context.Templates.FirstOrDefaultAsync(m => m.TemplateId == id);
            if (Template == null)
            {
                return NotFound();
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string id,int releasestatus)
        {
            Template = await _context.Templates.FirstOrDefaultAsync(m => m.TemplateId == id);

            if (Template == null)
            {
                return NotFound();
            }
            Template.ReleaseStatus=(ReleaseStatus)releasestatus;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TemplateExists(Template.TemplateId))
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
        private bool TemplateExists(string id)
        {
            return _context.Templates.Any(e => e.TemplateId == id);
        }
    }
}
