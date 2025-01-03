using CurseForgeAPI;
using CurseForgeAPI.Config;
using Microsoft.AspNetCore.Authentication.Negotiate;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<ICurseForgeClient, CurseForgeClient>();

builder.Services.AddHttpClient();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

builder.Services.Configure<CurseForgeConfig>(builder.Configuration.GetSection("CurseForgeConfig"));

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
