.PHONY: db-clean db-reset
# Install dependencies and tools
init:
	dotnet restore
	dotnet tool restore
	# pip install pre-commit
	# pre-commit install

# Get new clean database
db-clean:
	rm -rf ./Migrations
	rm -f database.db
	dotnet ef migrations add "Initial"
	dotnet ef database update 

# Seed database
db-seed:
	cd ../seeder; dotnet run

db-reset: db-clean db-seed

typescript-client:
	java -jar '../swagger-codegen-cli.jar' generate \
        -Dio.swagger.v3.parser.util.RemoteUrl.trustAll=true \
		-i https://localhost:5001/swagger/v1/swagger.json \
		-l typescript-axios \
		-o './ClientApp/src/api' 
	find ./ClientApp/src/api \
        -type f \
        -exec sed -i -e 's/localVarRequestOptions.headers\[/localVarRequestOptions.headers?.[/' {} +