using System;
using System.Data.SqlClient;

namespace Charon
{
    public class SqlRepo
    {
        private readonly string connectionString;

        public SqlRepo(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new NotSupportedException("You must specify a connection string.");
            this.connectionString = connectionString;
        }

        private SqlConnection getConnection()
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public T GetConnection<T>(Func<SqlConnection, T> action)
        {
            using var conn = getConnection();
            return action(conn);
        }

        public void GetConnectionTransaction(Action<SqlConnection, SqlTransaction> action)
        {
            using var conn = getConnection();
            using var transaction = conn.BeginTransaction();

            try
            {
                action(conn, transaction);
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
