# 🌍 Viaggia - Sistema de Gerenciamento de Pacotes de Viagem

Este repositório contém o **back-end** do sistema **Viaggia**, desenvolvido com **ASP.NET Core 8**, utilizando o **Entity Framework Core**, autenticação com JWT, integração com APIs externas, e arquitetura limpa baseada em **camadas**.

## Tecnologias Utilizadas

- [.NET 8 (ASP.NET Core Web API)](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0)
- [Entity Framework Core (SQL Server)](https://learn.microsoft.com/en-us/ef/core/)
- [JWT (Json Web Token)](https://jwt.io/)
- [Swashbuckle (Swagger)](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [APIs externas](https://) (Google Maps, Stripe para pagamentos e Google SMTP para envio de e-mails e resete de senhas)

---

## 📁 Estrutura de Pastas
```
/viaggia_server
│
├── Controllers/         # Endpoints das APIs
├── Services/            # Lógica de negócio
├── Repositories/        # Acesso ao banco (repositórios + interfaces)
├── Models/              # Entidades principais
├── DTOs/                # Objetos de transferência de dados
├── Data/                # DbContext
├── EmailTemplates/      # Templates HTML dos e-mails enviados
├── wwwroot/             # Arquivos estáticos (ex: imagens)
│
├── Program.cs           # Entrada da aplicação
├── appsettings.json     # Configurações do projeto
```

---

### Modelo do Banco de Dados
```mermaid
erDiagram
    User {
        int Id
        string Name
        string GoogleId
        string Email
        string Password
        string PhoneNumber
        string AvatarUrl
        DateTime CreateDate
        bool IsActive
        string Cpf
        string CompanyName
        string CompanyLegalName
        string EmployerCompanyName
        string EmployeeId
        string StripeCustomerId
        int HotelId
    }

    Role {
        int Id
        string Name
        bool IsActive
    }

    Hotel {
        int HotelId
        string Name
        string Cnpj
        string Street
        string City
        string State
        string ZipCode
        string Description
        int StarRating
        string CheckInTime
        string CheckOutTime
        string ContactPhone
        string ContactEmail
        bool IsActive
        double AverageRating
        int UserId
    }

    HotelRoomType {
        int RoomTypeId
        RoomTypeEnum Name
        string Description
        decimal Price
        int Capacity
        string BedType
        int TotalRooms
        int AvailableRooms
        bool IsActive
        int HotelId
    }

    Reserve {
        int ReserveId
        int UserId
        int HotelId
        int PackageId
        int RoomTypeId
        DateTime CheckInDate
        DateTime CheckOutDate
        string Status
        decimal TotalPrice
        decimal TotalDiscount
        int NumberOfRooms
        int NumberOfGuests
        bool IsActive
        DateTime CreatedAt
    }

    Package {
        int PackageId
        string Name
        string Destination
        string Description
        decimal BasePrice
        int HotelId
        int UserId
        bool IsActive
    }

    PackageDate {
        int PackageDateId
        DateTime StartDate
        DateTime EndDate
        int PackageId
        bool IsActive
    }

    Media {
        int MediaId
        string MediaUrl
        string MediaType
        int PackageId
        int HotelId
        bool IsActive
    }

    Commodity {
        int CommodityId
        bool HasParking
        bool IsParkingPaid
        decimal ParkingPrice
        bool HasBreakfast
        bool IsBreakfastPaid
        decimal BreakfastPrice
        bool HasLunch
        bool IsLunchPaid
        decimal LunchPrice
        bool HasDinner
        bool IsDinnerPaid
        decimal DinnerPrice
        bool HasSpa
        bool IsSpaPaid
        decimal SpaPrice
        bool HasPool
        bool IsPoolPaid
        decimal PoolPrice
        bool HasGym
        bool IsGymPaid
        decimal GymPrice
        bool HasWiFi
        bool IsWiFiPaid
        decimal WiFiPrice
        bool HasAirConditioning
        bool IsAirConditioningPaid
        decimal AirConditioningPrice
        bool HasAccessibilityFeatures
        bool IsAccessibilityFeaturesPaid
        decimal AccessibilityFeaturesPrice
        bool IsPetFriendly
        bool IsPetFriendlyPaid
        decimal PetFriendlyPrice
        bool IsActive
        int HotelId
    }

    CustomCommodity {
        int CustomCommodityId
        string Name
        bool IsPaid
        decimal Price
        string Description
        bool IsActive
        int CommodityId
        int HotelId
    }

    ReserveRoom {
        int Id
        int ReserveId
        int RoomTypeId
        int Quantity
    }

    %% Relationships
    User ||--o{ Reserve : makes
    User ||--o{ Package : creates
    User ||--o{ Hotel : manages

    Role ||--o{ User : assigns

    Hotel ||--o{ HotelRoomType : has
    Hotel ||--o{ Package : offers
    Hotel ||--o{ Media : has
    Hotel ||--o{ Commodity : includes
    Hotel ||--o{ CustomCommodity : adds
    Hotel ||--o{ Reserve : receives

    HotelRoomType ||--o{ Reserve : booked_in
    HotelRoomType ||--o{ ReserveRoom : linked_to

    Package ||--o{ PackageDate : schedules
    Package ||--o{ Media : contains
    Package ||--o{ Reserve : reserved_in

    Commodity ||--o{ CustomCommodity : expands

    Reserve ||--o{ ReserveRoom : contains

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
/viaggia_server
│
├── Controllers/                 # APIs expostas (camada de entrada)
│   ├── HotelsController.cs      # Controlador para hotéis
│   ├── ReviewsController.cs     # Controlador para avaliações
│   ├── UsersController.cs       # Controlador para usuários
│   ├── PackagesController.cs    # Controlador para pacotes
│
├── Services/                    # Lógica de negócio (camada de serviço)
│   ├── Hotels/
│   │   ├── IHotelService.cs     # Interface do serviço de hotéis
│   │   ├── HotelService.cs      # Implementação do serviço de hotéis
│   ├── Reviews/
│   │   ├── IReviewService.cs    # Interface do serviço de avaliações
│   │   ├── ReviewService.cs     # Implementação do serviço de avaliações
│   ├── Users/
│   │   ├── IUserService.cs      # Interface do serviço de usuários
│   │   ├── UserService.cs       # Implementação do serviço de usuários
│
├── Repositories/                # Acesso ao banco de dados
│   ├── IRepository.cs           # Interface genérica do repositório
│   ├── Repository.cs            # Implementação genérica do repositório
│   ├── Hotels/
│   │   ├── IHotelRepository.cs  # Interface do repositório de hotéis
│   │   ├── HotelRepository.cs   # Implementação do repositório de hotéis
│   ├── Reviews/
│   │   ├── IReviewRepository.cs # Interface do repositório de avaliações
│   │   ├── ReviewRepository.cs  # Implementação do repositório de avaliações
│   ├── Users/
│   │   ├── IUserRepository.cs   # Interface do repositório de usuários
│   │   ├── UserRepository.cs    # Implementação do repositório de usuários
│   ├── Packages/
│   │   ├── IPackageRepository.cs # Interface do repositório de pacotes
│   │   ├── PackageRepository.cs  # Implementação do repositório de pacotes
│
├── Models/                      # Entidades principais que refletem o banco
│   ├── ISoftDeletable.cs        # Interface para exclusão lógica
│   ├── Hotels/
│   │   ├── Hotel.cs             # Modelo de hotel
|   |   ├── HotelRoomType.cs     # Modelo de tipo de quarto
|   |   ├── HotelDate.cs         # Modelo de datas de disponibilidade
│   ├── Packages/
│   │   ├── Package.cs           # Modelo de pacote
│   │   ├── PackageDate.cs       # Modelo de datas do pacote
│   ├── Medias/
│   │   ├── Media.cs             # Modelo de mídia
│   ├── Reservations/
│   │   ├── Reservation.cs       # Modelo de reserva
│   ├── Payments/
│   │   ├── Payment.cs           # Modelo de pagamento
│   ├── Reviews/
│   │   ├── Review.cs            # Modelo de avaliação
│   ├── Users/
│   │   ├── User.cs              # Modelo de usuário
│   │   ├── Role.cs              # Modelo de papel
│   │   ├── UserRole.cs          # Modelo de relação usuário-papel
│
├── DTOs/                        # Objetos de transferência de dados
│   ├── ApiResponse.cs           # Resposta padrão da API
│   ├── Hotels/
│   │   ├── HotelDTO.cs          # DTO para leitura de hotéis
│   │   ├── CreateHotelDTO.cs    # DTO para criação de hotéis
│   │   ├── HotelRoomTypeDTO.cs  # DTO para tipo de quarto
│   │   ├── HotelDateDTO.cs      # DTO para datas de disponibilidade
│   ├── Reviews/
│   │   ├── ReviewDTO.cs         # DTO para leitura de avaliações
│   │   ├── CreateReviewDTO.cs   # DTO para criação de avaliações
│   ├── Users/
│   │   ├── UserDTO.cs           # DTO para leitura de usuários
│   │   ├── CreateClientDTO.cs   # DTO para criação de clientes
│   │   ├── CreateServiceProviderDTO.cs # DTO para criação de prestadores de serviço
│   │   ├── CreateAttendantDTO.cs # DTO para criação de atendentes
│   ├── Packages/
│   │   ├── PackageDTO.cs        # DTO para leitura de pacotes
│   │   ├── PackageCreateDTO.cs  # DTO para criação de pacotes
│   │   ├── PackageUpdateDTO.cs  # DTO para atualização de pacotes
│   │   ├── PackageDateDTO.cs    # DTO para datas de pacotes
│   │   ├── MediaDTO.cs          # DTO para mídia
│
├── Data/                        # DbContext
│   ├── AppDbContext.cs          # Contexto do banco de dados
│
├── wwwroot/                     # Arquivos estáticos (ex.: imagens)
│   ├── Uploads/
│   │   ├── Hotels/              # Mídias de hotéis
|   |   ├── Pacotes/             # Mídias de pactoes
│
├── Program.cs                   # Ponto de entrada da aplicação
├── appsettings.json             # Configurações gerais do projeto
├── appsettings.Development.json # Configurações específicas para desenvolvimento
```

---

### Modelo do Banco de Dados
```mermaid
erDiagram

USER {
  int UserId PK
  string Name
  string Email
  string Password
  bool IsActive
}

ROLE {
  int RoleId PK
  string Name
}

USER_ROLE {
  int UserId PK, FK
  int RoleId PK, FK
}

HOTEL {
  int HotelId PK
  string Name
  string Street
  string City
  string State
  string ZipCode
  string Description
  int StarRating
  bool HasParking
  bool HasBreakfast
  bool HasSpa
  bool HasPool
  bool HasGym
  bool HasWiFi
  bool IsPetFriendly
  string CheckInTime
  string CheckOutTime
  string ContactPhone
  string ContactEmail
  bool IsActive
}

HOTEL_ROOM_TYPE {
  int RoomTypeId PK
  string Name
  string Description
  decimal Price
  int Capacity
  string BedType
  int HotelId FK
  bool IsActive
}

HOTEL_DATE {
  int HotelDateId PK
  datetime StartDate
  datetime EndDate
  int AvailableRooms
  int RoomTypeId FK
  int HotelId FK
  bool IsActive
}

PACKAGE {
  int PackageId PK
  string Name
  string Description
  decimal Price
  bool IsActive
}

PACKAGE_DATE {
  int PackageDateId PK
  datetime StartDate
  datetime EndDate
  int PackageId FK
  bool IsActive
}

RESERVATION {
  int ReservationId PK
  int UserId FK
  int PackageId FK
  int HotelId FK
  int RoomTypeId FK
  datetime StartDate
  datetime EndDate
  decimal TotalPrice
  int NumberOfGuests
  string Status
  bool IsActive
}

PAYMENT {
  int PaymentId PK
  int UserId FK
  int ReservationId FK
  decimal Amount
  datetime PaymentDate
  string PaymentMethod
  string Status
  bool IsActive
}

MEDIA {
  int MediaId PK
  string MediaUrl
  string MediaType
  int PackageId FK
  int HotelId FK
  bool IsActive
}

REVIEW {
  int ReviewId PK
  int UserId FK
  string ReviewType
  int HotelId FK
  int Rating
  string Comment
  datetime CreatedAt
  bool IsActive
}

USER ||--o{ USER_ROLE : possui
ROLE ||--o{ USER_ROLE : possui
USER ||--o{ RESERVATION : realiza
USER ||--o{ PAYMENT : realiza
USER ||--o{ REVIEW : escreve

HOTEL ||--o{ HOTEL_ROOM_TYPE : possui
HOTEL ||--o{ HOTEL_DATE : possui
HOTEL ||--o{ RESERVATION : possui
HOTEL ||--o{ MEDIA : possui
HOTEL ||--o{ REVIEW : recebe

HOTEL_ROOM_TYPE ||--o{ HOTEL_DATE : possui
HOTEL_ROOM_TYPE ||--o{ RESERVATION : possui

PACKAGE ||--o{ PACKAGE_DATE : possui
PACKAGE ||--o{ RESERVATION : possui
PACKAGE ||--o{ MEDIA : possui

RESERVATION ||--o{ PAYMENT : possui

MEDIA ||--o{ HOTEL : associada
MEDIA ||--o{ PACKAGE : associada

REVIEW ||--o{ HOTEL : avalia
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

