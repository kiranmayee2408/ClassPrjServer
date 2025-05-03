namespace KiranmayeeServer.Dtos
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public required string Token { get; set; }
    }
}
