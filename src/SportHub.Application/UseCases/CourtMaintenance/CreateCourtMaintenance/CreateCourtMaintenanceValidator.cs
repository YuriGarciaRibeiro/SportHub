using Domain.Enums;
using FluentValidation;

namespace Application.UseCases.CourtMaintenance.CreateCourtMaintenance;

public class CreateCourtMaintenanceValidator : AbstractValidator<CreateCourtMaintenanceCommand>
{
    public CreateCourtMaintenanceValidator()
    {
        RuleFor(x => x.CourtId).NotEmpty();
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.EndTime).NotEmpty();
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("EndTime deve ser maior que StartTime");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .When(x => x.Description != null);

        // Recorrente: DayOfWeek obrigatório
        RuleFor(x => x.DayOfWeek)
            .NotNull()
            .WithMessage("DayOfWeek é obrigatório para manutenção recorrente")
            .When(x => x.Type == MaintenanceType.Recurring);

        // Pontual: Date obrigatório
        RuleFor(x => x.Date)
            .NotNull()
            .WithMessage("Date é obrigatório para manutenção pontual")
            .When(x => x.Type == MaintenanceType.OneTime);
    }
}
