using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Demo.Pages
{
    public class index8 : PageModel
    {
        private readonly ILogger<index8> _logger;

        public index8(ILogger<index8> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}