using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using RulesEngine.Extensions;
using RulesEngine.Models;
using sampleRulesEngine.Models;

namespace sampleRulesEngine.Pages;

public class WorkflowTesting : PageModel
{
    private readonly ILogger<PrivacyModel> _logger;
    private List<Workflow> Workflows { get; set; } 

    private static List<Rule>[] defaultRules = new List<Rule>[4] { new List<Rule>(), new List<Rule>(), new List<Rule>(), new List<Rule>() };  
    private static List<InputObject>[] defaultInputs = new List<InputObject>[4] { new List<InputObject>(), new List<InputObject>(), new List<InputObject>(), new List<InputObject>() };  

    private static List<PresetRule> PresetRules { get; set; }

    private static List<PresetInput> PresetInputs { get; set; }

    
    [BindProperty]
    public static string InputItems { get; set; }
    [BindProperty]
    public string WorkflowRules { get; set; }

    [BindProperty]
    public Guid InputProductsDropdown { get; set; }
    [BindProperty]
    public Guid PresetRulesDropdown { get; set; }

    [BindProperty]
    public List<PresetRule> PresetRulesList { get; set; }

    [BindProperty]
    public List<PresetInput> PresetInputsList { get; set; }

    [BindProperty]
    public ResultOutput Output { get; set; }

    static WorkflowTesting()
    {
        InitPresetRules();
        InitPresetInputs();

    }

    public WorkflowTesting(ILogger<PrivacyModel> logger)
    {
        _logger = logger;

        PresetRulesList = new List<PresetRule>();
        foreach(var r in PresetRules)
        {
            PresetRulesList.Add(r);
        }

        PresetInputsList = new List<PresetInput>();
        foreach(var r in PresetInputs)
        {
            PresetInputsList.Add(r);
        } 

        InitRulesEngine();
    }

    private void InitRulesEngine()
    {
        Workflows = new List<Workflow>();
    }

    public void OnGet()
    {
    }

    [HttpGet]
    public string GetRuleJson(string ruleId) {
        return PresetRules.FirstOrDefault(x => x.Id == Guid.Parse(ruleId));
    }

