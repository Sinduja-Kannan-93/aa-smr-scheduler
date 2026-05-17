namespace AaSmr.Api.Shared;

public sealed class NotFoundException(string message) : Exception(message);
