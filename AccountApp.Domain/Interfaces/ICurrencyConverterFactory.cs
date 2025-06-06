
namespace AccountApp.Domain.Interfaces
{
    public interface ICurrencyConverterFactory
    {
        ICurrencyConverter Create(Dictionary<string, decimal> rates);
    }
}
