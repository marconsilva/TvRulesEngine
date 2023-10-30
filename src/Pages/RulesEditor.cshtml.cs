using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace sampleRulesEngine.Pages;

public class RulesEditor : PageModel
{
    private readonly ILogger<PrivacyModel> _logger;

    public RulesEditor(ILogger<PrivacyModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}

