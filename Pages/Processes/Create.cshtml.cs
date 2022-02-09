#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ModuleManager.Data;
using ModuleManager.Models;

namespace ModuleManager.Pages.Processes
{
    public class CreateModel : PageModel
    {
        private readonly ModuleManager.Data.ApplicationDbContext _context;

        public CreateModel(ModuleManager.Data.ApplicationDbContext context)
        {
            _context = context;
            Templates = context.Templates.AsNoTracking().Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.TemplateId.ToString(),
                                      Text = a.Name
                                  }).ToList();
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public List<SelectListItem> Templates { get; set; }
        [BindProperty]
        public Process Process { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            Process.ProcessId=Guid.NewGuid().ToString("N");
            ModelState.ClearValidationState(nameof(Process));
            if (!TryValidateModel(Process, nameof(Process)))
            {
                return Page();
            }

            _context.Processes.Add(Process);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
