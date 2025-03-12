using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static Pesomain.Model.tbl_pesomain;


namespace MainpesoRepository.Dbcontext
{
    public class MainRepository(IConfiguration configuration)
    {
        private readonly string? _connectionString = configuration.GetConnectionString("pesoConnection");


        private IDbConnection Connection => new SqlConnection(_connectionString);

        public async Task<bool> verifyUser(Log_user Nlog_user)
        {
            using (var connection = Connection)
            {

                connection.Open();

                string query = @"select Email,password from UserAccounts WHERE Email= '" + Nlog_user.Username + "' AND password = '" + Nlog_user.Password + "' AND UserType IN (1, 2)";

                var log_users = await Connection.QueryAsync<Log_user>(query);
                bool rt = false;

                if (log_users.Count() > 0)
                {
                    rt = true;
                }               


                return rt;

            }
        }


      
       
    }
}
