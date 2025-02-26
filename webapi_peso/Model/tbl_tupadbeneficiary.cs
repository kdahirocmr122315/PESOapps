namespace webapi_peso.Model
{
    public class tbl_tupadbeneficiary
    {
        public class Beneficiary
        {
            public int ID { get; set; }
            public string? Firstname { get; set; }
            public string? MiddleName { get; set; }
            public string? Lastname { get; set; }
            
            public string? ExtensionName { get; set; }
            public string? Birthday { get; set; }
            public string? Barangay { get; set; }
            public string? Municipality { get; set; }
            public string? Province { get; set; }
            public string? District { get; set; }
            public string? IDType { get; set; }
            public string? IDNumber { get; set; }
            public string? ContactNo { get; set; }
            public string? Epayment { get; set; }
            public string? TypeOfBenef { get; set; }
            public string? Occupation { get; set; }
            public string? Sex { get; set; }
            public string? CivilStatus { get; set; }
            public string? Age { get; set; }
            public string? AverageIncome { get; set; }
            public string? Dependent { get; set; }
            public string? InterestWage { get; set; }
            public string? SkillsTraining { get; set; }
        }
    }
}
