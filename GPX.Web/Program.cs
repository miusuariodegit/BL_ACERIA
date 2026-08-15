using GPX.Web.Components;
using GPX.Web.Components.Account;
using GPX.Web.Data;
using GPX.Web.Utils;
using GPX.Negocio.COP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

// [GPX-DOC-v1] ================================================================================
// Punto de entrada de la aplicacion Blazor Server: configuracion de servicios, autenticacion (cookie +
// SSO Microsoft 365 opcional), Entity Framework Core, autorizacion dinamica por modulos y pipeline
// HTTP.
// ================================================================================================

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDevExpressBlazor();

builder.Services.AddAppServices();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();
builder.Services.AddScoped<CookieEvents>();
builder.Services.AddHttpClient();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "BL.SGPP.Auth";
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});


//  builders de proyecto OJO cologar no repetirlos



var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        })
    .ConfigureWarnings(warnings =>
        warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var microsoft365 = configuration.GetSection("Authentication:Microsoft365");
var tenantId = microsoft365["TenantId"];
var clientId = microsoft365["AppId"];
var clientSecret = microsoft365["ClientSecret"];

if(!string.IsNullOrWhiteSpace(tenantId) && !string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret)) {
    builder.Services.AddAuthentication()
        .AddOpenIdConnect("Microsoft365", "Microsoft 365", options => {
            options.SignInScheme = IdentityConstants.ExternalScheme;
            options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.CallbackPath = "/signin-microsoft365";
            options.SaveTokens = true;
            options.UsePkce = true;
            options.GetClaimsFromUserInfoEndpoint = false;
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.Scope.Add("offline_access");
            options.Scope.Add("User.Read");
            options.TokenValidationParameters = new TokenValidationParameters {
                NameClaimType = "name",
                RoleClaimType = "roles"
            };
            options.Events = new OpenIdConnectEvents {
                OnTokenValidated = context => {
                    var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                    var preferredUserName = context.Principal?.FindFirst("preferred_username")?.Value;
                    var email = context.Principal?.FindFirst("email")?.Value ?? preferredUserName;

                    if(identity != null && !string.IsNullOrWhiteSpace(email) && !identity.HasClaim(claim => claim.Type == System.Security.Claims.ClaimTypes.Email)) {
                        identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email));
                    }

                    if(identity != null && !string.IsNullOrWhiteSpace(preferredUserName) && !identity.HasClaim(claim => claim.Type == System.Security.Claims.ClaimTypes.NameIdentifier)) {
                        identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, preferredUserName));
                    }

                    return Task.CompletedTask;
                }
            };
        });
}

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, AppAuthorizationPolicyProvider>();
builder.Services.AddAuthorization();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.WebHost.UseStaticWebAssets();



// registro de tu capa de negocio OJO no Cagarla..
builder.Services.AddNegocio();


var app = builder.Build();

await ApplicationDbInitializer.InitializeAsync(app.Services);

string? pathBase = configuration.GetValue<string>("pathbase");
if(!string.IsNullOrEmpty(pathBase)) {
    string pathString = pathBase.StartsWith('/') ? pathBase : "/" + pathBase;
    app.UsePathBase(pathString);
}

app.UseRouting();

if(app.Environment.IsDevelopment()) {
    app.UseMigrationsEndPoint();
} else {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();
