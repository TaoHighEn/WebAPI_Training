using System.Linq;
using WebSite.Models.WebSiteDB;

public class SiteService
{
    private readonly WebsiteDBContext _webSiteDBContext;

    public SiteService(WebsiteDBContext webSiteDBContext)
    {
        _webSiteDBContext = webSiteDBContext;
    }

    /// <summary>
    /// 取得語言設定
    /// </summary>
    /// <returns></returns>
    public string[] GetCultures()
    {
        return _webSiteDBContext.Language
                .Where(s => s.IsEnabled == 1)
                .OrderBy(s => s.Seq)
                .Select(s => s.ID)
                .ToArray();
    }
}