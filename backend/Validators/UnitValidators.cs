using FluentValidation;
using TeensChurch.API.DTOs;

namespace TeensChurch.API.Validators;

public class CreateUnitValidator : AbstractValidator<CreateUnitDto>
{
    public CreateUnitValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Unit Name is required.")
            .MaximumLength(100).WithMessage("Unit Name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.BadgeColor)
            .MaximumLength(30).WithMessage("BadgeColor cannot exceed 30 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.BadgeColor));
    }
}

public class UpdateUnitValidator : AbstractValidator<UpdateUnitDto>
{
    public UpdateUnitValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Unit Name is required.")
            .MaximumLength(100).WithMessage("Unit Name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.BadgeColor)
            .MaximumLength(30).WithMessage("BadgeColor cannot exceed 30 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.BadgeColor));
    }
}
