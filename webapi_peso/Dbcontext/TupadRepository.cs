using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static webapi_peso.Model.tbl_tupadbeneficiary;

namespace webapi_peso.Dbcontext
{
    public class TupadRepository(IConfiguration configuration)
    {
        private readonly string? _connectionString = configuration.GetConnectionString("DefaultConnection");


       
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
            using (var connection = Connection)
            {
                connection.Open();
                var sql = "INSERT INTO tbl_tupadbeneficiary (Firstname, Middlename, Lastname, ExtenstionName, Birthday, Barangay, Municipality, Province, District, IDType, IDNumber, ContactNo, Epayment, TypeOfBenef, Occupation, Sex, CivilStatus, Age, AverageIncome, Dependent, InterestWage, SkillsTraining) VALUES (@Firstname, @Middlename, @Lastname, @ExtenstionName, @Birthday, @Barangay, @Municipality, @Province, @District, @IDType, @IDNumber, @ContactNo, @Epayment, @TypeOfBenef, @Occupation, @Sex, @CivilStatus, @Age, @AverageIncome, @Dependent, @InterestWage, @SkillsTraining)";
                connection.Execute(sql, beneficiary);
            }
        }

        public void UpdateUser(Beneficiary beneficiary)
        {
            using (var connection = Connection)
            {
                connection.Open();
                var sql = "UPDATE tbl_tupadbeneficiary SET Firstname = @Firstname, Lastname = @Lastname, Age = @Age, Email = @Email WHERE Id = @ID";
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
