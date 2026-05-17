namespace AaSmr.Api.Shared;

public sealed class ConflictException(string message) : Exception(message);
