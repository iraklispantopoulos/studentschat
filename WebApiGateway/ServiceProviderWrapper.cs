public static class ServiceProviderWrapper
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;        
    }
    public static IServiceProvider ServiceProvider { get; private set; }
}
