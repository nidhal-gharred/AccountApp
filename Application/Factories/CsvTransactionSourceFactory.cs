using AccountApp.Domain.Interfaces;
using AccountApp.Infrastructure.DataSources;

namespace AccountApp.Infrastructure.Factories
{
    public class CsvTransactionSourceFactory : ITransactionSourceFactory
    {
        public ITransactionSource Create(string filePath)
        {
            return new CsvTransactionSource(filePath);
        }
    }
}
