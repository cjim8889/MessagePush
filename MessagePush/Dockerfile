FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["MessagePush/MessagePush.csproj", "MessagePush/"]
RUN dotnet restore "MessagePush/MessagePush.csproj"
COPY . .
WORKDIR "/src/MessagePush"
RUN dotnet build "MessagePush.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "MessagePush.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MessagePush.dll"]