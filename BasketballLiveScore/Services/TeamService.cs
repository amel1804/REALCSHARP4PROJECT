using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasketballLiveScore.Models;
using BasketballLiveScore.Repositories.Interfaces;
using BasketballLiveScore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Interface pour le service de gestion des �quipes
    /// </summary>
    public interface ITeamService
    {
        Task<Team> GetByIdAsync(int id);
        Task<IEnumerable<Team>> GetAllAsync();
        Task<Team> CreateAsync(Team team);
        Task<Team> UpdateAsync(int id, Team team);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Player>> GetPlayersAsync(int teamId);
        Task<bool> AddPlayerToTeamAsync(int teamId, int playerId);
        Task<bool> RemovePlayerFromTeamAsync(int teamId, int playerId);
    }

    /// <summary>
    /// Service pour la gestion des �quipes de basketball
    /// </summary>
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TeamService> _logger;

        public TeamService(IUnitOfWork unitOfWork, ILogger<TeamService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Team> GetByIdAsync(int id)
        {
            try
            {
                var team = _unitOfWork.Teams.GetTeamWithPlayers(id);
                return await Task.FromResult(team);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la r�cup�ration de l'�quipe {TeamId}", id);
                return null;
            }
        }

        public async Task<IEnumerable<Team>> GetAllAsync()
        {
            try
            {
                var teams = _unitOfWork.Teams.GetActiveTeams();
                return await Task.FromResult(teams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la r�cup�ration des �quipes");
                return new List<Team>();
            }
        }

        public async Task<Team> CreateAsync(Team team)
        {
            if (team == null)
                throw new ArgumentNullException(nameof(team));

            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(team.Name))
                    throw new ArgumentException("Le nom de l'�quipe est obligatoire");

                // V�rifier l'unicit� du nom
                var existingTeam = _unitOfWork.Teams.Find(t => t.Name == team.Name).FirstOrDefault();
                if (existingTeam != null)
                {
                    throw new InvalidOperationException($"Une �quipe avec le nom '{team.Name}' existe d�j�");
                }

                team.CreatedAt = DateTime.UtcNow;
                _unitOfWork.Teams.Add(team);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("�quipe {TeamName} cr��e avec succ�s", team.Name);
                return team;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la cr�ation de l'�quipe");
                throw;
            }
        }

        public async Task<Team> UpdateAsync(int id, Team team)
        {
            if (team == null)
                throw new ArgumentNullException(nameof(team));

            try
            {
                var existingTeam = _unitOfWork.Teams.GetById(id);
                if (existingTeam == null)
                {
                    _logger.LogWarning("�quipe {TeamId} non trouv�e", id);
                    return null;
                }

                // Mise � jour des propri�t�s
                existingTeam.Name = team.Name;
                existingTeam.City = team.City;
                existingTeam.Coach = team.Coach;

                _unitOfWork.Teams.Update(existingTeam);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("�quipe {TeamId} mise � jour avec succ�s", id);
                return existingTeam;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise � jour de l'�quipe {TeamId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var team = _unitOfWork.Teams.GetById(id);
                if (team == null)
                {
                    _logger.LogWarning("�quipe {TeamId} non trouv�e pour suppression", id);
                    return false;
                }

                // V�rifier qu'il n'y a pas de matchs associ�s
                var hasMatches = _unitOfWork.Matches
                    .Find(m => m.HomeTeamId == id || m.AwayTeamId == id)
                    .Any();

                if (hasMatches)
                {
                    _logger.LogWarning("Impossible de supprimer l'�quipe {TeamId} car elle a des matchs associ�s", id);
                    return false;
                }

                _unitOfWork.Teams.Remove(team);
                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                {
                    _logger.LogInformation("�quipe {TeamId} supprim�e avec succ�s", id);
                }

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'�quipe {TeamId}", id);
                return false;
            }
        }

        public async Task<IEnumerable<Player>> GetPlayersAsync(int teamId)
        {
            try
            {
                var players = _unitOfWork.Players.GetPlayersByTeam(teamId);
                return await Task.FromResult(players);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la r�cup�ration des joueurs de l'�quipe {TeamId}", teamId);
                return new List<Player>();
            }
        }

        public async Task<bool> AddPlayerToTeamAsync(int teamId, int playerId)
        {
            try
            {
                var team = _unitOfWork.Teams.GetById(teamId);
                var player = _unitOfWork.Players.GetById(playerId);

                if (team == null || player == null)
                {
                    _logger.LogWarning("�quipe ou joueur non trouv�");
                    return false;
                }

                player.TeamId = teamId;
                _unitOfWork.Players.Update(player);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Joueur {PlayerId} ajout� � l'�quipe {TeamId}", playerId, teamId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout du joueur � l'�quipe");
                return false;
            }
        }

        public async Task<bool> RemovePlayerFromTeamAsync(int teamId, int playerId)
        {
            try
            {
                var player = _unitOfWork.Players.GetById(playerId);

                if (player == null || player.TeamId != teamId)
                {
                    _logger.LogWarning("Joueur non trouv� ou n'appartient pas � l'�quipe");
                    return false;
                }

                player.TeamId = 0; // Retirer de l'�quipe
                _unitOfWork.Players.Update(player);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Joueur {PlayerId} retir� de l'�quipe {TeamId}", playerId, teamId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du retrait du joueur de l'�quipe");
                return false;
            }
        }
    }
}