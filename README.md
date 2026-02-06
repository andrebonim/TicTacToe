```markdown
# TicTacToe API

API RESTful para gerenciar partidas do clássico **Jogo da Velha (Tic-Tac-Toe)**, desenvolvida com **.NET 8** e **PostgreSQL**.

A API permite criar jogos, realizar jogadas, verificar status/vencedor, gerenciar usuários (autenticação via JWT) e persistir o histórico de partidas.

## Tecnologias Utilizadas

- **.NET 8** (ASP.NET Core Web API)
- **PostgreSQL** (banco de dados relacional)
- **Entity Framework Core** (ORM + migrations)
- **JWT** (autenticação e autorização via tokens)
- **Clean Architecture** / **Layered Architecture** (separação em camadas: Model, Util, Data, Repository, Business, API)
- **Serilog** (logging estruturado)
- **Swagger / OpenAPI** (documentação interativa da API)

## Estrutura do Projeto

```
TicTacToe
├── TicTacToe.API                # Projeto principal (Web API)
├── TicTacToe.Business           # Regras de negócio, services, validações
├── TicTacToe.Data               # DbContext, configurações do EF Core
├── TicTacToe.Repository         # Repositórios (acesso a dados)
├── TicTacToe.Model              # Entidades/DTOs compartilhados
└── TicTacToe.Util               # Utilitários, helpers, extensões
```

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL 15+ (ou Docker com imagem postgres)
- Ferramenta de migrações EF Core:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Configuração Local (Desenvolvimento)

1. Clone o repositório
   ```bash
   git clone <url-do-seu-repo>
   cd TicTacToe/TicTacToe.API
   ```

2. Verifique / ajuste a connection string em `appsettings.Development.json` (ou `appsettings.json`)
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=tictactoe;Username=postgres;Password=admin"
   }
   ```

3. Crie o banco de dados (via pgAdmin, DBeaver ou CLI):
   ```sql
   CREATE DATABASE tictactoe;
   ```

4. Aplique as migrations
   ```bash
   # Dentro da pasta TicTacToe.API
   dotnet ef migrations add InitialCreate --project ../TicTacToe.Data/TicTacToe.Data.csproj --startup-project .
   dotnet ef database update --project ../TicTacToe.Data/TicTacToe.Data.csproj --startup-project .
   ```

   Ou, se preferir rodar direto do projeto principal:
   ```bash
   dotnet ef database update
   ```

5. Execute a API
   ```bash
   dotnet run
   ```

   A documentação Swagger estará disponível em:  
   http://localhost:5000/swagger (ou porta configurada)

## Variáveis de Ambiente Importantes

| Variável                  | Descrição                                      | Exemplo / Padrão                          |
|---------------------------|------------------------------------------------|--------------------------------------------|
| `ConnectionStrings__DefaultConnection` | String de conexão PostgreSQL              | `Host=...;Database=...;Username=...`      |
| `Jwt__Key`                | Chave secreta para assinatura JWT (mín. 32 chars) | `sua_chave_muito_longa_e_segura_123456` |
| `Jwt__Issuer`             | Emissor do token                               | `TTT`                                      |
| `Jwt__Audience`           | Audiência do token                             | `TTTClient`                                |
| `Jwt__Hours`              | Tempo de expiração do token (horas)            | `12`                                       |

## Deploy na Azure (App Service + PostgreSQL Flex Server)

### Passo a passo recomendado

1. **Crie os recursos no Azure**
   - **Azure Database for PostgreSQL - Flexible Server** (recomendado em 2025+)
     - Crie o servidor e o banco `tictactoe`
     - Anote a connection string completa
   - **App Service** (Web App)
     - Runtime: .NET 8
     - Linux (recomendado)
     - Plano: Básico ou superior (B1+)

2. **Configure as variáveis de ambiente no App Service**
   Vá em: **Configuração → Variáveis de ambiente (Application settings)**

   Adicione:
   - `ConnectionStrings__DefaultConnection` → sua connection string do Azure PostgreSQL
   - `Jwt__Key` → uma chave segura (gere uma forte, nunca use a de desenvolvimento!)
   - `Jwt__Issuer` → `TTT`
   - `Jwt__Audience` → `TTTClient`
   - `Jwt__Hours` → `12` (ou o valor desejado)
   - `ASPNETCORE_ENVIRONMENT` → `Production` (opcional, mas recomendado)

   > **Dica de segurança**: Nunca commite chaves reais. Use Azure Key Vault para produção real.

