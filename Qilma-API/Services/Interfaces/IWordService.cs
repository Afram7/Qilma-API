namespace Qilma_API.Services.Interfaces;

public interface IWordService
{
    // Generates a random 5-letter valid english word
    Task<string> GenerateRandomWordAsync();
}
