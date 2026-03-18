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
            .WithMessage("Sale date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("Sale date cannot be in the future.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.PricePerUnit)
            .GreaterThan(0)
            .WithMessage("Price per unit must be greater than zero.");

        RuleFor(x => x.BuyerName)
            .MaximumLength(200)
            .WithMessage("Buyer name must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.BuyerName));

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes must not exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
