#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ModuleManager.Data;
using ModuleManager.Models;

namespace ModuleManager.Pages.Templates
{
    public class CreateModel : PageModel
    {
        private readonly ModuleManager.Data.ApplicationDbContext _context;

        public CreateModel(ModuleManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Template Template { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            Template.TemplateID=Guid.NewGuid().ToString("N");
            ModelState.ClearValidationState(nameof(Template));
            if (!TryValidateModel(Template, nameof(Template)))
            {
                return Page();
            }

            _context.Templates.Add(Template);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
