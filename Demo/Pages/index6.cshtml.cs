using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Demo.Pages
{
    public class index6 : PageModel
    {
        private readonly ILogger<index6> _logger;

        public index6(ILogger<index6> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}