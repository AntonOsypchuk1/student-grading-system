namespace SGS.IntegrationTests.Controllers.Helpers;

public static class TestContext
{
    public static bool SimulateUnauthorized { get; private set; }

    public static IDisposable SimulateUnauthorizedScope()
    {
        return new UnauthorizedScope();
    }
    
    public static void SimulationAuthorizedContext()
    {
        SimulateUnauthorized = false;
    }
    
    private class UnauthorizedScope : IDisposable
    {
        public UnauthorizedScope()
        {
            SimulateUnauthorized = true;
        }
        
        public void Dispose()
        {
            SimulateUnauthorized = false;
        }
    }
}