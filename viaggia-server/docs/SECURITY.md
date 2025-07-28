# 🔐 Documentação de Segurança - Viaggia Server

## 🛡️ Visão Geral de Segurança

Esta documentação detalha as medidas de segurança implementadas no Viaggia Server para proteger dados de usuários, transações financeiras e operações do sistema.

## 🔑 Autenticação e Autorização

### 🎫 JWT (JSON Web Tokens)

#### Configuração
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
        };
    });
```

#### Estrutura do Token
```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user_id",
    "email": "user@example.com",
    "roles": ["CLIENT"],
    "exp": 1640995200,
    "iss": "ViaggiaServer",
    "aud": "ViaggiaClient"
  }
}
```

#### Tempo de Vida
- **Access Token**: 1 hora (recomendado)
- **Refresh Token**: 7 dias
- **Token de Reset de Senha**: 15 minutos

### 🌐 Google OAuth 2.0

#### Fluxo de Autenticação
1. Usuário clica em "Login com Google"
2. Redirecionamento para Google OAuth
3. Usuário autoriza a aplicação
4. Google retorna authorization code
5. Troca do code por access token
6. Criação/login do usuário no sistema
7. Geração de JWT próprio

#### Claims Mapeados
```csharp
options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
options.ClaimActions.MapJsonKey("picture", "picture");
```

### 👥 Sistema de Roles

#### Hierarquia de Permissões
```
ADMIN
├── Gerenciar usuários
├── Gerenciar hotéis
├── Gerenciar pacotes
├── Relatórios completos
└── Configurações do sistema

SERVICE_PROVIDER
├── Gerenciar próprios hotéis
├── Gerenciar próprios pacotes
├── Ver reservas de seus hotéis
└── Relatórios próprios

ATTENDANT
├── Ver usuários
├── Auxiliar em reservas
├── Ver hotéis e pacotes
└── Relatórios limitados

CLIENT
├── Fazer reservas
├── Ver próprias reservas
├── Avaliar hotéis
└── Gerenciar perfil
```

#### Implementação no Controller
```csharp
[Authorize(Roles = "ADMIN")]
[HttpPost("admin")]
public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDTO dto)
{
    // Apenas admins podem criar outros admins
}

[Authorize(Roles = "CLIENT,ADMIN")]
[HttpPost("reservations")]
public async Task<IActionResult> CreateReservation([FromBody] ReservationDTO dto)
{
    // Clientes e admins podem fazer reservas
}
```

## 🔒 Proteção de Dados

### 🔐 Hash de Senhas

#### BCrypt Implementation
```csharp
public class PasswordService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, 12); // Work factor 12
    }
    
    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
```

#### Política de Senhas
- **Mínimo**: 8 caracteres
- **Deve conter**: 
  - 1 letra maiúscula
  - 1 letra minúscula
  - 1 número
  - 1 caractere especial
- **Não pode**: 
  - Conter email/nome do usuário
  - Ser reutilizada (últimas 5 senhas)

### 🏠 Proteção de PII (Informações Pessoais)

#### Dados Sensíveis
- **CPF/CNPJ**: Armazenados com hash
- **Dados de Pagamento**: Não armazenados (integração com gateway)
- **Endereços**: Criptografados em repouso
- **Telefones**: Mascarados em logs

#### Implementação de Criptografia
```csharp
public class EncryptionService
{
    private readonly string _key;
    
    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(_key);
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var writer = new StreamWriter(cs);
        
        writer.Write(plainText);
        writer.Close();
        
        var iv = aes.IV;
        var encrypted = ms.ToArray();
        var result = new byte[iv.Length + encrypted.Length];
        
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);
        
        return Convert.ToBase64String(result);
    }
}
```

## 🛡️ Segurança da API

### 🔥 Rate Limiting

#### Implementação com AspNetCoreRateLimit
```csharp
services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 100
        },
        new RateLimitRule
        {
            Endpoint = "*/auth/login",
            Period = "1m",
            Limit = 5
        }
    };
});
```

### 🔧 CORS Configuration

```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "https://viaggia.com",
                "https://app.viaggia.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
