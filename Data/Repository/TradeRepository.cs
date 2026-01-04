using Core.Entities;
using Core.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Data.Repository
{
    public class TradeRepository : ITradeRepository
    {
        private readonly IDatabase _database;

        public TradeRepository(IDatabase database)
        {
            _database = database;
        }

        public async Task<long> RecordTradeAsync(Transaction transaction, decimal tradeValue)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@TradeId", transaction.TradeId);
            parameters.Add("@TickerSymbol", transaction.TickerSymbol);
            parameters.Add("@Price", transaction.Price);
            parameters.Add("@Shares", transaction.Shares);
            parameters.Add("@BrokerId", transaction.BrokerId);
            parameters.Add("@Timestamp", transaction.Timestamp);
            parameters.Add("@TradeValue", tradeValue);
            parameters.Add("@TransactionId", dbType: DbType.Int64, direction: ParameterDirection.Output);

            await _database.ExecuteAsync("sp_RecordTrade", parameters);

            return parameters.Get<long>("@TransactionId");
        }
    }
}
