using FluentValidation;
using TeensChurch.API.DTOs;

namespace TeensChurch.API.Validators;

public class CreateMemberValidator : AbstractValidator<CreateMemberDto>
{
    public CreateMemberValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FullName is required.")
            .MaximumLength(100).WithMessage("FullName cannot exceed 100 characters.");

        RuleFor(x => x.ServiceTime)
            .NotEmpty().WithMessage("ServiceTime is required.")
            .MaximumLength(20).WithMessage("ServiceTime cannot exceed 20 characters.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("PhoneNumber cannot exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.AcademicLevel)
            .MaximumLength(50).WithMessage("AcademicLevel cannot exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.AcademicLevel));

        RuleFor(x => x.Age)
            .InclusiveBetween(5, 35).WithMessage("Age must be between 5 and 35.")
            .When(x => x.Age.HasValue);

        RuleFor(x => x.GuardianName)
            .MaximumLength(100).WithMessage("GuardianName cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.GuardianName));
    }
}

public class UpdateMemberValidator : AbstractValidator<UpdateMemberDto>
{
    public UpdateMemberValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FullName is required.")
            .MaximumLength(100).WithMessage("FullName cannot exceed 100 characters.");

        RuleFor(x => x.ServiceTime)
            .NotEmpty().WithMessage("ServiceTime is required.")
            .MaximumLength(20).WithMessage("ServiceTime cannot exceed 20 characters.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("PhoneNumber cannot exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.AcademicLevel)
            .MaximumLength(50).WithMessage("AcademicLevel cannot exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.AcademicLevel));

        RuleFor(x => x.Age)
            .InclusiveBetween(5, 35).WithMessage("Age must be between 5 and 35.")
            .When(x => x.Age.HasValue);

        RuleFor(x => x.GuardianName)
            .MaximumLength(100).WithMessage("GuardianName cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.GuardianName));
    }
}
