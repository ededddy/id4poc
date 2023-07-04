namespace TestApi
{
    public class Startup
    {
        public IWebHostEnvironment _environment { get; }
        private readonly IConfiguration _configuration;
        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var origins = _configuration.GetSection("CorsPolicy:Origins").Get<string[]>();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    //builder.WithOrigins(origins)
                    //        .AllowAnyMethod()
                    //        .AllowAnyHeader()
                    //        .AllowCredentials();
                    builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                });
            });
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    _configuration.Bind("Authentication", options);
                });

            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app)
        {
            // Configure the HTTP request pipeline.
            if (_environment.IsDevelopment()) 
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();
            
            app.UseHttpsRedirection();
            
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}

