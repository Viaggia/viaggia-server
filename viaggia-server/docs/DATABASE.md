# 🗄️ Documentação do Banco de Dados - Viaggia Server

## 📊 Visão Geral

O banco de dados do Viaggia Server foi projetado para suportar um sistema completo de reservas de viagens e hotéis, utilizando **Entity Framework Core** com **SQL Server** como SGBD principal.

## 🏗️ Estratégias de Design

### 🔄 Soft Delete
Todas as entidades principais implementam a interface `ISoftDeletable` com o campo `IsActive`, permitindo exclusão lógica em vez de física.

### 🔍 Query Filters
Filtros globais aplicados automaticamente via `OnModelCreating` para ocultar registros inativos:

```csharp
modelBuilder.Entity<User>().HasQueryFilter(u => u.IsActive);
modelBuilder.Entity<Package>().HasQueryFilter(p => p.IsActive);
modelBuilder.Entity<Hotel>().HasQueryFilter(h => h.IsActive);
```

### 🔗 Relacionamentos
- **NoAction** como padrão para `OnDelete` para evitar cascade deletions acidentais
- Chaves estrangeiras opcionais onde necessário (`IsRequired(false)`)
- Relacionamentos muitos-para-muitos implementados via entidades de junção

## 📋 Entidades Detalhadas

### 👤 **Users** - Usuários do Sistema

```sql
CREATE TABLE Users (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    Email nvarchar(100) NOT NULL,
    Password nvarchar(max) NOT NULL,
    PhoneNumber nvarchar(20),
    GoogleId nvarchar(100), -- Para OAuth
    AvatarUrl nvarchar(500), -- URL da foto do perfil
    
    -- Campos específicos por tipo de usuário
    Cpf nvarchar(14), -- Para clientes
    DateOfBirth datetime2, -- Para clientes
    Cnpj nvarchar(18), -- Para provedores
    CompanyLegalName nvarchar(100), -- Para provedores
    EmployerCompanyName nvarchar(100), -- Para atendentes
    EmployeeId nvarchar(50), -- Para atendentes
    
    IsActive bit NOT NULL DEFAULT 1,
    CreateDate datetime2 NOT NULL DEFAULT GETUTCDATE()
);
```

### 🏢 **Roles** - Papéis/Funções

```sql
CREATE TABLE Roles (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(50) NOT NULL UNIQUE -- CLIENT, SERVICE_PROVIDER, ATTENDANT, ADMIN
);
```

### 🔗 **UserRoles** - Relacionamento Usuário-Papel

```sql
CREATE TABLE UserRoles (
    UserId int NOT NULL,
    RoleId int NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);
```

### 🏨 **Hotels** - Hotéis

```sql
CREATE TABLE Hotels (
    HotelId int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    Cnpj nvarchar(18) NOT NULL,
    Description nvarchar(500),
    StarRating int CHECK (StarRating >= 1 AND StarRating <= 5),
    CheckInTime nvarchar(10),
    CheckOutTime nvarchar(10),
    ContactPhone nvarchar(20),
    ContactEmail nvarchar(100),
    AddressId int NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    
    FOREIGN KEY (AddressId) REFERENCES Addresses(AddressId)
);
```

### 📍 **Addresses** - Endereços (TPH - Table Per Hierarchy)

```sql
CREATE TABLE Addresses (
    AddressId int IDENTITY(1,1) PRIMARY KEY,
    AddressType nvarchar(max) NOT NULL, -- Discriminator: 'Address' or 'BillingAddress'
    Street nvarchar(100) NOT NULL,
    City nvarchar(50) NOT NULL,
    State nvarchar(50) NOT NULL,
    ZipCode nvarchar(20) NOT NULL,
    
    -- Campos específicos para BillingAddress
    CardHolderName nvarchar(100),
    
    IsActive bit NOT NULL DEFAULT 1
);
```

### 🛏️ **RoomTypes** - Tipos de Quarto

```sql
CREATE TABLE RoomTypes (
    RoomTypeId int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    Description nvarchar(500),
    Price decimal(10,2) NOT NULL,
    Capacity int NOT NULL CHECK (Capacity >= 1 AND Capacity <= 10),
    BedType nvarchar(50) NOT NULL,
    HotelId int NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    
    FOREIGN KEY (HotelId) REFERENCES Hotels(HotelId)
);
```

