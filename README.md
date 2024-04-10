# https://api.trzebachleba.pl
# Bakery Website API

The bakery API was created for an engineering paper and because of later plans for a family member to open a bakery. The API is used for basic functionality on the user interface page. It performs both client-side and admin-side operations, managing orders, products and all resources. It was created based on the REST API framework. The code is part of learning and personal development in programming in the .net platform.
## Features

- **MYSQL** Currently using MYSQL database put on ubuntu server along with the website. Previously I was using AZURE portal
- **Azure Functions:** Implements Azure Functions for automatic deletion of overdone table records.
- **Blob Container for Images:** Uses a blob container on the Azure platform to manage and store images.

## Migrations
The API contains database migrations for MYSQL. Run these migrations to configure the necessary tables and schema.

## Azure Blob Container
The API utilizes Azure Blob Storage for image handling. Ensure the blob container is correctly configured and accessible by the API.

## Usage
1. **Clone this repository** to your local environment.
2. **Configure** the `.env` file according to `appsettings.json` with Azure connection strings and other necessary configurations.
3. **Ensure all dependencies** are installed and configured.
4. **Run the API** on your development environment. You can add swagger to program.cs.
