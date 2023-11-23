using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Newtonsoft.Json;
using RulesEngine.Extensions;
using RulesEngine.Models;
using sampleRulesEngine.Models;

namespace sampleRulesEngine.Pages;

public class WorkflowTesting : PageModel
{
    private readonly ILogger<PrivacyModel> _logger;
    private List<Workflow> Workflows { get; set; } 

    private static List<DefaultInput> defaultInputs = new List<DefaultInput>();  
    
    [BindProperty]
    public static string InputItems { get; set; }
    [BindProperty]
    public string WorkflowRules { get; set; }
    [BindProperty]
    public string UsersInfo { get; set; }

    [BindProperty]
    public Guid? InputsDropdownSelectedGuid { get; set; }

    [BindProperty]
    public DefaultInput? InputsDropdownSelected { get; set; }

    [BindProperty]
    public List<DefaultInput> InputsDropdownElements { get; set; }

    [BindProperty]
    public ResultOutput Output { get; set; }

    static WorkflowTesting()
    {
        InitPresetInputs();
    }

    public WorkflowTesting(ILogger<PrivacyModel> logger)
    {
        _logger = logger;

        InputsDropdownElements = defaultInputs;

        if(InputsDropdownSelectedGuid != null && InputsDropdownSelectedGuid != Guid.Empty)
        {
            InputsDropdownSelected = InputsDropdownElements.FirstOrDefault(x => x.InputId == InputsDropdownSelectedGuid);
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

    public void OnPost()
    {
        DefaultInput? selectedInput = null;
        bool outcome = false;
        
        StringBuilder sb = new StringBuilder();
        
        if(InputsDropdownSelectedGuid != null && InputsDropdownSelectedGuid != Guid.Empty)
        {
            selectedInput = defaultInputs.FirstOrDefault(x => x.InputId == InputsDropdownSelectedGuid);
        }
        if(selectedInput != null)
        {
            InputItems =  JsonConvert.SerializeObject(selectedInput.CatalogItems);
            WorkflowRules = JsonConvert.SerializeObject(selectedInput.Rules);
            UsersInfo = JsonConvert.SerializeObject(selectedInput.UsersInfo);

            Workflow[] workflows = { new Workflow() { WorkflowName = selectedInput.InputDisplayText, RuleExpressionType= RuleExpressionType.LambdaExpression } };
            workflows[0].Rules = selectedInput.Rules;
            var rulesEngine = new RulesEngine.RulesEngine(workflows, null);
            foreach(var user in selectedInput.UsersInfo)
            {
                foreach(var item in selectedInput.CatalogItems)
                {
                    List<RuleResultTree> resultList = rulesEngine.ExecuteAllRulesAsync(
                        selectedInput.InputDisplayText,
                        item, user, selectedInput.DiscontCodes).Result;
                    
                    outcome = resultList.TrueForAll(r => r.IsSuccess);

                    sb.AppendLine($"Rule Workflow '{selectedInput.InputDisplayText}'<br/>");
                    sb.AppendLine($"'{user.Username}' rules applied to {item.Title}({item.ItemID}) is {(outcome ? "<strong style=\"color: green\">Success</strong>" : "<strong style=\"color: red\">Fail</strong>")}<br/>");
                    sb.AppendLine($"Rules Result: <br/>");
                    resultList.ForEach(r => { 
                        sb.AppendLine($"Rule '{r.Rule.RuleName}' is {(r.IsSuccess ? "<strong style=\"color: green\">Success</strong>" : "<strong style=\"color: red\">Fail</strong>")}<br/>");
                        if(!r.IsSuccess)
                        {
                            sb.AppendLine($"Error Message: {r.ExceptionMessage}<br/>");
                        }
                        sb.AppendLine($"<br/>");
                    });
                }
            }
        }else
        {
            Output = new ResultOutput() { isError = true, ErrorMessage = "Please select input and rules" };
            return;
        }

        InputsDropdownSelected = selectedInput;
        InputsDropdownElements = defaultInputs;

        Output = new ResultOutput() { OutputText = sb.ToString() };
    }

    
    private static void InitPresetInputs()
    {
        defaultInputs = new List<DefaultInput>();

        // Discount for category (item in category + item not in category)
        #region Discount for category (item in category + item not in category)
        DefaultInput input = new DefaultInput() { 
            InputDisplayText = "Single Product base on Category", 
            InputId = Guid.NewGuid(), 
            CatalogItems = new List<CatalogItem>() { 
                new CatalogItem() { 
                    ItemID = "1", 
                    Price = 10.5, 
                    Category = "Infantil", 
                    CategoryPath = "Canais\\Infantil", 
                    Title = "Disney Channel", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    },
                new CatalogItem() { 
                    ItemID = "2", 
                    Price = 10.5, 
                    Category = "Desporto", 
                    CategoryPath = "Canais\\Desporto", 
                    Title = "Sport TV 1", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Desporto", "Subscription", "LiveChannel" } 
                    }, 
                }, 
             Rules = new List<Rule>() {
                new Rule() { 
                    RuleName = "Discount for Category Desporto",
                     SuccessEvent = "20%",
                     ErrorMessage = "Item can't be discounted",
                     Expression = "input1.Category == \"Desporto\"",

                     RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                },
             UsersInfo = new List<UserInfo>() { 
                new UserInfo() { 
                    Username = "user1", 
                    IsCatalogItemSubscribed = false, 
                    IsCatalgItemPreviousSubscribed = false, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = null, 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-30), 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" } 
                    } 
                }
            };

        defaultInputs.Add(input);
        #endregion
        // Discount for Category and SubCategories (Items in category and not + items in subcategory and not)
        #region Discount for Category and SubCategories (Items in category and not + items in subcategory and not)
        DefaultInput input2 = new DefaultInput() { 
            InputDisplayText = "Products discount based on Category and Subcategory", 
            InputId = Guid.NewGuid(), 
            CatalogItems = new List<CatalogItem>() { 
                new CatalogItem() { 
                    ItemID = "1", 
                    Price = 10.5, 
                    Category = "Infantil", 
                    CategoryPath = "Canais\\Infantil", 
                    Title = "Disney Channel", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    },
                
                new CatalogItem() { 
                    ItemID = "2", 
                    Price = 10.5, 
                    Category = "SportTV", 
                    CategoryPath = "Canais\\Desporto\\SportTV", 
                    Title = "Sport TV 1", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Desporto", "Subscription", "LiveChannel" } 
                    }, 
                
                new CatalogItem() { 
                    ItemID = "3", 
                    Price = 10.5, 
                    Category = "BenficaTV", 
                    CategoryPath = "Canais\\Desporto\\BenficaTV", 
                    Title = "Benfica TV 1", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Desporto", "Subscription", "LiveChannel" } 
                    }, 
                }, 
             Rules = new List<Rule>() {
                new Rule() { 
                    RuleName = "Discount for Category Desporto for all subcategories",
                     SuccessEvent = "20%",
                     ErrorMessage = "Item can't be discounted",
                     Expression = "input1.CategoryPath.StartsWith(\"Canais\\\\Desporto\\\\\")",

                     RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                },
             UsersInfo = new List<UserInfo>() { 
                new UserInfo() { 
                    Username = "user1", 
                    IsCatalogItemSubscribed = false, 
                    IsCatalgItemPreviousSubscribed = false, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = null, 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-30), 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" } 
                    } 
                }
            };
        
        defaultInputs.Add(input2);

        #endregion
        // Discount for multiple items (List of items and specific items discount based on IDs)
        #region Discount for multiple items (List of items and specific items discount based on IDs)
        DefaultInput input3 = new DefaultInput() { 
            InputDisplayText = "Multiple Products discount based on product IDs", 
            InputId = Guid.NewGuid(), 
            CatalogItems = new List<CatalogItem>() { 
                new CatalogItem() { 
                    ItemID = "1", 
                    Price = 10.5, 
                    Category = "Infantil", 
                    CategoryPath = "Canais\\Infantil", 
                    Title = "Disney Channel", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    },
                
                new CatalogItem() { 
                    ItemID = "2", 
                    Price = 10.5, 
                    Category = "SportTV", 
                    CategoryPath = "Canais\\Desporto\\SportTV", 
                    Title = "Sport TV 1", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Desporto", "Subscription", "LiveChannel" } 
                    }, 
                
                new CatalogItem() { 
                    ItemID = "3", 
                    Price = 10.5, 
                    Category = "BenficaTV", 
                    CategoryPath = "Canais\\Desporto\\BenficaTV", 
                    Title = "Benfica TV 1", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Desporto", "Subscription", "LiveChannel" } 
                    }, 
                }, 
             Rules = new List<Rule>() {
                new Rule() { 
                    RuleName = "Discount for Category Desporto for all subcategories",
                     SuccessEvent = "20%",
                     ErrorMessage = "Item can't be discounted",
                     Expression = "input1.itemID == \"1\" || input1.itemID == \"3\"",

                     RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                },
             UsersInfo = new List<UserInfo>() { 
                new UserInfo() { 
                    Username = "user1", 
                    IsCatalogItemSubscribed = false, 
                    IsCatalgItemPreviousSubscribed = false, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = null, 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-30), 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" } 
                    } 
                }
            };
        
        defaultInputs.Add(input3);


        #endregion
        
        // Discount for multiple items (List of items and specific items discount based on Tags)
        #region Discount for multiple items (List of items and specific items discount based on Tags)
        DefaultInput input4 = new DefaultInput() { 
            InputDisplayText = "Multiple Products discount based on product Tags", 
            InputId = Guid.NewGuid(), 
            CatalogItems = new List<CatalogItem>() { 
                new CatalogItem() { 
                    ItemID = "1", 
                    Price = 10.5, 
                    Category = "Infantil", 
                    CategoryPath = "Canais\\Infantil", 
                    Title = "Disney Channel", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    },
                
                new CatalogItem() { 
                    ItemID = "2", 
                    Price = 10.5, 
                    Category = "SportTV", 
                    CategoryPath = "Canais\\Desporto\\SportTV", 
                    Title = "Sport TV 1", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Desporto", "Subscription", "LiveChannel" } 
                    }, 
                
                new CatalogItem() { 
                    ItemID = "3", 
                    Price = 10.5, 
                    Category = "BenficaTV", 
                    CategoryPath = "Canais\\Desporto\\BenficaTV", 
                    Title = "Benfica TV 1", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Desporto", "Subscription", "LiveChannel" } 
                    }, 
                }, 
             Rules = new List<Rule>() {
                new Rule() { 
                    RuleName = "Discount for Category Desporto for all subcategories",
                     SuccessEvent = "20%",
                     ErrorMessage = "Item can't be discounted",
                     Expression = "input1.Tags.Contains(\"Desporto\")",

                     RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                },
             UsersInfo = new List<UserInfo>() { 
                new UserInfo() { 
                    Username = "user1", 
                    IsCatalogItemSubscribed = false, 
                    IsCatalgItemPreviousSubscribed = false, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = null, 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-30), 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" } 
                    } 
                }
            };
        
        defaultInputs.Add(input4);
        #endregion
        
        // Discount item based on user previous purchase
        #region Discount item based on user previous purchase (Positive and Negative)
        DefaultInput input5 = new DefaultInput() { 
            InputDisplayText = "Discount item based on user previous purchase (less than 30 days))", 
            InputId = Guid.NewGuid(), 
            CatalogItems = new List<CatalogItem>() { 
                new CatalogItem() { 
                    ItemID = "1", 
                    Price = 10.5, 
                    Category = "Infantil", 
                    CategoryPath = "Canais\\Infantil", 
                    Title = "Disney Channel", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    },
                    
                new CatalogItem() { 
                    ItemID = "2", 
                    Price = 10.5, 
                    Category = "SportTV", 
                    CategoryPath = "Canais\\Desporto\\SportTV", 
                    Title = "Sport TV 1", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Desporto", "Subscription", "LiveChannel" } 
                    }
                }, 
             Rules = new List<Rule>() {
                new Rule() { 
                    RuleName = "Discount for Purchase SportTV in the past (no date limit)",
                     SuccessEvent = "20%",
                     ErrorMessage = "Item can't be discounted",
                     Expression = "input1.ItemID == \"2\" && input2.IsCatalogItemSubscribed == false && input2.IsCatalgItemPreviousSubscribed == true",

                     RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                },
             UsersInfo = new List<UserInfo>() { 
                new UserInfo() { 
                    Username = "user1", 
                    IsCatalogItemSubscribed = false, 
                    IsCatalgItemPreviousSubscribed = true, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = DateTime.Now.AddDays(-20), 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-30), 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" } 
                    },
                new UserInfo() { 
                    Username = "user2", 
                    IsCatalogItemSubscribed = false, 
                    IsCatalgItemPreviousSubscribed = false, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = null, 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-30), 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" } 
                    }
                }
            };
        
