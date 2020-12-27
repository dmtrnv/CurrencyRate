using CurrencyRate.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CurrencyRate.Services
{
    public class GetCurrencyRateService : BackgroundService
    {
        private readonly ILogger<GetCurrencyRateService> _logger;
        private readonly IMemoryCache _memoryCache;

        public GetCurrencyRateService(ILogger<GetCurrencyRateService> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Так как эта задача выполняется в другом потоке, то велика вероятность, что
                    // культура по умолчанию может отличаться от той, которая установлена в нашем приложении,
                    // поэтому явно укажем нужную нам, чтобы не было проблем с разделителями, названиями и т.д.
                    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");

                    // Кодировка файла xml с сайта ЦБ == windows-1251.
                    // По умолчанию она недоступна в .NET Core, поэтому регистрируем нужный провайдер. 
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    var moneyRateXml = LoadMoneyRateXml();                  
                    var moneyModelList = GetMoneyModelListFromXml(moneyRateXml);
                    _memoryCache.Set("moneyRate", moneyModelList, TimeSpan.FromMinutes(70));

                    var date = GetDateFromMoneyRateXml(moneyRateXml);
                    _memoryCache.Set("date", date, TimeSpan.FromMinutes(70));

                    var preciousMetalRateXml = LoadPreciousMetalRateXml(date);
                    var preciousMetalModelList = GetPreciousMetalModelListFromXml(preciousMetalRateXml);
                    _memoryCache.Set("preciousMetalRate", preciousMetalModelList, TimeSpan.FromMinutes(70));
                }
                catch(InvalidOperationException ex)
                {
                    _logger.LogError(ex, DateTime.Now.ToString());
                }

                await Task.Delay(3600000, stoppingToken);
            }
        }

        private List<MoneyModel> GetMoneyModelListFromXml(XDocument moneyRateXml)
        {    
            var moneyModelList = new List<MoneyModel>();

            try
            {
                moneyModelList = moneyRateXml
                    .Element("ValCurs")
                    .Elements("Valute")
                    .Select(valute => new MoneyModel
                    {
                        CharCode = valute.Element("CharCode").Value,
                        Name = valute.Element("Name").Value,
                        NumCode = Convert.ToInt32(valute.Element("NumCode").Value),
                        Nominal = Convert.ToInt32(valute.Element("Nominal").Value),
                        Value = Convert.ToDecimal(valute.Element("Value").Value)
                    })
                    .ToList();
            }
            catch(NullReferenceException ex)
            {
                _logger.LogError(ex, DateTime.Now.ToString());
            }

            if(moneyModelList.Count == 0)
            {
                throw new InvalidOperationException("Could not get list of money models from xml");
            }
            else
            {
                moneyModelList.Add(new MoneyModel 
                { 
                    CharCode = "RUB", 
                    Name = "Рубль", 
                    Nominal = 1, 
                    NumCode = 0, 
                    Value = 1 
                });
            }

            return moneyModelList;
        }

        private string GetDateFromMoneyRateXml(XDocument moneyRateXml)
        {
            var date = new DateTime();

            try
            {
                date = Convert.ToDateTime(moneyRateXml
                    .Element("ValCurs")
                    .Attribute("Date")
                    .Value);
            }
            catch(NullReferenceException ex)
            {
                _logger.LogError(ex, DateTime.Now.ToString());
                throw new InvalidOperationException("Could not get date from xml");
            }

            return $"{string.Format("{0:00}", date.Day)}/{date.Month}/{date.Year}";
        }
     
        private List<PreciousMetalModel> GetPreciousMetalModelListFromXml(XDocument preciousMetalRateXml)
        {
            var preciousMetalModelList = new List<PreciousMetalModel>();

            try
            {
                preciousMetalModelList = preciousMetalRateXml
                    .Element("Metall")
                    .Elements("Record")
                    .Select(record => new PreciousMetalModel
                    {
                        Code = Convert.ToInt32(record.Attribute("Code").Value),
                        Buy = Convert.ToDecimal(record.Element("Buy").Value),
                        Sell = Convert.ToDecimal(record.Element("Sell").Value)
                    })
                    .ToList();
            }
            catch(NullReferenceException ex)
            {
                _logger.LogError(ex, DateTime.Now.ToString());
            }

            if(preciousMetalModelList.Count == 0)
            {
                throw new InvalidOperationException("Could not get list of precious metal models from xml");
            }

            return preciousMetalModelList;
        }

        private XDocument LoadMoneyRateXml()
        {
            try
            {
                return XDocument.Load("http://www.cbr.ru/scripts/XML_daily.asp");
            }
            catch(System.Net.WebException ex)
            {
                _logger.LogError(ex, DateTime.Now.ToString());
                return new XDocument();
            }
        }

        private XDocument LoadPreciousMetalRateXml(string date)
        {
            try
            {
                return XDocument.Load($"http://www.cbr.ru/scripts/xml_metall.asp?date_req1={date}&date_req2={date}");
            }
            catch(System.Net.WebException ex)
            {
                _logger.LogError(ex, DateTime.Now.ToString());
                return new XDocument();
            }
        }
    }
}
