using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dapper;

namespace Data.DataAccess
{
    public abstract class Database : IDatabase
    {
        protected abstract IDbConnection CreateConnection();
        protected abstract CommandType CommandType { get; }

        public async Task ExecuteAsync(string command, object parameters = null)
        {
            using var connection = CreateConnection();
            connection.Open();

            await connection.ExecuteAsync(
                command,
                parameters,
                commandType: CommandType
            );
        }

        public async Task<T> QuerySingleAsync<T>(string command, object parameters = null)
        {
            using var connection = CreateConnection();
            connection.Open();

            return await connection.QuerySingleAsync<T>(
                command,
                parameters,
                commandType: CommandType
            );
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string command, object parameters = null)
        {
            using var connection = CreateConnection();
            connection.Open();

            return await connection.QueryAsync<T>(
                command,
                parameters,
                commandType: CommandType
            );
        }
    }
}
