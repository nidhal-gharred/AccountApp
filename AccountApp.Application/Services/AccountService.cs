using AccountApp.Application.DTOs;
using AccountApp.Application.Interfaces;
using AccountApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AccountApp.Application.Services;

public class AccountService : IAccountService
{
    private readonly ITransactionSourceFactory _transactionSourceFactory;
    private readonly ICurrencyConverterFactory _currencyConverterFactory;

    private readonly DateTime _minDate = new DateTime(2022, 1, 1);
    private readonly DateTime _maxDate = new DateTime(2023, 3, 1);
    private static readonly HashSet<string> AllowedCurrencies = new(new[] { "EUR", "USD", "JPY" });

    public AccountService(
        ITransactionSourceFactory transactionSourceFactory,
        ICurrencyConverterFactory currencyConverterFactory
        )
    {
        _transactionSourceFactory = transactionSourceFactory;
        _currencyConverterFactory = currencyConverterFactory;
    }

    public async Task<AccountValueDto> GetAccountValueAtAsync(DateTime date, string filePath)
    {
        // Validation de la date selon l'énoncé
        if (date < _minDate || date > _maxDate)
        {
            throw new ArgumentException($"Date must be between {_minDate:dd/MM/yyyy} and {_maxDate:dd/MM/yyyy}.");
        }

        var source = _transactionSourceFactory.Create(filePath);
        var transactions = await source.GetTransactionsAsync();

        var rates = await source.GetExchangeRatesAsync();
        var converter = _currencyConverterFactory.Create(rates);

        var finalValue = await source.GetFinalAccountValueAsync();

        // Filtrer les transactions postérieures à la date demandée et avec devise autorisée
        var total = transactions
            .Where(t => t.Date > date && AllowedCurrencies.Contains(t.Currency))
            .Sum(t => converter.ConvertToEuro(t.Amount, t.Currency));

        // Valeur du compte à la date = valeur finale - sommes des transactions ultérieures
        return new AccountValueDto(finalValue - total);
    }

    public async Task<List<CategoryTotalDto>> GetTopCategoriesAsync(string filePath)
    {
        var source = _transactionSourceFactory.Create(filePath);
        var transactions = await source.GetTransactionsAsync();

        var rates = await source.GetExchangeRatesAsync();
        var converter = _currencyConverterFactory.Create(rates);


        var categorySums = transactions
     .Where(t => t.Amount < 0 && AllowedCurrencies.Contains(t.Currency))
     .GroupBy(t => t.Category.Trim(), StringComparer.OrdinalIgnoreCase)
     .ToDictionary(
         g => g.Key,
         g => g.Sum(t => t.Amount));

        var topCategories = categorySums
            .OrderBy(val => val.Value)  // Les montants sont négatifs donc ordre croissant = plus grande dépense
            .Take(3)
            .Select(x => new CategoryTotalDto(x.Key, x.Value))
            .ToList();
        return topCategories;
    }


}

