using FluentValidation;

namespace HugeTextProcessing.Sorting.Configuration;

internal class SortOptionsValidator : AbstractValidator<SortOptions>
{
    public SortOptionsValidator()
    {
        RuleFor(x => x.ChunkLimit)
            .InclusiveBetween(SortOptions.MinChunkSize, SortOptions.MaxChunkSize)
            .WithMessage(SortOptions.ChunkErrorMessage);

        RuleFor(x => x.CommonSeparator).NotEmpty().MinimumLength(1);
    }
}
