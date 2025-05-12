using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using SkillSwap.Services;
using System.Text.Json;

namespace SkillSwap.Pages.Api
{
    public class GeminiModel : PageModel
    {
        private readonly GeminiService _geminiService;
        private readonly ILogger<GeminiModel> _logger;
        private readonly IMemoryCache _memoryCache;

        public GeminiModel(GeminiService geminiService, ILogger<GeminiModel> logger, IMemoryCache memoryCache)
        {
            _geminiService = geminiService;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        // Importante: Questo handler risponde a /api/gemini?handler=improvedescription
        public async Task<IActionResult> OnGetImprovedescriptionAsync(string skillName, string description)
        {
            try
            {
                _logger.LogInformation("API: Miglioramento descrizione per {SkillName}", skillName);

                if (string.IsNullOrEmpty(skillName))
                {
                    return new JsonResult(new { error = "Il nome della competenza è obbligatorio" });
                }

                var cacheKey = $"improve_{skillName}_{description}";
                if (!_memoryCache.TryGetValue(cacheKey, out string improvedDescription))
                {
                    improvedDescription = await _geminiService.ImproveSkillDescription(skillName, description ?? "");

                    // Risposta di fallback in caso Gemini fallisca
                    if (string.IsNullOrEmpty(improvedDescription))
                    {
                        improvedDescription = $"Ho solide competenze in {skillName} e sono in grado di condividere la mia esperienza con altri.";
                    }

                    // Cache per 30 minuti
                    _memoryCache.Set(cacheKey, improvedDescription, TimeSpan.FromMinutes(30));
                }

                _logger.LogInformation("Descrizione migliorata: {Desc}", improvedDescription);
                return new JsonResult(new { description = improvedDescription });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'API di miglioramento descrizione");
                return new JsonResult(new
                {
                    description = $"Ho esperienza pratica in {skillName} e posso aiutarti ad apprendere le basi di questa competenza."
                });
            }
        }

        // Importante: Questo handler risponde a /api/gemini?handler=conversationstarter
        public async Task<IActionResult> OnGetConversationstarterAsync(string skill1, string skill2)
        {
            try
            {
                _logger.LogInformation("API: Suggerimento conversazione per {Skill1} e {Skill2}", skill1, skill2);

                if (string.IsNullOrEmpty(skill1) || string.IsNullOrEmpty(skill2))
                {
                    return new JsonResult(new { error = "Entrambe le competenze sono obbligatorie" });
                }

                var cacheKey = $"conv_{skill1}_{skill2}";
                if (!_memoryCache.TryGetValue(cacheKey, out string starter))
                {
                    starter = await _geminiService.GetConversationStarters(skill1, skill2);

                    // Risposta di fallback
                    if (string.IsNullOrEmpty(starter))
                    {
                        starter = $"Ciao! Ho visto che sei esperto in {skill2} mentre io conosco bene {skill1}. Ti andrebbe di scambiarci qualche consiglio?";
                    }

                    // Cache per 30 minuti
                    _memoryCache.Set(cacheKey, starter, TimeSpan.FromMinutes(30));
                }

                _logger.LogInformation("Suggerimento generato: {Starter}", starter);
                return new JsonResult(new { starter });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'API di suggerimento conversazione");
                return new JsonResult(new
                {
                    starter = $"Ciao! Mi piacerebbe sapere come hai sviluppato le tue competenze in {skill2}, mentre io potrei condividere la mia esperienza in {skill1}."
                });
            }
        }

        // Importante: Questo handler risponde a /api/gemini?handler=suggestion
        public async Task<IActionResult> OnGetSuggestionAsync(string skills)
        {
            try
            {
                _logger.LogInformation("API: Richiesta suggerimenti per competenze: {Skills}", skills);

                if (string.IsNullOrEmpty(skills))
                {
                    return new JsonResult(new { error = "È necessario fornire almeno una competenza" });
                }

                var cacheKey = $"sugg_{skills}";
                if (!_memoryCache.TryGetValue(cacheKey, out string suggestion))
                {
                    suggestion = await _geminiService.GetSkillSuggestions(skills);

                    // Genera risposte diverse
                    if (string.IsNullOrEmpty(suggestion))
                    {
                        var suggestions = new[]
                        {
                            "Ti consiglio di approfondire le tue competenze esistenti e collegarle tra loro per creare una proposta di valore unica.",
                            "Prova ad aggiungere competenze complementari a quelle che già possiedi per offrire un pacchetto più completo.",
                            "Considera di specializzarti in un'area specifica delle tue competenze per diventare un esperto riconosciuto.",
                            "Le competenze che hai elencato mostrano versatilità. Potresti beneficiare dall'apprendere anche alcune basi di gestione dei progetti.",
                            "Basandomi sulle tue competenze, potresti provare ad esplorare anche l'ambito del coaching o della formazione."
                        };

                        var random = new Random();
                        suggestion = suggestions[random.Next(suggestions.Length)];
                    }

                    // Cache per 30 minuti
                    _memoryCache.Set(cacheKey, suggestion, TimeSpan.FromMinutes(30));
                }

                _logger.LogInformation("Suggerimento generato: {Suggestion}", suggestion);
                return new JsonResult(new { suggestion });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'API di suggerimenti personalizzati");
                Random random = new Random();
                var fallbacks = new[]
                {
                    "Per ottimizzare il tuo profilo, bilancia le competenze offerte con quelle richieste, cercando di creare sinergie tra i due gruppi.",
                    "Le tue competenze sono già ottime. Prova ad aggiungere una breve descrizione che spieghi come hai acquisito esperienza in ciascuna area.",
                    "Considera di aggiungere anche competenze trasversali al tuo profilo, come comunicazione o problem solving.",
                };
                return new JsonResult(new { suggestion = fallbacks[random.Next(fallbacks.Length)] });
            }
        }
    }
}