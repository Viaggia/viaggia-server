# 🔧 Script de Configuração do Stripe CLI

## 📥 Instalação do Stripe CLI

### Windows (Chocolatey)
```powershell
choco install stripe-cli
```

### Windows (Scoop)
```powershell
scoop install stripe
```

### Windows (Download Manual)
1. Baixe de: https://github.com/stripe/stripe-cli/releases
2. Extraia e adicione ao PATH

## 🔐 Configuração Inicial

### 1. Fazer Login no Stripe
```bash
stripe login
```
*Isso abrirá o navegador para autorizar o CLI*

### 2. Verificar Conexão
```bash
stripe --version
stripe config --list
```

## 🔄 Configurar Webhooks para Desenvolvimento

### 1. Escutar Webhooks Localmente
```bash
# Escutar na porta 5223 (porta do seu projeto)
stripe listen --forward-to localhost:5223/api/webhook/stripe

# Ou com HTTPS se configurado
stripe listen --forward-to https://localhost:7164/api/webhook/stripe
```

### 2. Obter Webhook Secret para Desenvolvimento
```bash
# O comando acima exibirá algo como:
# > Ready! Your webhook signing secret is whsec_1234567890abcdef...
# Copie esse valor para o appsettings.json
```

## 🧪 Testar Webhooks

### 1. Simular Eventos Específicos
```bash
# Pagamento bem-sucedido
stripe trigger payment_intent.succeeded

# Pagamento falhou
stripe trigger payment_intent.payment_failed

# Pagamento cancelado
stripe trigger payment_intent.canceled

# Reembolso criado
stripe trigger charge.refunded

# Disputa criada
stripe trigger charge.dispute.created

# Pagamento requer ação (3D Secure)
stripe trigger payment_intent.requires_action
```

### 2. Simular com Dados Customizados
```bash
# Criar evento com valor específico
stripe trigger payment_intent.succeeded --add payment_intent:amount=59999

# Criar evento com metadados
stripe trigger payment_intent.succeeded --add payment_intent:metadata[user_id]=123
```

## 🔗 Configurar Webhook no Dashboard (Produção)

### 1. Acessar Dashboard
- Vá para: https://dashboard.stripe.com/webhooks
- Clique em "Add endpoint"

### 2. Configurar Endpoint
```
URL: https://sua-api.com/api/webhook/stripe
Events: Selecionar os eventos listados abaixo
```

### 3. Eventos Essenciais para o Viaggia
```
payment_intent.succeeded
payment_intent.payment_failed  
payment_intent.canceled
charge.refunded
charge.dispute.created
charge.dispute.updated
payment_intent.requires_action
payment_intent.processing
charge.captured
charge.failed
refund.created
```

### 4. Obter Webhook Secret
- Após criar o endpoint, clique nele
- Copie o "Signing secret" (começa com whsec_)
- Cole no appsettings.json

## 🚀 Scripts Úteis para Desenvolvimento

### PowerShell - Iniciar Stripe Listen
```powershell
# stripe-dev.ps1
Write-Host "🚀 Iniciando Stripe CLI para desenvolvimento..." -ForegroundColor Green
Write-Host "📡 Webhook endpoint: localhost:5223/api/webhook/stripe" -ForegroundColor Yellow

stripe listen --forward-to localhost:5223/api/webhook/stripe --print-json
```

### PowerShell - Testar Fluxo Completo
```powershell
# test-payment-flow.ps1
Write-Host "🧪 Testando fluxo completo de pagamento..." -ForegroundColor Green

Write-Host "1️⃣ Criando pagamento bem-sucedido..." -ForegroundColor Yellow
stripe trigger payment_intent.succeeded --add payment_intent:amount=59999

Start-Sleep -Seconds 2

Write-Host "2️⃣ Criando pagamento falhou..." -ForegroundColor Yellow  
stripe trigger payment_intent.payment_failed

Start-Sleep -Seconds 2

Write-Host "3️⃣ Criando reembolso..." -ForegroundColor Yellow
stripe trigger charge.refunded --add charge:amount=29999

Write-Host "✅ Testes concluídos! Verifique os logs da API." -ForegroundColor Green
```

## 📋 Verificações Importantes

### 1. Verificar se a API está rodando
```bash
curl http://localhost:5223/api/webhook/stripe -X POST -H "Content-Type: application/json" -d "{}"
```

### 2. Verificar logs da aplicação
- Monitorar console da aplicação .NET
- Verificar se os webhooks estão sendo recebidos
- Confirmar se os eventos estão sendo processados

### 3. Testar com dados reais (Modo Test)
```bash
# Usar cartões de teste do Stripe
# 4242424242424242 - Sucesso
# 4000000000000002 - Cartão recusado  
# 4000000000009995 - Saldo insuficiente
```

## 🔧 Troubleshooting

### Webhook não está sendo recebido
1. Verificar se a porta 5223 está correta
2. Confirmar se o endpoint /api/webhook/stripe existe
3. Verificar firewall/antivírus

### Erro de assinatura inválida  
1. Confirmar webhook secret no appsettings.json
2. Verificar se não há espaços extras no secret
3. Reiniciar aplicação após alterar configuração

### Eventos não estão sendo processados
1. Verificar logs de erro na aplicação
2. Confirmar se os métodos HandleXXX estão implementados
3. Verificar conexão com banco de dados

---

**💡 Dica**: Mantenha o `stripe listen` rodando em um terminal separado enquanto desenvolve!
