using CurrencyRate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using CurrencyRate.ViewModels;
using System.Linq;

namespace CurrencyRate.Controllers
{
    [Route("[controller]")]
    public class MoneyController : Controller
    {
        private readonly ILogger<MoneyController> _logger;
        private readonly IMemoryCache _memoryCache;

        public MoneyController(ILogger<MoneyController> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        [Route("[action]")]
        public IActionResult Rate()
        {
            List<MoneyModel> moneyModelList;

            try
            {
                moneyModelList = GetMoneyRateFromMemoryCache();
                ViewData["Date"] = GetDateFromMemoryCache();
            }
            catch(InvalidOperationException ex)
            {
                _logger.LogError(ex, DateTime.Now.ToString());
                return Redirect("~/Home/Error");
            }
            
            return View(moneyModelList.Where(m => m.CharCode != "RUB"));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult Converter()
        {
            List<MoneyModel> moneyModelList;

            try
            {
                moneyModelList = GetMoneyRateFromMemoryCache();
                ViewData["Date"] = GetDateFromMemoryCache();
            }
            catch(InvalidOperationException ex)
            {
                _logger.LogError(ex, DateTime.Now.ToString());
                return Redirect("~/Home/Error");
            }

            var nameList = moneyModelList.Select(m => m.Name);

            return View(nameList);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Converter(string nameFrom, decimal valueFrom, string nameTo)
        {
            List<MoneyModel> moneyModelList;

            try
            {
                moneyModelList = GetMoneyRateFromMemoryCache();
            }
            catch(InvalidOperationException ex)
            {
                _logger.LogError(ex, DateTime.Now.ToString());
                return Redirect("~/Home/Error");
            }

            var moneyFrom = moneyModelList.First(m => m.Name.Equals(nameFrom));
            var moneyTo = moneyModelList.First(m => m.Name.Equals(nameTo));
            var rubFrom = (moneyFrom.Value / moneyFrom.Nominal) * valueFrom;
            var valueTo = rubFrom / (moneyTo.Value / moneyTo.Nominal);

            return View("ConverterResult", new MoneyConverterResultViewModel 
            { 
                NameFrom = nameFrom, 
                NameTo = nameTo, 
                ValueFrom = valueFrom, 
                ValueTo = valueTo 
            });
        }

        private List<MoneyModel> GetMoneyRateFromMemoryCache()
        {
            if(!_memoryCache.TryGetValue("moneyRate", out List<MoneyModel> model))
            {
                throw new InvalidOperationException("Could not get list of money models from memory cache");
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
