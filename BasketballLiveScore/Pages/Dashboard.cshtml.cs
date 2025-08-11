using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasketballLiveScore.DTOs.Match;
using BasketballLiveScore.Models;
using BasketballLiveScore.Models.Enums;
using BasketballLiveScore.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BasketballLiveScore.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(IUnitOfWork unitOfWork, ILogger<DashboardModel> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public List<MatchDto> RecentMatches { get; set; } = new();
        public int TotalMatchesCount { get; set; }
        public int ActiveMatchesCount { get; set; }

        public IActionResult OnGet()
        {
            // V�rifier l'authentification
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                // R�cup�rer les matchs r�cents
                var matches = _unitOfWork.Matches.GetAll()
                    .OrderByDescending(m => m.ScheduledDate)
                    .Take(10)
                    .ToList();

                RecentMatches = matches.Select(m => new MatchDto
                {
                    Id = m.Id,
                    ScheduledDate = m.ScheduledDate,
                    HomeTeamName = m.HomeTeam?.Name ?? "�quipe domicile",
                    AwayTeamName = m.AwayTeam?.Name ?? "�quipe ext�rieure",
                    HomeTeamScore = m.HomeTeamScore,
                    AwayTeamScore = m.AwayTeamScore,
                    Status = m.Status.ToString(),
                    CurrentQuarter = m.CurrentQuarter
                }).ToList();

                // Statistiques
                TotalMatchesCount = _unitOfWork.Matches.GetAll().Count();
                ActiveMatchesCount = _unitOfWork.Matches
                    .Find(m => m.Status == Models.Enums.MatchStatus.InProgress)
                    .Count();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du dashboard");
                return Page();
            }
        }
    }
}