# Task 7.0: Migração de Dados Existentes

> **Complexidade**: MEDIUM  
> **Dependências**: Task 1.0, 2.0  
> **Estimativa**: 2-3 horas  
> **Status**: Pendente

---

## Objetivo

Migrar os dados existentes de roles antigos (`User`, `Admin`) para os novos valores (`Customer`, `Owner`), garantindo integridade e consistência dos dados.

---

## Contexto

Com a mudança do enum `UserRole`, os valores antigos precisam ser atualizados:
- `User` → `Customer`
- `Admin` → `Owner`
- `EstablishmentMember` → (verificar se existe, provavelmente não usado)
- `SuperAdmin` → mantém

Esta migração pode ser feita via EF Core Migration ou script SQL manual.

---

## Subtarefas

### 7.1 - Analisar Dados Existentes
**Descrição**: Verificar quais roles existem atualmente no banco de dados.

**Query de Análise**:
```sql
-- Para cada tenant schema
SELECT 
    "Role", 
    COUNT(*) as Total
FROM "Users"
GROUP BY "Role"
ORDER BY "Role";
```

**Ação**:
- Executar query em ambiente de desenvolvimento
- Documentar distribuição de roles
- Identificar valores inesperados

**Validação**:
- Saber exatamente quais roles existem e quantos users têm cada role

---

### 7.2 - Criar Migration de Dados
**Descrição**: Criar migration EF Core com script SQL para atualizar roles.

**Arquivos**:
- Nova migration em `src/SportHub.Infrastructure/Persistence/Migrations/`

**Comando**:
```bash
cd src/SportHub.Api
dotnet ef migrations add MigrateUserRolesToNewValues --project ../SportHub.Infrastructure
```

**Editar Migration Gerada**:
```csharp
public partial class MigrateUserRolesToNewValues : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Atualizar User → Customer (0 → 0, mesmo valor numérico)
        // Atualizar Admin → Owner (valor antigo → 3)
        // SuperAdmin mantém (99 → 99)
        
        // Nota: Como estamos usando enum, precisamos atualizar o valor numérico
        // User = 0 (antigo) → Customer = 0 (novo) - SEM MUDANÇA
        // Admin = valor antigo → Owner = 3
        
        // Se os valores numéricos mudaram, usar UPDATE
        migrationBuilder.Sql(@"
            -- Esta query precisa ser executada em CADA schema de tenant
            -- Por enquanto, vamos criar uma função que itera pelos schemas
            
            DO $$
            DECLARE
                schema_name TEXT;
            BEGIN
                FOR schema_name IN 
                    SELECT nspname 
                    FROM pg_namespace 
                    WHERE nspname NOT IN ('pg_catalog', 'information_schema', 'public')
                    AND nspname NOT LIKE 'pg_%'
                LOOP
                    -- Atualizar roles em cada schema de tenant
                    EXECUTE format('UPDATE %I.""Users"" SET ""Role"" = 3 WHERE ""Role"" = 2', schema_name);
                    -- Assumindo que Admin tinha valor 2, agora Owner = 3
                    -- Ajustar conforme valores reais do enum antigo
                END LOOP;
            END $$;
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Reverter mudanças se necessário
        migrationBuilder.Sql(@"
            DO $$
            DECLARE
                schema_name TEXT;
            BEGIN
                FOR schema_name IN 
                    SELECT nspname 
                    FROM pg_namespace 
                    WHERE nspname NOT IN ('pg_catalog', 'information_schema', 'public')
                    AND nspname NOT LIKE 'pg_%'
                LOOP
                    EXECUTE format('UPDATE %I.""Users"" SET ""Role"" = 2 WHERE ""Role"" = 3', schema_name);
                END LOOP;
            END $$;
        ");
    }
}
```

**IMPORTANTE**: Ajustar os valores numéricos conforme o enum antigo. Verificar quais eram os valores antes da mudança.

**Validação**:
- Migration criada
- Script SQL correto para multi-tenant

---

### 7.3 - Criar Script SQL Alternativo (Backup)
**Descrição**: Criar script SQL manual como alternativa à migration.

**Arquivos**:
- `src/SportHub.Infrastructure/Persistence/Migrations/Scripts/migrate_user_roles.sql` (novo)

