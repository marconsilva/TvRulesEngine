using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Newtonsoft.Json;
using RulesEngine.Extensions;
using RulesEngine.Models;
using sampleRulesEngine.Models;

namespace sampleRulesEngine.Pages;

public class WorkflowBenchmark : PageModel
{
    private readonly ILogger<PrivacyModel> _logger;
    private List<Workflow> Workflows { get; set; } 

    private static List<DefaultBenchmark> defaultBenchmarks = new List<DefaultBenchmark>();  

    [BindProperty]
    public List<BenchmarkDataPoint> BenchmarkDataPoints { get; set; }
    
    [BindProperty]
    public Guid? BenchmarksDropdownSelectedGuid { get; set; }
    [BindProperty]
    public DefaultBenchmark? BenchmarksDropdownSelected { get; set; }
    [BindProperty]
    public List<DefaultBenchmark> BenchmarksDropdownElements { get; set; }

    [BindProperty]
    public ResultOutput Output { get; set; }

    static WorkflowBenchmark()
    {
        InitPresetBenchmarks();
    }

    public WorkflowBenchmark(ILogger<PrivacyModel> logger)
    {
        _logger = logger;

        BenchmarksDropdownElements = defaultBenchmarks;

        if(BenchmarksDropdownSelectedGuid != null && BenchmarksDropdownSelectedGuid != Guid.Empty)
        {
            BenchmarksDropdownSelected = defaultBenchmarks.FirstOrDefault(x => x.BenchmarkId == BenchmarksDropdownSelectedGuid);
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
        DefaultBenchmark? selectedBenchmark = null;
        
        StringBuilder sb = new StringBuilder();
        BenchmarksDropdownElements = defaultBenchmarks;

        Output = new ResultOutput();

        if(BenchmarksDropdownSelectedGuid != null && BenchmarksDropdownSelectedGuid != Guid.Empty)
        {
            selectedBenchmark = defaultBenchmarks.FirstOrDefault(x => x.BenchmarkId == BenchmarksDropdownSelectedGuid);
        }

        if(selectedBenchmark != null)
        {
            Workflow[] workflows = { new Workflow() { WorkflowName = selectedBenchmark.BenchmarkTestDisplayText, 
                RuleExpressionType= RuleExpressionType.LambdaExpression } };
            workflows[0].Rules = selectedBenchmark.Rules;

            var rulesEngine = new RulesEngine.RulesEngine(workflows, null);
            
            List<TimeSpan> workItemElapsedTimes = new List<TimeSpan>();
            Stopwatch globalTimer = new Stopwatch();
            int workCounter = 0;
            int workId = 0;
            BenchmarkDataPoints = new List<BenchmarkDataPoint>();

            globalTimer.Start();
            if(selectedBenchmark.IsParallel)
            {                
                Parallel.ForEach(selectedBenchmark.UsersInfo, user =>
                {
                    Parallel.ForEach(selectedBenchmark.CatalogItems, item =>
                    {   
                        Stopwatch workTimer = new Stopwatch();
                        var localWorkId = workId++;
                        workTimer.Start();
                        selectedBenchmark.CatalogItems.Select(item =>
                            rulesEngine.ExecuteAllRulesAsync(
                                selectedBenchmark.BenchmarkTestDisplayText,
                                item, user, selectedBenchmark.DiscountCodes));
                        workTimer.Stop();
                        BenchmarkDataPoints.Add(new BenchmarkDataPoint() 
                        {
                             ElapsedMilliseconds =  globalTimer.ElapsedMilliseconds,
                             WorkCommissionedID = localWorkId,
                             WorkDone = workCounter++,
                        });
                        workItemElapsedTimes.Add(workTimer.Elapsed);
                    });
                });
            }else
            {
                foreach(var user in selectedBenchmark.UsersInfo)
                {
                    foreach(var item in selectedBenchmark.CatalogItems)
                    {
                        Stopwatch workTimer = new Stopwatch();
                        var localWorkId = workId++;
                        workTimer.Start();
                        List<RuleResultTree> resultList = rulesEngine.ExecuteAllRulesAsync(
                            selectedBenchmark.BenchmarkTestDisplayText,
                            item, user, selectedBenchmark.DiscountCodes).Result;
                        workTimer.Stop();
                        BenchmarkDataPoints.Add(new BenchmarkDataPoint() 
                        {
                             ElapsedMilliseconds =  globalTimer.ElapsedMilliseconds,
                             WorkCommissionedID = localWorkId,
                             WorkDone = workCounter++,
                        });;
                        workItemElapsedTimes.Add(workTimer.Elapsed);
                    }
                }
            }

            globalTimer.Stop();

            TimeSpan averageElapsedTime = new TimeSpan();
            foreach(var item in workItemElapsedTimes)
            {
                averageElapsedTime += item;
            }
            averageElapsedTime = new TimeSpan(averageElapsedTime.Ticks / workItemElapsedTimes.Count);

            sb.AppendLine($"Benchmark: {selectedBenchmark.BenchmarkTestDisplayText}<br/>");
            sb.AppendLine($"WorkDone: {workCounter} workItems<br/>");
            sb.AppendLine($"Total Elapsed Time: {globalTimer.Elapsed.ToString()} workItems<br/>");
            sb.AppendLine($"Average WorkItem Elapsed Time: {averageElapsedTime.ToString()}<br/>");
            sb.AppendLine($"Min WorkItem Elapsed Time: {workItemElapsedTimes.Min().ToString()}<br/>");
            sb.AppendLine($"Max WorkItem Elapsed Time: {workItemElapsedTimes.Max().ToString()}<br/>");

            Output.OutputText = sb.ToString();
        }
    }

    
    private static void InitPresetBenchmarks()
    {
        Random rand = new Random();

        defaultBenchmarks = new List<DefaultBenchmark>();

        // Benckmark 10k users (Sequential)
        #region Benckmark 10k users (Sequential)
        DefaultBenchmark benchmark1 = new DefaultBenchmark() { 
            BenchmarkTestDisplayText = "Benchmark 10k users (Sequential)", 
            IsParallel = false,
            BenchmarkId = Guid.NewGuid(),
            CatalogItems =  new List<CatalogItem>() {new CatalogItem() 
                {
                    ItemID = "1", 
                    Price = rand.NextDouble()*20, 
                    Category = "Infantil", 
                    CategoryPath = "Canais\\Infantil", 
                    Title = $"Disney Channel", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    
                }
            }, 
             Rules = new List<Rule>() {
                new Rule() { 
                    RuleName = "Discount for Category Desporto",
                     SuccessEvent = "20% Discount",
                     ErrorMessage = "Item can't be discounted",
                     Expression = "input2.UserSinceDate > DateTime.Now.AddDays(-20)",
                     RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                },
             UsersInfo = new List<UserInfo>()
            };

            for(int i = 0; i<10000; i++)
            {
                benchmark1.UsersInfo.Add(new UserInfo() { 
                    Username = $"user{i}", 
                    IsCatalogItemSubscribed = rand.NextDouble() > 0.5, 
                    IsCatalgItemPreviousSubscribed = rand.NextDouble() > 0.5, 
                    IsCatalogItemPreviousPurchase = rand.NextDouble() > 0.5,
                    CatalogItemEndSubscriptionDate = null, 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(rand.Next(-100, 1)), 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" } 
                    } 
                );
            }

            defaultBenchmarks.Add(benchmark1);
        #endregion

        // Benckmark 10k users (Parallel)
        #region Benckmark 10k users (Parallel)
        DefaultBenchmark benchmark2 = new DefaultBenchmark() { 
            BenchmarkTestDisplayText = "Benchmark 10k users (Parallel)", 
            IsParallel = true,
            BenchmarkId = Guid.NewGuid(),
            CatalogItems =  new List<CatalogItem>() {new CatalogItem() 
                {
                    ItemID = "1", 
                    Price = rand.NextDouble()*20, 
                    Category = "Infantil", 
                    CategoryPath = "Canais\\Infantil", 
                    Title = $"Disney Channel", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    
                }
            }, 
             Rules = new List<Rule>() {
                new Rule() { 
                    RuleName = "Discount for Category Desporto",
                     SuccessEvent = "20% Discount",
                     ErrorMessage = "Item can't be discounted",
                     Expression = "input2.UserSinceDate > DateTime.Now.AddDays(-20)",
                     RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                },
             UsersInfo = new List<UserInfo>()
            };

            for(int i = 0; i<10000; i++)
            {
                benchmark2.UsersInfo.Add(new UserInfo() { 
                    Username = $"user{i}", 
                    IsCatalogItemSubscribed = rand.NextDouble() > 0.5, 
                    IsCatalgItemPreviousSubscribed = rand.NextDouble() > 0.5, 
                    IsCatalogItemPreviousPurchase = rand.NextDouble() > 0.5,
                    CatalogItemEndSubscriptionDate = null, 
                    CatalogItemLastPurchaseDate = null, 
                    UserSinceDate = DateTime.Now.AddDays(rand.Next(-100, 1)), 
                    UserAssignedDiscountCodes = new List<string>() { "DISCOUNT1" } 
                    } 
                );
            }

            defaultBenchmarks.Add(benchmark2);
        #endregion

        // Benchmark 40K Catalog Items (Sequential)
        #region Benchmark 40K Catalog Items (Sequential)
        DefaultBenchmark benchmark3 = new DefaultBenchmark() { 
            BenchmarkTestDisplayText = "Benchmark 40k Catalog Items (Sequential)", 
            IsParallel = false,
            BenchmarkId = Guid.NewGuid(),
            CatalogItems =  new List<CatalogItem>(), 
             Rules = new List<Rule>() {
                new Rule() { 
                    RuleName = "Discount for Category Desporto",
                     SuccessEvent = "20% Discount",
                     ErrorMessage = "Item can't be discounted",
                     Expression = "input1.Price < 10",
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

            for(int i = 0; i<40000; i++)
            {
                benchmark3.CatalogItems.Add(new CatalogItem() 
                {
                    ItemID = i.ToString(), 
                    Price = rand.NextDouble()*20, 
                    Category = "Infantil", 
                    CategoryPath = "Canais\\Infantil", 
                    Title = $"Disney Channel {i}", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    
                });
            }

            defaultBenchmarks.Add(benchmark3);
        #endregion
        
        // Benchmark 40K Catalog Items (Parallel)
        #region Benchmark 40K Catalog Items (Parallel)
        DefaultBenchmark benchmark4 = new DefaultBenchmark() { 
            BenchmarkTestDisplayText = "Benchmark 40k Catalog Items (Parallel)", 
            IsParallel = true,
            BenchmarkId = Guid.NewGuid(),
            CatalogItems =  new List<CatalogItem>(), 
             Rules = new List<Rule>() {
                new Rule() { 
                    RuleName = "Discount for Category Desporto",
                     SuccessEvent = "20% Discount",
                     ErrorMessage = "Item can't be discounted",
                     Expression = "input1.Price < 10",
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

            for(int i = 0; i<40000; i++)
            {
                benchmark4.CatalogItems.Add(new CatalogItem() 
                {
                    ItemID = i.ToString(), 
                    Price = rand.NextDouble()*20, 
                    Category = "Infantil", 
                    CategoryPath = "Canais\\Infantil", 
                    Title = $"Disney Channel {i}", 
                    IsSubscription = true, 
                    IsBundle = false, 
                    IsDiscountable=true, 
                    Tags = new List<string>() { "Infantil", "Subscription", "LiveChannel" } 
                    
                });
            }

            defaultBenchmarks.Add(benchmark4);
        #endregion

        // Benchmark 10K Active Rules
        #region Benchmark 10K Active Rules
        #endregion

        // Benchmark 10K Active Rules for 10K Users (Sequencial)
        #region Benchmark 10K Active Rules for 10K Users (Sequencial)
        #endregion

        // Benchmark 10K Active Rules for 10K Users (Paralel) 
        #region Benchmark 10K Active Rules for 10K Users (Paralel)
        #endregion
        
    }
}

