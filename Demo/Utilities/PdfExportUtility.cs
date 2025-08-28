using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using iText.IO.Font;
using iText.Kernel.Pdf;
using Microsoft.Extensions.Logging;

namespace Demo.Utilities
{
    /// <summary>
    /// PDF 匯出工具類別
    /// 專門處理中文字型支援的 PDF 產生
    /// </summary>
    public static class PdfExportUtility
    {
        /// <summary>
        /// 將 HTML 轉換為 PDF，支援中文字元
        /// </summary>
        /// <param name="htmlContent">HTML 內容</param>
        /// <param name="logger">記錄器</param>
        /// <returns>PDF 位元組陣列</returns>
        public static byte[] ConvertHtmlToPdfWithChineseSupport(string htmlContent, ILogger logger)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                using (var htmlStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent)))
                {
                    var converterProperties = new ConverterProperties();
                    
                    // 設定編碼
                    converterProperties.SetCharset("UTF-8");
                    
                    // 建立字型提供者
                    var fontProvider = CreateChineseFontProvider(logger);
                    converterProperties.SetFontProvider(fontProvider);
                    
                    // 設定基礎 URI
                    converterProperties.SetBaseUri("");
                    
                    // 轉換為 PDF
                    HtmlConverter.ConvertToPdf(htmlStream, memoryStream, converterProperties);
                    
                    var pdfBytes = memoryStream.ToArray();
                    
                    if (pdfBytes.Length == 0)
                    {
                        throw new InvalidOperationException("PDF 檔案大小為 0，轉換失敗");
                    }
                    
                    logger?.LogInformation("PDF 轉換成功，檔案大小: {Size} bytes", pdfBytes.Length);
                    return pdfBytes;
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "HTML 轉 PDF 時發生錯誤");
                throw;
            }
        }
        
        /// <summary>
        /// 建立支援中文的字型提供者
        /// </summary>
        /// <param name="logger">記錄器</param>
        /// <returns>字型提供者</returns>
        private static DefaultFontProvider CreateChineseFontProvider(ILogger logger)
        {
            var fontProvider = new DefaultFontProvider(true, true, true);
            
            try
            {
                // 嘗試新增系統中文字型
                var chineseFonts = new[]
                {
                    @"C:\Windows\Fonts\msjh.ttc",      // Microsoft JhengHei (微軟正黑體)
                    @"C:\Windows\Fonts\msyh.ttc",      // Microsoft YaHei (微軟雅黑)
                    @"C:\Windows\Fonts\simsun.ttc",    // SimSun (宋體)
                    @"C:\Windows\Fonts\mingliu.ttc",   // MingLiU (細明體)
                    @"C:\Windows\Fonts\kaiu.ttf",      // DFKai-SB (標楷體)
                };
                
                foreach (var fontPath in chineseFonts)
                {
                    if (File.Exists(fontPath))
                    {
                        try
                        {
                            fontProvider.AddFont(fontPath);
                            logger?.LogDebug("成功載入字型: {FontPath}", fontPath);
                        }
                        catch (Exception ex)
                        {
                            logger?.LogWarning(ex, "無法載入字型: {FontPath}", fontPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "載入中文字型時發生錯誤，將使用預設字型");
            }
            
            return fontProvider;
        }
        
        /// <summary>
        /// 產生支援中文的 HTML 樣式
        /// </summary>
        /// <returns>HTML 樣式字串</returns>
        public static string GetChineseSupportedCss()
        {
            return @"
                body { 
                    font-family: 'Microsoft JhengHei', 'Microsoft YaHei', 'SimSun', 'MingLiU', 'DFKai-SB', 
                                 'Noto Sans CJK TC', 'Source Han Sans TC', 'PingFang TC', 
                                 'Hiragino Sans GB', Arial, sans-serif; 
                    margin: 40px; 
                    line-height: 1.6;
                    font-size: 14px;
                }
                .header { 
                    text-align: center; 
                    margin-bottom: 30px; 
                    font-family: 'Microsoft JhengHei', 'Microsoft YaHei', 'SimSun', Arial, sans-serif;
                }
                .note { 
                    margin-bottom: 30px; 
                    border-bottom: 1px solid #ccc; 
                    padding-bottom: 20px; 
                    page-break-inside: avoid;
                }
                .note-title { 
                    font-size: 18px; 
                    font-weight: bold; 
                    color: #333; 
                    margin-bottom: 10px;
                    font-family: 'Microsoft JhengHei', 'Microsoft YaHei', 'SimSun', Arial, sans-serif;
                }
                .note-content { 
                    margin: 10px 0; 
                    line-height: 1.6; 
                    font-family: 'Microsoft JhengHei', 'Microsoft YaHei', 'SimSun', Arial, sans-serif;
                }
                .note-meta { 
                    font-size: 12px; 
                    color: #666; 
                    font-family: 'Microsoft JhengHei', 'Microsoft YaHei', 'SimSun', Arial, sans-serif;
                }
                .tags { 
                    margin: 5px 0; 
                }
                .tag { 
                    background: #007bff; 
                    color: white; 
                    padding: 2px 6px; 
                    border-radius: 3px; 
                    margin-right: 5px; 
                    font-size: 12px;
                    font-family: 'Microsoft JhengHei', 'Microsoft YaHei', 'SimSun', Arial, sans-serif;
                }
                h1, h2, h3, h4, h5, h6 {
                    font-family: 'Microsoft JhengHei', 'Microsoft YaHei', 'SimSun', Arial, sans-serif;
                }
                @page {
                    size: A4;
                    margin: 2cm;
                }";
        }
    }
}
