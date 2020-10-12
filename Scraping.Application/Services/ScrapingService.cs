using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Scraping.Application.Interfaces;
using Scraping.Application.ViewModel;
using Scraping.Core.Common.Exceptions;
using Scraping.Core.Common.ResponseBuilder;
using Scraping.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Scraping.Application.Services
{
    public class ScrapingService : IScrapingService
    {
        private readonly IDistributedCache _distributedCache;
        public ScrapingService(IDistributedCache cache)
        {
            this._distributedCache = cache;
        }
        readonly string urlGitHub = "https://github.com/";
        public async Task<ApiResponse<IEnumerable<ScrapingViewModel>>> GetStatistic(string url)
        {

            ApiResponseBuilder<IEnumerable<ScrapingViewModel>> res = new ApiResponseBuilder<IEnumerable<ScrapingViewModel>>();

            List<GitFile> returnfiles;
            try
            {

                var result = _distributedCache.Get(url);
                if (result != null)
                {
                    var bytesAsString = Encoding.UTF8.GetString(result);
                    var deserializeObject = JsonConvert.DeserializeObject<ApiResponse<IEnumerable<ScrapingViewModel>>>(bytesAsString);
                    return deserializeObject;
                }


                if (!url.Contains(urlGitHub))
                {
                    throw new DomainException("The unexpected url. Please use "+this.urlGitHub);
                }
                returnfiles = await ProcessUrl(url);

                IEnumerable<ScrapingViewModel> listView = returnfiles.Where(x=>x!=null).GroupBy(d => d.Extension)
                .Select(
                    g => new ScrapingViewModel()
                    {
                        Extension = g.Key,
                        TotalBytes = g.Sum(s => s.TotalBytes),
                        TotalRows = g.Sum(s => s.TotalRows),
                        TotalFiles = g.Count()
                    });

                var scrap = res.Success(listView).Build();
                if (scrap.StatusCode == (int)HttpStatusCode.OK)
                {
                    string serializeObject = JsonConvert.SerializeObject(scrap);
                    byte[] data = Encoding.UTF8.GetBytes(serializeObject);
                    _distributedCache.Set(url, data, new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10)
                    });
                }


                return scrap;
            }
            catch (Exception ex)
            {
                return res.Error().DebugMessage(ex.Message).Build();
            }
        }

        private async Task<List<GitFile>> ProcessUrl(string url)
        {
            List<GitFile> returnfiles = new List<GitFile>();
            var doc = new HtmlDocument();
            using (WebClient web = new WebClient())
            {
                web.Encoding = System.Text.Encoding.GetEncoding("utf-8");
                string html = web.DownloadString(url);
                doc.LoadHtml(html);
            }


            var directory = doc.DocumentNode.SelectNodes("//svg[@aria-label='Directory']");
            var nodesFiles = doc.DocumentNode.SelectNodes("//svg[@aria-label='File']");
            if (nodesFiles != null)
            {
                returnfiles.AddRange(this.Files(nodesFiles));
            }

            if (directory != null)
            {
                foreach (var item in directory)
                {
                    var htmlDir = new HtmlDocument();
                    htmlDir.LoadHtml(item.ParentNode.ParentNode.InnerHtml);
                    var href = htmlDir.DocumentNode.SelectSingleNode("//a").GetAttributeValue("href", string.Empty);
                    if (!string.IsNullOrWhiteSpace(href))
                    {
                        var newUrl = this.urlGitHub + href;
                        returnfiles.AddRange(await ProcessUrl(newUrl));
                    }

                }
            }

            return returnfiles;
        }


        private IEnumerable<GitFile> Files(HtmlNodeCollection list)
        {
            List<GitFile> returnfiles = new List<GitFile>();

            foreach (var item in list)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(item.ParentNode.ParentNode.InnerHtml);
                var href = doc.DocumentNode.SelectSingleNode("//a").GetAttributeValue("href", string.Empty);
                returnfiles.Add(this.FileInfo(href));
            }
            return returnfiles;


        }

        private GitFile FileInfo(string url)
        {
            GitFile fileInfo = new GitFile();
            try
            {
                using (WebClient web = new WebClient())
                {
                    var newUrl = this.urlGitHub + url;
                    web.Encoding = Encoding.GetEncoding("utf-8");
                    string html = web.DownloadString(newUrl);

                    if (!html.Contains("lines"))
                    {
                        return null;
                    }

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var divInfo = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'text-mono')]");
                    if (divInfo != null && divInfo.InnerText.Contains("lines"))
                    {
                        var vetInfo = divInfo.InnerText.Trim().Replace("\n", "").Split("  ", StringSplitOptions.RemoveEmptyEntries);
                        fileInfo.TotalRows = Convert.ToInt32(vetInfo[0].Split(" ", StringSplitOptions.RemoveEmptyEntries)[0]);
                        fileInfo.TotalBytes = Convert.ToDecimal(vetInfo[1].Split(" ", StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", ","));
                        fileInfo.Extension = url.Split(".", StringSplitOptions.RemoveEmptyEntries).Last();
                        fileInfo.Name = url.Split("/", StringSplitOptions.RemoveEmptyEntries).Last();
                        fileInfo.Path = url.Replace("/" + fileInfo.Name, "");
                    }

                }

                return fileInfo;
            }
            catch (Exception ex)
            {
                throw new DomainException("error loading files " + url, ex);
            }
        }
    }
}
