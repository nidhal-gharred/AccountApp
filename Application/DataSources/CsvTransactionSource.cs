using AccountApp.Domain.Entities;
using AccountApp.Domain.Interfaces;
using System.Globalization;

namespace AccountApp.Infrastructure.DataSources;

public class CsvTransactionSource : ITransactionSource
{
    private readonly string _path;
    private decimal _finalValue;
    private readonly Dictionary<string, decimal> _rates = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> AllowedCurrencies = new(new[] { "EUR", "USD", "JPY" });

    public CsvTransactionSource(string path) => _path = path;

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync()
    {
        var lines = await File.ReadAllLinesAsync(_path);
        var transactions = new List<Transaction>();
        bool inData = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("Compte au"))
            {
                var value = line.Split(':')[1].Trim().Replace(" EUR", "");
                _finalValue = decimal.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (line.Contains("/EUR"))  // Lecture des taux au format JPY/EUR, USD/EUR
            {
                var parts = line.Split(':');
                var currencyPair = parts[0].Trim();    // ex: "JPY/EUR"
                var rateValue = decimal.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);

                var currency = currencyPair.Split('/')[0]; // ex: "JPY"

                // On inverse le taux pour avoir EUR -> Devise (car fichier donne Devise/EUR)
                _rates[currency] = 1 /rateValue;
            }
            else if (line.StartsWith("Date"))
            {
                inData = true;
                continue;
            }
            else if (inData)
            {
                var parts = line.Split(';');
                var date = DateTime.ParseExact(parts[0], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var amount = decimal.Parse(parts[1], CultureInfo.InvariantCulture);
                var currency = parts[2];
                var category = parts[3];

                if (!AllowedCurrencies.Contains(currency)) continue;

                transactions.Add(new Transaction
                {
                    Date = date,
                    Amount = amount,
                    Currency = currency,
                    Category = category
                });
            }
        }

        _rates["EUR"] = 1.0m; // Assurer la présence de l'euro dans les taux

        return transactions;
    }

    public Task<decimal> GetFinalAccountValueAsync() => Task.FromResult(_finalValue);

    public Task<Dictionary<string, decimal>> GetExchangeRatesAsync()
    {
        // Retourner le dictionnaire de taux déjà rempli dans GetTransactionsAsync
        return Task.FromResult(_rates);
    }
}
