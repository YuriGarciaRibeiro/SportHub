-- =====================================================
-- QUERIES PARA VISUALIZAR USUÁRIOS E ESTABELECIMENTOS
-- =====================================================

-- 1. QUERY PRINCIPAL: Usuários com seus estabelecimentos e roles
SELECT 
    u."Id" as "UserId",
    u."Email" as "UserEmail",
    u."FirstName" || ' ' || u."LastName" as "FullName",
    u."Role" as "UserGeneralRole",
    u."IsActive" as "IsUserActive",
    u."CreatedAt" as "UserCreatedAt",
    u."LastLoginAt" as "LastLogin",
    
    -- Informações do estabelecimento (se houver)
    e."Id" as "EstablishmentId",
    e."Name" as "EstablishmentName",
    e."Email" as "EstablishmentEmail",
    e."PhoneNumber" as "EstablishmentPhone",
    eu."Role" as "EstablishmentRole",
    
    -- Status de membership
    CASE 
        WHEN eu."UserId" IS NOT NULL THEN 'Sim'
        ELSE 'Não'
    END as "IsMemberOfEstablishment"
    
FROM "Users" u
LEFT JOIN "EstablishmentUsers" eu ON u."Id" = eu."UserId"
LEFT JOIN "Establishments" e ON eu."EstablishmentId" = e."Id"
ORDER BY u."FirstName", u."LastName", e."Name";

-- =====================================================

-- 2. QUERY RESUMIDA: Apenas usuários com estabelecimentos
SELECT 
    u."FirstName" || ' ' || u."LastName" as "Nome_Usuario",
    u."Email" as "Email_Usuario",
    u."Role" as "Role_Geral",
    e."Name" as "Nome_Estabelecimento",
    eu."Role" as "Role_Estabelecimento"
FROM "Users" u
INNER JOIN "EstablishmentUsers" eu ON u."Id" = eu."UserId"
INNER JOIN "Establishments" e ON eu."EstablishmentId" = e."Id"
ORDER BY u."FirstName", e."Name";

-- =====================================================

-- 3. CONTAGEM DE USUÁRIOS POR ROLE GERAL
SELECT 
    "Role" as "Role_Geral",
    COUNT(*) as "Quantidade_Usuarios"
FROM "Users"
GROUP BY "Role"
ORDER BY COUNT(*) DESC;

-- =====================================================

-- 4. CONTAGEM DE USUÁRIOS POR ROLE DE ESTABELECIMENTO
SELECT 
    eu."Role" as "Role_Estabelecimento",
    COUNT(*) as "Quantidade_Usuarios"
FROM "EstablishmentUsers" eu
GROUP BY eu."Role"
ORDER BY COUNT(*) DESC;

-- =====================================================

-- 5. ESTABELECIMENTOS COM SEUS USUÁRIOS E ROLES
SELECT 
    e."Name" as "Nome_Estabelecimento",
    e."Email" as "Email_Estabelecimento",
    e."CreatedAt" as "Criado_Em",
    COUNT(eu."UserId") as "Total_Usuarios",
    COUNT(CASE WHEN eu."Role" = 'Owner' THEN 1 END) as "Owners",
    COUNT(CASE WHEN eu."Role" = 'Manager' THEN 1 END) as "Managers",
    COUNT(CASE WHEN eu."Role" = 'Staff' THEN 1 END) as "Staff"
FROM "Establishments" e
LEFT JOIN "EstablishmentUsers" eu ON e."Id" = eu."EstablishmentId"
GROUP BY e."Id", e."Name", e."Email", e."CreatedAt"
ORDER BY "Total_Usuarios" DESC, e."Name";

-- =====================================================

-- 6. USUÁRIOS ATIVOS SEM ESTABELECIMENTOS
SELECT 
    u."Id" as "UserId",
    u."FirstName" || ' ' || u."LastName" as "Nome_Completo",
    u."Email",
    u."Role" as "Role_Geral",
    u."CreatedAt" as "Cadastrado_Em"
FROM "Users" u
LEFT JOIN "EstablishmentUsers" eu ON u."Id" = eu."UserId"
WHERE u."IsActive" = true 
AND eu."UserId" IS NULL
ORDER BY u."CreatedAt" DESC;

-- =====================================================

-- 7. USUÁRIOS COM MÚLTIPLOS ESTABELECIMENTOS
SELECT 
    u."FirstName" || ' ' || u."LastName" as "Nome_Usuario",
    u."Email" as "Email_Usuario",
    COUNT(eu."EstablishmentId") as "Quantidade_Estabelecimentos",
    STRING_AGG(e."Name", ', ' ORDER BY e."Name") as "Estabelecimentos",
    STRING_AGG(eu."Role", ', ' ORDER BY e."Name") as "Roles_Respectivas"
FROM "Users" u
INNER JOIN "EstablishmentUsers" eu ON u."Id" = eu."UserId"
INNER JOIN "Establishments" e ON eu."EstablishmentId" = e."Id"
GROUP BY u."Id", u."FirstName", u."LastName", u."Email"
HAVING COUNT(eu."EstablishmentId") > 1
ORDER BY COUNT(eu."EstablishmentId") DESC;

-- =====================================================

-- 8. DASHBOARD - ESTATÍSTICAS GERAIS
SELECT 
    'Total de Usuários' as "Metrica",
    COUNT(*)::text as "Valor"
FROM "Users"
WHERE "IsActive" = true

UNION ALL

SELECT 
    'Total de Estabelecimentos' as "Metrica",
    COUNT(*)::text as "Valor"
FROM "Establishments"

UNION ALL

SELECT 
    'Usuários com Estabelecimentos' as "Metrica",
    COUNT(DISTINCT eu."UserId")::text as "Valor"
FROM "EstablishmentUsers" eu

UNION ALL

SELECT 
    'Usuários Ativos (últimos 30 dias)' as "Metrica",
    COUNT(*)::text as "Valor"
FROM "Users"
WHERE "LastLoginAt" >= CURRENT_DATE - INTERVAL '30 days'

ORDER BY "Metrica";
