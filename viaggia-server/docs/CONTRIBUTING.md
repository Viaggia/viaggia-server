# 🤝 Guia de Contribuição - Viaggia Server

## 📋 Índice

1. [Como Contribuir](#-como-contribuir)
2. [Configuração do Ambiente](#-configuração-do-ambiente)
3. [Padrões de Código](#-padrões-de-código)
4. [Workflow de Desenvolvimento](#-workflow-de-desenvolvimento)
5. [Pull Requests](#-pull-requests)
6. [Reportando Issues](#-reportando-issues)
7. [Padrões de Commit](#-padrões-de-commit)
8. [Code Review](#-code-review)

## 🤝 Como Contribuir

Agradecemos seu interesse em contribuir com o Viaggia Server! Existem várias formas de contribuir:

### 🐛 Reportando Bugs
- Use o template de issue para bugs
- Inclua informações detalhadas de reprodução
- Adicione logs e screenshots quando possível

### 💡 Sugerindo Features
- Use o template de issue para features
- Descreva o problema que a feature resolve
- Inclua exemplos de uso

### 📝 Melhorando Documentação
- Corrigir erros de digitação
- Adicionar exemplos de código
- Traduzir documentação

### 🔧 Contribuindo com Código
- Correção de bugs
- Implementação de novas features
- Otimizações de performance
- Melhorias de segurança

## 🛠 Configuração do Ambiente

### Pré-requisitos

```bash
# .NET 8 SDK
dotnet --version
# Deve retornar 8.0.x

# SQL Server ou SQL Server LocalDB
# Git
git --version

# Visual Studio 2022 ou VS Code (recomendado)
```

### Setup do Projeto

```bash
# 1. Fork o repositório no GitHub
# 2. Clone seu fork
git clone https://github.com/SEU_USERNAME/viaggia-server.git
cd viaggia-server

# 3. Adicione o repositório original como upstream
git remote add upstream https://github.com/ORIGINAL_OWNER/viaggia-server.git

# 4. Instale as dependências
dotnet restore

# 5. Configure as User Secrets
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=ViaggiaDb;Trusted_Connection=true;TrustServerCertificate=true;"
dotnet user-secrets set "JwtSettings:Key" "sua-chave-jwt-super-secreta-aqui"

# 6. Execute as migrations
dotnet ef database update

# 7. Execute o projeto
dotnet run
```

### Configuração do IDE

#### Visual Studio Code
Extensões recomendadas (`.vscode/extensions.json`):
```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.vscode-dotnet-runtime",
    "ms-mssql.mssql",
    "bradlc.vscode-tailwindcss",
    "esbenp.prettier-vscode",
    "ms-vscode.vscode-json"
  ]
}
```

Configurações do workspace (`.vscode/settings.json`):
```json
{
  "dotnet.defaultSolution": "viaggia-server.sln",
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.fixAll": true,
    "source.organizeImports": true
  },
  "files.exclude": {
    "**/bin": true,
    "**/obj": true
  }
}
```

## 📏 Padrões de Código

### Convenções de Nomenclatura

```csharp
// ✅ Classes - PascalCase
public class HotelService { }

// ✅ Métodos públicos - PascalCase
public async Task<Hotel> GetHotelByIdAsync(int id) { }

// ✅ Métodos privados - PascalCase
private void ValidateHotelData() { }

// ✅ Propriedades - PascalCase
public string HotelName { get; set; }

// ✅ Campos privados - _camelCase
private readonly IHotelRepository _hotelRepository;

// ✅ Parâmetros - camelCase
public Hotel CreateHotel(string hotelName, int starRating) { }

// ✅ Variáveis locais - camelCase
var hotelList = await _repository.GetAllAsync();

// ✅ Constantes - PascalCase
public const string DefaultConnectionString = "...";

// ✅ Interfaces - I + PascalCase
public interface IHotelRepository { }

// ✅ DTOs - PascalCase + DTO
public class CreateHotelDTO { }
```

### Estrutura de Arquivos

```
Controllers/
├── BaseController.cs           # ✅ Controller base
├── HotelsController.cs         # ✅ Específico por entidade
└── V2/                         # ✅ Versionamento de API
    └── HotelsController.cs

Services/
├── Interfaces/                 # ✅ Interfaces separadas
│   └── IHotelService.cs
├── HotelService.cs            # ✅ Implementação
└── Extensions/                 # ✅ Extension methods
    └── ServiceCollectionExtensions.cs

DTOs/
├── Base/                      # ✅ DTOs base
│   └── BaseDTO.cs
├── Hotel/                     # ✅ Por entidade
│   ├── HotelDTO.cs
│   ├── CreateHotelDTO.cs
│   └── UpdateHotelDTO.cs
└── Validators/                # ✅ Validators FluentValidation
    └── CreateHotelDTOValidator.cs
```

### Padrões de Implementação

#### Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // ✅ Autorização por controller
public class HotelsController : ControllerBase
{
    private readonly IHotelService _hotelService;
    private readonly ILogger<HotelsController> _logger;

    public HotelsController(
        IHotelService hotelService, 
        ILogger<HotelsController> logger)
    {
        _hotelService = hotelService;
        _logger = logger;
    }

    /// <summary>
    /// Busca hotel por ID
    /// </summary>
    /// <param name="id">ID do hotel</param>
    /// <returns>Dados do hotel</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(HotelDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HotelDTO>> GetHotel(int id)
    {
        try
        {
            var hotel = await _hotelService.GetByIdAsync(id);
            if (hotel == null)
            {
                _logger.LogWarning("Hotel with ID {HotelId} not found", id);
                return NotFound($"Hotel with ID {id} not found");
            }

            return Ok(hotel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hotel with ID {HotelId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
```

#### Services
```csharp
public class HotelService : IHotelService
{
    private readonly IHotelRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<HotelService> _logger;

    public HotelService(
        IHotelRepository repository,
        IMapper mapper,
        ILogger<HotelService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HotelDTO?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving hotel with ID {HotelId}", id);
        
        var hotel = await _repository.GetByIdAsync(id);
        if (hotel == null)
        {
            _logger.LogWarning("Hotel with ID {HotelId} not found", id);
            return null;
        }

        return _mapper.Map<HotelDTO>(hotel);
    }
}
```

#### Repositories
```csharp
public class HotelRepository : Repository<Hotel>, IHotelRepository
{
    public HotelRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Hotel>> GetActiveHotelsAsync()
    {
        return await _context.Hotels
            .Where(h => h.IsActive)
            .Include(h => h.Address)
            .Include(h => h.Commodities)
            .ToListAsync();
    }

    public async Task<Hotel?> GetHotelWithDetailsAsync(int id)
    {
        return await _context.Hotels
            .Include(h => h.Address)
            .Include(h => h.RoomTypes)
            .Include(h => h.Commodities)
            .Include(h => h.Reviews)
            .Include(h => h.Medias)
            .FirstOrDefaultAsync(h => h.HotelId == id && h.IsActive);
    }
}
```

### Documentação de Código

```csharp
/// <summary>
/// Serviço responsável pela gestão de hotéis
/// </summary>
public interface IHotelService
{
    /// <summary>
    /// Busca hotel por ID
    /// </summary>
    /// <param name="id">ID único do hotel</param>
    /// <returns>
    /// Dados do hotel se encontrado, null caso contrário
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Lançado quando o ID é menor ou igual a zero
    /// </exception>
    Task<HotelDTO?> GetByIdAsync(int id);
}
```

## 🔄 Workflow de Desenvolvimento

### Branch Strategy (Git Flow)

```
main
├── develop
│   ├── feature/ISSUE-123-add-hotel-search
│   ├── feature/ISSUE-124-implement-reviews
│   └── hotfix/ISSUE-125-fix-login-bug
└── release/v1.2.0
```

### Tipos de Branches

| Branch | Propósito | Convenção |
|--------|-----------|-----------|
| `main` | Código em produção | Apenas merges |
| `develop` | Integração de features | Branch principal de desenvolvimento |
| `feature/*` | Novas funcionalidades | `feature/ISSUE-{number}-{description}` |
| `hotfix/*` | Correções urgentes | `hotfix/ISSUE-{number}-{description}` |
| `release/*` | Preparação para release | `release/v{major}.{minor}.{patch}` |

### Processo de Desenvolvimento

```bash
# 1. Sincronizar com upstream
git checkout develop
git fetch upstream
git merge upstream/develop

# 2. Criar branch para feature
git checkout -b feature/ISSUE-123-add-hotel-search

# 3. Desenvolver e commitar
git add .
git commit -m "feat: add hotel search by location and date"

# 4. Push para seu fork
git push origin feature/ISSUE-123-add-hotel-search

# 5. Criar Pull Request no GitHub
```

## 🔀 Pull Requests

### Template de PR

```markdown
## 📝 Descrição
Breve descrição das mudanças implementadas.

## 🔗 Issue Relacionada
Closes #123

## 🧪 Tipo de Mudança
- [ ] 🐛 Bug fix (correção que resolve um problema)
- [ ] ✨ Nova feature (funcionalidade que adiciona algo novo)
- [ ] 💥 Breaking change (mudança que quebra compatibilidade)
- [ ] 📝 Documentação
- [ ] 🎨 Refactoring (mudança de código sem alterar funcionalidade)
- [ ] ⚡ Performance
- [ ] 🧪 Testes

## 🧪 Testes
- [ ] Testes unitários passando
- [ ] Testes de integração passando
- [ ] Novos testes adicionados (se aplicável)

## 📝 Checklist
- [ ] Código segue as convenções do projeto
- [ ] Self-review do código foi feito
- [ ] Comentários foram adicionados em código complexo
- [ ] Documentação foi atualizada
- [ ] Não há warnings de build
- [ ] Testes relacionados passam

## 📷 Screenshots (se aplicável)
Adicione screenshots para mudanças de UI.

## 📋 Notas Adicionais
Informações extras que os reviewers devem saber.
```

### Critérios de Aprovação

- ✅ Pelo menos 1 aprovação de code review
- ✅ Todos os checks passando (CI/CD)
- ✅ Sem conflitos com branch target
- ✅ Cobertura de testes mantida/melhorada
- ✅ Documentação atualizada (se necessário)

## 🐛 Reportando Issues

### Template de Bug Report

```markdown
## 🐛 Descrição do Bug
Descrição clara e concisa do bug.

## 🔄 Passos para Reproduzir
1. Vá para '...'
2. Clique em '...'
3. Role para baixo '...'
4. Veja o erro

## ✅ Comportamento Esperado
Descrição do que deveria acontecer.

## 📷 Screenshots
Adicione screenshots se aplicável.

## 🖥 Ambiente
- OS: [e.g. Windows 11]
- Browser: [e.g. Chrome 120]
- Versão: [e.g. v1.2.0]

## 📋 Contexto Adicional
Qualquer informação adicional sobre o problema.

## 📊 Logs
```json
{
  "error": "error message here",
  "timestamp": "2024-01-15T10:00:00Z"
}
```

### Template de Feature Request

```markdown
## 🚀 Feature Request

### 📝 Descrição
Descrição clara da feature desejada.

### 💡 Motivação
Por que esta feature é importante?

### 🎯 Casos de Uso
- Como cliente, eu quero...
- Como administrador, eu preciso...

### 💭 Solução Proposta
Descrição de como você imagina que a feature deveria funcionar.

### 🔄 Alternativas Consideradas
Outras abordagens que você considerou.

### 📋 Critérios de Aceitação
- [ ] Deve fazer X
- [ ] Deve validar Y
- [ ] Deve retornar Z
```

## 📝 Padrões de Commit

### Formato de Commit (Conventional Commits)

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Tipos de Commit

| Tipo | Descrição | Exemplo |
|------|-----------|---------|
| `feat` | Nova funcionalidade | `feat: add hotel search by rating` |
| `fix` | Correção de bug | `fix: resolve login timeout issue` |
| `docs` | Documentação | `docs: update API documentation` |
| `style` | Formatação, espaços | `style: fix code formatting` |
| `refactor` | Refatoração | `refactor: improve hotel service performance` |
| `test` | Adição/correção de testes | `test: add unit tests for auth service` |
| `chore` | Tarefas de manutenção | `chore: update dependencies` |
| `perf` | Melhoria de performance | `perf: optimize database queries` |
| `ci` | Mudanças de CI/CD | `ci: add automated testing workflow` |

### Exemplos de Commits

```bash
# ✅ Bons commits
git commit -m "feat: add hotel availability check endpoint"
git commit -m "fix: resolve booking confirmation email issue"
git commit -m "docs: add API documentation for reservations"
git commit -m "test: add integration tests for payment flow"

# ❌ Commits ruins
git commit -m "changes"
git commit -m "fix stuff"
git commit -m "WIP"
git commit -m "asdfadsf"
```

### Commit Body e Footer

```bash
git commit -m "feat: add hotel booking cancellation

Allow users to cancel their hotel bookings up to 24 hours
before check-in date. Includes email notification and
automatic refund processing.

Closes #123
Breaking-change: Changes booking status enum values"
```

## 👥 Code Review

### Processo de Review

1. **Self Review**: Sempre revise seu próprio código primeiro
2. **Automated Checks**: CI/CD deve passar
3. **Peer Review**: Pelo menos 1 aprovação necessária
4. **Address Feedback**: Responda todos os comentários

### Checklist para Reviewers

#### ✅ Funcionalidade
- [ ] O código faz o que deveria fazer?
- [ ] A lógica está correta?
- [ ] Edge cases foram considerados?
- [ ] Tratamento de erros está adequado?

#### ✅ Arquitetura & Design
- [ ] Código segue os padrões do projeto?
- [ ] Responsabilidades estão bem definidas?
- [ ] Princípios SOLID foram seguidos?
- [ ] Não há duplicação desnecessária?

#### ✅ Performance
- [ ] Queries de banco são eficientes?
- [ ] Não há loops desnecessários?
- [ ] Recursos são liberados adequadamente?
- [ ] Async/await usado corretamente?

#### ✅ Segurança
- [ ] Inputs são validados?
- [ ] Autorização está implementada?
- [ ] Dados sensíveis não são expostos?
- [ ] Vulnerabilidades conhecidas foram evitadas?

#### ✅ Testes
- [ ] Novos testes foram adicionados?
- [ ] Testes existentes ainda passam?
- [ ] Cobertura é adequada?
- [ ] Casos edge foram testados?

#### ✅ Documentação
- [ ] Código complexo está comentado?
- [ ] README foi atualizado se necessário?
- [ ] API docs foram atualizadas?

### Como Dar Feedback Construtivo

```markdown
# ✅ Bom feedback
Considere usar `FirstOrDefaultAsync()` aqui em vez de `Where().FirstOrDefault()` 
para melhor performance com grandes datasets.

# ✅ Feedback específico
Esta validação de CPF está incorreta na linha 45. Deveria usar o algoritmo 
de módulo 11. Aqui está um link para referência: [link]

# ❌ Feedback ruim
Este código está ruim.

# ❌ Feedback não construtivo
Mude isso.
```

### Respondendo a Feedback

```markdown
# ✅ Boa resposta
Obrigado pela sugestão! Implementei a mudança e também adicionei 
um teste para o caso edge que você mencionou.

# ✅ Explicação válida
Mantive essa abordagem porque precisamos manter compatibilidade 
com a versão anterior da API. Adicionei um comentário explicando isso.

# ❌ Resposta defensiva
Não, meu código está certo.
```

## 🏷 Labels e Milestones

### Labels de Issues/PRs

| Label | Cor | Descrição |
|-------|-----|-----------|
| `bug` | `#d73a4a` | Algo não está funcionando |
| `enhancement` | `#a2eeef` | Nova feature ou melhoria |
| `documentation` | `#0075ca` | Melhorias na documentação |
| `good first issue` | `#7057ff` | Bom para iniciantes |
| `help wanted` | `#008672` | Ajuda extra é bem-vinda |
| `priority: high` | `#b60205` | Alta prioridade |
| `priority: medium` | `#fbca04` | Média prioridade |
| `priority: low` | `#0e8a16` | Baixa prioridade |
| `status: blocked` | `#000000` | Bloqueado por dependência |
| `status: in progress` | `#yellow` | Em desenvolvimento |

## 🎯 Roadmap e Planejamento

### Milestones

- **v1.1.0** - Melhorias de Performance
- **v1.2.0** - Sistema de Reviews
- **v1.3.0** - Integração com Pagamentos
- **v2.0.0** - Arquitetura de Microserviços

### Como Priorizar Issues

1. **P0 - Crítico**: Bugs que quebram funcionalidade principal
2. **P1 - Alto**: Features importantes ou bugs significativos
3. **P2 - Médio**: Melhorias e features secundárias
4. **P3 - Baixo**: Nice-to-have e otimizações

---

**🙏 Obrigado por contribuir com o Viaggia Server!**

Sua contribuição faz a diferença. Se tiver dúvidas, não hesite em perguntar nas issues ou Discord da comunidade.
