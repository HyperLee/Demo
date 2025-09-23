using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Demo.Pages
{
    public class index10 : PageModel
    {
        private readonly ILogger<index10> _logger;

        public index10(ILogger<index10> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}