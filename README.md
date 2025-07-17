# 🌍 Viaggia - Sistema de Gerenciamento de Pacotes de Viagem

Este repositório contém o **back-end** do sistema **Viaggia**, desenvolvido com **ASP.NET Core 8**, utilizando o **Entity Framework Core**, autenticação com JWT, integração com APIs externas, e arquitetura limpa baseada em **camadas**.

## Tecnologias Utilizadas

- [.NET 8 (ASP.NET Core Web API)](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0)
- [Entity Framework Core (SQL Server)](https://learn.microsoft.com/en-us/ef/core/)
- [JWT (Json Web Token)](https://jwt.io/)
- [Swashbuckle (Swagger)](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [APIs externas](https://) (localização, pagamentos e validação de login)

---

## 📁 Estrutura de Pastas
```
Viaggia.Backend/
├── Viaggia.API/
│   ├── Controllers/
│   ├── Middlewares/
│   └── Program.cs
│
├── Viaggia.Application/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Services/
│
├── Viaggia.Domain/
│   ├── Entities/
│   ├── Enums/
│   └── ValueObjects/
│
├── Viaggia.Infrastructure/
│   ├── Data/
│   ├── Repositories/
│   └── ExternalServices/
│
├── Viaggia.Tests/
│
├── Viaggia.sln
└── README.md
```

---

### Modelo do Banco de Dados
```mermaid
erDiagram

USUARIO {
  int id PK
  string nome
  string email
  string senha_hash
  string telefone
  datetime criado_em
  boolean is_admin
}

DESTINO {
  int id PK
  string nome
  string descricao
  string imagem_url
  string cidade
  string estado
  string pais
}

PACOTE {
  int id PK
  string nome
  string descricao
  decimal preco_base
  string imagem_url
  boolean eh_fechado
  int destino_id FK
}

PACOTE_DATA {
  int id PK
  int pacote_id FK
  date data_inicio
  date data_fim
}

TIPO_QUARTO {
  int id PK
  string nome
  int capacidade
  decimal valor_extra
}

PACOTE_DATA_QUARTO {
  int id PK
  int pacote_data_id FK
  int tipo_quarto_id FK
  int vagas
}

RESERVA {
  int id PK
  int usuario_id FK
  int pacote_data_quarto_id FK
  int numero_pessoas
  decimal valor_total
  datetime data_reserva
  string status
}

ACOMPANHANTE {
  int id PK
  int reserva_id FK
  string nome
  string documento
  date data_nascimento
}

PAGAMENTO {
  int id PK
  int reserva_id FK
  decimal valor_pago
  string metodo_pagamento
  string status
  datetime data_pagamento
  string comprovante_url
}

HISTORICO_COMPRA {
  int id PK
  int usuario_id FK
  int reserva_id FK
  datetime data_visualizacao
  string observacao
}

AVALIACAO {
  int id PK
  int usuario_id FK
  int pacote_id FK
  int nota
  string comentario
  datetime data_avaliacao
}


USUARIO ||--o{ RESERVA : realiza
USUARIO ||--o{ AVALIACAO : escreve
USUARIO ||--o{ HISTORICO_COMPRA : possui

DESTINO ||--o{ PACOTE : contem

PACOTE ||--o{ PACOTE_DATA : possui
PACOTE ||--o{ AVALIACAO : recebe

PACOTE_DATA ||--o{ PACOTE_DATA_QUARTO : possui
PACOTE_DATA_QUARTO }o--|| TIPO_QUARTO : utiliza

PACOTE_DATA_QUARTO ||--o{ RESERVA : permite

RESERVA ||--|{ ACOMPANHANTE : inclui
RESERVA ||--o{ PAGAMENTO : possui
RESERVA ||--o{ HISTORICO_COMPRA : referenciada
```

---
### Como Executar Localmente

### ✅ Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [SQL Server](https://www.microsoft.com/pt-br/sql-server/sql-server-downloads)
- [Visual Studio 2022 ou superior](https://visualstudio.microsoft.com/pt-br/)

### ⚙️ Passo a Passo

1. **Clone o repositório**
   ```bash
   git clone https://github.com/seu-usuario/seu-repositorio.git
   cd Viaggia.Backend
   ```

2. **Configure o banco de dados**
   - Crie um banco de dados no SQL Server com o nome `ViaggiaDb` (ou o nome que preferir).
   - Altere a connection string em `Viaggia.API/appsettings.Development.json`:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=ViaggiaDb;User Id=sa;Password=SuaSenhaForte;"
     }
     ```

3. **Restaure os pacotes e compile o projeto**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Aplique as migrações (se houver)**
   ```bash
   dotnet ef database update --project Viaggia.Infrastructure --startup-project Viaggia.API
   ```

5. **Execute a aplicação**
   ```bash
   dotnet run --project Viaggia.API
   ```

6. **Acesse no navegador**
   - A API estará disponível em: `https://localhost:5001` ou `http://localhost:5000`

7. **Testar via Swagger**
   - Acesse: `https://localhost:5001/swagger/index.html`

### 🧪 Rodar os testes

```bash
dotnet test Viaggia.Tests
```
---
