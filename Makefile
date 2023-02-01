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

db-reset: db-clean db-seed
