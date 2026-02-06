-- Jogadores (pode ser só nome temporário ou cadastro persistente)
CREATE TABLE players (
    id              BIGSERIAL PRIMARY KEY,
    name            VARCHAR(100) NOT NULL,
    created_at      TIMESTAMPTZ DEFAULT NOW(),
    games_played    INTEGER DEFAULT 0,
    wins            INTEGER DEFAULT 0,
    draws           INTEGER DEFAULT 0,
    
    CONSTRAINT chk_name_length CHECK (LENGTH(name) >= 2)
);

-- Partidas
CREATE TABLE games (
    id              BIGSERIAL PRIMARY KEY,
    player_x_id     BIGINT REFERENCES players(id) ON DELETE SET NULL,
    player_o_id     BIGINT REFERENCES players(id) ON DELETE SET NULL,
    
    -- Alternativa se não quiser cadastrar jogadores fixos:
    -- player_x_name   VARCHAR(100),
    -- player_o_name   VARCHAR(100),
    
    status          VARCHAR(20) NOT NULL DEFAULT 'ongoing' 
                    CHECK (status IN ('ongoing', 'finished', 'abandoned', 'awaiting_opponent')),
                    
    winner_id       BIGINT REFERENCES players(id) ON DELETE SET NULL,
    is_draw         BOOLEAN DEFAULT FALSE,
    
    board_state     JSONB NOT NULL DEFAULT '[[null,null,null],[null,null,null],[null,null,null]]'::jsonb,
                    -- ou TEXT se preferir string "         | | |..." etc.
                    
    started_at      TIMESTAMPTZ DEFAULT NOW(),
    finished_at     TIMESTAMPTZ,
    
    -- Útil para multiplayer online simples
    room_code       VARCHAR(10) UNIQUE,          -- ex: "ABC123"
    last_move_at    TIMESTAMPTZ,
	
	connected_players    INTEGER,
	current_player_id    BIGINT,
	
	last_move_at    TIMESTAMPTZ,
    
    CONSTRAINT chk_one_winner CHECK (
        NOT (winner_id IS NOT NULL AND is_draw = true)
    )
);

CREATE INDEX idx_games_status ON games(status);
CREATE INDEX idx_games_player_x ON games(player_x_id);
CREATE INDEX idx_games_player_o ON games(player_o_id);


-- Histórico de jogadas (muito valorizado para replay e estatísticas)
CREATE TABLE moves (
    id          BIGSERIAL PRIMARY KEY,
    game_id     BIGINT NOT NULL REFERENCES games(id) ON DELETE CASCADE,
    player_id   BIGINT REFERENCES players(id) ON DELETE SET NULL,
    position    SMALLINT NOT NULL CHECK (position BETWEEN 0 AND 8),
    move_number SMALLINT NOT NULL CHECK (move_number BETWEEN 1 AND 9),
    played_at   TIMESTAMPTZ DEFAULT NOW(),
    
    UNIQUE (game_id, position),           -- não pode jogar na mesma casa 2x
    UNIQUE (game_id, move_number)         -- sequência correta
);

CREATE INDEX idx_moves_game ON moves(game_id);