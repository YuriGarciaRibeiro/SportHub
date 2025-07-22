# 📊 SQL Queries - SportHub

Esta pasta contém queries SQL úteis para administração e análise do banco de dados do SportHub.

## 📁 Arquivos Disponíveis

### 🔍 `queries-usuarios-estabelecimentos.sql`
**Queries principais para visualização de dados de usuários e estabelecimentos**

Contém 8 queries organizadas:
1. **Query Principal** - Usuários com estabelecimentos e roles (LEFT JOIN - mostra todos)
2. **Query Resumida** - Apenas usuários que são membros de estabelecimentos
3. **Contagem por Role Geral** - Estatísticas de UserRole
4. **Contagem por Role de Estabelecimento** - Estatísticas de EstablishmentRole
5. **Estabelecimentos com Usuários** - Visão por estabelecimento com contadores
6. **Usuários sem Estabelecimentos** - Usuários que não estão vinculados
7. **Usuários com Múltiplos Estabelecimentos** - Usuários com mais de um estabelecimento
8. **Dashboard de Estatísticas** - Métricas gerais do sistema

### 🛠️ `queries-admin-debug.sql`
**Queries para administração e debugging**

Contém 8 queries para:
1. **Buscar Usuário por Email** - Informações completas de um usuário específico
2. **Buscar Estabelecimento por Nome** - Informações completas de um estabelecimento
3. **Verificar Integridade** - Detectar problemas nos relacionamentos
4. **Últimos Cadastros** - Usuários cadastrados nos últimos 7 dias
5. **Estabelecimentos Órfãos** - Estabelecimentos sem usuários
6. **Relatório de Atividade** - Cadastros e logins por mês
7. **Usuários Admin** - Lista todos os administradores
8. **Referência para Reset de Senha** - Template para alteração de senhas

## 🚀 Como Usar

### 1. **Via PgAdmin (Recomendado)**
```bash
# Iniciar o ambiente Docker
docker-compose up -d

# Acessar PgAdmin em: http://localhost:5050
# Email: admin@admin.com
# Senha: admin
```

### 2. **Via linha de comando**
```bash
# Conectar ao PostgreSQL
docker exec -it sporthub-db-1 psql -U postgres -d SportHubDb

# Executar uma query específica
\i /path/to/query.sql
```

### 3. **Via VS Code com extensão PostgreSQL**
1. Instale uma extensão PostgreSQL (ex: PostgreSQL por Chris Kolkman)
2. Configure a conexão: `localhost:5432`, user: `postgres`, pass: `postgres`, db: `SportHubDb`
3. Execute as queries diretamente no VS Code

## 📋 Estrutura do Banco

```
Users
├── Id (Guid)
├── Email (string)
├── FirstName (string)  
├── LastName (string)
├── PasswordHash (string)
├── Salt (string)
├── Role (UserRole: Admin|EstablishmentMember|User)
├── CreatedAt (DateTime)
├── LastLoginAt (DateTime?)
└── IsActive (bool)

Establishments
├── Id (Guid)
├── Name (string)
├── Description (string)
├── PhoneNumber (string)
├── Email (string)
├── Website (string)
├── ImageUrl (string)
├── CreatedAt (DateTime)
└── Address (Address ValueObject)

EstablishmentUsers (Tabela de relacionamento N:M)
├── UserId (Guid) [FK -> Users.Id]
├── EstablishmentId (Guid) [FK -> Establishments.Id]
└── Role (EstablishmentRole: Owner|Manager|Staff)
```

## 🎯 Queries Mais Úteis

### Ver todos os usuários com seus estabelecimentos:
```sql
-- Query #1 do arquivo queries-usuarios-estabelecimentos.sql
SELECT u."FirstName" || ' ' || u."LastName" as "Nome", 
       u."Email", u."Role" as "Role_Geral",
       e."Name" as "Estabelecimento", eu."Role" as "Role_Estabelecimento"
FROM "Users" u
LEFT JOIN "EstablishmentUsers" eu ON u."Id" = eu."UserId"
LEFT JOIN "Establishments" e ON eu."EstablishmentId" = e."Id"
ORDER BY u."FirstName";
```

### Ver estatísticas gerais:
```sql
-- Query #8 do arquivo queries-usuarios-estabelecimentos.sql
SELECT 'Total de Usuários' as "Métrica", COUNT(*)::text as "Valor"
FROM "Users" WHERE "IsActive" = true
UNION ALL
SELECT 'Total de Estabelecimentos', COUNT(*)::text
FROM "Establishments";
```

## ⚠️ Observações

- Todas as queries usam aspas duplas para nomes de colunas/tabelas (padrão PostgreSQL)
- As queries são compatíveis com PostgreSQL 16+
- Teste sempre em ambiente de desenvolvimento antes de usar em produção
- Para queries de UPDATE/DELETE, sempre use WHERE com condições específicas

## 🔗 Links Úteis

- [Documentação PostgreSQL](https://www.postgresql.org/docs/)
- [PgAdmin Documentation](https://www.pgadmin.org/docs/)
- [Docker Compose SportHub](../docker-compose.yml)
