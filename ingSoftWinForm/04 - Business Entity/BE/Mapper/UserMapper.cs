using System.Data;
using BE.Base;
using BE.Entity;

namespace BE.Mapper
{
    public class UserMapper : BaseMapper<User>
    {
        public override User MapToEntity(DataRow row)
        {
            return new User
            {
                Id = (Guid)row["Id"],
                Username = (string)row["Username"],
                PasswordHash = (byte[])row["PasswordHash"],
                Salt = (byte[])row["Salt"],
                FirstName = (string)row["FirstName"],
                LastName = (string)row["LastName"],
                Email = row["Email"] as string,
                FailedAttempts = (int)row["FailedAttempts"],
                IsBlocked = (bool)row["IsBlocked"],
                IsActive = (bool)row["IsActive"],
                LastLoginAt = row["LastLoginAt"] as DateTime?,
                CreatedAt = (DateTime)row["CreatedAt"],
                CreatedBy = row["CreatedBy"] as string,
                UpdatedAt = row["UpdatedAt"] as DateTime?,
                UpdatedBy = row["UpdatedBy"] as string
            };
        }
    }
}
