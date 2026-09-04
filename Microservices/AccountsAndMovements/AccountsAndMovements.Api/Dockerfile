# =============================================================================
# AccountsAndMovements — microservicio gRPC del pago QR.
#
# El contexto de build es la RAIZ del repositorio, no esta carpeta: el servicio
# depende de los cuatro proyectos compartidos de YastaCoin/Shared, que viven
# fuera de Microservices/. Por eso el compose declara `context: .`.
#
#   docker build -f Microservices/AccountsAndMovements/AccountsAndMovements.Api/Dockerfile .
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Los .csproj van primero y solos: mientras no cambien las dependencias, Docker
# reutiliza la capa del restore y no vuelve a bajar los paquetes en cada build.
COPY YastaCoin/Shared/Core.Domain/Core.Domain.csproj                                             YastaCoin/Shared/Core.Domain/
COPY YastaCoin/Shared/Core.AuditTrail/Core.AuditTrail.csproj                                     YastaCoin/Shared/Core.AuditTrail/
COPY YastaCoin/Shared/Core.Infrastructure/Core.Infrastructure.csproj                             YastaCoin/Shared/Core.Infrastructure/
COPY YastaCoin/Shared/Core.ShareKernel/Core.ShareKernel.csproj                                   YastaCoin/Shared/Core.ShareKernel/
COPY Microservices/AccountsAndMovements/AccountsAndMovements.Domain/AccountsAndMovements.Domain.csproj                 Microservices/AccountsAndMovements/AccountsAndMovements.Domain/
COPY Microservices/AccountsAndMovements/AccountsAndMovements.Infrastructure/AccountsAndMovements.Infrastructure.csproj Microservices/AccountsAndMovements/AccountsAndMovements.Infrastructure/
COPY Microservices/AccountsAndMovements/AccountsAndMovements.Application/AccountsAndMovements.Application.csproj       Microservices/AccountsAndMovements/AccountsAndMovements.Application/
COPY Microservices/AccountsAndMovements/AccountsAndMovements.Api/AccountsAndMovements.Api.csproj                       Microservices/AccountsAndMovements/AccountsAndMovements.Api/

RUN dotnet restore Microservices/AccountsAndMovements/AccountsAndMovements.Api/AccountsAndMovements.Api.csproj

# Recien ahora el codigo, que cambia en cada commit.
COPY YastaCoin/Shared/ YastaCoin/Shared/
COPY Microservices/AccountsAndMovements/ Microservices/AccountsAndMovements/

RUN dotnet publish Microservices/AccountsAndMovements/AccountsAndMovements.Api/AccountsAndMovements.Api.csproj \
    -c Release -o /app --no-restore

# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Sin privilegios: la imagen de aspnet trae el usuario `app` ya creado.
RUN mkdir -p /app/logs && chown -R app:app /app
USER app

COPY --from=build --chown=app:app /app .

# gRPC en texto plano necesita HTTP/2 con conocimiento previo. Sin esto Kestrel
# negocia HTTP/1.1 y ningun cliente gRPC puede conectarse.
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_KESTREL__ENDPOINTDEFAULTS__PROTOCOLS=Http2 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080

ENTRYPOINT ["dotnet", "AccountsAndMovements.Api.dll"]
