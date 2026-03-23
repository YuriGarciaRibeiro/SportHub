namespace SportHub.Domain.Common;

/// <summary>
/// Representa o horário de funcionamento de um dia da semana específico.
/// DayOfWeek segue o padrão .NET/JS: 0 = Domingo, 1 = Segunda ... 6 = Sábado.
/// </summary>
public class DailySchedule
{
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsOpen { get; set; }

    /// <summary>Horário de abertura no formato "HH:mm". Null se IsOpen = false.</summary>
    public string? OpenTime { get; set; }

    /// <summary>Horário de fechamento no formato "HH:mm". Null se IsOpen = false.</summary>
    public string? CloseTime { get; set; }
}
