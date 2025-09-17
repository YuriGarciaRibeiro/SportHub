# 🏟️ SportHub

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791.svg)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED.svg)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

> 🌐 **Idiomas**: [English](README_EN.md) | [Português](README_PT.md)

## 📝 Descrição

**SportHub** é uma plataforma de gerenciamento de estabelecimentos esportivos que permite a administração de usuários, estabelecimentos e suas respectivas hierarquias de permissões. O sistema foi desenvolvido seguindo os princípios da Clean Architecture, utilizando .NET 9 e PostgreSQL.

### ✨ Funcionalidades Principais

- 🔐 **Sistema de Autenticação JWT** com roles hierárquicos
- 🏢 **Gerenciamento de Estabelecimentos** com informações detalhadas
- 👥 **Sistema de Usuários** com roles diferenciados (Staff, Manager, Owner)
- 🔒 **Autorização baseada em Policies** com políticas personalizadas
- 📊 **API RESTful** documentada com OpenAPI/Swagger
- 🐳 **Deploy com Docker** para fácil execução
- 📈 **Logs Estruturados** com Serilog
- 🧪 **Testes Unitários** e validação com FluentValidation

## 🏗️ Arquitetura

O projeto segue os princípios da **Clean Architecture** com a seguinte estrutura:

```
SportHub/
├── src/
│   ├── SportHub.Api/              # 🌐 Camada de Apresentação (Web API)
│   ├── SportHub.Application/      # 📋 Camada de Aplicação (Use Cases, CQRS)
│   ├── SportHub.Domain/           # 🏛️ Camada de Domínio (Entidades, Value Objects)
│   └── SportHub.Infrastructure/   # 🔧 Camada de Infraestrutura (Persistência, Serviços)
├── tests/
│   └── SportHub.Tests/            # 🧪 Testes Unitários
├── sql-queries/                   # 📊 Consultas SQL para administração
└── docker-compose.yml             # 🐳 Configuração Docker
```

### 📦 Camadas da Aplicação

#### 🌐 **SportHub.Api** (Camada de Apresentação)
- **Endpoints**: Definições das rotas da API
- **Middleware**: Tratamento de exceções e autenticação
- **Configuration**: Configuração da aplicação e dependências
- **Documentation**: Transformadores da documentação OpenAPI

#### 📋 **SportHub.Application** (Camada de Aplicação)
- **Padrão CQRS**: Commands e Queries com MediatR
- **Use Cases**: Lógica de negócio organizada por funcionalidade
- **Security**: Políticas de autorização e requirements
- **Behaviors**: Comportamentos de pipeline para validação e logging
- **Services**: Serviços da aplicação (JWT, Establishment, etc.)

#### 🏛️ **SportHub.Domain** (Camada de Domínio)
- **Entities**: Entidades principais do domínio
- **Value Objects**: Objetos de valor (Address, etc.)
- **Enums**: Enumerações do domínio

#### 🔧 **SportHub.Infrastructure** (Camada de Infraestrutura)
- **Persistence**: Configuração do Entity Framework
- **Repositories**: Implementações dos repositórios
- **Services**: Implementações de serviços externos
- **Security**: Implementações de segurança

## 🚀 Tecnologias Utilizadas

### Backend
- **.NET 9** - Framework principal
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco de dados
- **MediatR** - Padrão Mediator/CQRS
- **FluentValidation** - Validação de dados
- **JWT Bearer** - Autenticação
- **Serilog** - Logs estruturados

### Documentação & DevOps
- **OpenAPI/Swagger** - Documentação da API
- **Scalar** - Interface moderna de documentação
- **Docker & Docker Compose** - Containerização
- **pgAdmin** - Administração PostgreSQL

### Testes
- **xUnit** - Framework de testes
- **FluentAssertions** - Asserções fluentes

