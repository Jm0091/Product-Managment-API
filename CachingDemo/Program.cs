
using CachingDemo.Data;
using CachingDemo.DTOs;
using CachingDemo.Models;
using CachingDemo.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Reflection;

namespace CachingDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add services to the container.
            builder.Services.AddControllers();

            // Setup Serilog logging
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            builder.Host.UseSerilog();

            // Register custom services for dependency injection
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICachingService, CachingService>();
            // FluentValidation - Register Validators manually
            builder.Services.AddScoped<IValidator<ProductDTO>, ProductValidator>();


            //Chaching
            builder.Services.AddMemoryCache();


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // Enable routing and logging
            app.UseRouting();
            app.UseSerilogRequestLogging();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.Run();
        }
    }
}