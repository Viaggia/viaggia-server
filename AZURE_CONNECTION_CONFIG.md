# Azure SQL Database Connection Configuration

## Environment Variables Setup

The Azure connection string has been configured using environment variables for security and flexibility.

### Individual Environment Variables (Current Method)

In `.env` file:
```env
# Database Connection (Azure SQL Server)
DB_HOST=tcp:viaggiasqlserver.database.windows.net,1433
DB_NAME=viaggia-db
DB_USER=viaggiaadmin
DB_PASSWORD=Viaggia@123password
DB_ENCRYPT=true
DB_TRUST_CERT=false
DB_TIMEOUT=30
DB_AUTH_PARAM=MultipleActiveResultSets=False;Persist Security Info=False
```

### Complete Connection String (Alternative)

Also available in `.env` file:
```env
# Azure SQL Database Configuration
# REPLACE ALL VALUES BELOW WITH YOUR ACTUAL CREDENTIALS

# Individual connection parameters
DB_HOST=tcp:YOUR_SQL_SERVER.database.windows.net,1433
DB_NAME=YOUR_DATABASE_NAME  
DB_USER=YOUR_USERNAME
DB_PASSWORD=YOUR_PASSWORD

# Full connection string format (for direct use in Azure App Service)
# Application Settings → Connection Strings → Add new connection string
# Name: DefaultConnection
# Value: (use the string below with YOUR actual values)
# Type: SQLServer

AZURE_CONNECTION_STRING=Server=tcp:YOUR_SQL_SERVER.database.windows.net,1433;Initial Catalog=YOUR_DATABASE_NAME;Persist Security Info=False;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### Connection String Resolution

The `appsettings.json` is configured to:
1. First try to use `AZURE_CONNECTION_STRING` if available
2. Fall back to individual environment variables if `AZURE_CONNECTION_STRING` is not set

```json
"ConnectionStrings": {
  "DefaultConnection": "${AZURE_CONNECTION_STRING:-Server=${DB_HOST:-localhost};Database=${DB_NAME:-ViaggiaDB};User Id=${DB_USER};Password=${DB_PASSWORD};Encrypt=${DB_ENCRYPT:-true};TrustServerCertificate=${DB_TRUST_CERT:-false};Connection Timeout=${DB_TIMEOUT:-30};${DB_AUTH_PARAM}}"
}
```

## Usage

### Development
The environment variables are automatically loaded from `.env` file when running locally.

### Docker
The docker-compose.yml is configured to load the `.env` file automatically.

### Production
Set the `AZURE_CONNECTION_STRING` environment variable directly in your production environment.

## Security Notes
- The `.env` file should be added to `.gitignore` to prevent sensitive data from being committed
- In production, use secure environment variable management (Azure Key Vault, etc.)
- Consider using Managed Identity for Azure SQL Database in production
