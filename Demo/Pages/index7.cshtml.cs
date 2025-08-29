using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Demo.Pages
{
    public class index7 : PageModel
    {
        private readonly ILogger<index7> _logger;

        public index7(ILogger<index7> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}