# CarDex Docker Instructions

This guide explains how to run the CarDex backend in a Docker container, connecting to the live Supabase database.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.
- A `.env` file in the root directory containing the `SUPABASE_CONNECTION_STRING`.

## Quick Start

1.  Ensure you have a `.env` file with the required secrets (specifically `SUPABASE_CONNECTION_STRING`).
2.  Open a terminal in the root `CarDex` directory.
3.  Run the following command to build and start the containers:

    ```bash
    docker-compose up --build
    ```

4.  Wait for the build to complete and the services to start.

## Accessing the Application

-   **API Base URL**: `http://localhost:5001`
-   **Swagger UI**: `http://localhost:5001/swagger`
-   **Database**: Connected to Supabase (configured in `.env`).

## Stopping the Containers

To stop the containers, press `Ctrl+C` in the terminal where they are running.

To stop and remove the containers (and networks), run:

```bash
docker-compose down
```

## Data Persistence

Data is stored in the Supabase database in the cloud, so it persists independently of the Docker containers.

## Troubleshooting

-   **Port Conflicts**: If port `5001` is already in use, kill the process using it.
-   **Database Connection**: Ensure your `SUPABASE_CONNECTION_STRING` in `.env` is correct and you have internet access.
-   **Migrations**: The application is configured to automatically ensure the database is created (`EnsureDatabaseCreated`).

## Configuration

Environment variables are passed to the container from your `.env` file and `docker-compose.yml`.

## Production Deployment

For production environments, use the `docker-compose.prod.yml` file.

### 1. Create a `.env` file

Ensure your `.env` file contains production secrets.

### 2. Run with Production Configuration

Run the following command to start the API in production mode:

```bash
docker-compose -f docker-compose.prod.yml --env-file .env up --build -d
```

This will:
-   Start **only** the API container.
-   Expose the API on port **8080** (mapped to host port defined in docker-compose.prod.yml, usually 80 or 8080).
-   Connect to your Supabase database using the connection string in `.env`.
-   Run in `Production` environment mode.

### 3. Verify Production Status

Check the logs to ensure successful connection:

```bash
docker logs cardex-api-prod
```
