using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Weather_AppContext") ?? throw new InvalidOperationException("Connection string 'Weather_AppContext' not found.");

builder.Services.AddDbContext<Weather_AppContext>(options => options.UseSqlServer(connectionString));

var weatherSSApiKey = builder.Configuration["Weather_Site_Specific:ServiceApiKey"];
var weatherMapImgApiKey = builder.Configuration["Weather_Map_Images:ServiceApiKey"];
var weatherObsApiKey = builder.Configuration["Weather_Observations:ServiceApiKey"];

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
