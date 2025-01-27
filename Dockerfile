# Build javascript client
FROM node:20-slim AS build_js

ENV PNPM_HOME="/pnpm"
ENV PATH="$PNPM_HOME:$PATH"
RUN corepack enable

WORKDIR /App
COPY ./Client ./
RUN --mount=type=cache,id=pnpm,target=/pnpm/store pnpm install --frozen-lockfile
RUN pnpm run build

# Build server
FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:3fcf6f1e809c0553f9feb222369f58749af314af6f063f389cbd2f913b4ad556 AS build_cs
WORKDIR /App

COPY . ./
COPY --from=build_js /App/dist ./Client/dist
RUN dotnet restore
RUN dotnet publish Server/Server.csproj -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0@sha256:b4bea3a52a0a77317fa93c5bbdb076623f81e3e2f201078d89914da71318b5d8
WORKDIR /App
COPY --from=build_cs /App/out .
VOLUME /App/Frierun
ENTRYPOINT ["dotnet", "Frierun.Server.dll"]