## 📋 Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker & Docker Compose](https://www.docker.com/get-started)
- [PostgreSQL 16](https://www.postgresql.org/) (opcional - pode usar via Docker)

## 🛠️ Instalação e Execução

### 🐳 **Opção 1: Usando Docker (Recomendado)**

1. **Clone o repositório**
```bash
git clone https://github.com/YuriGarciaRibeiro/SportHub.git
cd SportHub
```

2. **Execute com Docker Compose**
```bash
docker-compose up -d
```

3. **Acesse a aplicação**
- API: http://localhost:5000
- Documentação: http://localhost:5000/scalar/v1
- pgAdmin: http://localhost:8080 (admin@admin.com / admin)

### 💻 **Opção 2: Execução Local**

1. **Clone e configure o ambiente**
```bash
git clone https://github.com/YuriGarciaRibeiro/SportHub.git
cd SportHub
```

2. **Configure o banco de dados**
```bash
# Execute PostgreSQL via Docker
docker run --name sporthub-db -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=SportHubDb -p 5432:5432 -d postgres:16
```

3. **Execute as migrações**
```bash
cd src/SportHub.Api
dotnet ef database update
```

4. **Execute a aplicação**
```bash
dotnet run
```

## 📚 Uso da API

### 🔐 **Autenticação**

A API utiliza JWT Bearer Token para autenticação. Para acessar endpoints protegidos:

1. **Registrar um usuário**
```http
POST /auth/register
Content-Type: application/json

{
  "email": "usuario@exemplo.com",
  "password": "SuaSenhaSegura123!",
  "firstName": "Primeiro",
  "lastName": "Último"
}
```

2. **Fazer login**
```http
POST /auth/login
Content-Type: application/json

{
  "email": "usuario@exemplo.com",
  "password": "SuaSenhaSegura123!"
}
```

3. **Usar o token retornado**
```http
Authorization: Bearer <seu-jwt-token>
```

### 🏢 **Gerenciamento de Estabelecimentos**

```http
# Criar estabelecimento
POST /establishments
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Academia Central",
  "description": "Academia completa no centro da cidade",
  "phoneNumber": "(11) 99999-9999",
  "email": "contato@academiacentral.com",
  "website": "https://academiacentral.com",
  "imageUrl": "https://exemplo.com/imagem.jpg",
  "address": {
    "street": "Rua Principal, 123",
    "city": "São Paulo",
    "state": "SP",
    "country": "Brasil",
    "zipCode": "01234-567"
  }
}

# Listar estabelecimentos do usuário
GET /establishments/owner
Authorization: Bearer <token>

# Obter estabelecimento por ID
GET /establishments/{id}
```

### 👥 **Sistema de Roles**

O sistema possui três níveis hierárquicos de permissões:

- **👤 Staff**: Funcionários com acesso básico
- **👔 Manager**: Gerentes com permissões administrativas
- **👑 Owner**: Proprietários com acesso total

## 🔧 Configuração

### ⚙️ **Variáveis de Ambiente**

```env
# Banco de dados
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=SportHubDb;Username=postgres;Password=postgres

# JWT
JwtSettings__SecretKey=SuaChaveSecretaAqui
JwtSettings__Issuer=SportHub
JwtSettings__Audience=SportHub
JwtSettings__ExpirationMinutes=60

# Usuário Admin
AdminUserSettings__Email=admin@sporthub.com
AdminUserSettings__Password=Admin123!
AdminUserSettings__FirstName=Admin
AdminUserSettings__LastName=User
```

### 📁 **Estrutura de Configuração**

```
appsettings.json              # Configurações base
appsettings.Development.json  # Configurações de desenvolvimento
appsettings.Production.json   # Configurações de produção
```

## 🌱 Dados de Teste (Seeders)

A aplicação cria automaticamente dados de teste abrangentes durante a inicialização, incluindo usuários, estabelecimentos esportivos, quadras, reservas e avaliações. Estes dados permitem testes imediatos e demonstração de todas as funcionalidades do sistema.

**📋 Para detalhes completos sobre todos os dados do seeder, veja [SEEDERS_EN.md](SEEDERS_EN.md) | [SEEDERS_PT.md](SEEDERS_PT.md)**

### 🔑 Acesso Rápido
- **Usuário Admin**: Configurado via `appsettings.json`
- **Usuários de Teste**: 8 usuários com diferentes roles (membros + usuários regulares)
- **Estabelecimentos**: 25+ estabelecimentos esportivos em Aracaju/SE
- **Esportes**: 8 modalidades esportivas diferentes
- **Quadras**: 200+ quadras geradas automaticamente
- **Reservas**: Reservas dinâmicas para todos os usuários
- **Avaliações**: Ratings realistas e comentários para todos os estabelecimentos

Todos os usuários de teste usam a senha: `SportHub123!`

## 📊 Monitoramento e Logs

### 📝 **Serilog**
O sistema utiliza Serilog para logs estruturados com os seguintes sinks:
- Console (desenvolvimento)
- Arquivo (produção)
- Formato JSON compacto

### 📈 **Métricas**
Os logs estruturados incluem:
- Requisições HTTP
- Erros e exceções
- Performance de queries
- Eventos de domínio

## 🧪 Testes

```bash
# Executar todos os testes
dotnet test

# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Executar testes específicos
dotnet test --filter "Category=Unit"
```

## 📊 Consultas Administrativas

O projeto inclui um conjunto de consultas SQL úteis para administração:

### 📁 `sql-queries/`
- **`queries-usuarios-estabelecimentos.sql`** - Consultas de usuários e estabelecimentos
- **`queries-admin-debug.sql`** - Consultas de debug e administração
- **`dados-teste.sql`** - Dados de teste para desenvolvimento

### 🔍 **Principais Consultas**
1. Usuários com estabelecimentos e roles
2. Estatísticas de roles por estabelecimento
3. Estabelecimentos com contagem de usuários
4. Dashboard de métricas gerais

## 🤝 Contribuindo

1. **Faça um Fork do projeto**
2. **Crie uma branch para sua feature** (`git checkout -b feature/MinhaFeature`)
3. **Commit suas mudanças** (`git commit -m 'Adiciona MinhaFeature'`)
4. **Push para a branch** (`git push origin feature/MinhaFeature`)
5. **Abra um Pull Request**

### 📋 **Diretrizes de Contribuição**
- Siga os princípios SOLID e Clean Architecture
- Escreva testes para novas funcionalidades
- Mantenha o código documentado
- Use conventional commits
- Atualize a documentação quando necessário

## 📈 Roadmap

### 🚧 **Próximas Funcionalidades**
- [ ] Sistema de notificações
- [ ] Upload de imagens
- [ ] Relatórios avançados
- [ ] API de geolocalização
- [ ] Sistema de avaliações
- [ ] Integração de pagamentos
- [ ] Aplicativo mobile

### 🔄 **Melhorias Técnicas**
- [ ] Implementar Health Checks
- [ ] Adicionar Redis para cache
- [ ] Configurar CI/CD
- [ ] Implementar rate limiting
- [ ] Adicionar métricas Prometheus
- [ ] Configurar observabilidade completa

## 📄 Licença

Este projeto está licenciado sob a Licença MIT. Veja o arquivo [LICENSE](LICENSE) para detalhes.

## 👨‍💻 Autor

**Yuri Garcia Ribeiro**
- GitHub: [@YuriGarciaRibeiro](https://github.com/YuriGarciaRibeiro)
- LinkedIn: [Yuri Garcia Ribeiro](https://linkedin.com/in/yurigarciaribeiro)

## 🙏 Agradecimentos

- Comunidade .NET pelas contribuições
- Mantenedores das bibliotecas open source utilizadas
- Membros da equipe que colaboraram com feedback e sugestões

---

⭐ **Se este projeto foi útil para você, considere dar uma estrela no GitHub!**