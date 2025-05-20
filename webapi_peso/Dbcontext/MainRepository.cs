using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using webapi_peso.Model;
using static Pesomain.Model.Tbl_pesomain;


namespace MainpesoRepository.Dbcontext
{
    public class MainRepository(IConfiguration configuration)
    {
        private readonly string? _connectionString = configuration.GetConnectionString("pesoConnection");


        private IDbConnection Connection => new SqlConnection(_connectionString);

        public User VerifyUser(Log_user log_user)
        {
            using (var connection = Connection)
            {

                connection.Open();

                string query = @"SELECT * from UserAccounts WHERE Email= @Email AND Password = @Password";

                return connection.QueryFirstOrDefault<User>(query, new
                {
                    Email = log_user.Email,
                    Password = log_user.Password
                });

            }
        }


      
       
    }
}
