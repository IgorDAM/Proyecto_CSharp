using Xunit;

namespace MarinaApi.Tests;

[CollectionDefinition("MySql")]
public class MySqlCollection : ICollectionFixture<MySqlFixture>
{
}