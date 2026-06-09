using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SURVEY.Model.Common;
using SURVEY.Model.Models_SURVEY;
using SURVEY.Service.Configs.AutoMapper;
using System.Data;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var surveyConnection = builder.Configuration.GetConnectionString("SurveyConnection");
var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "";

builder.Services.AddDbContext<SURVEYContext>(options =>
    options.UseSqlServer(surveyConnection));

builder.Services.Configure<ConnectionStringOptions>(builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddTransient<IDbConnection>(sp => new SqlConnection(surveyConnection));

// Khai báo AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
