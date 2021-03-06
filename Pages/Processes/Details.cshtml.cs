#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ModuleManager.Data;
using ModuleManager.Models;
using System.Text.Json;

namespace ModuleManager.Pages.Processes
{
    public class DetailsModel : PageModel
    {
        private readonly ModuleManager.Data.ApplicationDbContext _context;

        public DetailsModel(ModuleManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Process Process { get; set; }
        public List<SelectListItem> RequiredModuleTemplates { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Process = await _context.Processes.FirstOrDefaultAsync(m => m.ProcessId == id);

            if (Process == null)
            {
                return NotFound();
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string id, int releasestatus)
        {
            Process = await _context.Processes.FirstOrDefaultAsync(m => m.ProcessId == id);

            if (Process == null)
            {
                return NotFound();
            }
            Process.ReleaseStatus = (ReleaseStatus)releasestatus;
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

