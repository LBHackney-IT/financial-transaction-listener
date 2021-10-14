FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'
ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./BaseListener.sln ./
COPY ./BaseListener/BaseListener.csproj ./BaseListener/
COPY ./BaseListener.Tests/BaseListener.Tests.csproj ./BaseListener.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./BaseListener/BaseListener.csproj
RUN dotnet restore ./BaseListener.Tests/BaseListener.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out BaseListener.Tests/BaseListener.Tests.csproj

CMD dotnet test
