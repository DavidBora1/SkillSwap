using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SkillSwap.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiService> _logger;
        private readonly string _apiKey;
        // Aggiornato per utilizzare Gemini 2.0 Flash
        private readonly string _apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
        private readonly Random _random = new Random();

        public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["Gemini:ApiKey"];

            _logger.LogInformation("API Gemini: Inizializzazione con endpoint {Endpoint}", _apiEndpoint);
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogError("API Gemini: Chiave API non configurata");
            }
            else
            {
                _logger.LogInformation("API Gemini: Chiave API configurata");
            }
        }

        public async Task<string> GetSkillSuggestions(string skillDescription)
        {
            try
            {
                var prompt = $"Sei un consulente esperto in sviluppo delle competenze. " +
                             $"Analizza queste competenze di un utente: '{skillDescription}', " +
                             $"e fornisci UN SOLO consiglio specifico e motivante su come migliorare " +
                             $"il proprio profilo professionale. La risposta deve essere breve (max 30 parole), in italiano.";

                return await CallGeminiApiAsync(prompt) ?? GetRandomSkillSuggestion();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella chiamata all'API Gemini per suggerimenti skill");
                return GetRandomSkillSuggestion();
            }
        }

        public async Task<string> GetConversationStarters(string skill1, string skill2)
        {
            try
            {
                var prompt = $"Genera UNO spunto di conversazione informale tra due persone che vogliono scambiarsi competenze. " +
                             $"La prima persona è esperta in '{skill1}' e la seconda in '{skill2}'. " +
                             $"Lo spunto deve essere in italiano, informale e breve (max 20 parole).";

                return await CallGeminiApiAsync(prompt) ??
                       $"Ciao! Noto che conosci {skill1} mentre io ho esperienza con {skill2}. Ti andrebbe di scambiare qualche consiglio?";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella chiamata all'API Gemini per suggerimenti conversazione");
                return $"Ciao! Sarei curioso di scambiare idee su {skill1} e {skill2}. Cosa ti ha portato ad imparare {skill2}?";
            }
        }

        public async Task<string> ImproveSkillDescription(string skillName, string currentDescription)
        {
            try
            {
                var baseDescription = string.IsNullOrEmpty(currentDescription) ?
                    $"Sono competente in {skillName}" : currentDescription;

                var prompt = $"Migliora questa descrizione di competenza: '{baseDescription}' " +
                             $"per la competenza '{skillName}'. " +
                             $"Scrivi UNA SOLA descrizione migliorata, professionale e in prima persona. " +
                             $"La descrizione deve essere di massimo 2 frasi e non deve includere opzioni multiple o elenchi.";

                _logger.LogInformation("API Gemini: Richiesta per '{SkillName}'", skillName);

                var result = await CallGeminiApiAsync(prompt);

                if (!string.IsNullOrEmpty(result))
                {
                    _logger.LogInformation("API Gemini: Risposta ricevuta con successo");
                    return result;
                }

                _logger.LogWarning("API Gemini: Risposta vuota, uso fallback");
                return string.IsNullOrEmpty(currentDescription) ?
                    GetDefaultSkillDescription(skillName) : currentDescription;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il miglioramento della descrizione");
                return string.IsNullOrEmpty(currentDescription) ?
                    GetDefaultSkillDescription(skillName) : currentDescription;
            }
        }

        private async Task<string> CallGeminiApiAsync(string prompt)
        {
            try
            {
                // Struttura semplificata allineata con l'esempio curl
                var requestData = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var requestJson = JsonSerializer.Serialize(requestData);
                _logger.LogInformation("API Gemini: Invio richiesta");

                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var url = $"{_apiEndpoint}?key={_apiKey}";

                var response = await _httpClient.PostAsync(url, requestContent);

                // Leggi la risposta come stringa
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API Gemini: Errore {Status}: {Content}",
                                  response.StatusCode, responseContent);
                    return null;
                }

                // Log della risposta per debug
                _logger.LogDebug("API Gemini: Risposta ricevuta: {Response}", responseContent);

                // Parsing della risposta JSON
                using (var doc = JsonDocument.Parse(responseContent))
                {
                    // Cammino attraverso la risposta JSON per estrarre il testo
                    if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                        candidates.GetArrayLength() > 0 &&
                        candidates[0].TryGetProperty("content", out var content) &&
                        content.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0 &&
                        parts[0].TryGetProperty("text", out var textElement))
                    {
                        var text = textElement.GetString();
                        if (!string.IsNullOrEmpty(text))
                        {
                            return text.Trim();
                        }
                    }

                    _logger.LogWarning("API Gemini: Formato risposta non previsto");
                }

                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API Gemini: Errore HTTP");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "API Gemini: Errore parsing JSON");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Gemini: Errore generico");
                return null;
            }
        }

        private string GetRandomSkillSuggestion()
        {
            var suggestions = new[]
            {
                "Cerca di bilanciare le tue competenze tecniche con quelle trasversali come comunicazione e teamwork per un profilo più completo.",
                "Approfondisci le competenze in cui sei già esperto e aggiungi certificazioni per aumentare la tua credibilità.",
                "Prova a collegare le tue diverse competenze per creare una proposta di valore unica e differenziarti.",
                "Considera di aggiungere competenze complementari a quelle che già possiedi per offrire soluzioni più complete.",
                "Specializzati in un'area specifica delle tue competenze per diventare un punto di riferimento in quel settore."
            };

            return suggestions[_random.Next(suggestions.Length)];
        }

        private string GetDefaultSkillDescription(string skillName)
        {
            var descriptions = new[]
            {
                $"Ho una solida esperienza in {skillName} e sono appassionato di condividere le mie conoscenze con altri.",
                $"Pratico {skillName} da diversi anni e posso aiutare chi desidera imparare, partendo dalle basi fino a tecniche più avanzate.",
                $"Ho sviluppato competenze in {skillName} attraverso studio ed esperienza pratica, e sono sempre entusiasta di confrontarmi con altri appassionati.",
                $"Sono specializzato in {skillName} e posso offrire un approccio metodico e pratico per chi vuole apprendere questa competenza."
            };

            return descriptions[_random.Next(descriptions.Length)];
        }
    }
}