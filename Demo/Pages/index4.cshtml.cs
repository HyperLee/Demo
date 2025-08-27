using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Demo.Pages
{
    public class index4 : PageModel
    {
        private readonly ILogger<index4> _logger;

        public index4(ILogger<index4> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}