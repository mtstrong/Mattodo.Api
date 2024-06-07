using System.Data;

namespace Mattodo.Api;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}