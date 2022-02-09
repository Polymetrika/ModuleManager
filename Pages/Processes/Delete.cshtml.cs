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

namespace ModuleManager.Pages.Processes
{
    public class DeleteModel : PageModel
    {
        private readonly ModuleManager.Data.ApplicationDbContext _context;

        public DeleteModel(ModuleManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Process BusinessProcess { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BusinessProcess = await _context.Processes.FirstOrDefaultAsync(m => m.ProcessId == id);

            if (BusinessProcess == null)
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

            BusinessProcess = await _context.Processes.FindAsync(id);

            if (BusinessProcess != null)
            {
                _context.Processes.Remove(BusinessProcess);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
