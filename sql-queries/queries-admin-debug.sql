-- =====================================================
-- QUERIES DE ADMINISTRAÇÃO E DEBUG
-- =====================================================

-- 1. BUSCAR USUÁRIO ESPECÍFICO POR EMAIL
SELECT 
    u."Id",
    u."FirstName" || ' ' || u."LastName" as "Nome_Completo",
    u."Email",
    u."Role" as "Role_Geral",
    u."IsActive" as "Ativo",
    u."CreatedAt" as "Criado_Em",
    u."LastLoginAt" as "Ultimo_Login",
    
    -- Estabelecimentos do usuário
    COALESCE(
        (SELECT STRING_AGG(e."Name" || ' (' || eu."Role" || ')', ', ')
         FROM "EstablishmentUsers" eu 
         JOIN "Establishments" e ON eu."EstablishmentId" = e."Id"
         WHERE eu."UserId" = u."Id"), 
        'Sem estabelecimentos'
    ) as "Estabelecimentos_E_Roles"
    
FROM "Users" u
WHERE u."Email" = 'admin@sporthub.com' -- Altere o email conforme necessário
ORDER BY u."CreatedAt" DESC;

-- =====================================================

-- 2. BUSCAR ESTABELECIMENTO ESPECÍFICO POR NOME
SELECT 
    e."Id",
    e."Name" as "Nome",
    e."Description" as "Descricao",
    e."Email",
    e."PhoneNumber" as "Telefone",
    e."CreatedAt" as "Criado_Em",
    
    -- Usuários do estabelecimento
    COALESCE(
        (SELECT STRING_AGG(u."FirstName" || ' ' || u."LastName" || ' (' || eu."Role" || ')', ', ')
         FROM "EstablishmentUsers" eu 
         JOIN "Users" u ON eu."UserId" = u."Id"
         WHERE eu."EstablishmentId" = e."Id"), 
        'Sem usuários'
    ) as "Usuarios_E_Roles"
    
FROM "Establishments" e
WHERE e."Name" ILIKE '%Academia%' -- Altere o nome conforme necessário
ORDER BY e."CreatedAt" DESC;

-- =====================================================

-- 3. VERIFICAR INTEGRIDADE DOS DADOS
SELECT 
    'EstablishmentUsers órfãos (usuário não existe)' as "Problema",
    COUNT(*) as "Quantidade"
FROM "EstablishmentUsers" eu
LEFT JOIN "Users" u ON eu."UserId" = u."Id"
WHERE u."Id" IS NULL

UNION ALL

SELECT 
    'EstablishmentUsers órfãos (estabelecimento não existe)' as "Problema",
    COUNT(*) as "Quantidade"
FROM "EstablishmentUsers" eu
LEFT JOIN "Establishments" e ON eu."EstablishmentId" = e."Id"
WHERE e."Id" IS NULL

UNION ALL

SELECT 
    'Usuários inativos' as "Problema",
    COUNT(*) as "Quantidade"
FROM "Users"
WHERE "IsActive" = false;

-- =====================================================

-- 4. ÚLTIMOS USUÁRIOS CADASTRADOS (últimos 7 dias)
SELECT 
    u."FirstName" || ' ' || u."LastName" as "Nome_Completo",
    u."Email",
    u."Role" as "Role_Geral",
    u."CreatedAt" as "Cadastrado_Em",
    u."LastLoginAt" as "Ultimo_Login",
    CASE 
        WHEN u."LastLoginAt" IS NULL THEN 'Nunca fez login'
        WHEN u."LastLoginAt" >= CURRENT_DATE - INTERVAL '1 day' THEN 'Último dia'
        WHEN u."LastLoginAt" >= CURRENT_DATE - INTERVAL '7 days' THEN 'Última semana'
        ELSE 'Mais de uma semana'
    END as "Status_Login"
FROM "Users" u
WHERE u."CreatedAt" >= CURRENT_DATE - INTERVAL '7 days'
ORDER BY u."CreatedAt" DESC;

-- =====================================================

-- 5. ESTABELECIMENTOS SEM USUÁRIOS (ÓRFÃOS)
SELECT 
    e."Id",
    e."Name" as "Nome_Estabelecimento",
    e."Email",
    e."CreatedAt" as "Criado_Em",
    'Sem usuários cadastrados' as "Status"
FROM "Establishments" e
LEFT JOIN "EstablishmentUsers" eu ON e."Id" = eu."EstablishmentId"
WHERE eu."EstablishmentId" IS NULL
ORDER BY e."CreatedAt" DESC;

-- =====================================================

-- 6. RELATÓRIO DE ATIVIDADE POR MÊS
SELECT 
    DATE_TRUNC('month', u."CreatedAt") as "Mes",
    COUNT(*) as "Usuarios_Cadastrados",
    COUNT(CASE WHEN u."LastLoginAt" IS NOT NULL THEN 1 END) as "Usuarios_Que_Fizeram_Login"
FROM "Users" u
WHERE u."CreatedAt" >= CURRENT_DATE - INTERVAL '6 months'
GROUP BY DATE_TRUNC('month', u."CreatedAt")
ORDER BY "Mes" DESC;

-- =====================================================

-- 7. USUÁRIOS COM ROLE DE ADMIN
SELECT 
    u."Id",
    u."FirstName" || ' ' || u."LastName" as "Nome_Completo",
    u."Email",
    u."CreatedAt" as "Criado_Em",
    u."LastLoginAt" as "Ultimo_Login",
    u."IsActive" as "Ativo"
FROM "Users" u
WHERE u."Role" = 'Admin'
ORDER BY u."CreatedAt";

-- =====================================================

-- 8. RESETAR SENHA DE USUÁRIO (para testes)
-- ATENÇÃO: Esta query é apenas para referência, 
-- o hash e salt devem ser gerados pela aplicação
/*
UPDATE "Users" 
SET 
    "PasswordHash" = 'novo_hash_aqui',
    "Salt" = 'novo_salt_aqui'
WHERE "Email" = 'usuario@exemplo.com';
*/
