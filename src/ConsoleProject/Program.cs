using Microsoft.Extensions.DependencyInjection;

namespace ConsoleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = new ServiceCollection();
            var provider = s.BuildServiceProvider(true);
            var a = provider.GetRequiredService<Seva>();

        }
    }
    
    public class Seva {}
}