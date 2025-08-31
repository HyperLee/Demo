using Microsoft.AspNetCore.Mvc;
using Demo.Models;
using Demo.Services;
using System.Text.Json;

namespace Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmartCategoryController : ControllerBase
    {
        private readonly SmartCategoryService _smartCategoryService;
        private readonly CategoryLearningService _learningService;
        private readonly ILogger<SmartCategoryController> _logger;

        public SmartCategoryController(
            SmartCategoryService smartCategoryService,
            CategoryLearningService learningService,
            ILogger<SmartCategoryController> logger)
        {
            _smartCategoryService = smartCategoryService;
            _learningService = learningService;
            _logger = logger;
        }

        /// <summary>
        /// 取得智能分類建議
        /// </summary>
        [HttpPost("suggest")]
        public async Task<IActionResult> GetSuggestions([FromBody] SmartCategoryRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Description) && 
                    string.IsNullOrWhiteSpace(request.Merchant) && 
                    request.Amount <= 0)
                {
                    return Ok(new { suggestions = new List<CategorySuggestion>() });
                }

                var suggestions = await _smartCategoryService.SuggestCategoriesAsync(
                    request.Description ?? string.Empty,
                    request.Amount,
                    request.Merchant ?? string.Empty,
                    request.MaxSuggestions
                );

                _logger.LogInformation($"智能分類建議: {request.Description}, 找到 {suggestions.Count} 個建議");

                return Ok(new { 
                    success = true,
                    suggestions = suggestions 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得智能分類建議時發生錯誤");
                return Ok(new { 
                    success = false,
                    suggestions = new List<CategorySuggestion>(),
                    error = "取得分類建議時發生錯誤"
                });
            }
        }

        /// <summary>
        /// 提交用戶回饋
        /// </summary>
        [HttpPost("feedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] CategoryFeedback feedback)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(feedback.CategoryId))
                {
                    return BadRequest(new { 
                        success = false, 
                        error = "分類ID不能為空" 
                    });
                }

                // 設定時間戳
                feedback.Timestamp = DateTime.Now;
                feedback.UserId = "user1"; // 暫時設定固定用戶，實際應該從認證中取得

                await _learningService.LearnFromFeedbackAsync(feedback);

                _logger.LogInformation($"收到用戶回饋: 分類={feedback.CategoryId}, 正確={feedback.IsCorrect}");

                return Ok(new { 
                    success = true,
                    message = feedback.IsCorrect ? "感謝您的回饋！" : "感謝回饋，我們會改進分類準確度。"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理用戶回饋時發生錯誤");
                return Ok(new { 
                    success = false,
                    error = "處理回饋時發生錯誤"
                });
            }
        }

        /// <summary>
        /// 取得模型準確度報告
        /// </summary>
        [HttpGet("accuracy")]
        public async Task<IActionResult> GetAccuracyReport()
        {
            try
            {
                var report = await _learningService.EvaluateModelAccuracyAsync();
                
                return Ok(new { 
                    success = true,
                    report = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得準確度報告時發生錯誤");
                return Ok(new { 
                    success = false,
                    error = "取得報告時發生錯誤"
                });
            }
        }

        /// <summary>
        /// 手動觸發規則生成
        /// </summary>
        [HttpPost("generate-rules")]
        public async Task<IActionResult> GenerateRules()
        {
            try
            {
                await _learningService.GenerateRulesFromTrainingDataAsync();
                
                return Ok(new { 
                    success = true,
                    message = "規則生成完成"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成規則時發生錯誤");
                return Ok(new { 
                    success = false,
                    error = "生成規則時發生錯誤"
                });
            }
        }

        /// <summary>
        /// 取得分類統計資訊
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetCategoryStatistics()
        {
            try
            {
                // 讀取訓練資料進行統計
                var trainingDataPath = Path.Combine("App_Data", "category-training.json");
                
                var totalRecords = 0;
                var correctPredictions = 0;
                var categoryBreakdown = new Dictionary<string, int>();
                var recentActivity = new List<object>();

                if (System.IO.File.Exists(trainingDataPath))
                {
                    var json = await System.IO.File.ReadAllTextAsync(trainingDataPath);
                    var trainingData = JsonSerializer.Deserialize<List<CategoryTrainingData>>(json) ?? new List<CategoryTrainingData>();
                    
                    totalRecords = trainingData.Count;
                    correctPredictions = trainingData.Count(t => t.IsCorrect);
                    categoryBreakdown = trainingData.GroupBy(t => t.CategoryId).ToDictionary(g => g.Key, g => g.Count());
                    recentActivity = trainingData.TakeLast(10).Select(t => new 
                    {
                        Description = t.Description.Length > 30 ? t.Description.Substring(0, 30) + "..." : t.Description,
                        Category = t.CategoryId,
                        IsCorrect = t.IsCorrect,
                        Date = t.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                    }).Cast<object>().ToList();
                }

                var statistics = new
                {
                    TotalRecords = totalRecords,
                    CorrectPredictions = correctPredictions,
                    CategoryBreakdown = categoryBreakdown,
                    RecentActivity = recentActivity
                };
                
                return Ok(new { 
                    success = true,
                    statistics = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得統計資訊時發生錯誤");
                return Ok(new { 
                    success = false,
                    error = "取得統計資訊時發生錯誤"
                });
            }
        }
    }
}