### 📅 **HotelDates** - Disponibilidade de Hotéis

```sql
CREATE TABLE HotelDates (
    HotelDateId int IDENTITY(1,1) PRIMARY KEY,
    StartDate datetime2 NOT NULL,
    EndDate datetime2 NOT NULL,
    AvailableRooms int NOT NULL CHECK (AvailableRooms >= 0),
    RoomTypeId int NOT NULL,
    HotelId int NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    
    FOREIGN KEY (RoomTypeId) REFERENCES RoomTypes(RoomTypeId),
    FOREIGN KEY (HotelId) REFERENCES Hotels(HotelId)
);
```

### 📦 **Packages** - Pacotes de Viagem

```sql
CREATE TABLE Packages (
    PackageId int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    Destination nvarchar(100) NOT NULL,
    Description nvarchar(500),
    BasePrice decimal(10,2) NOT NULL,
    HotelId int NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    
    FOREIGN KEY (HotelId) REFERENCES Hotels(HotelId)
);
```

### 📅 **PackageDates** - Datas dos Pacotes

```sql
CREATE TABLE PackageDates (
    PackageDateId int IDENTITY(1,1) PRIMARY KEY,
    StartDate datetime2 NOT NULL,
    EndDate datetime2 NOT NULL,
    PackageId int NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    
    FOREIGN KEY (PackageId) REFERENCES Packages(PackageId)
);
```

### 📅 **Reservations** - Reservas

```sql
CREATE TABLE Reservations (
    ReservationId int IDENTITY(1,1) PRIMARY KEY,
    UserId int NOT NULL,
    PackageId int, -- Opcional
    RoomTypeId int, -- Opcional
    HotelId int, -- Opcional
    StartDate datetime2 NOT NULL,
    EndDate datetime2 NOT NULL,
    TotalPrice decimal(10,2) NOT NULL,
    NumberOfGuests int NOT NULL CHECK (NumberOfGuests >= 1 AND NumberOfGuests <= 10),
    Status nvarchar(20) NOT NULL, -- 'Confirmed', 'Cancelled', 'Pending'
    IsActive bit NOT NULL DEFAULT 1,
    
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (PackageId) REFERENCES Packages(PackageId),
    FOREIGN KEY (RoomTypeId) REFERENCES RoomTypes(RoomTypeId),
    FOREIGN KEY (HotelId) REFERENCES Hotels(HotelId)
);
```

### 💳 **Payments** - Pagamentos

```sql
CREATE TABLE Payments (
    PaymentId int IDENTITY(1,1) PRIMARY KEY,
    UserId int NOT NULL,
    ReservationId int NOT NULL,
    Amount decimal(10,2) NOT NULL,
    PaymentDate datetime2 NOT NULL,
    PaymentMethod nvarchar(50) NOT NULL, -- 'CreditCard', 'BankTransfer', etc.
    Status nvarchar(20) NOT NULL, -- 'Completed', 'Pending', 'Failed'
    BillingAddressId int NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (ReservationId) REFERENCES Reservations(ReservationId),
    FOREIGN KEY (BillingAddressId) REFERENCES Addresses(AddressId)
);
```

### 👥 **Companions** - Acompanhantes

```sql
CREATE TABLE Companions (
    CompanionId int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    DocumentNumber nvarchar(20) NOT NULL,
    DateOfBirth datetime2 NOT NULL,
    ReservationId int NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    
    FOREIGN KEY (ReservationId) REFERENCES Reservations(ReservationId)
);
```

### 📸 **Medias** - Mídias (Fotos/Vídeos)

