using CurrencyRate.Models;
using CurrencyRate.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CurrencyRate.Controllers
{
    [Route("[controller]")]
    public class PreciousMetalController : Controller
    {
        private readonly ILogger<PreciousMetalController> _logger;
        private readonly IMemoryCache _memoryCache;

        public PreciousMetalController(ILogger<PreciousMetalController> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        [Route("[action]")]
        public IActionResult Rate()
        {
            List<PreciousMetalModel> preciousMetalModelList;

            try
            {
                preciousMetalModelList = GetPrecoiusMetalRateFromMemoryCache();
                ViewData["Date"] = GetDateFromMemoryCache();
            }
            catch(InvalidOperationException ex)
            {
                _logger.LogError(ex, DateTime.Now.ToString());
                return Redirect("~/Home/Error");
            }

            return View(preciousMetalModelList);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult Converter()
        {
            List<PreciousMetalModel> preciousMetalModelList;

            try
            {
                preciousMetalModelList = GetPrecoiusMetalRateFromMemoryCache();
                ViewData["Date"] = GetDateFromMemoryCache();
            }
            catch(InvalidOperationException ex)
            {
                _logger.LogError(ex, DateTime.Now.ToString());
                return Redirect("~/Home/Error");
            }

            var preciousMetalCodeList = preciousMetalModelList.Select(m => m.Code);
           
            return View(preciousMetalCodeList);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Converter(int buyCode, decimal buySum, int sellCode, decimal sellWeight)
        {
            List<PreciousMetalModel> preciousMetalModelList;

            try
            {
                preciousMetalModelList = GetPrecoiusMetalRateFromMemoryCache();
            }
            catch(InvalidOperationException ex)
            {
                _logger.LogError(ex, DateTime.Now.ToString());
                return Redirect("~/Home/Error");
            }

            var buyWeight = buySum / preciousMetalModelList.First(m => m.Code == buyCode).Buy;
            var sellSum = sellWeight * preciousMetalModelList.First(m => m.Code == sellCode).Sell;

            return View("ConverterResult", new PreciousMetalConverterResultViewModel 
            { 
                BuyCode = buyCode,
                BuySum = buySum,
                BuyWeight = buyWeight,
                SellCode = sellCode,
                SellWeight = sellWeight,
                SellSum = sellSum
            });
        }

        private List<PreciousMetalModel> GetPrecoiusMetalRateFromMemoryCache()
        {
            if(!_memoryCache.TryGetValue("preciousMetalRate", out List<PreciousMetalModel> model))
            {
                throw new InvalidOperationException("Could not get list of precious metal models from memory cache");
            }

            return model;
        }

        private string GetDateFromMemoryCache()
        {
            if(!_memoryCache.TryGetValue("date", out string date))
            {
                throw new InvalidOperationException("Could not get date from memory cache");
            }

            return date.Replace('/', '.');
        }
    }
}
