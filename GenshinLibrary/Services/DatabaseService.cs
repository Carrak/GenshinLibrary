using Npgsql;
using System.Data;
using System.Threading.Tasks;

namespace GenshinLibrary.Services
{
    public class DatabaseService
    {
        private NpgsqlConnection Connection { get; set; }
        private string ConnectionString { get; set; }

        public async Task InitAsync(string connectionString)
        {
            ConnectionString = connectionString;

            // Open connection
            Connection = new NpgsqlConnection(connectionString);
            await Connection.OpenAsync();

            // Register StateChange event
            Connection.StateChange += ConnectionStateChanged;
        }

        /// <summary>
        ///     Closes and reopens connection in case its state was changed.
        /// </summary>
        private void ConnectionStateChanged(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState != ConnectionState.Open)
            {
                Connection.Close();
                Connection.Open();
                Logger.Log("Database", "Reopened connection");
            }
        }

        public NpgsqlCommand GetCommand(string query, bool newConnection = false)
        {
            if (newConnection)
            {
                NpgsqlConnection conn = new NpgsqlConnection(ConnectionString);
                conn.Open();
                return new NpgsqlCommand(query, conn);
            }
            else
                return new NpgsqlCommand(query, Connection);
        }
    }
}
