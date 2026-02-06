```markdown
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

Sinta-se à vontade para abrir issues e pull requests.  
Ideias bem-vindas: animações extras, sons, estatísticas por jogador, etc.

Feito por **André Bonim** ♟️  
[André Bonim](andre.bonim@email.com)

```
