using FluentValidation;

namespace Chickquita.Application.Features.Purchases.Commands.Create;

/// <summary>
/// Validator for CreatePurchaseCommand to ensure all validation rules are met.
/// </summary>
public sealed class CreatePurchaseCommandValidator : AbstractValidator<CreatePurchaseCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePurchaseCommandValidator"/> class.
    /// </summary>
    public CreatePurchaseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Purchase name is required.")
            .MaximumLength(100)
            .WithMessage("Purchase name must not exceed 100 characters.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Purchase type must be a valid value.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount must be greater than or equal to zero.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.Unit)
            .IsInEnum()
            .WithMessage("Quantity unit must be a valid value.");

        RuleFor(x => x.PurchaseDate)
            .NotEmpty()
            .WithMessage("Purchase date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("Purchase date cannot be in the future.");

        RuleFor(x => x.ConsumedDate)
            .GreaterThanOrEqualTo(x => x.PurchaseDate)
            .WithMessage("Consumed date cannot be before purchase date.")
            .When(x => x.ConsumedDate.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