**Script**:
```sql
-- Script de Migração de Roles
-- Executar ANTES de aplicar a migration da Task 1.0

-- 1. Backup (recomendado)
-- pg_dump -h localhost -U postgres -d sporthub > backup_before_role_migration.sql

-- 2. Verificar valores atuais
SELECT 
    schemaname,
    "Role", 
    COUNT(*) as Total
FROM (
    SELECT 
        schemaname,
        (SELECT "Role" FROM information_schema.tables WHERE table_schema = schemaname LIMIT 1) as "Role"
    FROM pg_tables 
    WHERE tablename = 'Users' 
    AND schemaname NOT IN ('pg_catalog', 'information_schema', 'public')
) subquery
GROUP BY schemaname, "Role";

-- 3. Atualizar roles em todos os schemas de tenant
DO $$
DECLARE
    schema_record RECORD;
BEGIN
    FOR schema_record IN 
        SELECT nspname 
        FROM pg_namespace 
        WHERE nspname NOT IN ('pg_catalog', 'information_schema', 'public')
        AND nspname NOT LIKE 'pg_%'
    LOOP
        RAISE NOTICE 'Atualizando schema: %', schema_record.nspname;
        
        -- User (0) → Customer (0) - SEM MUDANÇA
        -- Admin (valor antigo) → Owner (3)
        -- Ajustar conforme valores reais
        
        EXECUTE format('
            UPDATE %I."Users" 
            SET "Role" = 3 
            WHERE "Role" = 2
        ', schema_record.nspname);
        
        RAISE NOTICE 'Schema % atualizado', schema_record.nspname;
    END LOOP;
END $$;

-- 4. Verificar resultado
SELECT 
    schemaname,
    "Role", 
    COUNT(*) as Total
FROM (
    SELECT 
        t.schemaname,
        u."Role"
    FROM pg_tables t
    CROSS JOIN LATERAL (
        SELECT "Role" 
        FROM information_schema.tables 
        WHERE table_schema = t.schemaname 
        AND table_name = 'Users'
    ) u
    WHERE t.tablename = 'Users' 
    AND t.schemaname NOT IN ('pg_catalog', 'information_schema', 'public')
) subquery
GROUP BY schemaname, "Role"
ORDER BY schemaname, "Role";
```

**Validação**:
- Script SQL criado e documentado

---

### 7.4 - Testar Migration em Ambiente de Desenvolvimento
**Descrição**: Aplicar migration em ambiente de dev e validar resultados.

**Passos**:
1. Fazer backup do banco de desenvolvimento
2. Aplicar migration: `dotnet ef database update`
3. Verificar dados atualizados
4. Testar login com usuários migrados
5. Verificar que tokens JWT são gerados corretamente

**Queries de Validação**:
```sql
-- Verificar distribuição de roles após migração
SELECT "Role", COUNT(*) 
FROM "Users" 
GROUP BY "Role";

-- Verificar se não há roles inválidos
SELECT * 
FROM "Users" 
WHERE "Role" NOT IN (0, 1, 2, 3, 99);

-- Verificar owners por tenant
SELECT COUNT(*) as OwnerCount
FROM "Users"
WHERE "Role" = 3;
-- Deve retornar 1 por tenant
```

**Validação**:
- Migration aplicada com sucesso
- Dados migrados corretamente
- Nenhum role inválido
- Aplicação funciona normalmente

---

### 7.5 - Documentar Processo de Migração
**Descrição**: Criar documentação para aplicar em produção.

**Arquivos**:
- `src/SportHub.Infrastructure/Persistence/Migrations/Scripts/README_MIGRATION.md` (novo)

**Conteúdo**:
```markdown
# Migração de Roles - Guia de Produção

## Pré-requisitos
- Backup completo do banco de dados
- Janela de manutenção agendada
- Acesso ao banco de produção

## Passos

### 1. Backup
```bash
pg_dump -h <host> -U <user> -d sporthub > backup_$(date +%Y%m%d_%H%M%S).sql
```

### 2. Verificar Estado Atual
Execute o script de verificação em `verify_current_state.sql`

### 3. Aplicar Migration
Opção A - Via EF Core:
```bash
dotnet ef database update --project src/SportHub.Infrastructure
```

Opção B - Via Script SQL:
```bash
psql -h <host> -U <user> -d sporthub -f migrate_user_roles.sql
```

### 4. Validar Resultado
Execute queries de validação

### 5. Testar Aplicação
- Login de Customer
- Login de Owner
- Verificar permissões

### 6. Rollback (se necessário)
```bash
dotnet ef database update <previous_migration> --project src/SportHub.Infrastructure
```

## Impacto
- Tokens JWT existentes serão invalidados
- Usuários precisarão fazer login novamente
- Tempo estimado: 5-10 minutos
```

