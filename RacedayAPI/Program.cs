using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

// 1. Add Session Services

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>

{

    options.IdleTimeout = TimeSpan.FromHours(2);

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;

});

// Add Database Context with corrected namespace

builder.Services.AddDbContext<RacedayAPI.Data.RaceDayDbContext>(options =>

    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())

{

    app.UseSwagger();

    app.UseSwaggerUI();

}

app.UseHttpsRedirection();

// 2. Enable Session Before Authorization

app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();
