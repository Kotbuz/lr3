using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace UsersProxy
{
    public static class ServiceCollectionExtensions
    {
        // Подключение библиотеки к бэкенду: builder.Services.AddUsersProxy(builder.Configuration)
        public static IServiceCollection AddUsersProxy(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<UsersProxyOptions>(configuration.GetSection(UsersProxyOptions.SectionName));

            services.AddHttpClient<IUsersApiClient, UsersApiClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<UsersProxyOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

            services.AddSingleton<RequestAttemptTracker>();
            services.AddTransient<IUserAccessLogic, UserAccessLogic>();

            return services;
        }
    }
}
