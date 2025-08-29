using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static webapi_peso.Model.Tbl_tupadbeneficiary;

namespace webapi_peso.Dbcontext
{
    public class TupadRepository(IConfiguration configuration)
    {
        private readonly string? _connectionString = configuration.GetConnectionString("pesoTest");


       
        // Ensure the connection string is not null or empty
       
        private IDbConnection Connection => new SqlConnection(_connectionString);

        public IEnumerable<Beneficiary> GetAllBeneficiary()
        {
            using (var connection = Connection)
            {
                connection.Open();
                return connection.Query<Beneficiary>("SELECT * FROM tbl_tupadbeneficiary").ToList();
            }
        }



        public Beneficiary GetbeneficiaryById(int id)
        {
            
            using (var connection = Connection)
            {

                connection.Open();
                return connection.QueryFirstOrDefault<Beneficiary>("SELECT * FROM tbl_tupadbeneficiary WHERE ID = @ID", new { ID = id });
            }
        }

        public void Addbeneficiary(Beneficiary beneficiary)
        {
            Console.WriteLine($"Birthdate: {beneficiary.Birthday}");
            using (var connection = new SqlConnection(_connectionString))

            {
                connection.Open();
                var sql = "INSERT INTO tbl_tupadbeneficiary (Firstname, Middlename, Lastname, ExtensionName, Birthday, Street, Barangay, Municipality, Province, District, IDType, IDNumber, ContactNo, Epayment, TypeOfBenef, Occupation, Sex, CivilStatus, Age, AverageIncome, Dependent, InterestWage, SkillsTraining) VALUES (@Firstname, @Middlename, @Lastname, @ExtensionName, @Birthday, @Street, @Barangay, @Municipality, @Province, @District, @IDType, @IDNumber, @ContactNo, @Epayment, @TypeOfBenef, @Occupation, @Sex, @CivilStatus, @Age, @AverageIncome, @Dependent, @InterestWage, @SkillsTraining)";
                connection.Execute(sql, new
                {
                    Firstname = beneficiary.Firstname,
                    MiddleName = beneficiary.MiddleName,
                    Lastname = beneficiary.Lastname,
                    ExtensionName = beneficiary.ExtensionName,
                    Birthday = beneficiary.Birthday ?? (object)DBNull.Value,  // ✅ Allow NULL in SQL
                    Street = beneficiary.Street,
                    Barangay = beneficiary.Barangay,
                    Municipality = beneficiary.Municipality,
                    Province = beneficiary.Province,
                    District = beneficiary.District,
                    IDType = beneficiary.IDType,
                    IDNumber = beneficiary.IDNumber,
                    ContactNo = beneficiary.ContactNo,
                    Epayment = beneficiary.Epayment,
                    TypeOfBenef = beneficiary.TypeOfBenef,
                    Occupation = beneficiary.Occupation,
                    Sex = beneficiary.Sex,
                    CivilStatus = beneficiary.CivilStatus,
                    Age = beneficiary.Age,
                    AverageIncome = beneficiary.AverageIncome,
                    Dependent = beneficiary.Dependent,
                    InterestWage = beneficiary.InterestWage,
                    SkillsTraining = beneficiary.SkillsTraining
                });
            }
        }

        public void UpdateUser(Beneficiary beneficiary)
        {
            using (var connection = Connection)
            {
                connection.Open();
                var sql = "UPDATE tbl_tupadbeneficiary SET Verification = @Verification, DeletedDate = GETDATE() WHERE Id = @ID";
                connection.Execute(sql, beneficiary);
            }
        }
        public void Deletebeneficiary(int id)
        {
            using (var connection = Connection)
            {
                connection.Open();
                var sql = "DELETE FROM tbl_tupadbeneficiary WHERE Id = @ID";
                connection.Execute(sql, new { Id = id });
            }
        }

        public IEnumerable<Beneficiary> VerifiedBeneficiary()
        {
            using (var connection = Connection)
            {
                connection.Open();
                return connection.Query<Beneficiary>("SELECT * FROM tbl_tupadbeneficiary WHERE Verification = 'YES'").ToList();
            }
        }

        public IEnumerable<Beneficiary> UnverifiedBeneficiary()
        {
            using (var connection = Connection)
            {
                connection.Open();
                return connection.Query<Beneficiary>("SELECT * FROM tbl_tupadbeneficiary WHERE Verification = 'NO'").ToList();
            }
        }
    }
}
