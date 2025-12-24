using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces
{
    public interface ITradeRepository
    {
        Task<long> RecordTradeAsync(Transaction transaction, decimal tradeValue);
    }
}
