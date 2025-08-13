using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Demo.Pages
{
    /// <summary>
    /// 多時區電子時鐘頁面 Model
    /// </summary>
    public class index2Model : PageModel
    {
        /// <summary>
        /// 頁面載入時呼叫，僅供前端初始化。
        /// </summary>
        public void OnGet()
        {
            // 目前所有時區時間由前端計算
        }

        /// <summary>
        /// 預留：取得多時區時間 API，未來可由後端提供。
        /// </summary>
        /// <returns>世界各地時區時間資訊</returns>
        /// <example>
        /// <code>
        /// var result = OnGetWorldTimes();
        /// </code>
        /// </example>
        public JsonResult OnGetWorldTimes()
        {
            // TODO: 實作後端多時區時間取得
            return new JsonResult(new { error = "尚未實作" });
        }
    }
}
