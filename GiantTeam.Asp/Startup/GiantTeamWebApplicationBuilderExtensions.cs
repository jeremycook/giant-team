using GiantTeam.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GiantTeam.Asp.Startup
{
    public static class GiantTeamWebApplicationBuilderExtensions
    {

        public class ServiceDictionary : Dictionary<Type, object>
        {
            public ServiceDictionary Add<TService>(TService implementation)
            {
                if (implementation is null)
                {
                    throw new ArgumentNullException(nameof(implementation), "Null argument.");
                }

                Add(typeof(TService), implementation);

                return this;
            }
        }

        public static void ConfigureWithServiceBuilders<TRootServiceBuilder>(this WebApplicationBuilder builder)
            where TRootServiceBuilder : IServiceBuilder
        {
            var rootServiceBuilder = typeof(TRootServiceBuilder);

            var services = new ServiceDictionary()
                .Add<IServiceCollection>(builder.Services)
                .Add<IHostEnvironment>(builder.Environment)
                .Add<IConfiguration>(builder.Configuration);

            // Discover service builders starting from the root service builder
            var serviceBuilderTypes = new List<Type>()
            {
                rootServiceBuilder,
            };
            GetDependentTypes(rootServiceBuilder, serviceBuilderTypes);

            if (serviceBuilderTypes.Count != serviceBuilderTypes.Distinct().Count())
            {
                throw new InvalidOperationException("Duplicate service builder dependencies.");
            }

            // Resolve service builders in the correct order
            serviceBuilderTypes.RemoveAll(t => !typeof(IServiceBuilder).IsAssignableFrom(t));
            serviceBuilderTypes.Reverse();
            Console.WriteLine($"Applying Service Builders: {string.Join("\n\t", serviceBuilderTypes.Select(t => t.AssemblyQualifiedName))}");
            foreach (var type in serviceBuilderTypes)
            {
                var ctor = type.GetConstructors().Single();
                var args = ctor.GetParameters()
                    .Select(param => services.TryGetValue(param.ParameterType, out var service) ?
                        service :
                        throw new ArgumentException($"Unable to resolve ({param.ParameterType} {param.Name}) constructor parameter of the \"{type.AssemblyQualifiedName}\" service builder.", param.Name))
                    .ToArray();

                var serviceBuilder = (IServiceBuilder)Activator.CreateInstance(type, args)!;

                // Service builders can depend on other service builders
                services.Add(type, serviceBuilder);
            }
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
