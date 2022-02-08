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
    public class IndexModel : PageModel
    {
        private readonly ModuleManager.Data.ApplicationDbContext _context;

        public IndexModel(ModuleManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Process> BusinessProcess { get;set; }

        public async Task OnGetAsync()
        {
            BusinessProcess = await _context.Processes.ToListAsync();
        }
    }
}
