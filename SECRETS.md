# Secrets Configuration (User Secrets)

This project uses .NET User Secrets to manage sensitive information during development. Secrets are stored locally on your machine and are never committed to the Git repository.

## Initial Setup

If you cloned this project for the first time, configure the following secrets:

### 1. Initialize User Secrets (already configured)
```bash
cd src/SportHub.Api
dotnet user-secrets init
```

### 2. Configure Required Secrets

```bash
# Database connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=SportHubDb;Username=postgres;Password=postgres"

# Secret key for JWT (IMPORTANT: Use a strong key in production)
dotnet user-secrets set "Jwt:Key" "your-very-long-and-secure-secret-key"

# Default admin user password
dotnet user-secrets set "AdminUser:Password" "YourSecurePassword123!"
```

### 3. Verify Configuration

```bash
# List all configured secrets
dotnet user-secrets list

# Remove a specific secret (if needed)
dotnet user-secrets remove "SecretName"

# Clear all secrets
dotnet user-secrets clear
```

## Secrets Structure

The following secrets must be configured:

| Key | Description | Example |
|-----|-------------|---------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=SportHubDb;Username=postgres;Password=postgres` |
| `Jwt:Key` | Key to sign JWT tokens | `a-very-long-and-secure-key-with-at-least-256-bits` |
| `AdminUser:Password` | Administrator user password | `MySecurePassword123!` |

## For Production

In production, **DO NOT USE** User Secrets. Instead, configure environment variables or use a secrets management service such as:

- **Azure Key Vault** (if using Azure)
- **AWS Secrets Manager** (if using AWS)
- **HashiCorp Vault**
- **System environment variables**

### Example with Environment Variables

```bash
# Linux/Mac
export ConnectionStrings__DefaultConnection="your-connection-string"
export JwtSettings__SecretKey="your-secret-key"
export AdminUser__Password="your-admin-password"

# Windows
set ConnectionStrings__DefaultConnection=your-connection-string
set JwtSettings__SecretKey=your-secret-key
set AdminUser__Password=your-admin-password
```

## Security

⚠️ **IMPORTANT**: 
- Never commit secrets in source code
- Use strong JWT keys (minimum 256 bits)
- Change default passwords in production
- Rotate keys regularly in production

## Troubleshooting

If secrets are not being loaded:

1. Verify you are in development environment
2. Confirm that `UserSecretsId` is in the `.csproj` file
3. Run `dotnet user-secrets list` to check values
4. Restart the application after changing secrets
