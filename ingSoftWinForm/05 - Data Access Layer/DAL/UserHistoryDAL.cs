using System.Data;
using Microsoft.Data.SqlClient;

namespace DAL
{
    public class UserHistoryDAL : IUserHistoryDAL
    {
        private readonly DatabaseHelper dbHelper = new();

        public void Insert(Guid? userId, string username, string eventType, bool success,
            int failedAttempts, string? detail = null,
            string? fieldName = null, string? oldValue = null, string? newValue = null)
        {
            const string query = @"
                INSERT INTO [UserHistory]
                    ([UserId], [Username], [EventType], [Success], [FailedAttempts],
                     [FieldName], [OldValue], [NewValue], [Detail])
                VALUES
                    (@UserId, @Username, @EventType, @Success, @FailedAttempts,
                     @FieldName, @OldValue, @NewValue, @Detail);";

            SqlParameter[] parameters =
            [
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = (object?)userId ?? DBNull.Value },
                new SqlParameter("@Username", Truncate(username, 50)),
                new SqlParameter("@EventType", eventType),
                new SqlParameter("@Success", success),
                new SqlParameter("@FailedAttempts", failedAttempts),
                new SqlParameter("@FieldName", SqlDbType.NVarChar, 100) { Value = (object?)fieldName ?? DBNull.Value },
                new SqlParameter("@OldValue", SqlDbType.NVarChar, -1) { Value = (object?)oldValue ?? DBNull.Value },
                new SqlParameter("@NewValue", SqlDbType.NVarChar, -1) { Value = (object?)newValue ?? DBNull.Value },
                new SqlParameter("@Detail", SqlDbType.NVarChar, 500) { Value = (object?)Truncate(detail, 500) ?? DBNull.Value }
            ];

            dbHelper.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        private static string? Truncate(string? value, int max)
            => value != null && value.Length > max ? value[..max] : value;
    }
}
