using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Demo.Pages
{
    public class PomodoroTechnique : PageModel
    {
        private readonly ILogger<PomodoroTechnique> _logger;

        public PomodoroTechnique(ILogger<PomodoroTechnique> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}