using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebAPI;
using WebAPI.Models.Database;
using WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
DefaultTypeMap.MatchNamesWithUnderscores = true;
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("AppDB"));
});
//builder.Services.AddNpgsql<ApplicationContext>(builder.Configuration.GetConnectionString("AppDB"));
builder.Services.AddTransient(typeof(IUploadDocumentService),typeof(UploadDocumentService));
builder.Services.AddAutoMapper(typeof(Startup));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowOrigin",
        builder => {
            builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseCors("AllowOrigin");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