3. **Deploy da aplicação**
   Opções mais comuns:

   **Via Visual Studio (mais simples)**
   - Clique direito no projeto `TicTacToe.API` → Publish → Azure → Azure App Service (Windows/Linux)
   - Selecione o App Service criado
   - Publique

   **Via GitHub Actions (recomendado para CI/CD)**
   Crie um workflow `.github/workflows/deploy.yml` com algo assim:

   ```yaml
   name: Deploy to Azure Web App

   on:
     push:
       branches: [ main ]

   jobs:
     build-and-deploy:
       runs-on: ubuntu-latest
       steps:
       - uses: actions/checkout@v4

       - name: Set up .NET
         uses: actions/setup-dotnet@v4
         with:
           dotnet-version: '8.0.x'

       - name: Restore dependencies
         run: dotnet restore

       - name: Build
         run: dotnet build --configuration Release --no-restore

       - name: Publish
         run: dotnet publish --configuration Release --no-build --output ./publish

       - name: Deploy to Azure Web App
         uses: azure/webapps-deploy@v3
         with:
           app-name: 'seu-nome-do-app-service'
           publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
           package: ./publish
   ```

4. **Aplicar migrations no ambiente de produção**
   Existem algumas abordagens seguras:

   **Opção 1 – Automática na inicialização (simples, mas menos controlada)**  
   No `Program.cs`, adicione:

   ```csharp
   using (var scope = app.Services.CreateScope())
   {
       var db = scope.ServiceProvider.GetRequiredService<SeuDbContext>();
       db.Database.Migrate();
   }
   ```

   **Opção 2 – Migration Bundle (recomendada pela Microsoft em 2025+)**
   - Gere o bundle no pipeline:
     ```bash
     dotnet ef migrations bundle --runtime linux-x64 -o migrationsbundle
     ```
   - Inclua no deploy e execute via script de inicialização ou Azure DevOps/GitHub Action.

   **Opção 3 – Manual (mais seguro para produção crítica)**
   - Rode `dotnet ef database update` em uma máquina com acesso ao banco de produção (usando a connection string do Azure).


# Tic-Tac-Toe Next (Frontend)

Frontend moderno do clássico **Jogo da Velha**, construído com **Next.js 16+** (App Router), **React 19**, **Tailwind CSS** e integração real-time via **SignalR** com backend .NET 8.

Funcionalidades principais:
- Modo **Local** (2 jogadores no mesmo dispositivo, salva no banco)
- Modo **Rede** (multiplayer real-time na mesma rede Wi-Fi ou internet, via SignalR)
- Autenticação implícita via nomes de jogadores
- Ranking dos Top 10 vencedores (modal flutuante)
- Animações suaves, design responsivo e acessível
- Otimização automática com **React Compiler**

## Tecnologias Utilizadas

- **Next.js 16+** (App Router + Server Components onde possível)
- **React 19.2+** + **React Compiler** (memoização automática)
- **Tailwind CSS 4** (estilização utility-first)
- **@microsoft/signalr** (^10.0.0) – comunicação real-time com o backend
- **TypeScript** (strict mode)
- **ESLint** + **Next.js recommended config**

## Estrutura de Pastas

```
tic-tac-toe-next/
├── app/
│   ├── game/
│   │   ├── page.tsx              # Página principal do jogo
│   │   └── loading.tsx           # Loading state
│   ├── globals.css
│   ├── layout.tsx
│   ├── page.tsx                  # Home: menu de início (local/rede)
│   └── favicon.ico
├── components/
│   ├── ui/
│   │   ├── Board.tsx
│   │   ├── Square.tsx
│   │   └── GameStatus.tsx        # Exibe vez, vencedor, empate...
│   └── RankingModal.tsx          # Modal com Top 10 (fetch /api/games/top10)
├── config/
│   └── env.ts                    # Configurações (apiBaseUrl, signalrHubPath)
├── features/
│   └── game/
│       ├── hooks/
│       │   └── useGameLogic.ts   # Lógica do tabuleiro (local + sync server)
│       ├── services/
│       │   └── signalRGame.ts    # Conexão, join, sendMove, listeners
│       └── types.ts
├── lib/
│   └── utils.ts                  # calculateWinner, isBoardFull...
├── public/
└── .env.local / .env.production
```

