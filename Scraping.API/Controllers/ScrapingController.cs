using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Scraping.Application.Interfaces;
using Scraping.Application.ViewModel;
using Scraping.Core.Common.ResponseBuilder;

namespace Scraping.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScrapingController : Controller
    {
        private readonly IScrapingService _ScrapingService;

        private readonly IDistributedCache _distributedCache;

        public ScrapingController(IScrapingService scrapingService, IDistributedCache distributedCache)
        {
            this._ScrapingService = scrapingService;
            _distributedCache = distributedCache;
        }
       
        [HttpGet]
        public async Task<IActionResult> Detail([Required]string url)
        { 

            var scrap = await this._ScrapingService.GetStatistic(url);

            return Ok(scrap);
        }
    }
}