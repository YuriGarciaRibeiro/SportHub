namespace Domain.Enums;

public enum TenantStatus
{
    Active = 0,     // Operando normalmente
    Suspended = 1,  // Bloqueado (ex: inadimplência) — 403 em todas as requests
    Canceled = 2    // Encerrado — pode ser deletado futuramente
}
