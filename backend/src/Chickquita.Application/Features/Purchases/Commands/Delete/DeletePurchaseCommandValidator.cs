using FluentValidation;

namespace Chickquita.Application.Features.Purchases.Commands.Delete;

/// <summary>
/// Validator for DeletePurchaseCommand to ensure all validation rules are met.
/// </summary>
public sealed class DeletePurchaseCommandValidator : AbstractValidator<DeletePurchaseCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePurchaseCommandValidator"/> class.
    /// </summary>
    public DeletePurchaseCommandValidator()
    {
        RuleFor(x => x.PurchaseId)
            .NotEmpty()
            .WithMessage("Purchase ID is required.");
    }
}
