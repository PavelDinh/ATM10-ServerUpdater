namespace CurseForgeAPI.Exceptions
{
    public class CurseForgeApiException(string message, Exception innerException) : Exception(message, innerException)
    {
    }
}
