using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces
{
    public interface IDatabase
    {
        Task ExecuteAsync(string command, object? parameters = null);
        Task<T> QuerySingleAsync<T>(string command, object? parameters = null);
        Task<IEnumerable<T>> QueryAsync<T>(string command, object? parameters = null);
    }
}
