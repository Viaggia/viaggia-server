# Viaggia Server Configuration Guide

This guide explains how to configure the Viaggia Server application using appsettings.json files and environment variables.

## 📁 Configuration Files

### 1. `appsettings.json` (Production/Base Configuration)
- Uses environment variables for all sensitive data
- Safe to commit to version control
- Contains default fallback values

### 2. `appsettings.Development.json` (Development Configuration)  
- Contains actual development values
- **Should NOT be committed to git** (already in .gitignore)
- Used when `ASPNETCORE_ENVIRONMENT=Development`

### 3. `appsettings.Staging.json` (Staging Configuration)
- Uses environment variables like production
- Used when `ASPNETCORE_ENVIRONMENT=Staging`

## 🔧 Configuration Sections

### Database Connection
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=${DB_HOST:-localhost},${DB_PORT:-1433};Database=${DB_NAME:-ViaggiaDB};User Id=${DB_USER:-sa};Password=${DB_PASSWORD};TrustServerCertificate=True;MultipleActiveResultSets=true;"
}
```

**Environment Variables:**
- `DB_HOST` - Database server (default: localhost)
- `DB_PORT` - Database port (default: 1433)
- `DB_NAME` - Database name (default: ViaggiaDB)
- `DB_USER` - Database user (default: sa)
- `DB_PASSWORD` - Database password (required)

### JWT Authentication
```json
"Jwt": {
  "Key": "${JWT_SECRET_KEY}",
  "Issuer": "${JWT_ISSUER:-viaggia-api}",
  "Audience": "${JWT_AUDIENCE:-viaggia-client}",
  "ExpiryHours": "${JWT_EXPIRY_HOURS:-2}"
}
```

**Environment Variables:**
- `JWT_SECRET_KEY` - Secret key for JWT signing (required, 32+ chars)
- `JWT_ISSUER` - JWT issuer (default: viaggia-api)
- `JWT_AUDIENCE` - JWT audience (default: viaggia-client)
- `JWT_EXPIRY_HOURS` - Token expiry in hours (default: 2)

### Stripe Payment
```json
"Stripe": {
  "SecretKey": "${STRIPE_SECRET_KEY}",
  "PublishableKey": "${STRIPE_PUBLISHABLE_KEY}",
  "WebhookSecret": "${STRIPE_WEBHOOK_SECRET}"
}
```

**Environment Variables:**
- `STRIPE_SECRET_KEY` - Stripe secret key (required)
- `STRIPE_PUBLISHABLE_KEY` - Stripe publishable key (required)
- `STRIPE_WEBHOOK_SECRET` - Stripe webhook secret (required)

### Google OAuth
```json
"Authentication": {
  "Google": {
    "ClientId": "${GOOGLE_CLIENT_ID}",
    "ClientSecret": "${GOOGLE_CLIENT_SECRET}"
  }
}
```

**Environment Variables:**
- `GOOGLE_CLIENT_ID` - Google OAuth client ID (required)
- `GOOGLE_CLIENT_SECRET` - Google OAuth client secret (required)

### Email Settings
```json
"EmailSettings": {
  "SmtpServer": "${EMAIL_SMTP_SERVER:-smtp.gmail.com}",
  "Port": "${EMAIL_PORT:-587}",
  "Username": "${EMAIL_USERNAME}",
  "Password": "${EMAIL_PASSWORD}",
  "FromName": "${EMAIL_FROM_NAME:-Viaggia Travel}",
  "FromAddress": "${EMAIL_FROM_ADDRESS:-noreply@viaggia.com}",
  "EnableSsl": "${EMAIL_ENABLE_SSL:-true}"
}
```

**Environment Variables:**
- `EMAIL_SMTP_SERVER` - SMTP server (default: smtp.gmail.com)
- `EMAIL_PORT` - SMTP port (default: 587)
- `EMAIL_USERNAME` - SMTP username (required)
- `EMAIL_PASSWORD` - SMTP password (required)
- `EMAIL_FROM_NAME` - From name (default: Viaggia Travel)
- `EMAIL_FROM_ADDRESS` - From address (default: noreply@viaggia.com)
- `EMAIL_ENABLE_SSL` - Enable SSL (default: true)

### Application Settings
```json
"AppSettings": {
  "BaseUrl": "${API_BASE_URL:-http://localhost:5000}",
  "Environment": "${API_ENVIRONMENT:-Production}",
  "EnableSwagger": "${ENABLE_SWAGGER:-false}",
  "EnableDetailedErrors": "${ENABLE_DETAILED_ERRORS:-false}",
  "MaxFileSizeMB": "${MAX_FILE_SIZE_MB:-10}",
  "UploadPath": "${UPLOAD_PATH:-wwwroot/Uploads}"
}
```

**Environment Variables:**
- `API_BASE_URL` - Base API URL (default: http://localhost:5000)
- `API_ENVIRONMENT` - Environment name (default: Production)
- `ENABLE_SWAGGER` - Enable Swagger UI (default: false)
- `ENABLE_DETAILED_ERRORS` - Show detailed errors (default: false)
- `MAX_FILE_SIZE_MB` - Max file upload size (default: 10)
- `UPLOAD_PATH` - Upload directory path (default: wwwroot/Uploads)

### CORS Settings
```json
"CORS": {
  "AllowedOrigins": [
    "${FRONTEND_URL:-http://localhost:5173}",
    "${PRODUCTION_FRONTEND_URL:-https://your-production-frontend.com}"
  ]
}
```

**Environment Variables:**
- `FRONTEND_URL` - Frontend URL (default: http://localhost:5173)
- `PRODUCTION_FRONTEND_URL` - Production frontend URL

## 🚀 Usage Examples

### 1. Development Environment
```bash
# No environment variables needed - uses appsettings.Development.json
ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

