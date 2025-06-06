using AccountApp.Domain.Interfaces;
using AccountApp.Infrastructure.Services;


namespace AccountApp.Infrastructure.Factories
{
    public class CsvCurrencyConverterFactory : ICurrencyConverterFactory
    {
        public ICurrencyConverter Create(Dictionary<string, decimal> rates)
        {
            return new CurrencyConverter(rates);
        }
    }
}
