using Demo.Services;
using Demo.Hubs;

namespace Demo;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddControllers(); // 添加控制器支援
        
        // 註冊 SignalR
        builder.Services.AddSignalR();
        
        builder.Services.AddSingleton<INoteService, JsonNoteService>();
        builder.Services.AddSingleton<IEnhancedMemoNoteService, JsonMemoNoteService>();
        builder.Services.AddSingleton<IMemoNoteService>(provider => 
            provider.GetRequiredService<IEnhancedMemoNoteService>());
        
        // 註冊匯率服務
        builder.Services.AddHttpClient<ExchangeRateService>();
        builder.Services.AddScoped<ExchangeRateService>();
        
        // 註冊會計服務
        builder.Services.AddScoped<IAccountingService, AccountingService>();
        
        // 註冊語音解析 Phase 2 服務
        builder.Services.AddScoped<FieldConfidenceCalculator>();
        builder.Services.AddScoped<ProgressiveParseManager>();
        
        // 註冊家庭共享服務
        builder.Services.AddScoped<IFamilyService, FamilyService>();
        builder.Services.AddScoped<ISharedAccountingService, SharedAccountingService>();
        
        // 註冊 AI 分析服務
        builder.Services.AddScoped<AnomalyDetectionService>();
        builder.Services.AddScoped<BudgetManagementService>();
        builder.Services.AddScoped<FinancialInsightsService>();
        builder.Services.AddScoped<PredictiveAnalysisService>();
        
        // 註冊統計服務
        builder.Services.AddScoped<IStatisticsService, StatisticsService>();
        builder.Services.AddScoped<IStatisticsExportService, StatisticsExportService>();
        
        // 註冊待辦清單服務
        builder.Services.AddSingleton<TodoService>();

        // 註冊習慣追蹤服務
        builder.Services.AddSingleton<HabitService>();

        // 註冊智能分類相關服務
        builder.Services.AddSingleton<TextAnalysisService>();
        builder.Services.AddScoped<SmartCategoryService>();
        builder.Services.AddScoped<CategoryLearningService>();

        // 註冊匯出相關服務
        builder.Services.AddScoped<PdfExportService>();
        builder.Services.AddScoped<ExcelExportService>();
        builder.Services.AddScoped<CsvExportService>();
        builder.Services.AddScoped<ExportService>();

        // 註冊投資相關服務
        builder.Services.AddHttpClient<StockPriceService>();
        builder.Services.AddScoped<StockPriceService>();
        builder.Services.AddScoped<InvestmentService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers(); // 添加控制器路由
        app.MapHub<FamilyHub>("/familyHub"); // 註冊 SignalR Hub

        app.Run();
    }
}
