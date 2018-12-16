FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

COPY . ./
WORKDIR /app/ConsoleInterface
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:runtime
WORKDIR /app/ConsoleInterface
COPY --from=build-env /app/ConsoleInterface/out .
ENTRYPOINT ["dotnet", "ConsoleInterface.dll"]

# If using the cluster from this compose file, link the container to it.
# docker run -it --name dotnet-casssandra --link cassandra-seed-node --net stockexchange_default container-name