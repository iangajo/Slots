﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SlotAPI.Models;

namespace SlotAPI.DataStores
{
    public interface ITransactionHistory
    {
        void AddTransactionHistory(decimal winAmount, int playerId, string transaction, string gameId, int winningLine, string winningCombinations);

        TransactionHistory GetLastTransactionHistoryByPlayer(int playerId);
    }
}
