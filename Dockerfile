# Build javascript client
FROM node:20-slim AS build_js

ENV PNPM_HOME="/pnpm"
ENV PATH="$PNPM_HOME:$PATH"
RUN npm install -g corepack@latest \
    && corepack enable

WORKDIR /App
COPY ./Client ./
RUN --mount=type=cache,id=pnpm,target=/pnpm/store pnpm install --frozen-lockfile
RUN pnpm run build

# Build server
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build_cs
WORKDIR /App

COPY . ./
COPY --from=build_js /App/dist ./Client/dist
RUN dotnet restore --locked-mode
RUN dotnet publish Server/Server.csproj -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /App
COPY --from=build_cs /App/out .
VOLUME /App/Frierun

COPY ./entrypoint.sh /entrypoint.sh
RUN chmod 0755 /entrypoint.sh

EXPOSE 8080
ENTRYPOINT ["/entrypoint.sh"]
CMD ["dotnet", "Frierun.Server.dll", "serve"]

