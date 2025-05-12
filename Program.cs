using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SkillSwap.Data;
using SkillSwap.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurazione database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurazione Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configura i cookie per Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});

// Aggiungi la cache di memoria
builder.Services.AddMemoryCache();

// Configurazione servizi HTTP
builder.Services.AddHttpClient<GeminiService>();
builder.Services.AddHttpClient<QuoteService>();

// Registrazione servizi custom
builder.Services.AddScoped<GeminiService>();
builder.Services.AddScoped<MatchingService>();
builder.Services.AddScoped<QuoteService>();

// Configurazione Razor Pages con mappature esplicite
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Profile");
    options.Conventions.AuthorizeFolder("/Messages");
    options.Conventions.AuthorizeFolder("/Match");
    options.Conventions.AuthorizeFolder("/Dashboard");
})
.AddRazorRuntimeCompilation(); // Aggiungi questa riga per abilitare la compilazione a runtime

builder.Services.AddAntiforgery(options => {
    options.HeaderName = "X-CSRF-TOKEN";
});

var app = builder.Build();

// Configurazione pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Aggiungi queste righe dopo app.UseStaticFiles()
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Disabilita la cache per le immagini durante lo sviluppo
        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
        ctx.Context.Response.Headers.Append("Expires", "-1");
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Middleware per contare i messaggi non letti e renderli disponibili nel layout
app.Use(async (context, next) => {
    if (context.User.Identity.IsAuthenticated)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var user = await userManager.GetUserAsync(context.User);
        if (user != null)
        {
            var userProfile = await dbContext.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (userProfile != null)
            {
                var unreadCount = await dbContext.Messages
                    .CountAsync(m => m.RecipientId == userProfile.Id && !m.IsRead);

                context.Items["UnreadMessagesCount"] = unreadCount;
                context.Items["UserProfileId"] = userProfile.Id;
            }
        }
    }

    await next();
});

// Solo questa configurazione è necessaria
app.MapRazorPages();
app.MapControllers();

app.Run();