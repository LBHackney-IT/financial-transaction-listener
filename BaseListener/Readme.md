# Base Listener

The BaseListener application provides a template for creating for new Listener applications that is triggered by an AWS SQS message.

## Template project assumptions
These are the default assumptions the template project code makes.

* The trigger is done via an SQS message
* The application uses a DynamoDb datastore

Of course if any of these assumption do not fit the actual intended purpose (using a different trigger or a different datastore)
then that can be easily changed to suit each use case.

## Project anatmoy
The project files are categorised in the same way as the [base api template](https://github.com/LBHackney-IT/lbh-base-api).

### Core project files
#### Appsettings
These work in the same way as the settings file for a WebApi application. Whereas a WebApi application has the `ASPNETCORE_ENVIRONMENT`
environment variable to determine in which environment the application is running (and thus which custom appsettings file to use),
this listener application provides the same functionality using the `ENVIRONMENT` environment variable.
This value is set in the `launchSettings.json` and [docker compose](#Docker) file when running locally, and within the `servlerless.yml` when deployed to an environment.

#### BaseFunction
This abstract class provides core functionalit for implementing a trigger function.
It organises the initialisation of the entire DI stack for the implemented function as well as setting up the necessary configuration and logging providers.
To create a function that acts on an AWS trigger derive from this class and implement any custom initialisation in the appropriate overridable methods.

#### EventTypes
A simple static class listing all the event types supported by this listener application.

##### Serverless.yml
This is similar to that used by the base api template. Specific changes include:
* The `functions` section specifies 
  * The exact method on the `SqsFunction` class that will get called when triggered.
  * The type of trigger and the arn of the SQS queue (taken from the paramter store) that will be used. (Currently commented out.)
* A policy allowing the access to the queue used to trigger the function.
* A policy used to access the DynamoDB database required. (Currently commented out.)

#### SqsFunction
The function class that gets triggered by an SQS message, derived from [BaseFunction](#BaseFunction).
It overrides the `ConfigureServices()` method to perform the necessary DI registrations,
and implements the `FunctionHandler()` method that is called by AWS whenever 1 or more messages arrive in the configured SQS queue.

### Boundary
The code here represents model and functionality that relates to processing at the application boundary, the model definition of an incoming SQS message for example.
The `EntityEventSns` class included is the standard event message currently implemented within MTFH.

### Domain
Here lies the domain models and logic required for message processing.

### Factories
The factories are responsible for converting incoming boundary models to domain and or database models.

### Gateway
Each gateway represents how to communicate with an exterior data source - DynamoDb, Elsatic search, an S3 bucket, for example.

### Infrastructure
Here lies structural code like exception types, DI registration extension methods, database models, etc..

### UseCase
Each message type supported should provide its own UseCase implementation (derive an interface from the `IMessageProcessing` interface)
to do what it needs to do.

## Logging
Through the [`BaseFunction`](#BaseFunction), the application configures the application to use the AWS Lamba logger for you.
In addition, through the inclusion of the Hackney.Core.Logging NuGet package it also uses the [`LogCallAspect`](https://github.com/LBHackney-IT/lbh-core#log-call-aspect).
This means that any public method decorated with the `LogCall` attribute will automatically have log entries generated on entry and exit of the method.

## Testing
All code should have appropriate unit tests where possible.
(Some infrastructure code like the `SQSFunction` and `BaseFunction` cannot, but they should be marked with the `ExcludeFromCodeCoverage`
attribute so as not to skew the code coverage metrics.)

See the [test project readme](../BaseListener.Tests/ReadMe.md) for more details.

## Docker
The docker files in the application and test projects, and the `docker-compose.yml` in the root work in exactly the same way as in the base api template.

## Terraform
The `main.tf` terraform files included here contain large sections that are commented out, but that do have descriptive comments indicating what each section is for.
Basically the commented out sections include:
* Define a parameter from the AWS parameter store that contains the arn of the SNS topic from which this application wants to receive the events.
This should have been created be the service that published the events in the first place.
* The creation of the SQS queue and dead letter queue to be used by this application.
* A subscription to get the SNS topic to send events to this application's queue.
* A policy the will permit the SNS topic to send events to this application's queue.
* A Parameter store parameter that will contain the arn of the created SQS queue. (This is then used by [`Serverless.yml`](#Serverlessyml).)

**Notes:**

* The DynamoDb database is not set up here. It is assumed that a listener is acting on an existing datastore that has already been created by
a different application. As a result it is assumed that the DynamoDb database will already have been created elsewhere,
* The `variable.tf` file for each environment defines the `project_name` variable. This must be updated to the appropriate project name.,


## CircleCi
This is the same as that used by the base api template.

**Note:**

This application makes use of common Hackney.Core.xxx NuGet packages and as a result requires that an environment variable is set up both locally and within the CircleCi project.
[See here for more information on how to set this up.](https://github.com/LBHackney-IT/lbh-core/wiki/Using-the-package(s)-from-the-Hackney.Core-repository#environment-variable)
