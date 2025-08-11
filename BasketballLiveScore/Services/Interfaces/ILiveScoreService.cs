using System;
using System.Threading.Tasks;
using BasketballLiveScore.DTOs.LiveScore;

namespace BasketballLiveScore.Services.Interfaces
{
    /// <summary>
    /// Interface pour le service de score en temps réel
    /// </summary>
    public interface ILiveScoreService
    {
        Task<bool> StartClockAsync(int matchId);
        Task<bool> StopClockAsync(int matchId);
        Task<bool> RecordBasketAsync(int matchId, BasketScoreDto basketDto);
        Task<bool> RecordFoulAsync(int matchId, FoulCommittedDto foulDto);
        Task<bool> RecordSubstitutionAsync(int matchId, PlayerSubstitutionDto substitutionDto);
        Task<bool> RecordTimeoutAsync(int matchId, int teamId);
        Task<LiveScoreUpdateDto> GetLiveScoreAsync(int matchId);
        Task<bool> StartMatchAsync(int matchId);
    }
}