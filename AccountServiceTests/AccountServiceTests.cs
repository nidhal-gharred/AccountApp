using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AccountApp.Domain.Interfaces;
using AccountApp.Application.Services;
using AccountApp.Domain.Entities;

public class AccountServiceTests
{
    private readonly Mock<ITransactionSourceFactory> _mockSourceFactory;
    private readonly Mock<ICurrencyConverterFactory> _mockConverterFactory;
    private readonly Mock<ITransactionSource> _mockSource;
    private readonly Mock<ICurrencyConverter> _mockConverter;
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        _mockSourceFactory = new Mock<ITransactionSourceFactory>();
        _mockConverterFactory = new Mock<ICurrencyConverterFactory>();
        _mockSource = new Mock<ITransactionSource>();
        _mockConverter = new Mock<ICurrencyConverter>();

        _mockConverter.Setup(c => c.ConvertToEuro(It.IsAny<decimal>(), It.IsAny<string>()))
            .Returns<decimal, string>((amount, currency) => amount);

        _mockConverterFactory.Setup(f => f.Create(It.IsAny<Dictionary<string, decimal>>()))
            .Returns(_mockConverter.Object);

        _mockSourceFactory.Setup(f => f.Create(It.IsAny<string>()))
            .Returns(_mockSource.Object);

        _service = new AccountService(_mockSourceFactory.Object, _mockConverterFactory.Object);
    }

    [Fact]
    public async Task GetTopCategoriesAsync_ReturnsTop3Categories_WithNegativeAmounts()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction { Amount = -100m, Currency = "EUR", Category = "Alimentation" },
            new Transaction { Amount = -200m, Currency = "EUR", Category = "Sante" },
            new Transaction { Amount = -300m, Currency = "EUR", Category = "Habitation" },
            new Transaction { Amount = 500m, Currency = "EUR", Category = "Salaire" }
        };

        _mockSource.Setup(s => s.GetTransactionsAsync()).ReturnsAsync(transactions);
        _mockSource.Setup(s => s.GetExchangeRatesAsync()).ReturnsAsync(new Dictionary<string, decimal> { ["EUR"] = 1.0m });

        // Act
        var topCategories = await _service.GetTopCategoriesAsync("dummy.csv");

        // Assert
        Assert.Equal(3, topCategories.Count);
        Assert.Contains(topCategories, c => c.Category == "Habitation" && c.Total == -300m);
        Assert.Contains(topCategories, c => c.Category == "Sante" && c.Total == -200m);
        Assert.Contains(topCategories, c => c.Category == "Alimentation" && c.Total == -100m);
    }

    [Fact]
    public async Task GetTopCategoriesAsync_IgnoresPositiveAmounts()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction { Amount = 100m, Currency = "EUR", Category = "Alimentation" },
            new Transaction { Amount = 200m, Currency = "EUR", Category = "Sante" }
        };

        _mockSource.Setup(s => s.GetTransactionsAsync()).ReturnsAsync(transactions);
        _mockSource.Setup(s => s.GetExchangeRatesAsync()).ReturnsAsync(new Dictionary<string, decimal> { ["EUR"] = 1.0m });

        // Act
        var topCategories = await _service.GetTopCategoriesAsync("dummy.csv");

        // Assert
        Assert.Empty(topCategories); // Aucun débit négatif
    }

    [Fact]
    public async Task GetTopCategoriesAsync_HandlesCategoriesWithDifferentCasingAndSpaces()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction { Amount = -100m, Currency = "EUR", Category = " alimentation " },
            new Transaction { Amount = -200m, Currency = "EUR", Category = "Alimentation" },
            new Transaction { Amount = -150m, Currency = "EUR", Category = "ALIMENTATION" }
        };

        _mockSource.Setup(s => s.GetTransactionsAsync()).ReturnsAsync(transactions);
        _mockSource.Setup(s => s.GetExchangeRatesAsync()).ReturnsAsync(new Dictionary<string, decimal> { ["EUR"] = 1.0m });

        // Act
        var topCategories = await _service.GetTopCategoriesAsync("dummy.csv");

        // Assert
        Assert.Single(topCategories);
        var cat = topCategories[0];
        Assert.Equal("alimentation", cat.Category.ToLower());
        Assert.Equal(-450m, cat.Total);
    }

    [Fact]
    public async Task GetTopCategoriesAsync_ReturnsAll_WhenLessThan3Categories()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction { Amount = -100m, Currency = "EUR", Category = "Loisir" },
            new Transaction { Amount = -200m, Currency = "EUR", Category = "Sante" }
        };

        _mockSource.Setup(s => s.GetTransactionsAsync()).ReturnsAsync(transactions);
        _mockSource.Setup(s => s.GetExchangeRatesAsync()).ReturnsAsync(new Dictionary<string, decimal> { ["EUR"] = 1.0m });

        // Act
        var topCategories = await _service.GetTopCategoriesAsync("dummy.csv");

        // Assert
        Assert.Equal(2, topCategories.Count);
        Assert.Contains(topCategories, c => c.Category == "Sante");
        Assert.Contains(topCategories, c => c.Category == "Loisir");
    }

    [Fact]
    public async Task GetTopCategoriesAsync_ReturnsEmpty_WhenNoTransactions()
    {
        // Arrange
        var transactions = new List<Transaction>();

        _mockSource.Setup(s => s.GetTransactionsAsync()).ReturnsAsync(transactions);
        _mockSource.Setup(s => s.GetExchangeRatesAsync()).ReturnsAsync(new Dictionary<string, decimal> { ["EUR"] = 1.0m });

        // Act
        var topCategories = await _service.GetTopCategoriesAsync("dummy.csv");

        // Assert
        Assert.Empty(topCategories);
    }
}
