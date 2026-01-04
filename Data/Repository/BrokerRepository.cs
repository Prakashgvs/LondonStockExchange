using Core.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository
{
    public class BrokerRepository : IBrokerRepository
    {
        private readonly IDatabase _database;

        public BrokerRepository(IDatabase database)
        {
            _database = database;
        }

        public async Task<bool> IsBrokerActiveAsync(string brokerId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@BrokerId", brokerId);
            parameters.Add("@IsActive", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            await _database.ExecuteAsync("sp_IsBrokerActive", parameters);

            return parameters.Get<bool>("@IsActive");
        }
    }
}
