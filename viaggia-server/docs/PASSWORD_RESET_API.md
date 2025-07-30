# 📧 API de Recuperação de Senha - Documentação

## 🎯 Visão Geral

Sistema de recuperação de senha em 2 etapas:
1. **Solicitar Reset** - Usuário informa email e recebe token por email
2. **Validar Token** - Usuário insere token e obtém permissão para redefinir senha
3. **Redefinir Senha** - Usuário define nova senha usando token válido

---

## 🔗 Endpoints

### 1. Solicitar Reset de Senha

**POST** `/api/auth/forgot-password`

```json
{
  "email": "usuario@exemplo.com"
}
```

**Resposta de Sucesso (200):**
```json
{
  "message": "E-mail de redefinição de senha enviado com sucesso."
}
```

**Resposta de Erro (400):**
```json
{
  "message": "Usuário não encontrado ou não possui senha definida (usuário OAuth)."
}
```

---

### 2. Validar Token de Reset

**POST** `/api/auth/validate-token`

```json
{
  "token": "12345678-90ab-cdef-1234-567890abcdef"
}
```

**Resposta de Sucesso (200):**
```json
{
  "message": "Token válido.",
  "isValid": true,
  "userName": "João Silva",
  "email": "joao@exemplo.com",
  "expiryDate": "2025-07-28T15:30:00Z"
}
```

**Resposta de Erro (400):**
```json
{
  "message": "Token expirado. Solicite um novo reset de senha.",
  "isValid": false
}
```

---

### 3. Redefinir Senha

**POST** `/api/auth/reset-password`

```json
{
  "token": "12345678-90ab-cdef-1234-567890abcdef",
  "newPassword": "MinhaNovaSenh@123",
  "confirmPassword": "MinhaNovaSenh@123"
}
```

**Resposta de Sucesso (200):**
```json
{
  "message": "Senha redefinida com sucesso!",
  "userName": "João Silva"
}
```

**Resposta de Erro (400):**
```json
{
  "message": "Senhas não coincidem"
}
```

---

## 🔒 Validações de Senha

A nova senha deve atender aos seguintes critérios:

- **Mínimo 8 caracteres**
- **1 letra minúscula** (a-z)
- **1 letra maiúscula** (A-Z)
- **1 número** (0-9)
- **1 caractere especial** (@$!%*?&)

---

## 📧 Template de Email

O email enviado contém:

- **Nome personalizado** do usuário
- **Token de segurança** destacado
- **Link direto** para página de validação
- **Instruções claras** sobre validade (1 hora)
- **Avisos de segurança**

---

## 🎨 Fluxo Frontend Sugerido

> **📋 Documentação Completa:** Para implementação detalhada do frontend, consulte o arquivo [`FRONTEND_PASSWORD_RESET.md`](./FRONTEND_PASSWORD_RESET.md) que contém:
> - Wireframes detalhados de todas as páginas
> - Código JavaScript completo
> - CSS responsivo
> - Validações de segurança
> - Testes de integração

### Resumo do Fluxo:

#### **Página 1: Solicitar Reset** (`/forgot-password`)
```html
<form>
    <input type="email" placeholder="Digite seu email" required>
    <button>Enviar Link de Recuperação</button>
</form>
```

#### **Página 2: Email Enviado** (`/email-sent`)
```html
<div>
    <h2>📧 Email Enviado!</h2>
    <p>Verifique sua caixa de entrada</p>
    <a href="/validate-token">Inserir token manualmente</a>
</div>
```

#### **Página 3: Validar Token** (`/validate-token?token=XXX`)
```html
<form>
    <h2>Olá, [Nome do Usuário]!</h2>
    <input type="text" placeholder="Insira o token" required>
    <button>Validar Token</button>
</form>
```

#### **Página 4: Nova Senha** (`/reset-password`)
```html
<form>
    <h2>Defina sua nova senha</h2>
    <input type="password" placeholder="Nova senha" required>
    <input type="password" placeholder="Confirme a senha" required>
    <div class="password-requirements">
        <ul>
            <li>✅ Mínimo 8 caracteres</li>
            <li>✅ 1 letra maiúscula</li>
            <li>✅ 1 letra minúscula</li>
            <li>✅ 1 número</li>
            <li>✅ 1 caractere especial</li>
        </ul>
    </div>
    <button>Redefinir Senha</button>
</form>
```

#### **Página 5: Sucesso** (`/password-reset-success`)
```html
<div>
    <h2>🎉 Senha Redefinida com Sucesso!</h2>
    <a href="/login">Fazer Login</a>
</div>
```

---

## ⚠️ Considerações de Segurança

- ✅ Token expira em 1 hora
- ✅ Token é de uso único
- ✅ Validação robusta de senha
- ✅ Logs de auditoria
- ✅ Verificação de usuário OAuth
- ✅ Rate limiting recomendado (implementar)

---

## 🧪 Testes com curl

### Solicitar Reset:
```bash
curl -X POST http://localhost:5000/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email": "teste@exemplo.com"}'
```

### Validar Token:
```bash
curl -X POST http://localhost:5000/api/auth/validate-token \
  -H "Content-Type: application/json" \
  -d '{"token": "seu-token-aqui"}'
```

### Reset Senha:
```bash
curl -X POST http://localhost:5000/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "token": "seu-token-aqui",
    "newPassword": "MinhaNovaSenh@123",
    "confirmPassword": "MinhaNovaSenh@123"
  }'
```
