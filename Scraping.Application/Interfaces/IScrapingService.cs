using Scraping.Application.ViewModel;
using Scraping.Core.Common.ResponseBuilder;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scraping.Application.Interfaces
{
    public interface IScrapingService
    {
         Task<ApiResponse<IEnumerable<ScrapingViewModel>>> GetStatistic(string url);
    }
}