        defaultInputs.Add(input5);
        #endregion

        // Discount item based on user previous purchase based on end subscription date (Positive and Negative)
        #region Discount item based on user previous purchase based on end subscription date (Positive and Negative)
        DefaultInput input6 = new DefaultInput() { 
            InputDisplayText = "Discount item based on user previous purchase (less than 30 days))", 
            InputId = Guid.NewGuid(), 
            CatalogItems = new List<CatalogItem>() { 
                new CatalogItem() { 
                    ItemID = "1", 
                    Price = 10.5, 
                    Category = "Infantil", 
                    CategoryPath = "Canais\\Infantil", 
                    Title = "Disney Channel", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    },
                    
                new CatalogItem() { 
                    ItemID = "2", 
                    Price = 10.5, 
                    Category = "SportTV", 
                    CategoryPath = "Canais\\Desporto\\SportTV", 
                    Title = "Sport TV 1", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Desporto", "Subscription", "LiveChannel" } 
                    }
                }, 
             Rules = new List<Rule>() {
                new Rule() { 
                    RuleName = "Discount for Purchase SportTV less than 30 days",
                     SuccessEvent = "20%",
                     ErrorMessage = "Item can't be discounted",
                     Expression = "input1.ItemID == \"2\" && input2.IsCatalogItemSubscribed == false && input2.IsCatalgItemPreviousSubscribed == true && input2.CatalogItemEndSubscriptionDate < DateTime.Now.AddDays(-30)",

                     RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                },
             UsersInfo = new List<UserInfo>() { 
                new UserInfo() { 
                    Username = "user1", 
                    IsCatalogItemSubscribed = false, 
                    IsCatalgItemPreviousSubscribed = true, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = DateTime.Now.AddDays(-20), 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-30), 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" } 
                    },
                new UserInfo() { 
                    Username = "user2", 
                    IsCatalogItemSubscribed = false, 
                    IsCatalgItemPreviousSubscribed = true, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = DateTime.Now.AddDays(-40), 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-30), 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" } 
                    }
                }
            };
        
        defaultInputs.Add(input6);
        #endregion
        
        // Discount item based on user previous purchase based on subscription near end (retention)
        #region Discount item based on user previous purchase based on subscription near end (retention)
        DefaultInput input7 = new DefaultInput() { 
            InputDisplayText = "Discount item based on user previous purchase (less than 30 days))", 
            InputId = Guid.NewGuid(), 
            CatalogItems = new List<CatalogItem>() { 
                new CatalogItem() { 
                    ItemID = "1", 
                    Price = 10.5, 
                    Category = "Infantil", 
                    CategoryPath = "Canais\\Infantil", 
                    Title = "Disney Channel", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    },
                    
                new CatalogItem() { 
                    ItemID = "2", 
                    Price = 10.5, 
                    Category = "SportTV", 
                    CategoryPath = "Canais\\Desporto\\SportTV", 
                    Title = "Sport TV 1", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Desporto", "Subscription", "LiveChannel" } 
                    }
                }, 
             Rules = new List<Rule>() {
                new Rule() { 
                    RuleName = "Discount for Retension SportTV end subscription in less than 30 days",
                     SuccessEvent = "20%",
                     ErrorMessage = "Item can't be discounted",
                     Expression = "input1.ItemID == \"2\" && input2.IsCatalogItemSubscribed == true && input2.CatalogItemEndSubscriptionDate < DateTime.Now.AddDays(30)",

                     RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                },
             UsersInfo = new List<UserInfo>() { 
                new UserInfo() { 
                    Username = "user1", 
                    IsCatalogItemSubscribed = true, 
                    IsCatalgItemPreviousSubscribed = true, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = DateTime.Now.AddDays(30), 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-30), 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" } 
                    }, 
                new UserInfo() { 
                    Username = "user2", 
                    IsCatalogItemSubscribed = true, 
                    IsCatalgItemPreviousSubscribed = true, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = DateTime.Now.AddDays(60), 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-30), 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" } 
                    },
                new UserInfo() { 
                    Username = "user3", 
                    IsCatalogItemSubscribed = false, 
                    IsCatalgItemPreviousSubscribed = true, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = null, 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-30), 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" } 
                    }
                }
            };
        
        defaultInputs.Add(input7);
        #endregion

        // Discount item based on user recent join (Positive)
        #region Discount item based on user recent join (Positive)
        DefaultInput input8 = new DefaultInput() { 
            InputDisplayText = "Discount all items for user if joined at lest in the last month (30 days)", 
            InputId = Guid.NewGuid(), 
            CatalogItems = new List<CatalogItem>() { 
                new CatalogItem() { 
                    ItemID = "1", 
                    Price = 10.5, 
                    Category = "Infantil", 
                    CategoryPath = "Canais\\Infantil", 
                    Title = "Disney Channel", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    },
                    
                new CatalogItem() { 
                    ItemID = "2", 
                    Price = 10.5, 
                    Category = "SportTV", 
                    CategoryPath = "Canais\\Desporto\\SportTV", 
                    Title = "Sport TV 1", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Desporto", "Subscription", "LiveChannel" } 
                    }
                }, 
             Rules = new List<Rule>() {
                new Rule() { 
                    RuleName = "Discount Recent User (less than 30 days)",
                     SuccessEvent = "20%",
                     ErrorMessage = "Item can't be discounted",
                     Expression = "input2.UserSinceDate > DateTime.Now.AddDays(-30)",

                     RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                },
             UsersInfo = new List<UserInfo>() { 
                new UserInfo() { 
                    Username = "user1", 
                    IsCatalogItemSubscribed = true, 
                    IsCatalgItemPreviousSubscribed = true, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = DateTime.Now.AddDays(20), 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-20), 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" } 
                    }, 
                new UserInfo() { 
                    Username = "user2", 
                    IsCatalogItemSubscribed = true, 
                    IsCatalgItemPreviousSubscribed = true, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = DateTime.Now.AddDays(60), 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-60), 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" } 
                    }
                }
            };
        
        defaultInputs.Add(input8);
        #endregion

        // Discount item based on discount code
        #region Discount item based on discount code
        DefaultInput input9 = new DefaultInput() { 
            InputDisplayText = "Discount all items is a valid Discount Code is provided", 
            InputId = Guid.NewGuid(), 
            CatalogItems = new List<CatalogItem>() { 
                new CatalogItem() { 
                    ItemID = "1", 
                    Price = 10.5, 
                    Category = "Infantil", 
                    CategoryPath = "Canais\\Infantil", 
                    Title = "Disney Channel", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    },
                    
                new CatalogItem() { 
                    ItemID = "2", 
                    Price = 10.5, 
                    Category = "SportTV", 
                    CategoryPath = "Canais\\Desporto\\SportTV", 
                    Title = "Sport TV 1", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Desporto", "Subscription", "LiveChannel" } 
                    }
                }, 
             Rules = new List<Rule>() {
                new Rule() { 
                    RuleName = "Discount for Selected Discount Code",
                     SuccessEvent = "20%",
                     ErrorMessage = "Item can't be discounted",
                     Expression = "input3.ActiveDiscountCodes.Contains(input3.SelectedDiscountCode)",
                     RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                },
             UsersInfo = new List<UserInfo>() { 
                new UserInfo() { 
                    Username = "user1", 
                    IsCatalogItemSubscribed = true, 
                    IsCatalgItemPreviousSubscribed = true, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = DateTime.Now.AddDays(30), 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-30)
                    }
                },
             DiscontCodes = new DiscountCodes() { 
                ActiveDiscountCodes = new List<string>() { "DISCOUNT1" },
                SelectedDiscountCode = "DISCOUNT1"
             }
        };
        
        defaultInputs.Add(input9);
        #endregion

        // Discount item based on discount code specific product
        #region Discount item based on discount code specific product
        DefaultInput input10 = new DefaultInput() { 
            InputDisplayText = "Discount code based on specific product", 
            InputId = Guid.NewGuid(), 
            CatalogItems = new List<CatalogItem>() { 
                new CatalogItem() { 
                    ItemID = "1", 
                    Price = 10.5, 
                    Category = "Infantil", 
                    CategoryPath = "Canais\\Infantil", 
                    Title = "Disney Channel", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    },
                    
                new CatalogItem() { 
                    ItemID = "2", 
                    Price = 10.5, 
                    Category = "SportTV", 
                    CategoryPath = "Canais\\Desporto\\SportTV", 
                    Title = "Sport TV 1", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    AssociatedDiscountCodes = new List<string>() { "DISCOUNT1" },
                    Tags = new List<string>() { "Desporto", "Subscription", "LiveChannel" } 
                    }
                }, 
             Rules = new List<Rule>() {
                new Rule() { 
                    RuleName = "Discount for Selected Discount Code for specific product",
                     SuccessEvent = "20%",
                     ErrorMessage = "Item can't be discounted",
                     Expression = "input3.ActiveDiscountCodes.Contains(input3.SelectedDiscountCode) && input1.AssociatedDiscountCodes.Contains(input3.SelectedDiscountCode)",
                     RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                },
             UsersInfo = new List<UserInfo>() { 
                new UserInfo() { 
                    Username = "user1", 
                    IsCatalogItemSubscribed = true, 
                    IsCatalgItemPreviousSubscribed = true, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = DateTime.Now.AddDays(30), 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-30)
                    }
                },
             DiscontCodes = new DiscountCodes() { 
                ActiveDiscountCodes = new List<string>() { "DISCOUNT1" },
                SelectedDiscountCode = "DISCOUNT1"
             }
        };
        
        defaultInputs.Add(input10);
        #endregion

        // Discount item based on discount code specific user
        #region Discount item based on discount code specific user
        DefaultInput input11 = new DefaultInput() { 
            InputDisplayText = "Discount all items for a user that has a Discount Code associated with him", 
            InputId = Guid.NewGuid(), 
            CatalogItems = new List<CatalogItem>() { 
                new CatalogItem() { 
                    ItemID = "1", 
                    Price = 10.5, 
                    Category = "Infantil", 
                    CategoryPath = "Canais\\Infantil", 
                    Title = "Disney Channel", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    },
                    
                new CatalogItem() { 
                    ItemID = "2", 
                    Price = 10.5, 
                    Category = "SportTV", 
                    CategoryPath = "Canais\\Desporto\\SportTV", 
                    Title = "Sport TV 1", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    AssociatedDiscountCodes = new List<string>() { "DISCOUNT1" },
                    Tags = new List<string>() { "Desporto", "Subscription", "LiveChannel" } 
                    }
                }, 
             Rules = new List<Rule>() {
                new Rule() { 
                    RuleName = "Discount for Selected Discount Code for specific product",
                     SuccessEvent = "20%",
                     ErrorMessage = "Item can't be discounted",
                     Expression = "input3.ActiveDiscountCodes.Contains(input3.SelectedDiscountCode) && input2.UserAssignedDiscountCodes.Contains(input3.SelectedDiscountCode)",
                     RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                },
             UsersInfo = new List<UserInfo>() { 
                new UserInfo() { 
                    Username = "user1", 
                    IsCatalogItemSubscribed = true, 
                    IsCatalgItemPreviousSubscribed = true, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = DateTime.Now.AddDays(30), 
                    CatalogItemLastPurchaseDate = null, 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT2" },
                    UserSinceDate = DateTime.Now.AddDays(-30)
                    },
                new UserInfo() { 
                    Username = "user2", 
                    IsCatalogItemSubscribed = true, 
                    IsCatalgItemPreviousSubscribed = true, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = DateTime.Now.AddDays(30), 
                    CatalogItemLastPurchaseDate = null, 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" },
                    UserSinceDate = DateTime.Now.AddDays(-30)
                    },
                new UserInfo() { 
                    Username = "user3", 
                    IsCatalogItemSubscribed = true, 
                    IsCatalgItemPreviousSubscribed = true, 
                    IsCatalogItemPreviousPurchase = false, 
                    CatalogItemEndSubscriptionDate = DateTime.Now.AddDays(30), 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(-30)
                    },
                },
             DiscontCodes = new DiscountCodes() { 
                ActiveDiscountCodes = new List<string>() { "DISCOUNT1" },
                SelectedDiscountCode = "DISCOUNT1"
             }
        };
        
        defaultInputs.Add(input11);
        #endregion
        
    }
}

