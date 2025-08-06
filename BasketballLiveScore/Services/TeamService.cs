using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasketballLiveScore.DTOs.Team;
using BasketballLiveScore.DTOs.Player;
using BasketballLiveScore.Models;
using BasketballLiveScore.Repositories.Interfaces;
using BasketballLiveScore.Services.Interfaces;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Service pour la gestion des équipes
    /// Implémente la logique métier pour les équipes
    /// </summary>
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TeamService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Récupère toutes les équipes
        /// </summary>
        public async Task<IEnumerable<TeamDto>> GetAllTeamsAsync()
        {
            var teams = await _unitOfWork.Teams.GetAllAsync();

            return teams.Select(team => new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                City = team.City,
                Coach = team.Coach,
                PlayerCount = team.Players.Count,
                CreatedAt = team.CreatedAt
            });
        }

        /// <summary>
        /// Récupère une équipe détaillée par son identifiant
        /// </summary>
        public async Task<TeamDetailDto> GetTeamByIdAsync(int id)
        {
            var team = await _unitOfWork.Teams.GetTeamWithPlayersAsync(id);

            if (team == null)
                return null;

            return new TeamDetailDto
            {
                Id = team.Id,
                Name = team.Name,
                City = team.City,
                Coach = team.Coach,
                Players = team.Players.Select(p => new PlayerSummaryDto
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    JerseyNumber = p.JerseyNumber,
                    IsStarter = false // À déterminer selon le contexte du match
                }).ToList(),
                CreatedAt = team.CreatedAt
            };
        }

        /// <summary>
        /// Crée une nouvelle équipe
        /// </summary>
        public async Task<TeamDto> CreateTeamAsync(CreateTeamDto createTeamDto)
        {
            if (createTeamDto == null)
                throw new ArgumentNullException(nameof(createTeamDto));

            // Vérifier si le nom de l'équipe existe déjà
            if (await _unitOfWork.Teams.TeamExistsAsync(createTeamDto.Name))
                throw new InvalidOperationException($"Une équipe avec le nom '{createTeamDto.Name}' existe déjà");

            var team = new Team
            {
                Name = createTeamDto.Name,
                City = createTeamDto.City,
                Coach = createTeamDto.Coach,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Teams.AddAsync(team);
            await _unitOfWork.CompleteAsync();

            return new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                City = team.City,
                Coach = team.Coach,
                PlayerCount = 0,
                CreatedAt = team.CreatedAt
            };
        }

        /// <summary>
        /// Met à jour une équipe existante
        /// </summary>
        public async Task<TeamDto> UpdateTeamAsync(int id, UpdateTeamDto updateTeamDto)
        {
            if (updateTeamDto == null)
                throw new ArgumentNullException(nameof(updateTeamDto));

            var team = await _unitOfWork.Teams.GetByIdAsync(id);

            if (team == null)
                return null;

            // Mise à jour des propriétés si elles sont fournies
            if (!string.IsNullOrEmpty(updateTeamDto.Name))
            {
                // Vérifier l'unicité du nouveau nom
                if (team.Name != updateTeamDto.Name && await _unitOfWork.Teams.TeamExistsAsync(updateTeamDto.Name))
                    throw new InvalidOperationException($"Une équipe avec le nom '{updateTeamDto.Name}' existe déjà");

                team.Name = updateTeamDto.Name;
            }

            if (!string.IsNullOrEmpty(updateTeamDto.City))
                team.City = updateTeamDto.City;

            if (!string.IsNullOrEmpty(updateTeamDto.Coach))
                team.Coach = updateTeamDto.Coach;

            _unitOfWork.Teams.Update(team);
            await _unitOfWork.CompleteAsync();

            var updatedTeam = await _unitOfWork.Teams.GetTeamWithPlayersAsync(id);

            return new TeamDto
            {
                Id = updatedTeam.Id,
                Name = updatedTeam.Name,
                City = updatedTeam.City,
                Coach = updatedTeam.Coach,
                PlayerCount = updatedTeam.Players.Count,
                CreatedAt = updatedTeam.CreatedAt
            };
        }

        /// <summary>
        /// Supprime une équipe
        /// </summary>
        public async Task<bool> DeleteTeamAsync(int id)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(id);

            if (team == null)
                return false;

            _unitOfWork.Teams.Remove(team);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>
        /// Récupère les joueurs d'une équipe
        /// </summary>
        public async Task<IEnumerable<PlayerSummaryDto>> GetTeamPlayersAsync(int teamId)
        {
            var team = await _unitOfWork.Teams.GetTeamWithPlayersAsync(teamId);

            if (team == null)
                return null;

            return team.Players.Select(p => new PlayerSummaryDto
            {
                Id = p.Id,
                FullName = p.FullName,
                JerseyNumber = p.JerseyNumber,
                IsStarter = false // À déterminer selon le contexte du match
            });
        }
    }
}