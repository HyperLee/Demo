using Demo.Services;

namespace Demo;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddSingleton<INoteService, JsonNoteService>();
        builder.Services.AddSingleton<IEnhancedMemoNoteService, JsonMemoNoteService>();
        builder.Services.AddSingleton<IMemoNoteService>(provider => 
            provider.GetRequiredService<IEnhancedMemoNoteService>());
        
        // 註冊匯率服務
        builder.Services.AddHttpClient<ExchangeRateService>();
        builder.Services.AddScoped<ExchangeRateService>();
        
        // 註冊會計服務
        builder.Services.AddScoped<IAccountingService, AccountingService>();
        
        // 註冊 AI 分析服務
        builder.Services.AddScoped<AnomalyDetectionService>();
        builder.Services.AddScoped<BudgetManagementService>();
        builder.Services.AddScoped<FinancialInsightsService>();
        builder.Services.AddScoped<PredictiveAnalysisService>();
        
        // 註冊統計服務
        builder.Services.AddScoped<IStatisticsService, StatisticsService>();
        builder.Services.AddScoped<IStatisticsExportService, StatisticsExportService>();

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

        app.Run();
    }
}
