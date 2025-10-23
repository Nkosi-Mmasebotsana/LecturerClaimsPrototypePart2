using System;

namespace ContractMonthlyClaimSystem.Models
{
    public class Lecturer
    {
        public int LecturerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public decimal HourlyRate{ get; set; }
    }
}
