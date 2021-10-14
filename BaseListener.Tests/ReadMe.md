# BaseListener.Tests

This is the test project for the BaseListener application.

### Technology stack

- .Net Core
- [xUnit](https://xunit.net/)
- [Moq](https://github.com/Moq/moq4/wiki/Quickstart)
- [FluentAssertions](https://fluentassertions.com/introduction)
- [BDDfy](https://github.com/TestStack/TestStack.BDDfy) - for E2E tests

### Agreed Testing Approach
- Use xUnit, FluentAssertions and Moq
- Always follow a TDD approach
- Tests should be independent of each other
  - This means that any data created for a test (by the test setup or by the code being tested itself) shold be removed once the test is done.
- Gateway tests should interact with a real test instance of the database/datastore
- Test coverage should never go down. (See the [below](#run-coverage) for how to run a coverage check.)
- All use cases should be covered by E2E tests
- Optimise when test run speed starts to hinder development
- Unit tests and E2E tests should run in CI
- Test database schemas should match up with production database schema
- Have integration tests which test from the DynamoDb database to API Gateway

## Logging
The listener application makes use of `[LogCall]` attributes to provide automatic function level logging.

This cross-cutting code is generated at compile-time, meaning that any tests have to ensure that it is setup up accordingly
so that the tests will run as intended.

Any the test class for any class that uses the `[LogCall]` attribute (with the exception of DynamoDb gateway - see [below](#Gateway-&-E2E-DynamoDb-setup))
that uses the must ensure that it decorates the test class declaration with `[Collection("LogCall collection")]`
(this collection is implemented by the `LogCallAspectFixture` class).
For example:
```csharp
[Collection("LogCall collection")]
public class DoSomethingUseCaseTests
{
    ...
}
```

Failure to do this will result in tests failing with an error like the one below.
```
  Message: 
System.NullReferenceException : Object reference not set to an instance of an object.

  Stack Trace: 
LogCallAspectServices.GetInstance(Type type)
DoSomethingUseCase.__a$_initialize_aspects()
DoSomethingUseCase.ctor(IDbEntityGateway gateway) line 16
DoSomethingUseCaseTests.ctor() line 31
```

## Gateway & E2E DynamoDb setup

In order to create valid tests that use an actual DynamoDb table2 things are needed:
- A running local instance of DynamoDb. (The docker compose file creates one for you.)
- The required table (and indexes if needed) created.

To facilitate this there are 2 helper classes:
- `MockApplicationFactory`
  - This class creates an application host for the listener application on the fly, sets up the necessary DI container.
- `DynamoDbFixture`
  - This class:
    - It sets up any necessary environment variables required to start the application host.
    - It contains a definition of the DynamoDb table(s) required. This should be exactly how the table will be configured withiin the AWS environments.
    - Using the `MockApplicationFactory` it starts creates and starts the application host, ensuring the DynamoDb table definied actually exists in the local instance.
    - Ensures that the [`LogCallAspectFixture`](#Logging) is also set up.

### Usage
All of this is then encapsulated in an xUnit collection with the name `DynamoDb collection`.
This collection should be used to decorate the DymanoDb gateway test class and any E2E test class.
```
[Collection("DynamoDb collection")]
public class DynamoDbEntityGatewayTests : IDisposable
{
}
```
- The test class must implement the IDisposable pattern.
- The tests class is passed the `DynamoDbFixture` instance. 
It holds a `IDynamoDBContext` instance that can be used to both construct the class under test and directly add/remove database records as needed.
```
public DynamoDbEntityGatewayTests(DynamoDbFixture dbTestFixture)
{
    _dbTestFixture = dbTestFixture;
    _classUnderTest = new DynamoDbEntityGateway(_dbTestFixture.DynamoDbContext, _logger.Object);
}
```

## E2E Tests
These use BDDfy to implement tests that are constructed using the gherkin syntax.
The triggering of the publicly-facing function is not done using a real SQS queue instance, but rather we create an instance of the `SqsFunction` class
and call the `FunctionHandler()` method directly to simulate how AWS would do this in a deployed environment.
See the [BaseSteps class](/BaseListener.Tests/E2ETests/Steps/BaseSteps.cs) for more details.


## Run coverage
In the root of this test project directory is the file `RunCoverage.bat` which can be used (on Windows at least) to generate a test code coverage report.

### Pre-requisites
To use `RunCoverage.bat` the [reportgenerator](https://github.com/danielpalme/ReportGenerator) package needs to be installed locally.
```
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Usage
- Ensure that all tests are passing.
- Open a command prompt in the root of the test project.
- Run the following command 
```
RunCoverage.bat
```
- The tests will be run, an HTML coverage report generated and then opened in the default browser.
- If any tests fail then a new coverage report will not get created. If one _is_ shown it will be the previous one created from the last successful run.

