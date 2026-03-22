using FluentValidation;

namespace Chickquita.Application.Features.EggSales.Commands.Delete;

/// <summary>
/// Validator for DeleteEggSaleCommand to ensure all validation rules are met.
/// </summary>
public sealed class DeleteEggSaleCommandValidator : AbstractValidator<DeleteEggSaleCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteEggSaleCommandValidator"/> class.
    /// </summary>
    public DeleteEggSaleCommandValidator()
    {
        RuleFor(x => x.EggSaleId)
            .NotEmpty()
            .WithMessage("Egg sale ID is required.");
    }
}