**Validação**:
- Documentação clara e completa

---

### 7.6 - Criar Testes de Validação Pós-Migração
**Descrição**: Criar testes automatizados para validar migração.

**Arquivos**:
- `tests/SportHub.Tests/Migrations/UserRoleMigrationTests.cs` (novo)

**Testes**:
```csharp
[Fact]
public async Task AfterMigration_AllUsersHaveValidRoles()
{
    // Arrange & Act
    var users = await _userRepository.GetAllAsync();

    // Assert
    var validRoles = new[] 
    { 
        UserRole.Customer, 
        UserRole.Staff, 
        UserRole.Manager, 
        UserRole.Owner, 
        UserRole.SuperAdmin 
    };
    
    Assert.All(users, user => Assert.Contains(user.Role, validRoles));
}

[Fact]
public async Task AfterMigration_EachTenantHasExactlyOneOwner()
{
    // Arrange & Act
    var owners = await _userRepository.GetByRoleAsync(UserRole.Owner);

    // Assert
    Assert.Single(owners); // Assumindo teste em um tenant
}

[Fact]
public async Task AfterMigration_NoUserRoleOrAdminRoleExists()
{
    // Arrange & Act
    var users = await _context.Users.ToListAsync();

    // Assert
    // Verificar que nenhum user tem valores antigos do enum
    Assert.DoesNotContain(users, u => (int)u.Role == 1); // Se 1 era User antigo
    Assert.DoesNotContain(users, u => (int)u.Role == 2); // Se 2 era Admin antigo
}
```

**Validação**:
- Testes compilam
- Testes passam após migração

---

## Testes

### Checklist de Validação Pós-Migração

- [ ] Todos os users têm roles válidos (Customer, Staff, Manager, Owner, SuperAdmin)
- [ ] Cada tenant tem exatamente 1 Owner
- [ ] Nenhum user tem role `User` ou `Admin` (valores antigos)
- [ ] Login funciona para Customer
- [ ] Login funciona para Owner
- [ ] JWT contém claim `Role` correto
- [ ] Permissions funcionam corretamente

---

## Critérios de Sucesso

- ✅ Migration criada e testada
- ✅ Script SQL alternativo disponível
- ✅ Dados migrados corretamente em dev
- ✅ Nenhum role inválido após migração
- ✅ Cada tenant tem 1 Owner
- ✅ Aplicação funciona normalmente após migração
- ✅ Documentação de produção criada
- ✅ Testes de validação passam

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Perda de dados durante migração | Baixa | Crítico | Backup obrigatório antes de migrar |
| Valores de enum incorretos | Média | Alto | Validar valores antes de criar script |
| Migration falha em produção | Baixa | Alto | Testar exaustivamente em dev/staging |
| Downtime prolongado | Baixa | Médio | Janela de manutenção + rollback plan |

---

## Notas para o Desenvolvedor

- **BACKUP OBRIGATÓRIO**: Sempre fazer backup antes de migrar
- **Valores do Enum**: Verificar valores numéricos do enum antigo antes de criar script
- **Multi-Tenant**: Script deve iterar por todos os schemas de tenant
- **Rollback Plan**: Ter plano de rollback testado
- **Comunicação**: Avisar usuários sobre invalidação de tokens

---

## Checklist de Conclusão

- [ ] Dados existentes analisados
- [ ] Migration EF Core criada
- [ ] Script SQL alternativo criado
- [ ] Migration testada em dev
- [ ] Dados validados após migração
- [ ] Documentação de produção criada
- [ ] Testes de validação criados
- [ ] Todos os testes passam
- [ ] Rollback plan testado
- [ ] Code review solicitado
