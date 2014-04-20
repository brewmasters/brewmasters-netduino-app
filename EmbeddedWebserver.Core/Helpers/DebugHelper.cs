namespace EmbeddedWebserver.Core.Helpers
{
    public static class DebugHelper
    {
        public static void Print(string pMessage)
        {
            Microsoft.SPOT.Debug.Print(pMessage);
        }
    }
}
