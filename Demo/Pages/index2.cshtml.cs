using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Demo.Pages
{
    public class index2 : PageModel
    {
        private readonly ILogger<index2> _logger;

        public index2(ILogger<index2> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}