using Business.BusinessLogic;
using Core;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Services.Controllers
{
    [ApiController]
    [Route("api/trades")]
    public class TradeController : ControllerBase
    {
        private readonly ITradeBusinessLogic _tradeBusinessLogic;

        public TradeController(ITradeBusinessLogic tradeBusinessLogic)
        {
            _tradeBusinessLogic = tradeBusinessLogic;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitTrade([FromBody] TradeRequest request, CancellationToken ct)
        {
            var response = await _tradeBusinessLogic.RecordTradeAsync(request);

            return Accepted(response);
        }
    }
}
