using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Demo.Pages
{
    public class index5 : PageModel
    {
        private readonly ILogger<index5> _logger;

        public index5(ILogger<index5> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}