using Microsoft.Data.SqlClient;
using System;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace sistemaTickets.Services
{
    public class correo
    {
        private IConfiguration _configuration;

        public correo(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void enviar(string destinatario, string asunto, string cuerpo)
        {
            try
            {
                string connetionString = _configuration.GetSection("ConnectionStrings").GetSection("DB_TicketsDbConnection").Value;

                string sqlQuery = "exec msdb.dbo.sp_send_dbmail " +
                                  "@profile_name = 'SQL_Mail_CATOLICA2', " +
                                  "@recipients = @par_destinatarios, " +
                                  "@subject = @par_asunto, " +
                                  "@body = @par_mensaje";

                using (SqlConnection connection = new SqlConnection(connetionString))
                {
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@par_destinatarios", destinatario);
                        command.Parameters.AddWithValue("@par_asunto", asunto);
                        command.Parameters.AddWithValue("@par_mensaje", cuerpo);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                Console.WriteLine("Correo enviado correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar el correo:");
                Console.WriteLine(ex.Message);
            }
        }
    }
}