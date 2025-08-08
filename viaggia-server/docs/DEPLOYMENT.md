# 🚀 Guia de Deploy - Viaggia Server

## 📋 Visão Geral

Este guia aborda todas as estratégias de deploy do Viaggia Server, desde ambiente de desenvolvimento até produção em cloud, incluindo monitoramento e manutenção.

## 🌍 Ambientes

### 🏠 Development (Local)
- **URL**: `https://localhost:7000`
- **Banco**: LocalDB/SQL Server Express
- **Secrets**: User Secrets
- **SSL**: Certificado de desenvolvimento

### 🧪 Testing/Staging
- **URL**: `https://api-staging.viaggia.com`
- **Banco**: Azure SQL Database (pequeno)
- **Secrets**: Azure Key Vault
- **SSL**: Let's Encrypt

### 🏭 Production
- **URL**: `https://api.viaggia.com`
- **Banco**: Azure SQL Database (escalável)
- **Secrets**: Azure Key Vault
- **SSL**: Certificado comercial
- **CDN**: Azure CDN para assets estáticos

## ☁️ Deploy para Azure

### 🔧 Azure App Service

#### Pré-requisitos
```bash
# Instalar Azure CLI
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Login
az login

# Criar resource group
az group create --name viaggia-rg --location "East US"
```

#### Configuração do App Service
```bash
# Criar App Service Plan
az appservice plan create \
    --name viaggia-plan \
    --resource-group viaggia-rg \
    --sku P1V2 \
    --is-linux

# Criar Web App
az webapp create \
    --name viaggia-api \
    --resource-group viaggia-rg \
    --plan viaggia-plan \
    --runtime "DOTNETCORE|8.0"
```

#### Deploy via Azure CLI
```bash
# Build e publish
dotnet publish -c Release -o ./publish

# Criar zip do build
cd publish
zip -r ../release.zip .
cd ..

# Deploy
az webapp deployment source config-zip \
    --resource-group viaggia-rg \
    --name viaggia-api \
    --src release.zip
```

#### Deploy via GitHub Actions
```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore --configuration Release
      
    - name: Test
      run: dotnet test --no-build --verbosity normal
      
    - name: Publish
      run: dotnet publish --no-build --configuration Release --output ./publish
      
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'viaggia-api'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: './publish'
```

### 🗄️ Azure SQL Database

#### Criação do Banco
```bash
# Criar SQL Server
az sql server create \
    --name viaggia-sql-server \
    --resource-group viaggia-rg \
    --location "East US" \
    --admin-user admin-viaggia \
    --admin-password "YourSecurePassword123!"

# Criar banco de dados
az sql db create \
    --resource-group viaggia-rg \
    --server viaggia-sql-server \
    --name viaggia-db \
    --service-objective S2
```

#### Configurar Firewall
```bash
# Permitir Azure Services
az sql server firewall-rule create \
    --resource-group viaggia-rg \
    --server viaggia-sql-server \
    --name AllowAzureServices \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0

# Adicionar seu IP para management
az sql server firewall-rule create \
    --resource-group viaggia-rg \
    --server viaggia-sql-server \
    --name AllowMyIP \
    --start-ip-address $(curl -s ifconfig.me) \
    --end-ip-address $(curl -s ifconfig.me)
```

#### Connection String
```bash
# Obter connection string
az sql db show-connection-string \
    --client ado.net \
    --name viaggia-db \
    --server viaggia-sql-server
```

### 🔑 Azure Key Vault

#### Criação e Configuração
```bash
# Criar Key Vault
az keyvault create \
    --name viaggia-keyvault \
    --resource-group viaggia-rg \
    --location "East US"

# Adicionar secrets
az keyvault secret set \
    --vault-name viaggia-keyvault \
    --name "ConnectionStrings--DefaultConnection" \
    --value "Server=tcp:viaggia-sql-server.database.windows.net,1433;Database=viaggia-db;..."

az keyvault secret set \
    --vault-name viaggia-keyvault \
    --name "Jwt--Key" \
    --value "your-super-secret-jwt-key-here"

# Dar permissão para o App Service
az webapp identity assign \
    --name viaggia-api \
    --resource-group viaggia-rg

# Configurar access policy
az keyvault set-policy \
    --name viaggia-keyvault \
    --object-id $(az webapp identity show --name viaggia-api --resource-group viaggia-rg --query principalId -o tsv) \
    --secret-permissions get
```

#### Configuração no appsettings.json
```json
{
  "KeyVault": {
    "Url": "https://viaggia-keyvault.vault.azure.net/"
  }
}
```

## 🐳 Deploy com Docker

### 📦 Dockerfile
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["viaggia-server.csproj", "./"]
RUN dotnet restore

# Copy source code
COPY . .
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

EXPOSE 80
ENTRYPOINT ["dotnet", "viaggia-server.dll"]
```

### 🔧 Docker Compose
```yaml
version: '3.8'

services:
  viaggia-api:
    build: .
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=sql-server,1433;Database=ViaggiaDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
    depends_on:
      - sql-server
    networks:
      - viaggia-network

  sql-server:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    volumes:
      - sql-data:/var/opt/mssql
    networks:
      - viaggia-network

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    networks:
      - viaggia-network

