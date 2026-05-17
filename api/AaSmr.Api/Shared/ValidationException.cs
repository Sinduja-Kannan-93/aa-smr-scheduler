namespace AaSmr.Api.Shared;

public sealed class ValidationException(string message) : Exception(message);
