using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Services
{
    [ApiController]
    [Route("api/stocks")]
    public class StockPriceController : ControllerBase
    {
        private readonly IStockPriceBusinessLogic _stockPriceBusinessLogic;

        public StockPriceController(IStockPriceBusinessLogic stockPriceBusinessLogic)
        {
            _stockPriceBusinessLogic = stockPriceBusinessLogic;
        }

        [HttpGet("{ticker}")]
        public async Task<ActionResult<StockPrice>> GetStockPrice(string ticker)
        {
            var result = await _stockPriceBusinessLogic.GetStockPriceAsync(ticker);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("batch")]
        public async Task<ActionResult<List<StockPrice>>> GetStockPricesBatch([FromQuery] string? tickers = null)
        {
            var tickerList = string.IsNullOrEmpty(tickers)
                ? new List<string>()
                : tickers.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            var request = new StockBatchRequest { Tickers = tickerList };
            var result = await _stockPriceBusinessLogic.GetStockPricesBatchAsync(request);
            return Ok(result);
        }

    }
}
