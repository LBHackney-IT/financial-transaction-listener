FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'
ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./FinancialTransactionListener.sln ./
COPY ./FinancialTransactionListener/FinancialTransactionListener.csproj ./FinancialTransactionListener/
COPY ./FinancialTransactionListener.Tests/FinancialTransactionListener.Tests.csproj ./FinancialTransactionListener.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./FinancialTransactionListener/FinancialTransactionListener.csproj
RUN dotnet restore ./FinancialTransactionListener.Tests/FinancialTransactionListener.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out FinancialTransactionListener.Tests/FinancialTransactionListener.Tests.csproj

CMD dotnet test

#FROM mcr.microsoft.com/dotnet/core/sdk:3.1
#
#ARG LBHPACKAGESTOKEN
#ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
#WORKDIR /app
#
## Copy csproj and NUGet.Config and restore as distinct layers
#COPY ./FinancialTransactionListener/FinancialTransactionListener.csproj ./
#RUN dotnet restore ./FinancialTransactionListener.csproj
#
#
## Copy everything else and build
#COPY ./FinancialTransactionListener ./
#RUN dotnet build -c Release -o out ./FinancialTransactionListener.csproj
#RUN dotnet publish -c Release -o out ./FinancialTransactionListener.csproj
#
#EXPOSE ${PORT:-3000}
#CMD ASPNETCORE_URLS=http://+:${PORT:-3000} dotnet ./out/FinancialTransactionListener.dll
#
