

namespace AccountApp.Domain.Interfaces
{
    public interface ICurrencyConverter
    {
        decimal ConvertToEuro(decimal amount, string currency);
    }
}
