# 📚 Documentação da API - Viaggia Server

## 🚀 Base URL

- **Development**: `https://localhost:7164`
- **Staging**: `https://api-staging.viaggia.com`
- **Production**: `https://api.viaggia.com`

## 🔐 Autenticação

A API utiliza **JWT Bearer Token** para autenticação. Inclua o token no header de todas as requisições protegidas:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 📋 Status Codes Padrão

| Code | Descrição | Quando Usar |
|------|-----------|-------------|
| 200 | OK | Sucesso em operações GET/PUT |
| 201 | Created | Sucesso na criação de recursos |
| 204 | No Content | Sucesso em DELETE ou operações sem retorno |
| 400 | Bad Request | Dados inválidos na requisição |
| 401 | Unauthorized | Token ausente/inválido |
| 403 | Forbidden | Usuário sem permissão |
| 404 | Not Found | Recurso não encontrado |
| 409 | Conflict | Conflito (ex: email já existe) |
| 422 | Unprocessable Entity | Falha na validação |
| 429 | Too Many Requests | Rate limiting |
| 500 | Internal Server Error | Erro do servidor |

## 🔑 Endpoints de Autenticação

### POST /api/auth/login
Autentica usuário com email e senha.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "securePassword123!"
}
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2024-01-15T10:30:00Z",
  "user": {
    "id": 1,
    "name": "João Silva",
    "email": "user@example.com",
    "roles": ["CLIENT"]
  }
}
```

**Response (401):**
```json
{
  "error": {
    "message": "Email ou senha incorretos.",
    "code": "INVALID_CREDENTIALS"
  }
}
```

### POST /api/auth/google
Autentica usuário via Google OAuth.

**Request:**
```json
{
  "googleUid": "google_user_id",
  "email": "user@gmail.com",
  "name": "João Silva",
  "picture": "https://lh3.googleusercontent.com/...",
  "phoneNumber": "+5511999999999"
}
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2024-01-15T10:30:00Z",
  "user": {
    "id": 1,
    "name": "João Silva",
    "email": "user@gmail.com",
    "roles": ["CLIENT"],
    "isNewUser": false
  }
}
```

### POST /api/auth/refresh
Renova token JWT.

**Headers:**
```http
Authorization: Bearer <current_token>
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2024-01-15T11:30:00Z"
}
```

## 👤 Endpoints de Usuários

### GET /api/user
Lista todos os usuários (Admin only).

**Headers:**
```http
Authorization: Bearer <admin_token>
```

**Query Parameters:**
- `page` (int): Página (default: 1)
- `size` (int): Itens por página (default: 10, max: 100)
- `search` (string): Busca por nome/email
- `role` (string): Filtrar por role (CLIENT, SERVICE_PROVIDER, etc.)
- `isActive` (bool): Filtrar por status ativo

**Response (200):**
```json
{
  "data": [
    {
      "id": 1,
      "name": "João Silva",
      "email": "joao@example.com",
      "roles": ["CLIENT"],
      "isActive": true,
      "createDate": "2024-01-01T10:00:00Z",
      "lastLoginDate": "2024-01-15T08:30:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "totalPages": 5,
    "totalItems": 47,
    "pageSize": 10
  }
}
```

### GET /api/user/{id}
Busca usuário por ID.

**Headers:**
```http
Authorization: Bearer <token>
```

**Response (200):**
```json
{
  "id": 1,
  "name": "João Silva",
  "email": "joao@example.com",
  "phoneNumber": "+5511999999999",
  "roles": ["CLIENT"],
  "cpf": "***.***.***-**",
  "dateOfBirth": "1990-05-15",
  "isActive": true,
  "createDate": "2024-01-01T10:00:00Z"
}
```

### POST /api/user/client
Cria novo cliente.

**Request:**
```json
{
  "name": "João Silva",
  "email": "joao@example.com",
  "password": "securePassword123!",
  "phoneNumber": "+5511999999999",
  "cpf": "12345678901",
  "dateOfBirth": "1990-05-15"
}
```

**Response (201):**
```json
{
  "id": 1,
  "name": "João Silva",
  "email": "joao@example.com",
  "roles": ["CLIENT"],
  "isActive": true,
  "createDate": "2024-01-15T10:00:00Z"
}
```

**Response (422):**
```json
{
  "errors": {
    "Email": ["Email already exists"],
    "Password": ["Password must contain at least 1 uppercase, 1 lowercase, 1 digit and 1 special character"],
    "Cpf": ["Invalid CPF format"]
  }
}
```

### POST /api/user/provider
Cria novo provedor de serviços.

**Request:**
```json
{
  "name": "Hotel Paradise Ltda",
  "email": "contato@hotelparadise.com",
  "password": "securePassword123!",
  "phoneNumber": "+5511888888888",
  "cnpj": "12345678000199",
  "companyLegalName": "Hotel Paradise Ltda"
}
```

**Response (201):**
```json
{
  "id": 2,
  "name": "Hotel Paradise Ltda",
  "email": "contato@hotelparadise.com",
  "roles": ["SERVICE_PROVIDER"],
  "isActive": true,
  "createDate": "2024-01-15T10:00:00Z"
}
```

### DELETE /api/user/{id}
Soft delete de usuário (Admin only).

**Headers:**
```http
Authorization: Bearer <admin_token>
```

**Response (204):** No content

### PUT /api/user/{id}/reactivate
Reativa usuário desativado (Admin only).

**Headers:**
```http
Authorization: Bearer <admin_token>
```

**Response (200):**
```json
{
  "message": "User reactivated successfully",
  "userId": 1
}
```

## 🏨 Endpoints de Hotéis

### GET /api/hotels
Lista hotéis com filtros.

**Query Parameters:**
- `page` (int): Página (default: 1)
- `size` (int): Itens por página (default: 10)
- `city` (string): Filtrar por cidade
- `state` (string): Filtrar por estado
- `minRating` (int): Rating mínimo (1-5)
- `maxRating` (int): Rating máximo (1-5)
- `checkIn` (date): Data de check-in (YYYY-MM-DD)
- `checkOut` (date): Data de check-out (YYYY-MM-DD)
- `guests` (int): Número de hóspedes

**Response (200):**
```json
{
  "data": [
    {
      "hotelId": 1,
      "name": "Hotel Paradise",
      "description": "Um hotel maravilhoso na praia",
      "starRating": 4,
      "address": {
        "street": "Rua das Palmeiras, 123",
        "city": "Rio de Janeiro",
        "state": "RJ",
        "zipCode": "22070-100"
      },
      "contactPhone": "+5521999999999",
      "contactEmail": "contato@hotelparadise.com",
      "averageRating": 4.5,
      "totalReviews": 128,
      "medias": [
        {
          "mediaId": 1,
          "mediaUrl": "/uploads/hotels/paradise-1.jpg",
          "mediaType": "image"
        }
      ],
      "commodities": {
        "hasWiFi": true,
        "hasPool": true,
        "hasGym": false,
        "hasBreakfast": true
      }
    }
  ],
  "pagination": {
    "currentPage": 1,
    "totalPages": 3,
    "totalItems": 25,
    "pageSize": 10
  }
}
```

### GET /api/hotels/{id}
Busca hotel por ID com detalhes completos.

**Response (200):**
```json
{
  "hotelId": 1,
  "name": "Hotel Paradise",
  "description": "Um hotel maravilhoso na praia de Copacabana",
  "starRating": 4,
  "checkInTime": "15:00",
  "checkOutTime": "12:00",
  "address": {
    "street": "Rua das Palmeiras, 123",
    "city": "Rio de Janeiro",
    "state": "RJ",
    "zipCode": "22070-100"
  },
  "contactPhone": "+5521999999999",
  "contactEmail": "contato@hotelparadise.com",
  "roomTypes": [
    {
      "roomTypeId": 1,
      "name": "Quarto Standard",
      "description": "Quarto confortável com vista para o mar",
      "price": 250.00,
      "capacity": 2,
      "bedType": "Casal",
      "isActive": true
    }
  ],
  "hotelDates": [
    {
      "hotelDateId": 1,
      "startDate": "2024-01-15",
      "endDate": "2024-01-20",
      "availableRooms": 5,
      "roomTypeId": 1
    }
  ],
  "medias": [
    {
      "mediaId": 1,
      "mediaUrl": "/uploads/hotels/paradise-1.jpg",
      "mediaType": "image"
    }
  ],
  "reviews": [
    {
      "reviewId": 1,
      "rating": 5,
      "comment": "Excelente hotel! Recomendo!",
      "reviewDate": "2024-01-10T14:30:00Z",
      "userName": "Maria Santos"
    }
  ],
  "commodities": {
    "hasWiFi": true,
    "hasPool": true,
    "hasGym": false,
    "hasSpa": true,
    "hasParking": true,
    "hasBreakfast": true,
    "hasAirConditioning": true,
    "services": [
      {
        "name": "Room Service",
        "description": "Serviço de quarto 24h",
        "price": 25.00
      }
    ]
  },
  "averageRating": 4.5,
  "totalReviews": 128,
  "isActive": true
}
```

### POST /api/hotels
Cria novo hotel (Provider only).

**Headers:**
```http
Authorization: Bearer <provider_token>
Content-Type: multipart/form-data
```

**Request (FormData):**
```json
{
  "name": "Hotel Paradise",
  "cnpj": "12345678000199",
  "description": "Um hotel maravilhoso na praia",
  "starRating": 4,
  "street": "Rua das Palmeiras, 123",
  "city": "Rio de Janeiro",
  "state": "RJ",
  "zipCode": "22070-100",
  "checkInTime": "15:00",
  "checkOutTime": "12:00",
  "contactPhone": "+5521999999999",
  "contactEmail": "contato@hotelparadise.com",
  "roomTypes": [
    {
      "name": "Quarto Standard",
      "description": "Quarto confortável",
      "price": 250.00,
      "capacity": 2,
      "bedType": "Casal"
    }
  ],
  "hotelDates": [
    {
      "startDate": "2024-01-15",
      "endDate": "2024-12-31",
      "availableRooms": 10
    }
  ],
  "commodities": {
    "hasWiFi": true,
    "hasPool": true,
    "hasBreakfast": true
  },
  "mediaFiles": ["hotel-image-1.jpg", "hotel-image-2.jpg"]
}
```

**Response (201):**
```json
{
  "hotelId": 1,
  "name": "Hotel Paradise",
  "message": "Hotel created successfully",
  "createdAt": "2024-01-15T10:00:00Z"
}
```

### PUT /api/hotels/{id}
Atualiza hotel (Provider/Admin only).

**Headers:**
```http
Authorization: Bearer <provider_token>
Content-Type: application/json
```

**Request:**
```json
{
  "name": "Hotel Paradise Premium",
  "description": "Um hotel premium na praia de Copacabana",
  "starRating": 5,
  "contactPhone": "+5521888888888"
}
```

**Response (200):**
```json
{
  "hotelId": 1,
  "name": "Hotel Paradise Premium",
  "message": "Hotel updated successfully",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

### DELETE /api/hotels/{id}
Soft delete de hotel (Admin only).

**Headers:**
```http
Authorization: Bearer <admin_token>
```

**Response (204):** No content

## 📦 Endpoints de Pacotes

### GET /api/packages
Lista pacotes de viagem.

**Query Parameters:**
- `page` (int): Página (default: 1)
- `size` (int): Itens por página (default: 10)
- `destination` (string): Filtrar por destino
- `minPrice` (decimal): Preço mínimo
- `maxPrice` (decimal): Preço máximo
- `startDate` (date): Data de início
- `endDate` (date): Data de fim
- `hotelId` (int): Filtrar por hotel

**Response (200):**
```json
{
  "data": [
    {
      "packageId": 1,
      "name": "Fim de Semana no Rio",
      "destination": "Rio de Janeiro - RJ",
      "description": "Pacote completo para um fim de semana inesquecível",
      "basePrice": 599.99,
      "hotelId": 1,
      "hotelName": "Hotel Paradise",
      "medias": [
        {
          "mediaId": 1,
          "mediaUrl": "/uploads/packages/rio-package-1.jpg",
          "mediaType": "image"
        }
      ],
      "packageDates": [
        {
          "packageDateId": 1,
          "startDate": "2024-02-01",
          "endDate": "2024-02-03"
        }
      ],
      "isActive": true
    }
  ],
  "pagination": {
    "currentPage": 1,
    "totalPages": 2,
    "totalItems": 15,
    "pageSize": 10
  }
}
```

### GET /api/packages/{id}
Busca pacote por ID.

**Response (200):**
```json
{
  "packageId": 1,
  "name": "Fim de Semana no Rio",
  "destination": "Rio de Janeiro - RJ",
  "description": "Pacote completo incluindo hospedagem no Hotel Paradise, café da manhã e transfer do aeroporto",
  "basePrice": 599.99,
  "hotelId": 1,
  "hotelName": "Hotel Paradise",
  "hotel": {
    "hotelId": 1,
    "name": "Hotel Paradise",
    "starRating": 4,
    "address": {
      "city": "Rio de Janeiro",
      "state": "RJ"
    }
  },
  "medias": [
    {
      "mediaId": 1,
      "mediaUrl": "/uploads/packages/rio-package-1.jpg",
      "mediaType": "image"
    }
  ],
  "packageDates": [
    {
      "packageDateId": 1,
      "startDate": "2024-02-01",
      "endDate": "2024-02-03"
    }
  ],
  "isActive": true
}
```

### POST /api/packages
Cria novo pacote (Provider only).

**Headers:**
```http
Authorization: Bearer <provider_token>
Content-Type: multipart/form-data
```

**Request (FormData):**
```json
{
  "name": "Fim de Semana no Rio",
  "destination": "Rio de Janeiro - RJ",
  "description": "Pacote completo para um fim de semana",
  "basePrice": 599.99,
  "hotelId": 1,
  "packageDates": [
    {
      "startDate": "2024-02-01",
      "endDate": "2024-02-03"
    }
  ],
  "mediaFiles": ["package-image-1.jpg"]
}
```

**Response (201):**
```json
{
  "packageId": 1,
  "name": "Fim de Semana no Rio",
  "message": "Package created successfully",
  "createdAt": "2024-01-15T10:00:00Z"
}
```

### GET /api/packages/search
Busca avançada de pacotes.

**Query Parameters:**
- `destination` (string, required): Destino
- `startDate` (date, required): Data de início (YYYY-MM-DD)
- `endDate` (date, required): Data de fim (YYYY-MM-DD)
- `guests` (int): Número de hóspedes
- `minPrice` (decimal): Preço mínimo
- `maxPrice` (decimal): Preço máximo

**Example:** `/api/packages/search?destination=Rio&startDate=2024-02-01&endDate=2024-02-03&guests=2`

**Response (200):**
```json
{
  "searchCriteria": {
    "destination": "Rio",
    "startDate": "2024-02-01",
    "endDate": "2024-02-03",
    "guests": 2
  },
  "results": [
    {
      "packageId": 1,
      "name": "Fim de Semana no Rio",
      "destination": "Rio de Janeiro - RJ",
      "basePrice": 599.99,
      "hotelName": "Hotel Paradise",
      "availableDates": [
        {
          "startDate": "2024-02-01",
          "endDate": "2024-02-03"
        }
      ]
    }
  ],
  "totalResults": 3
}
```

## 📅 Endpoints de Reservas

### GET /api/reservations
Lista reservas do usuário.

**Headers:**
```http
Authorization: Bearer <token>
```

**Query Parameters:**
- `page` (int): Página (default: 1)
- `size` (int): Itens por página (default: 10)
- `status` (string): Filtrar por status (Confirmed, Cancelled, Pending)
- `startDate` (date): Data de início
- `endDate` (date): Data de fim

**Response (200):**
```json
{
  "data": [
    {
      "reservationId": 1,
      "packageId": 1,
      "packageName": "Fim de Semana no Rio",
      "hotelId": 1,
      "hotelName": "Hotel Paradise",
      "startDate": "2024-02-01",
      "endDate": "2024-02-03",
      "totalPrice": 599.99,
      "numberOfGuests": 2,
      "status": "Confirmed",
      "companions": [
        {
          "name": "Maria Silva",
          "documentNumber": "12345678901"
        }
      ],
      "createdAt": "2024-01-15T10:00:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "totalPages": 1,
    "totalItems": 3,
    "pageSize": 10
  }
}
```

### GET /api/reservations/{id}
Busca reserva por ID.

**Headers:**
```http
Authorization: Bearer <token>
```

**Response (200):**
```json
{
  "reservationId": 1,
  "userId": 1,
  "packageId": 1,
  "packageName": "Fim de Semana no Rio",
  "hotelId": 1,
  "hotelName": "Hotel Paradise",
  "roomTypeId": 1,
  "roomTypeName": "Quarto Standard",
  "startDate": "2024-02-01",
  "endDate": "2024-02-03",
  "totalPrice": 599.99,
  "numberOfGuests": 2,
  "status": "Confirmed",
  "companions": [
    {
      "companionId": 1,
      "name": "Maria Silva",
      "documentNumber": "12345678901",
      "dateOfBirth": "1985-03-20"
    }
  ],
  "payments": [
    {
      "paymentId": 1,
      "amount": 599.99,
      "paymentDate": "2024-01-15T10:00:00Z",
      "paymentMethod": "CreditCard",
      "status": "Completed"
    }
  ],
  "createdAt": "2024-01-15T10:00:00Z",
  "isActive": true
}
```

### POST /api/reservations
Cria nova reserva (Client only).

**Headers:**
```http
Authorization: Bearer <client_token>
Content-Type: application/json
```

**Request:**
```json
{
  "packageId": 1,
  "roomTypeId": 1,
  "startDate": "2024-02-01",
  "endDate": "2024-02-03",
  "numberOfGuests": 2,
  "companions": [
    {
      "name": "Maria Silva",
      "documentNumber": "12345678901",
      "dateOfBirth": "1985-03-20"
    }
  ],
  "payment": {
    "paymentMethod": "CreditCard",
    "billingAddress": {
      "street": "Rua das Flores, 456",
      "city": "São Paulo",
      "state": "SP",
      "zipCode": "01234-567",
      "cardHolderName": "João Silva"
    }
  }
}
```

**Response (201):**
```json
{
  "reservationId": 1,
  "status": "Confirmed",
  "totalPrice": 599.99,
  "confirmationCode": "VIA-2024-001",
  "message": "Reservation created successfully",
  "createdAt": "2024-01-15T10:00:00Z"
}
```

### PUT /api/reservations/{id}
Atualiza reserva.

**Headers:**
```http
Authorization: Bearer <token>
Content-Type: application/json
```

**Request:**
```json
{
  "numberOfGuests": 3,
  "companions": [
    {
      "name": "Maria Silva",
      "documentNumber": "12345678901",
      "dateOfBirth": "1985-03-20"
    },
    {
      "name": "Pedro Silva",
      "documentNumber": "98765432100",
      "dateOfBirth": "2010-08-15"
    }
  ]
}
```

**Response (200):**
```json
{
  "reservationId": 1,
  "message": "Reservation updated successfully",
  "newTotalPrice": 799.99,
  "updatedAt": "2024-01-15T11:00:00Z"
}
```

### DELETE /api/reservations/{id}
Cancela reserva.

**Headers:**
```http
Authorization: Bearer <token>
```

**Response (200):**
```json
{
  "reservationId": 1,
  "status": "Cancelled",
  "refundAmount": 599.99,
  "message": "Reservation cancelled successfully",
  "cancelledAt": "2024-01-15T12:00:00Z"
}
```

## ⭐ Endpoints de Reviews

### POST /api/reviews
Cria nova avaliação (Client only).

**Headers:**
```http
Authorization: Bearer <client_token>
Content-Type: application/json
```

**Request:**
```json
{
  "hotelId": 1,
  "rating": 5,
  "comment": "Excelente hotel! Atendimento perfeito e localização privilegiada."
}
```

**Response (201):**
```json
{
  "reviewId": 1,
  "rating": 5,
  "comment": "Excelente hotel! Atendimento perfeito e localização privilegiada.",
  "reviewDate": "2024-01-15T10:00:00Z",
  "userName": "João Silva",
  "message": "Review created successfully"
}
```

### GET /api/reviews/hotel/{hotelId}
Lista reviews de um hotel.

**Query Parameters:**
- `page` (int): Página (default: 1)
- `size` (int): Itens por página (default: 10)
- `rating` (int): Filtrar por rating (1-5)

**Response (200):**
```json
{
  "data": [
    {
      "reviewId": 1,
      "rating": 5,
      "comment": "Excelente hotel!",
      "reviewDate": "2024-01-15T10:00:00Z",
      "userName": "João Silva"
    }
  ],
  "summary": {
    "averageRating": 4.5,
    "totalReviews": 128,
    "ratingDistribution": {
      "5": 80,
      "4": 30,
      "3": 10,
      "2": 5,
      "1": 3
    }
  },
  "pagination": {
    "currentPage": 1,
    "totalPages": 5,
    "totalItems": 128,
    "pageSize": 10
  }
}
```

## 🔍 Endpoints de Busca

### GET /api/search/hotels
Busca avançada de hotéis.

**Query Parameters:**
- `q` (string): Termo de busca
- `city` (string): Cidade
- `state` (string): Estado
- `checkIn` (date): Data de check-in
- `checkOut` (date): Data de check-out
- `guests` (int): Número de hóspedes
- `minPrice` (decimal): Preço mínimo
- `maxPrice` (decimal): Preço máximo
- `rating` (int): Rating mínimo
- `amenities` (string[]): Comodidades (wifi,pool,gym,spa)

**Example:** `/api/search/hotels?city=Rio&checkIn=2024-02-01&checkOut=2024-02-03&guests=2&amenities=wifi,pool`

**Response (200):**
```json
{
  "query": {
    "city": "Rio",
    "checkIn": "2024-02-01",
    "checkOut": "2024-02-03",
    "guests": 2
  },
  "results": [
    {
      "hotelId": 1,
      "name": "Hotel Paradise",
      "city": "Rio de Janeiro",
      "state": "RJ",
      "rating": 4.5,
      "minPrice": 250.00,
      "image": "/uploads/hotels/paradise-1.jpg",
      "amenities": ["WiFi", "Pool", "Breakfast"],
      "availability": true
    }
  ],
  "totalResults": 15,
  "searchId": "search-123456789"
}
```

## 📊 Endpoints de Health Check

### GET /health
Verifica saúde da API.

**Response (200):**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:00:00Z",
  "version": "1.0.0",
  "environment": "Production"
}
```

### GET /health/detailed
Verifica saúde detalhada (Admin only).

**Headers:**
```http
Authorization: Bearer <admin_token>
```

**Response (200):**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:00:00Z",
  "version": "1.0.0",
  "environment": "Production",
  "dependencies": {
    "database": {
      "status": "Healthy",
      "responseTime": "25ms"
    },
    "externalApi": {
      "status": "Healthy",
      "responseTime": "150ms"
    },
    "cache": {
      "status": "Healthy",
      "responseTime": "5ms"
    }
  },
  "metrics": {
    "uptime": "72h 30m",
    "requestsPerMinute": 45,
    "memoryUsage": "512MB",
    "cpuUsage": "15%"
  }
}
```

## 🚨 Tratamento de Erros

### Formato Padrão de Erro
```json
{
  "error": {
    "message": "Descrição do erro",
    "code": "ERROR_CODE",
    "details": "Informações adicionais",
    "timestamp": "2024-01-15T10:00:00Z",
    "path": "/api/users/123"
  }
}
```

### Erros de Validação (422)
```json
{
  "errors": {
    "Email": [
      "Email is required",
      "Email format is invalid"
    ],
    "Password": [
      "Password must be at least 8 characters"
    ]
  },
  "title": "One or more validation errors occurred",
  "status": 422
}
```

### Rate Limiting (429)
```json
{
  "error": {
    "message": "Too many requests",
    "code": "RATE_LIMIT_EXCEEDED",
    "retryAfter": 60,
    "limit": 100,
    "remaining": 0,
    "resetTime": "2024-01-15T10:01:00Z"
  }
}
```

## 📚 Códigos de Exemplo

### cURL Examples

#### Login
```bash
curl -X POST "https://api.viaggia.com/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "securePassword123!"
  }'
```

#### Buscar Hotéis
```bash
curl -X GET "https://api.viaggia.com/api/hotels?city=Rio&checkIn=2024-02-01&checkOut=2024-02-03" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

#### Criar Reserva
```bash
curl -X POST "https://api.viaggia.com/api/reservations" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "packageId": 1,
    "startDate": "2024-02-01",
    "endDate": "2024-02-03",
    "numberOfGuests": 2
  }'
```

### JavaScript Examples

#### Fetch com Autenticação
```javascript
const token = localStorage.getItem('authToken');

const response = await fetch('https://api.viaggia.com/api/hotels', {
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

const hotels = await response.json();
```

#### Upload de Imagem
```javascript
const formData = new FormData();
formData.append('name', 'Hotel Paradise');
formData.append('mediaFiles', fileInput.files[0]);

const response = await fetch('https://api.viaggia.com/api/hotels', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
  },
  body: formData
});
```

---

**📝 Nota**: Esta documentação é gerada automaticamente a partir do código. Para a versão mais atualizada, acesse o Swagger UI em `/swagger`.
