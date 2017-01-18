FROM microsoft/aspnetcore:1.1.0
ENTRYPOINT ["dotnet", "SelfHostTest.dll"]
ARG source=.
WORKDIR /app
EXPOSE 44338
COPY $source .
