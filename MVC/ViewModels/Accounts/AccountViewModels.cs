using System;
using System.Collections.Generic;

namespace MVC.ViewModels.Accounts
{
    public class ClientAccountVM
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int TotalCartons { get; set; }
        public int TotalPallets { get; set; }
    }

    public class AccountTransactionVM
    {
        public int TransactionId { get; set; }
        public string Type { get; set; } = string.Empty; // Inbound | Outbound
        public DateTime Date { get; set; }
        public int Cartons { get; set; }
        public int Pallets { get; set; }
        public int AdditionalEntry { get; set; }
        public string? Notes { get; set; }
    }

    public class AccountTotalsVM
    {
        public int TotalInboundCartons { get; set; }
        public int TotalInboundPallets { get; set; }
        public int TotalOutboundCartons { get; set; }
        public int TotalOutboundPallets { get; set; }
        public int NetCartons { get; set; }
        public int NetPallets { get; set; }
    }

    public class AccountDetailsVM
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public int PhoneNumber { get; set; }
        public List<AccountTransactionVM> Transactions { get; set; } = new List<AccountTransactionVM>();
        public AccountTotalsVM Totals { get; set; } = new AccountTotalsVM();
    }
}
