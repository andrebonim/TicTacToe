using AutoMapper;
using TicTacToe.Data.Entity;


namespace TicTacToe.Model.Extensions 
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {            

            CreateMap<PlayersEntity, PlayersModel>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<PlayersEntity, PlayerSummaryDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<GamesEntity, GamesModel>()
                .ForMember(dest => dest.PlayerX, opt => opt.MapFrom(src => src.PlayerX))
                .ForMember(dest => dest.PlayerO, opt => opt.MapFrom(src => src.PlayerO))
                .ForMember(dest => dest.Winner, opt => opt.MapFrom(src => src.Winner))
                .ForMember(dest => dest.Board, opt => opt.MapFrom(src =>
                    // Converte JSONB string para lista flat de 9 posições (ou adapte conforme seu front espera)
                    ParseBoardJsonToFlatList(src.BoardState)))
                .ForMember(dest => dest.MoveCount, opt => opt.MapFrom(src => src.Moves.Count))
                // CurrentPlayerSymbol pode ser calculado no service/controller
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<GamesEntity, GameStatusDto>()
                .ForMember(dest => dest.Board, opt => opt.MapFrom(src =>
                    ParseBoardJsonToFlatList(src.BoardState)))
                .ForMember(dest => dest.WinnerName, opt => opt.MapFrom(src => src.Winner != null ? src.Winner.Name : null))
                // CurrentPlayerSymbol normalmente calculado no momento
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<MovesEntity, MovesModel>()
                .ForMember(dest => dest.PlayerName, opt => opt.MapFrom(src => src.Player != null ? src.Player.Name : "Desconhecido"))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<GamesEntity, GameHistoryDto>()
                .ForMember(dest => dest.PlayerXName, opt => opt.MapFrom(src => src.PlayerX != null ? src.PlayerX.Name : "Jogador X"))
                .ForMember(dest => dest.PlayerOName, opt => opt.MapFrom(src => src.PlayerO != null ? src.PlayerO.Name : "Jogador O"))
                .ForMember(dest => dest.WinnerName, opt => opt.MapFrom(src => src.Winner != null ? src.Winner.Name : null))
                .ForMember(dest => dest.PlayedAt, opt => opt.MapFrom(src => src.FinishedAt ?? src.StartedAt))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<PlayersEntity, RankingEntryDto>()
                .ForMember(dest => dest.PlayerName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.WinRate, opt => opt.MapFrom(src =>
                    src.GamesPlayed > 0 ? Math.Round((double)src.Wins / src.GamesPlayed * 100, 1) : 0))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


            // =============================================
            // Mapeamentos DTO → Entity (escrita / criação / atualização)
            // Ignora coleções de navegação e chaves estrangeiras que serão setadas manualmente
            // =============================================

            CreateMap<PlayerCreateDto, PlayersEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.GamesPlayed, opt => opt.Ignore())
                .ForMember(dest => dest.Wins, opt => opt.Ignore())
                .ForMember(dest => dest.Draws, opt => opt.Ignore())
                //.ForMember(dest => dest.GamesAsX, opt => opt.Ignore())
                //.ForMember(dest => dest.GamesAsO, opt => opt.Ignore())
                //.ForMember(dest => dest.WinsAsPlayer, opt => opt.Ignore())
                .ForMember(dest => dest.Moves, opt => opt.Ignore())
                .ReverseMap();  

            CreateMap<GameCreateDto, GamesEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // será "ongoing"
                .ForMember(dest => dest.PlayerXId, opt => opt.Ignore()) // setado manualmente
                .ForMember(dest => dest.PlayerOId, opt => opt.Ignore())
                .ForMember(dest => dest.WinnerId, opt => opt.Ignore())
                .ForMember(dest => dest.IsDraw, opt => opt.Ignore())
                .ForMember(dest => dest.BoardState, opt => opt.Ignore()) // inicializado no service
                .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
                .ForMember(dest => dest.FinishedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastMoveAt, opt => opt.Ignore())
                .ForMember(dest => dest.PlayerX, opt => opt.Ignore())
                .ForMember(dest => dest.PlayerO, opt => opt.Ignore())
                .ForMember(dest => dest.Winner, opt => opt.Ignore())
                .ForMember(dest => dest.Moves, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<MakeMoveDto, MovesEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.GameId, opt => opt.MapFrom(src => src.GameId))
                .ForMember(dest => dest.PlayerId, opt => opt.Ignore()) // setado no service
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => (short)src.Position))
                .ForMember(dest => dest.MoveNumber, opt => opt.Ignore()) // calculado
                .ForMember(dest => dest.PlayedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Game, opt => opt.Ignore())
                .ForMember(dest => dest.Player, opt => opt.Ignore())
                .ReverseMap();
            
        }

        // Helpers privados (exemplo de conversão de board)
        private static List<string> ParseBoardJsonToFlatList(string jsonBoard)
        {                        
            try
            {
                // Exemplo se for matriz 3x3 JSON
                var matrix = System.Text.Json.JsonSerializer.Deserialize<string[][]>(jsonBoard);
                if (matrix == null || matrix.Length != 3) return new List<string>(9);

                var flat = new List<string>(9);
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        flat.Add(matrix[i][j] ?? "");
                return flat;
            }
            catch
            {
                return new List<string>(9) { "", "", "", "", "", "", "", "", "" };
            }
        }
    }
}