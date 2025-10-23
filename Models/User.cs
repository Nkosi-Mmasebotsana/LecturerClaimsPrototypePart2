using System;

namespace ContractMonthlyClaimSystem.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; } // e.g., "Programme Coordinator", "Academic Manager", "HR"

    }
}
