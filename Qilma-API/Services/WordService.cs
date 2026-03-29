using CrypticWizard.RandomWordGenerator;
using Qilma_API.Services.Interfaces;

namespace Qilma_API.Services;

public class WordService : IWordService
{
    private readonly WordGenerator _wordGenerator;
    
    public WordService(WordGenerator wordGenerator)
    {
        _wordGenerator = wordGenerator;
    }

    public Task<string> GenerateRandomWordAsync()
    {
        string? word = null;
        int attempts = 0;
        const int maxAttempts = 300;
        while((word == null || word.Length != 5) && attempts < maxAttempts)
        {
            try
            {
                word = _wordGenerator.GetWord();
            }
            catch{}
            attempts++;
        }

        if (word == null || word.Length != 5)
        {
            throw new Exception ("Failed to generate a valid word");
        }

        return Task.FromResult(word);
    }
}
