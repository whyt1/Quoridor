using static com.shephertz.app42.gaming.multiplayer.client.command.WarpResponseResultCode;
public static class WarpResponseResultCodeExtensions
{
    public static string ResultCodeToString(this byte resultCode)
    {
        return resultCode switch
        {
            SUCCESS => "SUCCESS",
            AUTH_ERROR => "AUTH_ERROR",
            RESOURCE_NOT_FOUND => "RESOURCE_NOT_FOUND",
            RESOURCE_MOVED => "RESOURCE_MOVED",
            BAD_REQUEST => "BAD_REQUEST",
            CONNECTION_ERR => "CONNECTION_ERR",
            UNKNOWN_ERROR => "UNKNOWN_ERROR",
            RESULT_SIZE_ERROR => "RESULT_SIZE_ERROR",
            SUCCESS_RECOVERED => "SUCCESS_RECOVERED",
            CONNECTION_ERROR_RECOVERABLE => "CONNECTION_ERROR_RECOVERABLE",
            USER_PAUSED_ERROR => "USER_PAUSED_ERROR",
            _ => "UNKNOWN_ERROR"
        };
    }
}