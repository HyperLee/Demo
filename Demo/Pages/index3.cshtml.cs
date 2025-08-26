using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Demo.Pages
{
    public class index3 : PageModel
    {
        private readonly ILogger<index3> _logger;

        public index3(ILogger<index3> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}