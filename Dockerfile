FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FC.Codeflix.Catalog.Api/FC.Codeflix.Catalog.Api.csproj", "src/FC.Codeflix.Catalog.Api/"]
COPY ["src/FC.Codeflix.Catalog.Application/FC.Codeflix.Catalog.Application.csproj", "src/FC.Codeflix.Catalog.Application/"]
COPY ["src/FC.Codeflix.Catalog.Domain/FC.Codeflix.Catalog.Domain.csproj", "src/FC.Codeflix.Catalog.Domain/"]
COPY ["src/FC.Codeflix.Catalog.Infra.Data.EF/FC.Codeflix.Catalog.Infra.Data.EF.csproj", "src/FC.Codeflix.Catalog.Infra.Data.EF/"]
RUN dotnet restore "src/FC.Codeflix.Catalog.Api/FC.Codeflix.Catalog.Api.csproj"
COPY . .
WORKDIR "/src/src/FC.Codeflix.Catalog.Api"
RUN dotnet build "FC.Codeflix.Catalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FC.Codeflix.Catalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FC.Codeflix.Catalog.Api.dll"]
