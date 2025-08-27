using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Web;
using Demo.Services;

namespace Demo.Utilities
{
    /// <summary>
    /// 資料修復工具類別
    /// </summary>
    public class DataFixUtility
    {
        /// <summary>
        /// 修復 URL 編碼的標籤資料
        /// </summary>
        public static void FixUrlEncodedTagData()
        {
            try
            {
                var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
                var tagsFilePath = Path.Combine(appDataPath, "tags.json");
                var memoNotesFilePath = Path.Combine(appDataPath, "memo-notes.json");

                // 修復標籤檔案
                if (File.Exists(tagsFilePath))
                {
                    var tagsJson = File.ReadAllText(tagsFilePath);
                    var tags = JsonSerializer.Deserialize<List<Tag>>(tagsJson);
                    
                    if (tags != null)
                    {
                        bool hasChanges = false;
                        foreach (var tag in tags)
                        {
                            // 解碼標籤名稱
                            var decodedName = HttpUtility.UrlDecode(tag.Name);
                            if (decodedName != tag.Name)
                            {
                                Console.WriteLine($"修復標籤名稱: '{tag.Name}' -> '{decodedName}'");
                                tag.Name = decodedName;
                                hasChanges = true;
                            }

                            // 解碼顏色
                            var decodedColor = HttpUtility.UrlDecode(tag.Color);
                            if (decodedColor != tag.Color)
                            {
                                Console.WriteLine($"修復標籤顏色: '{tag.Color}' -> '{decodedColor}'");
                                tag.Color = decodedColor;
                                hasChanges = true;
                            }
                        }

                        if (hasChanges)
                        {
                            var updatedJson = JsonSerializer.Serialize(tags, new JsonSerializerOptions
                            {
                                WriteIndented = true,
                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                            });
                            
                            File.WriteAllText(tagsFilePath, updatedJson);
                            Console.WriteLine("標籤檔案修復完成！");
                        }
                    }
                }

                // 修復備忘錄檔案中的標籤資料
                if (File.Exists(memoNotesFilePath))
                {
                    var notesJson = File.ReadAllText(memoNotesFilePath);
                    var notes = JsonSerializer.Deserialize<List<Note>>(notesJson);
                    
                    if (notes != null)
                    {
                        bool hasChanges = false;
                        foreach (var note in notes)
                        {
                            foreach (var tag in note.Tags)
                            {
                                // 解碼標籤名稱
                                var decodedName = HttpUtility.UrlDecode(tag.Name);
                                if (decodedName != tag.Name)
                                {
                                    Console.WriteLine($"修復備忘錄 {note.Id} 中的標籤名稱: '{tag.Name}' -> '{decodedName}'");
                                    tag.Name = decodedName;
                                    hasChanges = true;
                                }

                                // 解碼顏色
                                var decodedColor = HttpUtility.UrlDecode(tag.Color);
                                if (decodedColor != tag.Color)
                                {
                                    Console.WriteLine($"修復備忘錄 {note.Id} 中的標籤顏色: '{tag.Color}' -> '{decodedColor}'");
                                    tag.Color = decodedColor;
                                    hasChanges = true;
                                }
                            }
                        }

                        if (hasChanges)
                        {
                            var updatedJson = JsonSerializer.Serialize(notes, new JsonSerializerOptions
                            {
                                WriteIndented = true,
                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                            });
                            
                            File.WriteAllText(memoNotesFilePath, updatedJson);
                            Console.WriteLine("備忘錄檔案修復完成！");
                        }
                    }
                }

                Console.WriteLine("資料修復完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"資料修復時發生錯誤: {ex.Message}");
            }
        }
    }
}
