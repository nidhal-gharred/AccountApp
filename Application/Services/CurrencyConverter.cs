
using AccountApp.Domain.Interfaces;

namespace AccountApp.Infrastructure.Services
{

    public class CurrencyConverter : ICurrencyConverter
    {
        private readonly Dictionary<string, decimal> _rates;

        public CurrencyConverter(Dictionary<string, decimal> rates)
        {
            _rates = new Dictionary<string, decimal>(rates, StringComparer.OrdinalIgnoreCase);
        }

        public decimal ConvertToEuro(decimal amount, string currency)
        {
            if (!_rates.TryGetValue(currency, out var rate))
            {
                throw new ArgumentException($"Unknown or unsupported currency: {currency}");
            }
            return currency == "EUR" ? amount : amount / rate;
        }
    }
}
