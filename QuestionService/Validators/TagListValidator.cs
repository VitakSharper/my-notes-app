using System.ComponentModel.DataAnnotations;

namespace QuestionService.Validators;

public class TagListValidator(int min, int max) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not List<string> tags)
            return new ValidationResult("Invalid tag list format.");

        if (tags.Count < min || tags.Count > max)
            return new ValidationResult($"The number of tags must be between {min} and {max}.");

        return tags.Any(string.IsNullOrWhiteSpace) ? new ValidationResult("Tags cannot be null or whitespace.") : ValidationResult.Success;
    }
}