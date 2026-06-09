using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using SURVEY.Model.Common;
using SURVEY.Model.Models_SURVEY;
using SURVEY.Data.Repositories.Interfaces;
using SURVEY.Data.Repositories.Implementations;
using SURVEY.Service.Configs.AutoMapper;
using SURVEY.Service.Services.Implementations;
using System.Data;
using AutoMapper;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews().AddViewLocalization();

var surveyConnection = builder.Configuration.GetConnectionString("SurveyConnection");
var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "";

builder.Services.AddDbContext<SURVEYContext>(options =>
    options.UseSqlServer(surveyConnection));

builder.Services.Configure<ConnectionStringOptions>(builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddTransient<IDbConnection>(sp => new SqlConnection(surveyConnection));

builder.Services.AddScoped<IEmployeeEvaluationRepsitory, EmployeeEvaluationRepsitory>();
builder.Services.AddScoped<EmployeeEvaluationService>();

// Khai báo AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

var supportedCultures = new[]
{
    new CultureInfo("vi"),
    new CultureInfo("ja")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("vi"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRequestLocalization(localizationOptions);

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
