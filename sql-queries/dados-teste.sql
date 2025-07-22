-- =====================================================
-- DADOS DE TESTE PARA O SPORTHUB
-- =====================================================
-- Execute estas queries após criar o banco e rodar as migrations
-- para ter dados de exemplo para testar as consultas

-- IMPORTANTE: Estas senhas são apenas para teste!
-- Em produção, use a API para criar usuários com hash seguro

-- =====================================================
-- 1. INSERIR USUÁRIOS DE TESTE
-- =====================================================

-- Usuário Admin (já criado automaticamente pelo seeder)
-- Email: admin@sporthub.com | Senha: Admin123!

-- Usuários regulares de exemplo
INSERT INTO "Users" ("Id", "Email", "FirstName", "LastName", "PasswordHash", "Salt", "Role", "IsActive", "CreatedAt") VALUES
('11111111-1111-1111-1111-111111111111', 'joao@example.com', 'João', 'Silva', 'hash_temporario_1', 'salt_1', 'User', true, '2024-01-15 10:30:00'),
('22222222-2222-2222-2222-222222222222', 'maria@example.com', 'Maria', 'Santos', 'hash_temporario_2', 'salt_2', 'EstablishmentMember', true, '2024-02-20 14:45:00'),
('33333333-3333-3333-3333-333333333333', 'pedro@example.com', 'Pedro', 'Costa', 'hash_temporario_3', 'salt_3', 'EstablishmentMember', true, '2024-03-10 09:15:00'),
('44444444-4444-4444-4444-444444444444', 'ana@example.com', 'Ana', 'Oliveira', 'hash_temporario_4', 'salt_4', 'User', true, '2024-04-05 16:20:00'),
('55555555-5555-5555-5555-555555555555', 'carlos@example.com', 'Carlos', 'Ferreira', 'hash_temporario_5', 'salt_5', 'EstablishmentMember', true, '2024-05-12 11:00:00');

-- =====================================================
-- 2. INSERIR ESTABELECIMENTOS DE TESTE
-- =====================================================

INSERT INTO "Establishments" ("Id", "Name", "Description", "PhoneNumber", "Email", "Website", "ImageUrl", "CreatedAt", "Address_Street", "Address_Number", "Address_Complement", "Address_Neighborhood", "Address_City", "Address_State", "Address_ZipCode") VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Academia Fitness Pro', 'A melhor academia da região com equipamentos modernos', '(11) 99999-1111', 'contato@fitnesspro.com', 'https://fitnesspro.com', 'https://example.com/academia1.jpg', '2024-01-20 08:00:00', 'Rua das Flores', '123', 'Térreo', 'Centro', 'São Paulo', 'SP', '01234-567'),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'CrossFit Warriors', 'Treinamento funcional de alta intensidade', '(11) 99999-2222', 'info@warriors.com', 'https://warriors.com', 'https://example.com/crossfit1.jpg', '2024-02-15 10:30:00', 'Avenida dos Esportes', '456', 'Sala 2', 'Vila Esportiva', 'São Paulo', 'SP', '02345-678'),
('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Pilates Zen Studio', 'Estúdio especializado em Pilates e bem-estar', '(11) 99999-3333', 'zen@pilates.com', 'https://pilateszen.com', 'https://example.com/pilates1.jpg', '2024-03-05 15:45:00', 'Rua da Paz', '789', '', 'Jardim Tranquilo', 'São Paulo', 'SP', '03456-789');

-- =====================================================
-- 3. RELACIONAR USUÁRIOS COM ESTABELECIMENTOS
-- =====================================================

-- Maria é Owner da Academia Fitness Pro
INSERT INTO "EstablishmentUsers" ("UserId", "EstablishmentId", "Role") VALUES
('22222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Owner');

-- Pedro é Manager do CrossFit Warriors
INSERT INTO "EstablishmentUsers" ("UserId", "EstablishmentId", "Role") VALUES
('33333333-3333-3333-3333-333333333333', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Manager');

-- Carlos é Staff do Pilates Zen Studio
INSERT INTO "EstablishmentUsers" ("UserId", "EstablishmentId", "Role") VALUES
('55555555-5555-5555-5555-555555555555', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Staff');

-- Carlos também é Manager da Academia Fitness Pro (exemplo de usuário com múltiplos estabelecimentos)
INSERT INTO "EstablishmentUsers" ("UserId", "EstablishmentId", "Role") VALUES
('55555555-5555-5555-5555-555555555555', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Manager');

-- =====================================================
-- 4. ATUALIZAR ALGUNS LOGINS PARA SIMULAR ATIVIDADE
-- =====================================================

UPDATE "Users" SET "LastLoginAt" = '2024-07-22 09:30:00' WHERE "Email" = 'maria@example.com';
UPDATE "Users" SET "LastLoginAt" = '2024-07-21 14:15:00' WHERE "Email" = 'pedro@example.com';
UPDATE "Users" SET "LastLoginAt" = '2024-07-20 16:45:00' WHERE "Email" = 'carlos@example.com';
-- João e Ana nunca fizeram login

-- =====================================================
-- QUERIES DE VERIFICAÇÃO
-- =====================================================

-- Verificar se os dados foram inseridos corretamente
SELECT 'Usuários inseridos' as "Tabela", COUNT(*) as "Total" FROM "Users"
UNION ALL
SELECT 'Estabelecimentos inseridos', COUNT(*) FROM "Establishments"  
UNION ALL
SELECT 'Relacionamentos inseridos', COUNT(*) FROM "EstablishmentUsers";

-- Ver resultado da query principal
SELECT 
    u."FirstName" || ' ' || u."LastName" as "Nome",
    u."Email",
    u."Role" as "Role_Geral",
    e."Name" as "Estabelecimento",
    eu."Role" as "Role_Estabelecimento",
    CASE WHEN u."LastLoginAt" IS NULL THEN 'Nunca' ELSE u."LastLoginAt"::text END as "Ultimo_Login"
FROM "Users" u
LEFT JOIN "EstablishmentUsers" eu ON u."Id" = eu."UserId"
LEFT JOIN "Establishments" e ON eu."EstablishmentId" = e."Id"
ORDER BY u."FirstName";

-- =====================================================
-- LIMPEZA (se necessário)
-- =====================================================

/*
-- Execute apenas se quiser limpar os dados de teste:

DELETE FROM "EstablishmentUsers" WHERE "UserId" IN (
    '11111111-1111-1111-1111-111111111111',
    '22222222-2222-2222-2222-222222222222',
    '33333333-3333-3333-3333-333333333333',
    '44444444-4444-4444-4444-444444444444',
    '55555555-5555-5555-5555-555555555555'
);

DELETE FROM "Users" WHERE "Id" IN (
    '11111111-1111-1111-1111-111111111111',
    '22222222-2222-2222-2222-222222222222',
    '33333333-3333-3333-3333-333333333333',
    '44444444-4444-4444-4444-444444444444',
    '55555555-5555-5555-5555-555555555555'
);

DELETE FROM "Establishments" WHERE "Id" IN (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    'cccccccc-cccc-cccc-cccc-cccccccccccc'
);
*/