```

### 🛡️ Headers de Segurança

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    
    await next();
});
```

## 🚫 Validação e Sanitização

### ✅ FluentValidation

#### Exemplo de Validador
```csharp
public class CreateUserValidator : AbstractValidator<CreateUserDTO>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(BeUniqueEmail)
            .WithMessage("Email already exists");
            
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage("Password must contain at least 1 uppercase, 1 lowercase, 1 digit and 1 special character");
            
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 100)
            .Matches(@"^[a-zA-Z\s]*$")
            .WithMessage("Name can only contain letters and spaces");
    }
    
    private async Task<bool> BeUniqueEmail(string email, CancellationToken token)
    {
        return !await _userRepository.EmailExistsAsync(email);
    }
}
```

### 🧼 Input Sanitization

```csharp
public class SanitizationService
{
    public string SanitizeHtml(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
            
        // Remove HTML tags
        return Regex.Replace(input, "<.*?>", string.Empty);
    }
    
    public string SanitizeSql(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
            
        // Remove SQL injection attempts
        var sqlKeywords = new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "EXEC", "UNION" };
        
        foreach (var keyword in sqlKeywords)
        {
            input = Regex.Replace(input, keyword, string.Empty, RegexOptions.IgnoreCase);
        }
        
        return input;
    }
}
```

## 🔍 Logging e Monitoramento

### 📊 Security Logging

```csharp
public class SecurityLogger
{
    private readonly ILogger<SecurityLogger> _logger;
    
    public void LogFailedLogin(string email, string ipAddress)
    {
        _logger.LogWarning("Failed login attempt for {Email} from {IPAddress}", 
            email, ipAddress);
    }
    
    public void LogSuspiciousActivity(string userId, string activity, string details)
    {
        _logger.LogWarning("Suspicious activity detected. User: {UserId}, Activity: {Activity}, Details: {Details}",
            userId, activity, details);
    }
    
    public void LogPrivilegeEscalation(string userId, string fromRole, string toRole)
    {
        _logger.LogWarning("Privilege escalation. User: {UserId}, From: {FromRole}, To: {ToRole}",
            userId, fromRole, toRole);
    }
}
```

### 🚨 Alertas de Segurança

#### Cenários de Alerta
1. **Tentativas de login falhadas** (>5 em 5 minutos)
2. **Acesso a recursos não autorizados**
3. **Mudanças de role/permissões**
4. **Upload de arquivos suspeitos**
5. **Atividade fora do horário normal**

## 🔐 Gestão de Segredos

### 🗝️ Azure Key Vault (Produção)

```csharp
public class KeyVaultService
{
    private readonly SecretClient _secretClient;
    
    public KeyVaultService(IConfiguration configuration)
    {
        var keyVaultUrl = configuration["KeyVault:Url"];
        _secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
    }
    
    public async Task<string> GetSecretAsync(string secretName)
    {
        var secret = await _secretClient.GetSecretAsync(secretName);
        return secret.Value.Value;
    }
}
```

### 🔧 User Secrets (Desenvolvimento)

```bash
# Configurar secrets para desenvolvimento
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=..."
dotnet user-secrets set "Jwt:Key" "your-super-secret-key-here"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-google-secret"
```

## 🛡️ Proteção contra Vulnerabilidades

### 🔓 SQL Injection Prevention

#### Usando Entity Framework (Safe)
```csharp
// ✅ Seguro - Parametrização automática
var users = await _context.Users
    .Where(u => u.Email == email)
    .FirstOrDefaultAsync();
```

#### Consultas Raw (Se necessário)
```csharp
// ✅ Seguro - Parâmetros
var sql = "SELECT * FROM Users WHERE Email = @email";
var users = await _context.Users
    .FromSqlRaw(sql, new SqlParameter("@email", email))
    .ToListAsync();
```

