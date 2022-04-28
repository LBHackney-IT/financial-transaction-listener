FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

ARG SONAR_TOKEN
ENV SONAR_TOKEN=$SONAR_TOKEN

WORKDIR /app

# Enable SonarCloud
RUN apt-get update && apt-get install -y openjdk-11-jdk
RUN dotnet tool install --global dotnet-sonarscanner
ENV PATH="$PATH:/root/.dotnet/tools"

RUN dotnet sonarscanner begin /k:"LBHackney-IT_financial-transaction-listener" /o:"lbhackney-it" /d:sonar.host.url=https://sonarcloud.io /d:sonar.login="${SONAR_TOKEN}"

# Copy csproj and restore as distinct layers
COPY ./FinancialTransactionListener.sln ./
COPY ./FinancialTransactionListener/FinancialTransactionListener.csproj ./FinancialTransactionListener/
COPY ./FinancialTransactionListener.Tests/FinancialTransactionListener.Tests.csproj ./FinancialTransactionListener.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./FinancialTransactionListener/FinancialTransactionListener.csproj
RUN dotnet restore ./FinancialTransactionListener.Tests/FinancialTransactionListener.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out FinancialTransactionListener/FinancialTransactionListener.csproj
RUN dotnet build -c debug -o out FinancialTransactionListener.Tests/FinancialTransactionListener.Tests.csproj

CMD dotnet test

RUN dotnet sonarscanner end /d:sonar.login="${SONAR_TOKEN}"