#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Net6TemplateWebApi/Net6TemplateWebApi.csproj", "Net6TemplateWebApi/"]
RUN dotnet restore "Net6TemplateWebApi/Net6TemplateWebApi.csproj"
COPY . .
WORKDIR "/src/Net6TemplateWebApi"
RUN dotnet build "Net6TemplateWebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Net6TemplateWebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Net6TemplateWebApi.dll"]