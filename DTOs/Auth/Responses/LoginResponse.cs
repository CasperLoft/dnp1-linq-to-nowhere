namespace DTOs.Auth.Responses;

public record LoginResponse(string Token, DTOs.UserResponse User);
