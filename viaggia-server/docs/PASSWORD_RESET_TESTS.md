# 🧪 Testes da API de Recuperação de Senha - Sistema de Token 6 Dígitos

## 📋 Preparação do Ambiente

### ⚡ NOVO: Sistema de Token de 6 Dígitos
O sistema agora utiliza tokens numéricos de **6 dígitos** (ex: 123456) em vez de GUIDs, facilitando a digitação e uso móvel.

### 1. Configurar Email de Teste
No `appsettings.json`, verifique se as configurações de SMTP estão corretas:

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "viaggia.societa.albergo@gmail.com",
    "Password": "wybo jlsa ilvb rnyb",
    "FromEmail": "viaggia.societa.albergo@gmail.com",
    "FromName": "Viaggia Password Reset"
  }
}
```

### 2. Verificar Tabelas no Banco
Execute no SQL Server para verificar a estrutura:

```sql
-- Verificar se a tabela PasswordResetTokens existe
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'PasswordResetTokens';

-- Verificar estrutura da tabela
SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PasswordResetTokens';
```

---

## 🔄 Cenários de Teste

### **Cenário 1: Fluxo Completo de Sucesso**

#### **1.1 - Solicitar Reset (Sucesso)**
```bash
curl -X POST http://localhost:5000/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{
    "email": "seu-email@teste.com"
  }'
```

**Resultado Esperado:**
```json
{
  "message": "E-mail de redefinição de senha enviado com sucesso."
}
```

#### **1.2 - Verificar Email Recebido**
- ✅ Email deve chegar com o nome do usuário
- ✅ Token deve estar destacado
- ✅ Link deve apontar para `/validate-token?token=XXXXX`

#### **1.3 - Validar Token (Sucesso)**
```bash
curl -X POST http://localhost:5000/api/auth/validate-token \
  -H "Content-Type: application/json" \
  -d '{
    "token": "TOKEN_RECEBIDO_NO_EMAIL"
  }'
```

**Resultado Esperado:**
```json
{
  "message": "Token válido.",
  "isValid": true,
  "userName": "Nome do Usuário",
  "email": "seu-email@teste.com",
  "expiryDate": "2025-07-28T15:30:00Z"
}
```

#### **1.4 - Redefinir Senha (Sucesso)**
```bash
curl -X POST http://localhost:5000/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "token": "TOKEN_RECEBIDO_NO_EMAIL",
    "newPassword": "MinhaNovaSenh@123",
    "confirmPassword": "MinhaNovaSenh@123"
  }'
```

**Resultado Esperado:**
```json
{
  "message": "Senha redefinida com sucesso!",
  "userName": "Nome do Usuário"
}
```

---

### **Cenário 2: Casos de Erro**

#### **2.1 - Email Não Encontrado**
```bash
curl -X POST http://localhost:5000/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{
    "email": "email-inexistente@teste.com"
  }'
```

**Resultado Esperado:**
```json
{
  "message": "Usuário não encontrado ou não possui senha definida (usuário OAuth)."
}
```

#### **2.2 - Token Inválido**
```bash
curl -X POST http://localhost:5000/api/auth/validate-token \
  -H "Content-Type: application/json" \
  -d '{
    "token": "token-inexistente-12345"
  }'
```

**Resultado Esperado:**
```json
{
  "message": "Token não encontrado ou inválido.",
  "isValid": false
}
```

#### **2.3 - Senhas Não Coincidem**
```bash
curl -X POST http://localhost:5000/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "token": "TOKEN_VÁLIDO",
    "newPassword": "MinhaNovaSenh@123",
    "confirmPassword": "SenhaDiferente@123"
  }'
```

**Resultado Esperado:**
```json
{
  "message": "Senhas não coincidem"
}
```

#### **2.4 - Senha Fraca**
```bash
curl -X POST http://localhost:5000/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "token": "TOKEN_VÁLIDO",
    "newPassword": "123",
    "confirmPassword": "123"
  }'
```

**Resultado Esperado:**
```json
{
  "message": "Senha deve ter no mínimo 8 caracteres"
}
```

---

### **Cenário 3: Segurança**

#### **3.1 - Token Usado Duas Vezes**
1. Use um token válido para resetar senha (primeira vez)
2. Tente usar o mesmo token novamente

**Resultado Esperado:**
```json
{
  "message": "Este token já foi utilizado.",
  "isValid": false
}
```

#### **3.2 - Token Expirado (após 1 hora)**
Aguarde 1 hora após gerar o token e tente validá-lo:

**Resultado Esperado:**
```json
{
  "message": "Token expirado. Solicite um novo reset de senha.",
  "isValid": false
}
```

---

## 🗃️ Verificações no Banco de Dados

### **Verificar Tokens Gerados**
```sql
SELECT 
    prt.Token,
    prt.CreatedAt,
    prt.ExpiryDate,
    prt.IsUsed,
    u.Nome as UserName,
    u.Email
FROM PasswordResetTokens prt
INNER JOIN Users u ON prt.UserId = u.Id
ORDER BY prt.CreatedAt DESC;
```

### **Verificar Senhas Hasheadas**
```sql
SELECT 
    Id,
    Nome,
    Email,
    LEFT(Password, 20) + '...' as PasswordHash,
    UpdatedAt
FROM Users 
WHERE Email = 'seu-email@teste.com';
```

---

## 📊 Checklist de Validação

### ✅ **Funcionalidades Básicas**
- [ ] Email enviado com nome personalizado
- [ ] Token único gerado corretamente
- [ ] Validação de token funciona
- [ ] Reset de senha funciona
- [ ] Validações de senha aplicadas

### ✅ **Segurança**
- [ ] Token expira em 1 hora
- [ ] Token é de uso único
- [ ] Senha é hasheada com BCrypt
- [ ] Validação de formato de senha
- [ ] Usuários OAuth são tratados corretamente

### ✅ **UX/UI**
- [ ] Mensagens de erro claras
- [ ] Email com template profissional
- [ ] Informações do usuário retornadas
- [ ] Fluxo intuitivo de 3 etapas

### ✅ **Logs e Monitoramento**
- [ ] Logs de email enviado
- [ ] Logs de erro de envio
- [ ] Logs de tentativas de reset
- [ ] Informações de auditoria

---

## 🚨 Troubleshooting

### **Email não está sendo enviado**
1. Verificar configurações SMTP no `appsettings.json`
2. Verificar se a senha de app do Gmail está correta
3. Verificar logs de erro no console

### **Token não está sendo validado**
1. Verificar se a tabela `PasswordResetTokens` existe
2. Verificar se o migration foi executado
3. Verificar se o token não expirou

### **Erro de validação de senha**
1. Verificar regex de validação
2. Testar com senha forte: `MinhaSenh@123`
3. Verificar se confirmação de senha está igual

---

## 🎯 Próximos Passos Sugeridos

1. **Rate Limiting**: Implementar limite de tentativas por IP
2. **Captcha**: Adicionar verificação anti-bot
3. **Configuração**: Mover URLs do frontend para configuração
4. **Monitoramento**: Implementar métricas de uso
5. **Testes Automatizados**: Criar testes unitários e de integração
