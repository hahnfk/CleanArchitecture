namespace CleanArchitecture.Contracts.Persistence;

public enum PersistenceProvider
{
    InMemory = 0,
    EfSqlite = 1,
    // Future:
    // EfSqlServer = 2,
    // EfPostgres = 3,
    // EfMySql = 4,
    // EfOracle = 5,
    // AdoSqlite = 100,
    // AdoSqlServer = 101,
    // AdoPostgres = 102,
    // AdoMySql = 103,
    // AdoOracle = 104
}
