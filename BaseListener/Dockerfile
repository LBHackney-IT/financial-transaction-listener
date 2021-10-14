FROM mcr.microsoft.com/dotnet/core/sdk:3.1

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

# Copy csproj and NUGet.Config and restore as distinct layers
COPY ./BaseListener/BaseListener.csproj ./
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config
RUN dotnet restore ./BaseListener.csproj


# Copy everything else and build
COPY ./BaseListener ./
RUN dotnet build -c Release -o out ./BaseListener.csproj
RUN dotnet publish -c Release -o out ./BaseListener.csproj

EXPOSE ${PORT:-3000}
CMD ASPNETCORE_URLS=http://+:${PORT:-3000} dotnet ./out/BaseListener.dll
