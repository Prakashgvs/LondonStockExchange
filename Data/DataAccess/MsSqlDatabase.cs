using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Data.DataAccess
{
    public class MsSqlDatabase : Database, IMsSqlDatabase
    {
        private readonly string _connectionString;

        public MsSqlDatabase(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DBConnectionString");
        }

        protected override IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        protected override CommandType CommandType => CommandType.StoredProcedure;
    }
}
