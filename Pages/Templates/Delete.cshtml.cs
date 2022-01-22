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
    public class DeleteModel : PageModel
    {
        private readonly ModuleManager.Data.ApplicationDbContext _context;

        public DeleteModel(ModuleManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Template Template { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Template = await _context.Templates.FirstOrDefaultAsync(m => m.TemplateID == id);

            if (Template == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Template = await _context.Templates.FindAsync(id);

            if (Template != null)
            {
                _context.Templates.Remove(Template);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
