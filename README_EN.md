# 🏟️ SportHub

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791.svg)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED.svg)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

> 🌐 **Languages**: [English](README_EN.md) | [Português](README_PT.md)

## 📝 Description

**SportHub** is a sports establishment management platform that enables administration of users, establishments, and their respective permission hierarchies. The system was developed following Clean Architecture principles, using .NET 9 and PostgreSQL.

### ✨ Key Features

- 🔐 **JWT Authentication System** with hierarchical roles
- 🏢 **Establishment Management** with detailed information
- 👥 **User System** with differentiated roles (Staff, Manager, Owner)
- 🔒 **Policy-based Authorization** with custom policies
- 📊 **RESTful API** documented with OpenAPI/Swagger
- 🐳 **Docker Deployment** for easy execution
- 📈 **Structured Logging** with Serilog
- 🧪 **Unit Testing** and validation with FluentValidation

## 🏗️ Architecture

The project follows **Clean Architecture** principles with the following structure:

```
SportHub/
├── src/
│   ├── SportHub.Api/              # 🌐 Presentation Layer (Web API)
│   ├── SportHub.Application/      # 📋 Application Layer (Use Cases, CQRS)
│   ├── SportHub.Domain/           # 🏛️ Domain Layer (Entities, Value Objects)
│   └── SportHub.Infrastructure/   # 🔧 Infrastructure Layer (Persistence, Services)
├── tests/
│   └── SportHub.Tests/            # 🧪 Unit Tests
├── sql-queries/                   # 📊 SQL Queries for administration
└── docker-compose.yml             # 🐳 Docker Configuration
```

### 📦 Application Layers

#### 🌐 **SportHub.Api** (Presentation Layer)
- **Endpoints**: API route definitions
- **Middleware**: Exception handling and authentication
- **Configuration**: Application and dependency configuration
- **Documentation**: OpenAPI documentation transformers

#### 📋 **SportHub.Application** (Application Layer)
- **CQRS Pattern**: Commands and Queries with MediatR
- **Use Cases**: Business logic organized by functionality
- **Security**: Authorization policies and requirements
- **Behaviors**: Pipeline behaviors for validation and logging
- **Services**: Application services (JWT, Establishment, etc.)

#### 🏛️ **SportHub.Domain** (Domain Layer)
- **Entities**: Main domain entities
- **Value Objects**: Value objects (Address, etc.)
- **Enums**: Domain enumerations

#### 🔧 **SportHub.Infrastructure** (Infrastructure Layer)
- **Persistence**: Entity Framework configuration
- **Repositories**: Repository implementations
- **Services**: External service implementations
- **Security**: Security implementations

## 🚀 Technologies Used

### Backend
- **.NET 9** - Main framework
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database
- **MediatR** - Mediator/CQRS pattern
- **FluentValidation** - Data validation
- **JWT Bearer** - Authentication
- **Serilog** - Structured logging

### Documentation & DevOps
- **OpenAPI/Swagger** - API documentation
- **Scalar** - Modern documentation interface
- **Docker & Docker Compose** - Containerization
- **pgAdmin** - PostgreSQL administration

### Testing
- **xUnit** - Testing framework
- **FluentAssertions** - Fluent assertions

