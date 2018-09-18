﻿using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SlotAPI.Domains;
using SlotAPI.Models;

namespace SlotAPI.Controllers
{

    [ApiController]
    public class Slot : ControllerBase
    {
        private readonly IGame _game;
        private readonly ITransaction _transaction;
        private readonly IAccount _account;

        public Slot(IGame game, ITransaction transaction, IAccount account)
        {
            _game = game;
            _transaction = transaction;
            _account = account;
        }

        [HttpPost]
        [Route("api/register")]
        public ActionResult Registration([FromBody]Registration registration)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var response = _account.Register(registration.Username, registration.Password);

            return Ok(response);
        }

        [HttpPost]
        [Route("api/auth/{playerId}")]
        public ActionResult Auth([FromRoute]int playerId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var response = _account.GenerateToken(playerId);

            return Ok(response);
        }

        [HttpPost]
        [Route("api/spin")]
        public ActionResult Spin([FromBody]SpinRequest spinRequest)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var requestToken = Request.Headers["Authorization"].ToString().Replace(" ", string.Empty);
            var currentToken = $"Bearer{_account.GetToken(spinRequest.PlayerId)}";

            if (requestToken != currentToken)
            {
                return Unauthorized();
            }

            var reelResults = _game.Spin();

            //var errorMessage = _game.CheckIfPlayerWin(reelResults, spinRequest.Bet, spinRequest.PlayerId);

            var errorMessage = new BaseResponse();

            var transaction = string.Empty;
            decimal balance = -1;

            if (string.IsNullOrEmpty(errorMessage.ErrorMessage))
            {
                transaction = _transaction.GetLastTransactionHistoryByPlayer(spinRequest.PlayerId).Transaction;
                balance = _account.GetBalance(spinRequest.PlayerId);
            }

            var response = new SpinResponse()
            {
                ErrorMessage = errorMessage.ErrorMessage,
                Balance = balance,
                Transaction = transaction,
                Success = !string.IsNullOrEmpty(transaction),
                SpinResult = reelResults.OrderBy(r => r.WheelNumber).ToList()
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("api/getstate/{playerId}")]
        public ActionResult GetPlayerState(int playerId)
        {
            var transaction = _transaction.GetLastTransactionHistoryByPlayer(playerId).Transaction;
            var balance = _account.GetBalance(playerId);

            var success = balance > 0;

            var response = new SpinResponse()
            {
                ErrorMessage = string.Empty,
                Balance = balance,
                Transaction = transaction,
                Success = success

            };

            return Ok(response);
        }

        [HttpGet]
        [Route("api/paylinestats")]
        public ActionResult GetPayLineWinStats()
        {
            var response = _transaction.GetPayLineStats();

            return Ok(response);
        }

        [HttpGet]
        [Route("api/symbolstats")]
        public ActionResult GetSymbolWinStats()
        {
            var response = _transaction.GetSymbolStats();

            return Ok(response);
        }

        [HttpGet]
        [Route("api/totalwinamount/{playerId}")]
        public ActionResult GetPlayerTotalWin([FromRoute]int playerId)
        {
            var response = _transaction.GetPlayerTotalWinAmount(playerId);

            return Ok(response);
        }

    }
}
