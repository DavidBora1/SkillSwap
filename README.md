# SkillSwap - Piattaforma di Scambio Competenze

## Descrizione
SkillSwap è una web app dove gli utenti possono offrire e richiedere competenze, con un sistema di matching intelligente che collega persone con competenze complementari.

## Caratteristiche principali
- 👤 **Sistema di autenticazione** completo con ASP.NET Core Identity
- 🧠 **Profilo utente** personalizzabile con competenze offerte/richieste
- 🔍 **Algoritmo di matching** per suggerire utenti compatibili
- 📬 **Sistema di messaggistica** per la comunicazione tra utenti
- 🤖 **Integrazione Google Gemini AI** per migliorare descrizioni e generare suggerimenti
- 🌐 **Design responsive** con Bootstrap e animazioni CSS
- 📝 **API di citazioni** per mostrare contenuti motivazionali

## Tecnologie utilizzate
- ASP.NET Core 7
- Entity Framework Core
- SQL Server
- Bootstrap 5
- Google Gemini API
- ZenQuotes API

## Struttura del progetto
- **/Pages**: Razor Pages per l'interfaccia utente
- **/Models**: Classi di dominio (Skill, UserProfile, Message)
- **/Data**: Contesto Entity Framework e configurazione database
- **/Services**: Servizi per integrazioni esterne (Gemini, Quote)
- **/wwwroot**: File statici (CSS, JavaScript, immagini)

## Installazione e avvio
1. Clona il repository
2. Configura la stringa di connessione in appsettings.json
3. Esegui le migrazioni: `dotnet ef database update`
4. Avvia l'applicazione: `dotnet run`