    public void OnPost()
    {
        PresetInput input = new PresetInput();
        if(InputProductsDropdown != Guid.Empty)
        {
            input = PresetInputs.FirstOrDefault(x => x.Id == InputProductsDropdown);
            InputItems = input.InputJson;
        }

        PresetRule rule = new PresetRule();
        if(PresetRulesDropdown != Guid.Empty)
        {
            rule = PresetRules.FirstOrDefault(x => x.Id == PresetRulesDropdown);
            WorkflowRules = rule.RuleJson;
        }

        if(string.IsNullOrEmpty(InputItems) || string.IsNullOrEmpty(WorkflowRules))
        {
            Output = new ResultOutput() { OutputError = "Please select input and rules" };
            return;
        }

        Workflow[] workflows = new Workflow[] { new Workflow() { WorkflowName = rule.DisplayText, RuleExpressionType= RuleExpressionType.LambdaExpression } };
        workflows[0].Rules = JsonConvert.DeserializeObject<List<Rule>>(WorkflowRules);
        var rulesEngine = new RulesEngine.RulesEngine(workflows, null);
        List<RuleResultTree> resultList = rulesEngine.ExecuteAllRulesAsync(rule.DisplayText, JsonConvert.DeserializeObject<InputObject[]>(InputItems)).Result;

        bool outcome = false;
        outcome = resultList.TrueForAll(r => r.IsSuccess);

        StringBuilder sb = new StringBuilder();

        if(outcome)
            sb.AppendLine("All Rules Evaluated Sucessfully and can be applied <br/>");
        else
            sb.AppendLine("Some Rules Failed and can't be applied<br/>");
        
        sb.AppendLine("Rules Results:");
        resultList.ForEach(r => { 
            sb.AppendLine($"Rule '{r.Rule.RuleName}' is {(r.IsSuccess ? "Success" : "Fail")}<br/>");
            if(!r.IsSuccess)
            {
                sb.AppendLine($"Error Message: {r.ExceptionMessage}<br/>");
            }
            foreach(var item in r.Inputs)
            {
                sb.AppendLine($"Input: {JsonConvert.SerializeObject(item)}<br/>");
            }
        });

        Output = new ResultOutput() { Message = sb.ToString() };
    }

    
    private static void InitPresetInputs()
    {
        PresetInputs = new List<PresetInput>();
        
        defaultInputs[0] = new List<InputObject>() {
            new InputObject() { 
                UserID = "user1", 
                UserLocation = "Lisboa", 
                ItemToPurchase = new CatalogItem() { 
                    ItemID = "1", Price = 10.5, Category = "Infantil", CategoryPath = "Canais\\Infantil", Title = "Disney Channel", IsSubscription = true, IsBundle = false, IsDiscountable=true, Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    },
                SubscribedServices = new List<CatalogItem>() {},
                PreviousPurchases = new List<CatalogItemPurchase>() { new CatalogItemPurchase() { Item = new CatalogItem() { ItemID = "2", Price = 10.5, Category = "Infantil", CategoryPath = "Canais\\Infantil", Title = "Panda Channel", IsSubscription = true, IsBundle = false, Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } }, PurchaseDate = DateTime.Now.AddDays(-30) } },
                CurrentOffer = new Offer() { OfferID = "1", OfferProducts = new List<CatalogItem>() { new CatalogItem() { ItemID = "100", Price = 10.5, Category = "Infantil", CategoryPath = "Canais\\Infantil", Title = "Disney Channel", IsSubscription = true, IsBundle = false, Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } } } } 
                }
            };
        PresetInputs.Add(new PresetInput() { DisplayText = "Single Product", Id = Guid.NewGuid(), InputJson = JsonConvert.SerializeObject(defaultInputs[0])});
        
        defaultInputs[1] = new List<InputObject>() {
            new InputObject() { 
                UserID = "user", 
                UserLocation = "Porto", 
                ItemToPurchase = new CatalogItem() {
                    ItemID = "1", Price = 10.5, Category = "Infantil", CategoryPath = "Canais\\Infantil", Title = "Disney Channel", IsSubscription = true, IsBundle = false, Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" }, 
                    },
                SubscribedServices = new List<CatalogItem>() {},
                PreviousPurchases = new List<CatalogItemPurchase>() { new CatalogItemPurchase() { Item = new CatalogItem() { ItemID = "3", Price = 10.5, Category = "SportTV", CategoryPath = "Canais\\Desporto\\SportTV", Title = "SportTV 2", IsSubscription = true, IsBundle = false, Tags = new List<string>() { "SportTV", "Desporto","Subscription", "LiveChannel" } }, PurchaseDate = DateTime.Now.AddDays(-30) } },
                CurrentOffer = new Offer() { OfferID = "1", OfferProducts = new List<CatalogItem>() { new CatalogItem() { ItemID = "100", Price = 10.5, Category = "Infantil", CategoryPath = "Canais\\Infantil", Title = "Disney Channel", IsSubscription = true, IsBundle = false, Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } } } } 
                }
            };
        PresetInputs.Add(new PresetInput() { DisplayText = "Products with User Purchase History", Id = Guid.NewGuid(), InputJson = JsonConvert.SerializeObject(defaultInputs[3])});
    }

    private static void InitPresetRules()
    {
        PresetRules = new List<PresetRule>();

        defaultRules[0] = new List<Rule>() { 
            new Rule() { 
                RuleName = "Direct Discount",
                 SuccessEvent = "Item is Discounted",
                 ErrorMessage = "Item can't be discounted",
                 Expression = "item.ItemToPurchase.IsDiscountable == true",

                 RuleExpressionType = RuleExpressionType.LambdaExpression
                } 
            };
        PresetRules.Add(new PresetRule() { DisplayText = "Direct Discount", Id = Guid.NewGuid(), RuleJson = JsonConvert.SerializeObject(defaultRules[0])});

        defaultRules[1] = new List<Rule>() { 
            new Rule() { 
                RuleName = "Category Discount",
                 SuccessEvent = "Item is Discounted",
                 ErrorMessage = "Item can't be discounted",
                 Expression = "input => input.ItemToPurchase.IsDiscountable",

                 RuleExpressionType = RuleExpressionType.LambdaExpression
                } 
            };
        PresetRules.Add(new PresetRule() { DisplayText = "Category Discount", Id = Guid.NewGuid(), RuleJson = JsonConvert.SerializeObject(defaultRules[1])});

        defaultRules[2] = new List<Rule>() { 
            new Rule() { 
                RuleName = "Sports Category Discount",
                 SuccessEvent = "Item is Discounted",
                 ErrorMessage = "Item can't be discounted",
                 Expression = "input => input.ItemToPurchase.IsDiscountable",

                 RuleExpressionType = RuleExpressionType.LambdaExpression
                },
            new Rule() { 
                RuleName = "Direct Discount",
                 SuccessEvent = "Item is Discounted",
                 ErrorMessage = "Item can't be discounted",
                 Expression = "input => input.ItemToPurchase.IsDiscountable",

                 RuleExpressionType = RuleExpressionType.LambdaExpression
                } 
            };
        PresetRules.Add(new PresetRule() { DisplayText = "Multiple Rules", Id = Guid.NewGuid(), RuleJson = JsonConvert.SerializeObject(defaultRules[2])});

        defaultRules[3] = new List<Rule>() { 
            new Rule() { 
                RuleName = "Previous Purchase SportTV 1 Discount",
                 SuccessEvent = "Item is Discounted",
                 ErrorMessage = "Item can't be discounted",
                 Expression = "input => input.ItemToPurchase.IsDiscountable",

                 RuleExpressionType = RuleExpressionType.LambdaExpression
                } 
            };
        PresetRules.Add(new PresetRule() { DisplayText = "Previous Purchase Rules", Id = Guid.NewGuid(), RuleJson = JsonConvert.SerializeObject(defaultRules[3])});
    }
}