## 📋 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker & Docker Compose](https://www.docker.com/get-started)
- [PostgreSQL 16](https://www.postgresql.org/) (optional - can use via Docker)

## 🛠️ Installation and Execution

### 🐳 **Option 1: Using Docker (Recommended)**

1. **Clone the repository**
```bash
git clone https://github.com/YuriGarciaRibeiro/SportHub.git
cd SportHub
```

2. **Run with Docker Compose**
```bash
docker-compose up -d
```

3. **Access the application**
- API: http://localhost:5000
- Documentation: http://localhost:5000/scalar/v1
- pgAdmin: http://localhost:8080 (admin@admin.com / admin)

### 💻 **Option 2: Local Execution**

1. **Clone and configure environment**
```bash
git clone https://github.com/YuriGarciaRibeiro/SportHub.git
cd SportHub
```

2. **Configure the database**
```bash
# Run PostgreSQL via Docker
docker run --name sporthub-db -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=SportHubDb -p 5432:5432 -d postgres:16
```

3. **Run migrations**
```bash
cd src/SportHub.Api
dotnet ef database update
```

4. **Run the application**
```bash
dotnet run
```

## 📚 API Usage

### 🔐 **Authentication**

The API uses JWT Bearer Token for authentication. To access protected endpoints:

1. **Register a user**
```http
POST /auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "YourSecurePassword123!",
  "firstName": "First",
  "lastName": "Last"
}
```

2. **Login**
```http
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "YourSecurePassword123!"
}
```

3. **Use the returned token**
```http
Authorization: Bearer <your-jwt-token>
```

### 🏢 **Establishment Management**

```http
# Create establishment
POST /establishments
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Central Gym",
  "description": "Complete gym in the city center",
  "phoneNumber": "(11) 99999-9999",
  "email": "contact@centralgym.com",
  "website": "https://centralgym.com",
  "imageUrl": "https://example.com/image.jpg",
  "address": {
    "street": "123 Main Street",
    "city": "São Paulo",
    "state": "SP",
    "country": "Brazil",
    "zipCode": "01234-567"
  }
}

# List user's establishments
GET /establishments/owner
Authorization: Bearer <token>

# Get establishment by ID
GET /establishments/{id}
```

### 👥 **Role System**

The system has three hierarchical permission levels:

- **👤 Staff**: Employees with basic access
- **👔 Manager**: Managers with administrative permissions
- **👑 Owner**: Owners with full access

## 🔧 Configuration

### ⚙️ **Environment Variables**

```env
# Database
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=SportHubDb;Username=postgres;Password=postgres

# JWT
JwtSettings__SecretKey=YourSecretKeyHere
JwtSettings__Issuer=SportHub
JwtSettings__Audience=SportHub
JwtSettings__ExpirationMinutes=60

# Admin User
AdminUserSettings__Email=admin@sporthub.com
AdminUserSettings__Password=Admin123!
AdminUserSettings__FirstName=Admin
AdminUserSettings__LastName=User
```

### 📁 **Configuration Structure**

```
appsettings.json              # Base configurations
appsettings.Development.json  # Development configurations
appsettings.Production.json   # Production configurations
```

## 🌱 Test Data (Seeders)

The application automatically creates comprehensive test data during initialization, including users, sports establishments, courts, reservations, and reviews. This data enables immediate testing and demonstration of all system features.

**📋 For complete details about all seeded data, see [SEEDERS_EN.md](SEEDERS_EN.md) | [SEEDERS_PT.md](SEEDERS_PT.md)**

### 🔑 Quick Access
- **Admin User**: Configured via `appsettings.json`
- **Test Users**: 8 users with different roles (members + regular users)
- **Establishments**: 25+ sports establishments across Aracaju/SE
- **Sports**: 8 different sports modalities
- **Courts**: 200+ courts automatically generated
- **Reservations**: Dynamic reservations for all users
- **Reviews**: Realistic ratings and comments for all establishments

All test users use the password: `SportHub123!`

## 📊 Monitoring and Logs

### 📝 **Serilog**
The system uses Serilog for structured logging with the following sinks:
- Console (development)
- File (production)
- Compact JSON format

### 📈 **Metrics**
Structured logs include:
- HTTP requests
- Errors and exceptions
- Query performance
- Domain events

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific tests
dotnet test --filter "Category=Unit"
```

## 📊 Administrative Queries

The project includes a set of useful SQL queries for administration:

### 📁 `sql-queries/`
- **`queries-usuarios-estabelecimentos.sql`** - User and establishment queries
- **`queries-admin-debug.sql`** - Debug and administration queries
- **`dados-teste.sql`** - Test data for development

### 🔍 **Main Queries**
1. Users with establishments and roles
2. Role statistics by establishment
3. Establishments with user count
4. General metrics dashboard

## 🤝 Contributing

1. **Fork the project**
2. **Create a feature branch** (`git checkout -b feature/AmazingFeature`)
3. **Commit your changes** (`git commit -m 'Add some AmazingFeature'`)
4. **Push to the branch** (`git push origin feature/AmazingFeature`)
5. **Open a Pull Request**

### 📋 **Contribution Guidelines**
- Follow SOLID principles and Clean Architecture
- Write tests for new features
- Keep code documented
- Use conventional commits
- Update documentation when necessary

## 📈 Roadmap

### 🚧 **Upcoming Features**
- [ ] Notification system
- [ ] Image upload
- [ ] Advanced reports
- [ ] Geolocation API
- [ ] Rating system
- [ ] Payment integration
- [ ] Mobile app

### 🔄 **Technical Improvements**
- [ ] Implement Health Checks
- [ ] Add Redis for caching
- [ ] Configure CI/CD
- [ ] Implement rate limiting
- [ ] Add Prometheus metrics
- [ ] Configure complete observability

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Yuri Garcia Ribeiro**
- GitHub: [@YuriGarciaRibeiro](https://github.com/YuriGarciaRibeiro)
- LinkedIn: [Yuri Garcia Ribeiro](https://linkedin.com/in/yurigarciaribeiro)

## 🙏 Acknowledgments

- .NET community for their contributions
- Maintainers of the open source libraries used
- Team members who collaborated with feedback and suggestions

---

⭐ **If this project was useful to you, consider giving it a star on GitHub!**