volumes:
  sql-data:

networks:
  viaggia-network:
    driver: bridge
```

### 🚀 Deploy para Azure Container Instances

```bash
# Build e push para Azure Container Registry
az acr create --resource-group viaggia-rg --name viaggiaacr --sku Basic
az acr login --name viaggiaacr

# Build image
docker build -t viaggiaacr.azurecr.io/viaggia-api:latest .
docker push viaggiaacr.azurecr.io/viaggia-api:latest

# Deploy para Container Instances
az container create \
    --resource-group viaggia-rg \
    --name viaggia-api-container \
    --image viaggiaacr.azurecr.io/viaggia-api:latest \
    --cpu 2 \
    --memory 4 \
    --registry-login-server viaggiaacr.azurecr.io \
    --registry-username $(az acr credential show --name viaggiaacr --query username -o tsv) \
    --registry-password $(az acr credential show --name viaggiaacr --query passwords[0].value -o tsv) \
    --dns-name-label viaggia-api \
    --ports 80 443
```

## 🎛️ Kubernetes (AKS)

### 📄 Deployment Manifests

#### namespace.yaml
```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: viaggia
```

#### deployment.yaml
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: viaggia-api
  namespace: viaggia
spec:
  replicas: 3
  selector:
    matchLabels:
      app: viaggia-api
  template:
    metadata:
      labels:
        app: viaggia-api
    spec:
      containers:
      - name: api
        image: viaggiaacr.azurecr.io/viaggia-api:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: viaggia-secrets
              key: connectionstring
        resources:
          requests:
            memory: "512Mi"
            cpu: "250m"
          limits:
            memory: "1Gi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 30
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 10
```

#### service.yaml
```yaml
apiVersion: v1
kind: Service
metadata:
  name: viaggia-api-service
  namespace: viaggia
spec:
  selector:
    app: viaggia-api
  ports:
    - protocol: TCP
      port: 80
      targetPort: 80
  type: LoadBalancer
```

#### ingress.yaml
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: viaggia-ingress
  namespace: viaggia
  annotations:
    kubernetes.io/ingress.class: azure/application-gateway
    cert-manager.io/cluster-issuer: letsencrypt-prod
spec:
  tls:
  - hosts:
    - api.viaggia.com
    secretName: viaggia-tls
  rules:
  - host: api.viaggia.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: viaggia-api-service
            port:
              number: 80
```

### 🚀 Deploy Commands
```bash
# Criar cluster AKS
az aks create \
    --resource-group viaggia-rg \
    --name viaggia-aks \
    --node-count 3 \
    --node-vm-size Standard_B2s \
    --attach-acr viaggiaacr \
    --enable-managed-identity

# Conectar ao cluster
az aks get-credentials --resource-group viaggia-rg --name viaggia-aks

# Deploy da aplicação
kubectl apply -f k8s/
```

## 📊 Monitoramento e Observabilidade

### 📈 Application Insights
```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();

// Custom telemetry
public class TelemetryService
{
    private readonly TelemetryClient _telemetryClient;
    
    public void TrackReservationCreated(string userId, decimal amount)
    {
        _telemetryClient.TrackEvent("ReservationCreated", new Dictionary<string, string>
        {
            ["UserId"] = userId,
            ["Amount"] = amount.ToString()
        });
    }
}
```

### 📊 Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddDbContext<AppDbContext>()
    .AddSqlServer(connectionString)
    .AddCheck<ExternalApiHealthCheck>("external-api");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### 🔍 Structured Logging com Serilog
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(TelemetryConfiguration.CreateDefault(), TelemetryConverter.Traces)
    .WriteTo.File("logs/viaggia-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

## 🔄 CI/CD Pipeline

### 🔧 Azure DevOps Pipeline
```yaml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'
  azureSubscription: 'viaggia-service-connection'
  webAppName: 'viaggia-api'

stages:
- stage: Build
  jobs:
  - job: Build
    steps:
    - task: UseDotNet@2
      inputs:
        version: '8.0.x'
    
    - task: DotNetCoreCLI@2
      displayName: 'Restore packages'
      inputs:
        command: 'restore'
        projects: '**/*.csproj'
    
    - task: DotNetCoreCLI@2
      displayName: 'Build application'
      inputs:
        command: 'build'
        projects: '**/*.csproj'
        arguments: '--configuration $(buildConfiguration)'
    
    - task: DotNetCoreCLI@2
      displayName: 'Run tests'
      inputs:
        command: 'test'
        projects: '**/*Tests.csproj'
        arguments: '--configuration $(buildConfiguration) --collect:"XPlat Code Coverage"'
    
    - task: DotNetCoreCLI@2
      displayName: 'Publish application'
      inputs:
        command: 'publish'
        publishWebProjects: true
        arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'
    
    - task: PublishBuildArtifacts@1
      inputs:
        PathtoPublish: '$(Build.ArtifactStagingDirectory)'
        ArtifactName: 'drop'

