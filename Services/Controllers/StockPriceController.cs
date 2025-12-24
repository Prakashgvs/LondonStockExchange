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

        [HttpGet("values")]
        public async Task<ActionResult<List<StockPrice>>> GetStockPrices([FromBody] StockBatchRequest request)
        {
            var result = await _stockPriceBusinessLogic.GetStockPricesAsync(request);
            return Ok(result);
        }
    }
}
