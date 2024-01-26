using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace MumbleApi.Database;

public static class SqlErrors
{
    public static bool IsUniqueViolation(this DbUpdateException e) =>
        e.InnerException is PostgresException { SqlState: "23505" };
}
