using FluentValidation;

namespace Chickquita.Application.Features.EggSales.Commands.Create;

/// <summary>
/// Validator for CreateEggSaleCommand to ensure all validation rules are met.
/// </summary>
public sealed class CreateEggSaleCommandValidator : AbstractValidator<CreateEggSaleCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateEggSaleCommandValidator"/> class.
    /// </summary>
    public CreateEggSaleCommandValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Sale date is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.PricePerUnit)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price per unit cannot be negative.");

        RuleFor(x => x.BuyerName)
            .MaximumLength(100)
            .WithMessage("Buyer name must not exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.BuyerName));

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
