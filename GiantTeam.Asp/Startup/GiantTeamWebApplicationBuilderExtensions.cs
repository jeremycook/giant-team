using GiantTeam.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Immutable;

namespace GiantTeam.Asp.Startup
{
    public static class GiantTeamWebApplicationBuilderExtensions
    {
        public static void ConfigureServicesWithServiceBuilders<TRootServiceBuilder>(this WebApplicationBuilder builder)
            where TRootServiceBuilder : IServiceBuilder
        {
            var rootServiceBuilder = typeof(TRootServiceBuilder);

            var serviceBuilderCollection = new ServiceCollection();

            // Add standard services
            serviceBuilderCollection.AddSingleton(builder.Services);
            serviceBuilderCollection.AddSingleton(builder.Environment);
            serviceBuilderCollection.AddSingleton<IHostEnvironment>(builder.Environment);
            serviceBuilderCollection.AddSingleton(builder.Configuration);
            serviceBuilderCollection.AddSingleton<IConfiguration>(builder.Configuration);
            serviceBuilderCollection.AddSingleton(builder.Logging);
            serviceBuilderCollection.AddSingleton(builder.Host);
            serviceBuilderCollection.AddSingleton<IHostBuilder>(builder.Host);
            serviceBuilderCollection.AddSingleton(builder.WebHost);
            serviceBuilderCollection.AddSingleton<IWebHostBuilder>(builder.WebHost);
            serviceBuilderCollection.AddLogging();

            var standardServiceTypes = serviceBuilderCollection
                .Select(s => s.ServiceType)
                .ToImmutableHashSet();

            // Discover service builders starting from the root service builder
            var serviceBuilderTypes = new List<Type>()
            {
                rootServiceBuilder,
            };
            GetDependentTypes(rootServiceBuilder, serviceBuilderTypes);

            // Remove the standard services
            serviceBuilderTypes.RemoveAll(sb => standardServiceTypes.Contains(sb));

            // Add service builders
            foreach (var type in serviceBuilderTypes)
            {
                serviceBuilderCollection.AddSingleton(type);
            }

            // Build the temporary container
            var serviceBuilderServices = serviceBuilderCollection.BuildServiceProvider();

            // Resolving the root service builder will cause all
            // service builders it depends on to be resolved in
            // the correct order, allowing them to do their work.
            // The result does not matter.
            serviceBuilderServices.GetService<TRootServiceBuilder>();
        }

        private static void GetDependentTypes(Type type, ICollection<Type> serviceBuilderTypes)
        {
            var candidates = type.GetConstructors()
                .SelectMany(ctor => ctor.GetParameters())
                .Select(p => p.ParameterType);

            foreach (var candidate in candidates)
            {
                if (serviceBuilderTypes.Contains(candidate))
                {
                    continue; // Skip circular dependencies
                }
                else
                {
                    serviceBuilderTypes.Add(candidate);
                    GetDependentTypes(candidate, serviceBuilderTypes);
                }
            }
        }
    }
}
