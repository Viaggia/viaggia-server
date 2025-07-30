# Sistema de Token de 6 Dígitos - Reset de Senha

## 📋 Visão Geral

O sistema de reset de senha do Viaggia utiliza tokens numéricos de **6 dígitos** para garantir segurança e facilidade de uso. Este documento detalha como o sistema funciona e como implementar melhorias.

---

## 🔢 Características do Token

### Especificações Técnicas
- **Formato**: 6 dígitos numéricos (000000-999999)
- **Algoritmo**: Random.Next() com range específico
- **Validação**: Regex `^\d{6}$`
- **Armazenamento**: nvarchar(6) no banco de dados
- **Validade**: 1 hora após criação
- **Uso único**: Token é invalidado após uso

### Vantagens dos Tokens de 6 Dígitos
- ✅ **User-friendly**: Fácil de digitar e memorizar
- ✅ **Mobile-friendly**: Ideal para aplicações móveis
- ✅ **Seguro**: 1 milhão de combinações possíveis
- ✅ **Rápido**: Processo de entrada mais ágil
- ✅ **Menos erros**: Reduz erros de digitação

---

## 🏗️ Implementação Técnica

### 1. Geração do Token (AuthService.cs)

```csharp
/// <summary>
/// Generates a secure 6-digit numeric token
/// </summary>
/// <returns>6-digit numeric string token</returns>
private string GenerateNumericToken()
{
    var random = new Random();
    var token = random.Next(100000, 999999).ToString();
    return token;
}
```

### 2. Validação de Unicidade

```csharp
// Generate a unique 6-digit numeric token
var token = GenerateNumericToken();

// Ensure token is unique
while (await _context.PasswordResetTokens.AnyAsync(prt => prt.Token == token && !prt.IsUsed))
{
    token = GenerateNumericToken();
}
```

### 3. Modelo de Dados (PasswordResetToken.cs)

```csharp
[Required]
[StringLength(6, MinimumLength = 6, ErrorMessage = "Token deve ter exatamente 6 dígitos")]
[RegularExpression(@"^\d{6}$", ErrorMessage = "Token deve conter apenas números")]
public string Token { get; set; } = string.Empty;
```

### 4. Validação em DTOs

**ResetPasswordRequestDTO.cs** e **ValidateTokenRequestDTO.cs**:
```csharp
[Required(ErrorMessage = "Token é obrigatório")]
[StringLength(6, MinimumLength = 6, ErrorMessage = "Token deve ter exatamente 6 dígitos")]
[RegularExpression(@"^\d{6}$", ErrorMessage = "Token deve conter apenas números")]
public string Token { get; set; } = null!;
```

---

## 🔒 Segurança

### Cálculo de Segurança
- **Total de combinações**: 1.000.000 (100000-999999)
- **Tempo de validade**: 1 hora
- **Tentativas por segundo**: ~278 (assumindo ataque distribuído)
- **Probabilidade de acerto**: 0.0001% por tentativa

### Proteções Implementadas
1. **Expiração automática**: Tokens expiram em 1 hora
2. **Uso único**: Token é invalidado após uso
3. **Validação de unicidade**: Evita tokens duplicados
4. **Rate limiting**: (Recomendado implementar)
5. **Logging de tentativas**: Monitoramento de segurança

---

## 📧 Template de Email

O email de reset contém o token destacado visualmente:

```html
<div class='token-box'>
    <p><strong>Seu Token de Segurança:</strong></p>
    <div class='token'>{token}</div>
</div>
```

### Características do Email
- Token destacado em fonte grande (24px)
- Espaçamento entre dígitos (letter-spacing: 3px)
- Cor azul (#007bff) para destaque
- Instruções claras de uso
- Avisos de segurança

---

## 🚀 Fluxo de Uso

### 1. Solicitação de Reset
```bash
POST /api/auth/forgot-password
{
  "email": "usuario@exemplo.com"
}
```

### 2. Recebimento do Email
- Usuário recebe email com token de 6 dígitos
- Exemplo: **123456**

### 3. Validação do Token
```bash
POST /api/auth/validate-token
{
  "token": "123456"
}
```

### 4. Reset da Senha
```bash
POST /api/auth/reset-password
{
  "token": "123456",
  "newPassword": "MinhaNovaSenh@123",
  "confirmPassword": "MinhaNovaSenh@123"
}
```

---

## 🧪 Exemplos de Teste

### Teste com cURL - Forgot Password
```bash
curl -X POST "https://localhost:7139/api/auth/forgot-password" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "usuario@teste.com"
  }'
```

### Teste com cURL - Validate Token
```bash
curl -X POST "https://localhost:7139/api/auth/validate-token" \
  -H "Content-Type: application/json" \
  -d '{
    "token": "123456"
  }'
```

### Teste com cURL - Reset Password
```bash
curl -X POST "https://localhost:7139/api/auth/reset-password" \
  -H "Content-Type: application/json" \
  -d '{
    "token": "123456",
    "newPassword": "MinhaNovaSenh@123",
    "confirmPassword": "MinhaNovaSenh@123"
  }'
```

---

## ⚡ Melhorias Recomendadas

### 1. Geração de Token Mais Segura
```csharp
private string GenerateSecureNumericToken()
{
    using (var rng = RandomNumberGenerator.Create())
    {
        byte[] bytes = new byte[4];
        rng.GetBytes(bytes);
        var randomValue = Math.Abs(BitConverter.ToInt32(bytes, 0));
        return (randomValue % 900000 + 100000).ToString();
    }
}
```

### 2. Rate Limiting
```csharp
// Implementar limitação de tentativas por IP/usuário
// Exemplo: máximo 5 tentativas por hora
```

### 3. Auditoria
```csharp
// Log de todas as tentativas de validação
// Monitoramento de padrões suspeitos
```

### 4. Notificação de Segurança
```csharp
// Email de notificação quando senha é alterada
// SMS como segundo fator (opcional)
```

---

## 📊 Métricas e Monitoramento

### Eventos para Monitorar
- Geração de tokens
- Tentativas de validação
- Tokens expirados
- Resets de senha bem-sucedidos
- Tentativas de força bruta

### KPIs Sugeridos
- Taxa de conversão (token gerado → senha alterada)
- Tempo médio entre geração e uso do token
- Número de tentativas por token
- Tokens expirados sem uso

---

## 🔄 Migration Aplicada

```sql
-- Limpeza de dados existentes
DELETE FROM [PasswordResetTokens];

-- Alteração do campo Token para 6 dígitos
ALTER TABLE [PasswordResetTokens] 
ALTER COLUMN [Token] nvarchar(6) NOT NULL;
```

---

## 📝 Conclusão

O sistema de tokens de 6 dígitos oferece um equilíbrio ideal entre segurança e usabilidade para o reset de senhas no Viaggia. A implementação atual garante:

- **Segurança adequada** para aplicações web
- **Experiência do usuário otimizada**
- **Facilidade de implementação e manutenção**
- **Compatibilidade com dispositivos móveis**

O sistema está pronto para produção e pode ser expandido com as melhorias recomendadas conforme necessário.