```sql
CREATE TABLE Medias (
    MediaId int IDENTITY(1,1) PRIMARY KEY,
    MediaUrl nvarchar(500) NOT NULL,
    MediaType nvarchar(20) NOT NULL, -- 'image', 'video'
    PackageId int, -- Exclusivo com HotelId
    HotelId int, -- Exclusivo com PackageId
    IsActive bit NOT NULL DEFAULT 1,
    
    FOREIGN KEY (PackageId) REFERENCES Packages(PackageId),
    FOREIGN KEY (HotelId) REFERENCES Hotels(HotelId),
    
    -- Constraint: Deve pertencer a Package OU Hotel, mas não ambos
    CONSTRAINT CK_Media_OneEntity CHECK (
        (PackageId IS NOT NULL AND HotelId IS NULL) OR 
        (PackageId IS NULL AND HotelId IS NOT NULL)
    )
);
```

### ⭐ **Reviews** - Avaliações

```sql
CREATE TABLE Reviews (
    ReviewId int IDENTITY(1,1) PRIMARY KEY,
    UserId int NOT NULL,
    HotelId int,
    Rating int NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment nvarchar(1000),
    ReviewDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive bit NOT NULL DEFAULT 1,
    
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (HotelId) REFERENCES Hotels(HotelId)
);
```

### 🏨 **Commodities** - Comodidades

```sql
CREATE TABLE Commodities (
    CommoditieId int IDENTITY(1,1) PRIMARY KEY,
    HotelId int NOT NULL UNIQUE, -- Relacionamento 1:1
    HasWiFi bit NOT NULL DEFAULT 0,
    HasPool bit NOT NULL DEFAULT 0,
    HasGym bit NOT NULL DEFAULT 0,
    HasSpa bit NOT NULL DEFAULT 0,
    HasParking bit NOT NULL DEFAULT 0,
    HasBreakfast bit NOT NULL DEFAULT 0,
    HasAirConditioning bit NOT NULL DEFAULT 0,
    IsActive bit NOT NULL DEFAULT 1,
    
    FOREIGN KEY (HotelId) REFERENCES Hotels(HotelId)
);
```

### 🛎️ **CommoditiesServices** - Serviços das Comodidades

```sql
CREATE TABLE CommoditiesServices (
    CommoditieServicesId int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    Description nvarchar(500),
    Price decimal(10,2),
    CommoditieId int NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    
    FOREIGN KEY (CommoditieId) REFERENCES Commodities(CommoditieId)
);
```

## 🔍 Índices Recomendados

### 📈 Índices de Performance

```sql
-- Busca de usuários por email (login)
CREATE INDEX IX_Users_Email ON Users(Email) WHERE IsActive = 1;

-- Busca de hotéis por localização
CREATE INDEX IX_Hotels_Location ON Hotels(AddressId) WHERE IsActive = 1;

-- Busca de pacotes por destino
CREATE INDEX IX_Packages_Destination ON Packages(Destination) WHERE IsActive = 1;

-- Busca de reservas por usuário
CREATE INDEX IX_Reservations_User ON Reservations(UserId) WHERE IsActive = 1;

-- Busca de reservas por data
CREATE INDEX IX_Reservations_Dates ON Reservations(StartDate, EndDate) WHERE IsActive = 1;

-- Busca de disponibilidade de hotéis
CREATE INDEX IX_HotelDates_Availability ON HotelDates(HotelId, StartDate, EndDate) WHERE IsActive = 1;

-- Busca de pagamentos por usuário
CREATE INDEX IX_Payments_User ON Payments(UserId) WHERE IsActive = 1;
```

## 🔧 Stored Procedures Sugeridas

### 🏨 Busca de Hotéis com Disponibilidade

```sql
CREATE PROCEDURE sp_SearchAvailableHotels
    @City NVARCHAR(50),
    @CheckIn DATETIME2,
    @CheckOut DATETIME2,
    @Guests INT
AS
BEGIN
    SELECT DISTINCT h.*
    FROM Hotels h
    INNER JOIN Addresses a ON h.AddressId = a.AddressId
    INNER JOIN HotelDates hd ON h.HotelId = hd.HotelId
    INNER JOIN RoomTypes rt ON hd.RoomTypeId = rt.RoomTypeId
    WHERE h.IsActive = 1
      AND a.City = @City
      AND hd.StartDate <= @CheckIn
      AND hd.EndDate >= @CheckOut
      AND rt.Capacity >= @Guests
      AND hd.AvailableRooms > 0;
END
```

