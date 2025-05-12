using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;

namespace SkillSwap.Services
{
    public class QuoteService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<QuoteService> _logger;
        private readonly IMemoryCache _cache;
        private readonly Random _random = new Random();

        public QuoteService(HttpClient httpClient, ILogger<QuoteService> logger, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<(string Quote, string Author)> GetDailyQuoteAsync(bool forceRefresh = false)
        {
            // Usa un timestamp che cambia ogni ora invece che ogni giorno
            string cacheKey = $"quote_{DateTime.UtcNow:yyyyMMdd_HH}";

            // Se richiesto un refresh o la cache è vuota
            if (forceRefresh || !_cache.TryGetValue(cacheKey, out (string Quote, string Author) cachedQuote))
            {
                _logger.LogInformation("Recupero nuova citazione (force={Force})", forceRefresh);

                try
                {
                    // Tenta di ottenere una citazione random dall'API
                    var response = await _httpClient.GetAsync("https://zenquotes.io/api/random");

                    if (response.IsSuccessStatusCode)
                    {
                        var quotes = await response.Content.ReadFromJsonAsync<List<QuoteDto>>();

                        if (quotes != null && quotes.Count > 0)
                        {
                            var quote = (quotes[0].q, quotes[0].a);
                            // Cache per 1 ora invece di 24
                            _cache.Set(cacheKey, quote, TimeSpan.FromHours(1));
                            return quote;
                        }
                    }

                    // Fallback a citazioni interne
                    var fallback = GetRandomQuote();
                    _cache.Set(cacheKey, fallback, TimeSpan.FromHours(1));
                    return fallback;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore nel recupero della citazione");
                    var fallback = GetRandomQuote();
                    _cache.Set(cacheKey, fallback, TimeSpan.FromHours(1));
                    return fallback;
                }
            }

            return cachedQuote;
        }

        private (string Quote, string Author) GetRandomQuote()
        {
            var quotes = new List<(string, string)>
            {
                ("La condivisione della conoscenza è la chiave per crescere insieme.", "SkillSwap"),
                ("Impara qualcosa di nuovo ogni giorno e condividi ciò che sai.", "SkillSwap"),
                ("L'educazione è l'arma più potente che puoi utilizzare per cambiare il mondo.", "Nelson Mandela"),
                ("Il vero valore delle competenze si realizza quando vengono condivise.", "Team SkillSwap"),
                ("Chi impara e non insegna è come chi miete e non semina.", "Proverbio"),
                ("Il sapere è l'unica ricchezza che aumenta quando viene condivisa.", "Socrate"),
                ("Insegnare è imparare due volte.", "Joseph Joubert"),
                ("La conoscenza parla, ma la saggezza ascolta.", "Jimi Hendrix"),
                ("Studiare senza pensare è inutile. Pensare senza studiare è pericoloso.", "Confucio"),
                ("La mente non è un vaso da riempire, ma un fuoco da accendere.", "Plutarco")
            };

            return quotes[_random.Next(quotes.Count)];
        }

        private class QuoteDto
        {
            public string q { get; set; } // Quote
            public string a { get; set; } // Author
        }
    }
}