## Pré-requisitos

- **Node.js** ≥ 20
- Backend rodando (veja [TicTacToe.API](../TicTacToe.API) – .NET 8 + PostgreSQL)

## Instalação e Execução Local

1. Clone o repositório e entre na pasta do frontend
   ```bash
   git clone <url-do-repo>
   cd tic-tac-toe-next
   ```

2. Instale as dependências
   ```bash
   npm install
   # ou pnpm install / yarn install
   ```

3. Configure as variáveis de ambiente  
   Crie / edite `.env.local`:

   ```env
   NEXT_PUBLIC_API_BASE_URL=http://localhost:5000   # ou https://localhost:44339
   NEXT_PUBLIC_SIGNALR_HUB=/gameHub
   ```

   > **Importante**: Use `NEXT_PUBLIC_` para variáveis expostas no client (necessário para SignalR e fetch).

4. Inicie o servidor de desenvolvimento
   ```bash
   npm run dev
   ```

   Acesse: http://localhost:3000

   - Home → escolha modo local ou rede
   - No modo rede → crie sala ou entre com código

## Variáveis de Ambiente Importantes

| Variável                        | Descrição                                      | Exemplo Local                          | Obrigatório? |
|---------------------------------|------------------------------------------------|----------------------------------------|--------------|
| `NEXT_PUBLIC_API_BASE_URL`      | URL base da API .NET                           | `http://localhost:5000`                | Sim          |
| `NEXT_PUBLIC_SIGNALR_HUB`       | Path relativo do hub SignalR                   | `/gameHub`                             | Sim          |

## Deploy na Vercel (Recomendado – Gratuito para hobby)

A Vercel é a plataforma oficial e mais simples para Next.js (zero-config na maioria dos casos).

### Passo a passo (2025/2026)

1. Crie conta gratuita em https://vercel.com (ou faça login com GitHub)

2. Conecte seu repositório  
   - Dashboard → New Project → Import Git Repository (GitHub/GitLab/Bitbucket)

3. Configure o projeto  
   - Framework Preset: **Next.js** (detectado automaticamente)
   - Root Directory: deixe em branco (ou aponte para a pasta se monorepo)
   - Build & Output Settings: defaults ok

4. Adicione variáveis de ambiente (Environment Variables)  
   - `NEXT_PUBLIC_API_BASE_URL` → URL da sua API deployada (ex: `https://tictactoe-api.azurewebsites.net`)
   - `NEXT_PUBLIC_SIGNALR_HUB` → `/gameHub` (ou o que estiver no backend)

   > **Dica**: Use "Production" scope para produção e crie preview branches para testes.

5. Deploy  
   Clique em **Deploy** → em ~1 minuto você tem URL pública (ex: `tic-tac-toe-next.vercel.app`)

6. Atualizações futuras  
   - Push no `main` → deploy automático  
   - Branches → previews automáticos (ex: `feature/novo-modal` → url-temporaria.vercel.app)

### Dicas para produção

- **CORS** no backend deve permitir o domínio da Vercel (ou `*` em dev)
- **SignalR** funciona bem em Vercel (WebSockets suportados)
- **React Compiler** está ativado (`reactCompiler: true` em `next.config.ts`) → melhora performance em jogos com muitos re-renders
- Monitore logs no Vercel Dashboard → Functions + Realtime Logs

## Scripts Disponíveis

```bash
npm run dev     # desenvolvimento (localhost:3000)
npm run build   # build de produção
npm run start   # roda o build (para testar localmente)
npm run lint    # eslint
```

## Próximos Passos / Melhorias Possíveis

- Login real com JWT (integrar com a autenticação do backend)
- Suporte a múltiplas salas simultâneas + lista de salas abertas
- Undo move / reiniciar partida
- Temas claro/escuro (Tailwind dark mode)
- PWA (offline play local)
- Testes (Jest + React Testing Library)

## Contribuição


Feito com ♥ por [André Bonim](andre.bonim@email.com)
```