### 📊 Relatório de Ocupação

```sql
CREATE PROCEDURE sp_HotelOccupancyReport
    @HotelId INT,
    @StartDate DATETIME2,
    @EndDate DATETIME2
AS
BEGIN
    SELECT 
        rt.Name AS RoomType,
        COUNT(r.ReservationId) AS TotalReservations,
        SUM(r.TotalPrice) AS TotalRevenue,
        AVG(CAST(r.NumberOfGuests AS FLOAT)) AS AvgGuests
    FROM RoomTypes rt
    LEFT JOIN Reservations r ON rt.RoomTypeId = r.RoomTypeId
        AND r.StartDate >= @StartDate 
        AND r.EndDate <= @EndDate
        AND r.IsActive = 1
    WHERE rt.HotelId = @HotelId
      AND rt.IsActive = 1
    GROUP BY rt.RoomTypeId, rt.Name
    ORDER BY TotalRevenue DESC;
END
```

## 🔒 Políticas de Segurança

### 🔐 Row-Level Security (RLS)

```sql
-- Usuários só podem ver suas próprias reservas
CREATE FUNCTION fn_UserReservationPredicate(@UserId INT)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS AccessResult
WHERE @UserId = USER_ID() OR IS_MEMBER('db_admin') = 1;

CREATE SECURITY POLICY ReservationSecurityPolicy
ADD FILTER PREDICATE fn_UserReservationPredicate(UserId) ON Reservations;
```

### 🔍 Auditoria

```sql
-- Tabela de auditoria para mudanças importantes
CREATE TABLE AuditLog (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    TableName NVARCHAR(100) NOT NULL,
    Operation NVARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
    EntityId INT NOT NULL,
    UserId INT,
    ChangeDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    OldValues NVARCHAR(MAX),
    NewValues NVARCHAR(MAX)
);
```

## 📈 Otimizações de Performance

### 🔄 Particionamento

```sql
-- Particionamento da tabela de Reservations por ano
CREATE PARTITION FUNCTION pf_ReservationsByYear (DATETIME2)
AS RANGE RIGHT FOR VALUES 
('2023-01-01', '2024-01-01', '2025-01-01', '2026-01-01');

CREATE PARTITION SCHEME ps_ReservationsByYear
AS PARTITION pf_ReservationsByYear
ALL TO ([PRIMARY]);

-- Aplicar ao criar a tabela
CREATE TABLE Reservations (
    -- campos...
) ON ps_ReservationsByYear(StartDate);
```

### 📊 Estatísticas

```sql
-- Manter estatísticas atualizadas para melhor performance
UPDATE STATISTICS Hotels;
UPDATE STATISTICS Reservations;
UPDATE STATISTICS Packages;
```

## 🧪 Scripts de Teste

### 📝 Dados de Exemplo

```sql
-- Inserir roles padrão
INSERT INTO Roles (Name) VALUES 
('CLIENT'), ('SERVICE_PROVIDER'), ('ATTENDANT'), ('ADMIN');

-- Inserir usuário admin padrão
INSERT INTO Users (Name, Email, Password, IsActive, CreateDate)
VALUES ('Admin', 'admin@viaggia.com', 'hashed_password', 1, GETUTCDATE());

INSERT INTO UserRoles (UserId, RoleId)
VALUES (1, 4); -- Admin role
```

## 🔄 Backup e Restore

### 💾 Estratégia de Backup

```sql
-- Backup completo diário
BACKUP DATABASE ViaggiaDb 
TO DISK = 'C:\Backups\ViaggiaDb_Full.bak'
WITH FORMAT, COMPRESSION;

-- Backup de log a cada 15 minutos
BACKUP LOG ViaggiaDb 
TO DISK = 'C:\Backups\ViaggiaDb_Log.trn'
WITH COMPRESSION;
```

### 🔧 Maintenance Plans

- **Reorganize Index**: Semanal
- **Update Statistics**: Diário
- **Check DB Integrity**: Semanal
- **Cleanup History**: Mensal

---

**📝 Nota**: Esta documentação deve ser atualizada sempre que houver mudanças no esquema do banco de dados.