- stage: Deploy
  dependsOn: Build
  jobs:
  - deployment: Deploy
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            inputs:
              azureSubscription: '$(azureSubscription)'
              appType: 'webApp'
              appName: '$(webAppName)'
              package: '$(Pipeline.Workspace)/drop/**/*.zip'
```

### 🧪 Tests automatizados
```bash
# Unit tests
dotnet test --configuration Release --logger "trx;LogFileName=TestResults.trx"

# Integration tests
dotnet test IntegrationTests/ --configuration Release --logger "trx;LogFileName=IntegrationTestResults.trx"

# Security scan
dotnet security-scan YourProject.csproj --output security-report.json

# Performance tests
dotnet run --project PerformanceTests/ --configuration Release
```

## 🔒 Configurações de Produção

### 🌍 Variáveis de Ambiente
```bash
# Azure App Service Configuration
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80
WEBSITE_ENABLE_SYNC_UPDATE_SITE=true
WEBSITE_RUN_FROM_PACKAGE=1

# Application settings
ConnectionStrings__DefaultConnection=@Microsoft.KeyVault(SecretUri=https://viaggia-keyvault.vault.azure.net/secrets/connection-string/)
Jwt__Key=@Microsoft.KeyVault(SecretUri=https://viaggia-keyvault.vault.azure.net/secrets/jwt-key/)
Authentication__Google__ClientSecret=@Microsoft.KeyVault(SecretUri=https://viaggia-keyvault.vault.azure.net/secrets/google-client-secret/)
```

### ⚡ Performance Settings
```json
{
  "Kestrel": {
    "Limits": {
      "MaxConcurrentConnections": 100,
      "MaxConcurrentUpgradedConnections": 100,
      "MaxRequestBodySize": 10485760,
      "KeepAliveTimeout": "00:02:00",
      "RequestHeadersTimeout": "00:00:30"
    }
  },
  "EntityFramework": {
    "CommandTimeout": 30,
    "MaxRetryCount": 3,
    "MaxRetryDelay": "00:00:30"
  }
}
```

## 📈 Escalabilidade

### 🔄 Auto Scaling
```bash
# Configurar auto scaling no Azure App Service
az appservice plan update \
    --name viaggia-plan \
    --resource-group viaggia-rg \
    --sku P1V2

# Configurar regras de auto scaling
az monitor autoscale create \
    --resource-group viaggia-rg \
    --resource viaggia-api \
    --resource-type Microsoft.Web/sites \
    --name viaggia-autoscale \
    --min-count 2 \
    --max-count 10 \
    --count 2

# Regra para scale out (CPU > 70%)
az monitor autoscale rule create \
    --resource-group viaggia-rg \
    --autoscale-name viaggia-autoscale \
    --condition "Percentage CPU > 70 avg 5m" \
    --scale out 1

# Regra para scale in (CPU < 30%)
az monitor autoscale rule create \
    --resource-group viaggia-rg \
    --autoscale-name viaggia-autoscale \
    --condition "Percentage CPU < 30 avg 5m" \
    --scale in 1
```

### 🗄️ Database Scaling
```sql
-- Configurar Read Replicas
ALTER DATABASE ViaggiaDb 
ADD SECONDARY ON SERVER 'viaggia-sql-server-read'
WITH (ALLOW_CONNECTIONS = READ_ONLY);

-- Configurar Auto-pause para desenvolvimento
ALTER DATABASE ViaggiaDb_Dev
MODIFY (AUTO_PAUSE_DELAY = 60); -- Pausa após 60 min de inatividade
```

## 🚨 Troubleshooting

### 📊 Comandos de Diagnóstico
```bash
# Verificar status do App Service
az webapp show --name viaggia-api --resource-group viaggia-rg --query state

# Ver logs da aplicação
az webapp log download --name viaggia-api --resource-group viaggia-rg

# Streaming de logs em tempo real
az webapp log tail --name viaggia-api --resource-group viaggia-rg

# Verificar métricas
az monitor metrics list --resource $(az webapp show --name viaggia-api --resource-group viaggia-rg --query id -o tsv) --metric "Http2xx"
```

### 🔍 Logs Importantes
```bash
# Application logs
tail -f /var/log/azure/viaggia-api.log

# SQL Connection logs
grep "Connection" /var/log/azure/viaggia-api.log

# Performance logs
grep "ResponseTime" /var/log/azure/viaggia-api.log | tail -100
```

## 💰 Otimização de Custos

### 💡 Dicas de Economia
1. **Reserved Instances** para ambientes de produção
2. **Auto-pause** para bancos de desenvolvimento
3. **Spot Instances** para testes de carga
4. **CDN** para assets estáticos
5. **Compression** para reduzir bandwidth

### 📊 Monitoramento de Custos
```bash
# Configurar budget alerts
az consumption budget create \
    --amount 500 \
    --budget-name viaggia-monthly-budget \
    --time-period-start 2024-01-01 \
    --time-period-end 2024-12-31 \
    --resource-group viaggia-rg
```

---

**🎯 Próximos Passos**: Após o deploy, configure monitoramento, alertas e defina processos de backup e disaster recovery.