### 🚫 XSS Prevention

#### Output Encoding
```csharp
public class XssProtectionService
{
    public string EncodeForHtml(string input)
    {
        return HttpUtility.HtmlEncode(input);
    }
    
    public string EncodeForJavaScript(string input)
    {
        return HttpUtility.JavaScriptStringEncode(input);
    }
    
    public string EncodeForUrl(string input)
    {
        return HttpUtility.UrlEncode(input);
    }
}
```

### 🔒 CSRF Protection

```csharp
services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
});
```

## 📋 Checklist de Segurança

### ✅ Implementações Obrigatórias

- [x] **Autenticação JWT** com expiração adequada
- [x] **Hash de senhas** com BCrypt
- [x] **Validação de entrada** com FluentValidation
- [x] **CORS** configurado corretamente
- [x] **HTTPS** obrigatório em produção
- [x] **Rate limiting** para APIs críticas
- [x] **Logging de segurança** para auditoria
- [ ] **Criptografia de dados sensíveis**
- [ ] **Monitoramento de anomalias**
- [ ] **Backup seguro** com criptografia

### 🔍 Testes de Segurança

#### Penetration Testing
- **OWASP ZAP** para testes automatizados
- **Testes manuais** de injeção SQL
- **Verificação de autenticação** bypass
- **Testes de autorização** privilege escalation

#### Security Headers Check
```bash
# Verificar headers de segurança
curl -I https://api.viaggia.com/health

# Verificar SSL/TLS
nmap --script ssl-enum-ciphers -p 443 api.viaggia.com
```

## 🚨 Resposta a Incidentes

### 📞 Plano de Resposta

1. **Detecção**: Monitoramento automático + relatórios manuais
2. **Contenção**: Bloqueio de IPs suspeitos, desativação de contas
3. **Investigação**: Análise de logs, determinação do escopo
4. **Recuperação**: Restauração de backups, correção de vulnerabilidades
5. **Lições Aprendidas**: Documentação e melhorias no processo

### 🔧 Comandos de Emergência

```bash
# Bloquear IP suspeito
iptables -A INPUT -s <IP_SUSPEITO> -j DROP

# Desativar usuário comprometido
UPDATE Users SET IsActive = 0 WHERE Id = <USER_ID>;

# Verificar tentativas de login suspeitas
SELECT Email, COUNT(*) as Attempts, MAX(LoginAttemptDate) as LastAttempt
FROM LoginAttempts 
WHERE Success = 0 AND LoginAttemptDate > DATEADD(hour, -1, GETUTCDATE())
GROUP BY Email
HAVING COUNT(*) > 5;
```

## 📚 Compliance e Regulamentações

### 🔐 LGPD (Lei Geral de Proteção de Dados)

#### Direitos dos Titulares
- **Acesso**: Endpoint para download de dados pessoais
- **Retificação**: Atualização de dados incorretos
- **Eliminação**: Exclusão completa dos dados
- **Portabilidade**: Exportação em formato estruturado

#### Implementação
```csharp
[HttpGet("my-data")]
[Authorize]
public async Task<IActionResult> GetMyPersonalData()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userData = await _userService.GetUserDataForExportAsync(userId);
    
    return File(userData.ToJson(), "application/json", "my-data.json");
}

[HttpDelete("delete-account")]
[Authorize]
public async Task<IActionResult> DeleteMyAccount()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    await _userService.AnonymizeUserDataAsync(userId);
    
    return NoContent();
}
```

### 💳 PCI DSS (Para dados de pagamento)

- **Não armazenar** dados de cartão
- **Tokenização** via gateway de pagamento
- **Logs seguros** sem dados sensíveis
- **Rede segmentada** para processamento

---

**⚠️ Importante**: Esta documentação deve ser revisada regularmente e atualizada conforme novas ameaças e regulamentações surgirem.
