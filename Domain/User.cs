using Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hassann_Khala.Domain
{
    public class User:BaseEntity
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!; // plain text for testing only
        public string Role { get; set; } = null!; // Admin or StoreKeeper
    }
}