using MechanicsSoftware.IntegrationTests.Fixtures;

namespace MechanicsSoftware.IntegrationTests;

[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollectionDefinition : ICollectionFixture<WebApplicationFactoryFixture>
{
}
