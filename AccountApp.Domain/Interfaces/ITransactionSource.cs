using AccountApp.Domain.Entities;

namespace AccountApp.Domain.Interfaces
{
    public interface ITransactionSource
    {
        Task<IEnumerable<Transaction>> GetTransactionsAsync();
        Task<decimal> GetFinalAccountValueAsync();
        Task<Dictionary<string, decimal>> GetExchangeRatesAsync();
    }
}
