namespace SoderiaLaNueva_Api.Services
{
    public class ServiceContainer
    {
        public static void AddServices(IServiceCollection services)
        {
            // General
            services.AddScoped<TokenService>();
            services.AddScoped<HomeService>();
            services.AddScoped<AuthService>();
            services.AddScoped<UserService>();
            services.AddScoped<RouteService>();
            services.AddScoped<CartService>();
            services.AddScoped<ProductService>();
            services.AddScoped<DealerService>();
        }
    }
}
