# =============================================================================
# Auth — emision de JWT. Mismo esquema que AccountsAndMovements: el contexto de
# build es la raiz del repositorio.
#
#   docker build -f Microservices/Auth/Auth.Api/Dockerfile .
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY YastaCoin/Shared/Core.Domain/Core.Domain.csproj                 YastaCoin/Shared/Core.Domain/
COPY YastaCoin/Shared/Core.AuditTrail/Core.AuditTrail.csproj         YastaCoin/Shared/Core.AuditTrail/
COPY YastaCoin/Shared/Core.Infrastructure/Core.Infrastructure.csproj YastaCoin/Shared/Core.Infrastructure/
COPY YastaCoin/Shared/Core.ShareKernel/Core.ShareKernel.csproj       YastaCoin/Shared/Core.ShareKernel/
COPY Microservices/Auth/Auth.Domain/Auth.Domain.csproj                 Microservices/Auth/Auth.Domain/
COPY Microservices/Auth/Auth.Infrastructure/Auth.Infrastructure.csproj Microservices/Auth/Auth.Infrastructure/
COPY Microservices/Auth/Auth.Application/Auth.Application.csproj       Microservices/Auth/Auth.Application/
COPY Microservices/Auth/Auth.Api/Auth.Api.csproj                       Microservices/Auth/Auth.Api/

RUN dotnet restore Microservices/Auth/Auth.Api/Auth.Api.csproj

COPY YastaCoin/Shared/ YastaCoin/Shared/
COPY Microservices/Auth/ Microservices/Auth/

RUN dotnet publish Microservices/Auth/Auth.Api/Auth.Api.csproj \
    -c Release -o /app --no-restore

# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN mkdir -p /app/logs && chown -R app:app /app
USER app

COPY --from=build --chown=app:app /app .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_KESTREL__ENDPOINTDEFAULTS__PROTOCOLS=Http2 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080

ENTRYPOINT ["dotnet", "Auth.Api.dll"]
