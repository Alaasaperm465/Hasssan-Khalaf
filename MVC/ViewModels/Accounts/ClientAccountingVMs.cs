using System;
using System.Collections.Generic;

namespace MVC.ViewModels.Accounts
{
    public class ClientAccountSummaryVM
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int TotalPalletsInWarehouse { get; set; }
        public decimal FinancialBalance { get; set; }
    }

    public class ClientAccountStatementFilter
    {
        public int? ClientId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }

    public class AccountStatementRow
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // Deposit | Withdrawal | ExtraOpening | Payment
        public int Pallets { get; set; }
        public int ExtraOpenings { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }

    public class AccountStatementVM
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int OpeningPallets { get; set; }
        public decimal OpeningBalance { get; set; }
        public List<AccountStatementRow> Rows { get; set; } = new List<AccountStatementRow>();
        public int ClosingPallets { get; set; }
        public decimal ClosingBalance { get; set; }

        // Client pricing
        public decimal PalletPrice { get; set; }
        public decimal ExtraOpeningPrice { get; set; }

        // Summary
        public decimal MonthlyCost { get; set; }
        public decimal PaymentsMade { get; set; }
        public decimal RemainingBalance { get; set; }
    }
}
