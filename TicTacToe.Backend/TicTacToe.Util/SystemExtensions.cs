using Microsoft.Extensions.DependencyInjection;
using System;

// ReSharper disable once CheckNamespace
public static class SystemExtensions
{

    public static TService GetService<TService>(this IServiceProvider serviceProvider)
    {
        return (TService)serviceProvider.GetService(typeof(TService));
    }

    public static TService GetService<TService>(this IServiceScope serviceScope)
    {
        return (TService)serviceScope.ServiceProvider.GetService(typeof(TService));
    }

}
