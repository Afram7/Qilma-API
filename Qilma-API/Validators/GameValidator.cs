using WeCantSpell.Hunspell;

namespace Qilma_API.Validators;

public class GameValidator
{
    private const int WORD_LENGTH = 5;
    private readonly WordList _wordList;
    
    public GameValidator(WordList wordList)
    {
        _wordList = wordList;
    }

    private (bool IsValid, string? ErrorMessage) ValidateWord(string word)
    {
        word = word.Trim();
        if (word.Length < WORD_LENGTH || string.IsNullOrWhiteSpace(word))
        {
            return (false, "Too short");
        }
        if (word.Length > WORD_LENGTH)
        {
            return (false, "Too long");
        }

        bool isCorrectWord = _wordList.Check(word.ToLower());
        if (!isCorrectWord)
        {
            return (false, "Word not recognized");
        }

        return (true, null);
    }

    // Validates that the guess word is a valid English word
    public (bool IsValid, string? ErrorMessage) ValidateGuessWord(string guessWord)
    {
        var wordResult = ValidateWord(guessWord);
        if (!wordResult.IsValid){
            return (false, wordResult.ErrorMessage);
        }
        
        return (true, null);
    }
}
