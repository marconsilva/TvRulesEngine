using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using sampleRulesEngine.Models;

namespace sampleRulesEngine.Pages;

public class WorkflowTesting : PageModel
{
    private readonly ILogger<PrivacyModel> _logger;

    public List<PresetRule> PresetRules { get; set; }
    public List<PresetInput> PresetInputs { get; set; }

    public ResultOutput Output { get; set; }     

    public WorkflowTesting(ILogger<PrivacyModel> logger)
    {
        _logger = logger;

        InitPresetRules();
        InitPresetInputs();
    }

    public void OnGet()
    {
    }

    public void OnPost()
    {
    }

    
    private void InitPresetInputs()
    {
        PresetInputs = new List<PresetInput>();

        PresetInputs.Add(new PresetInput() { DisplayText = "Single Product", Id = Guid.NewGuid()});
        PresetInputs.Add(new PresetInput() { DisplayText = "Multiple Products", Id = Guid.NewGuid()});
        PresetInputs.Add(new PresetInput() { DisplayText = "Multiple Products Different Categories", Id = Guid.NewGuid()});
        PresetInputs.Add(new PresetInput() { DisplayText = "Multiple Products with User Purchase History", Id = Guid.NewGuid()});
    }

    private void InitPresetRules()
    {
        PresetRules = new List<PresetRule>();

        PresetRules.Add(new PresetRule() { DisplayText = "Direct Discount", Id = Guid.NewGuid()});
        PresetRules.Add(new PresetRule() { DisplayText = "Category Discount", Id = Guid.NewGuid()});
        PresetRules.Add(new PresetRule() { DisplayText = "Multiple Rules", Id = Guid.NewGuid()});
        PresetRules.Add(new PresetRule() { DisplayText = "Previous Purchase Rules", Id = Guid.NewGuid()});
    }
}