### 2. Production with Environment Variables
```bash
# Set environment variables
export ASPNETCORE_ENVIRONMENT=Production
export DB_PASSWORD="YourStrongPassword123!"
export JWT_SECRET_KEY="your-super-secret-jwt-key-32-chars-minimum"
export STRIPE_SECRET_KEY="sk_live_your_stripe_secret_key"
export STRIPE_PUBLISHABLE_KEY="pk_live_your_stripe_publishable_key"
export STRIPE_WEBHOOK_SECRET="whsec_your_webhook_secret"
export GOOGLE_CLIENT_ID="your-google-client-id.apps.googleusercontent.com"
export GOOGLE_CLIENT_SECRET="your-google-client-secret"
export EMAIL_USERNAME="your-email@gmail.com"
export EMAIL_PASSWORD="your-app-password"

# Run the application
dotnet run
```

### 3. Docker Environment
```bash
# Docker automatically loads from .env file
cd docker/
./scripts/docker-helper.sh dev:up
```

### 4. Docker Production
```bash
# Uses environment variables from docker-compose.yml
cd docker/
./scripts/docker-helper.sh prod:up
```

## 🔐 Security Best Practices

### 1. Environment Variables Priority
1. **System Environment Variables** (highest priority)
2. **Docker .env files**
3. **appsettings.{Environment}.json**
4. **appsettings.json** (lowest priority/defaults)

### 2. Sensitive Data Guidelines
- ✅ **Store in environment variables**: Passwords, API keys, connection strings
- ✅ **Commit to git**: appsettings.json with `${VARIABLE}` placeholders
- ❌ **Never commit**: Actual passwords, keys, or sensitive values
- ❌ **Never commit**: appsettings.Development.json (already ignored)

### 3. JWT Security
- Use 32+ character secret keys
- Different keys for dev/staging/production
- Shorter expiry times for production (2-4 hours)
- Longer expiry for development (24 hours)

### 4. Database Security
- Different databases for dev/staging/production
- Strong passwords for production
- Weaker passwords acceptable for development

## 🛠️ Troubleshooting

### Missing Configuration Error
```
InvalidOperationException: Google ClientId is required
```
**Solution**: Set the required environment variable or update appsettings.json

### Database Connection Failed
```
SqlException: Login failed for user 'sa'
```
**Solution**: Check `DB_PASSWORD` environment variable or connection string

### JWT Token Invalid
```
UnauthorizedResult: The token is invalid
```
**Solution**: Verify `JWT_SECRET_KEY` matches between environments

### CORS Error
```
CORS policy: No 'Access-Control-Allow-Origin' header
```
**Solution**: Update `FRONTEND_URL` environment variable or CORS settings

## 📋 Environment Variable Checklist

### Required for Production:
- [ ] `DB_PASSWORD`
- [ ] `JWT_SECRET_KEY`
- [ ] `STRIPE_SECRET_KEY`
- [ ] `STRIPE_PUBLISHABLE_KEY`
- [ ] `STRIPE_WEBHOOK_SECRET`
- [ ] `GOOGLE_CLIENT_ID`
- [ ] `GOOGLE_CLIENT_SECRET`
- [ ] `EMAIL_USERNAME`
- [ ] `EMAIL_PASSWORD`

### Optional (have defaults):
- [ ] `DB_HOST`
- [ ] `DB_PORT`
- [ ] `DB_NAME`
- [ ] `JWT_ISSUER`
- [ ] `JWT_AUDIENCE`
- [ ] `JWT_EXPIRY_HOURS`
- [ ] `EMAIL_SMTP_SERVER`
- [ ] `EMAIL_PORT`
- [ ] `FRONTEND_URL`

---

**Note**: This configuration system allows for secure, flexible deployment across different environments while keeping sensitive data out of version control.
