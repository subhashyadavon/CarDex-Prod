# CarDex Docker Instructions

This guide explains how to run the CarDex backend and PostgreSQL database using Docker.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

## Quick Start

1.  Open a terminal in the root `CarDex` directory (where `docker-compose.yml` is located).
2.  Run the following command to build and start the containers:

    ```bash
    docker-compose up --build
    ```

3.  Wait for the build to complete and the services to start. You should see logs indicating the database is ready and the API is listening.

## Accessing the Application

-   **API Base URL**: `http://localhost:5001`
-   **Swagger UI**: `http://localhost:5001/swagger`
-   **Database**: `localhost:5432`
    -   **User**: `postgres`
    -   **Password**: `postgres`
    -   **Database**: `cardex`

## Stopping the Containers

To stop the containers, press `Ctrl+C` in the terminal where they are running.

To stop and remove the containers (and networks), run:

```bash
docker-compose down
```

## Data Persistence

Database data is persisted in a Docker volume named `postgres_data`. This means your data will survive container restarts.

To **reset the database** (delete all data), run:

```bash
docker-compose down -v
```

## Troubleshooting

-   **Port Conflicts**: If port `5001` or `5432` is already in use, kill those ports and try again. Do not change the ports in docker-compose.yml.
-   **Database Connection**: The API connects to the database using the hostname `cardex-db` (defined in `docker-compose.yml`). This is handled automatically by the Docker network.
-   **Migrations**: The application is configured to automatically create the database schema on startup (`EnsureDatabaseCreated`). If you need to run specific migrations, you may need to execute them manually or configure an entrypoint script.

## Configuration

Environment variables are defined in `docker-compose.yml`. Do not modify them. These are for development purposes only.




## Production Deployment (Supabase)

For production environments where you are using an external database like Supabase, use the `docker-compose.prod.yml` file.

### 1. Create a `.env` file

Create a file named `.env` in the root directory with your production secrets:
I will share in Discord


### 2. Run with Production Configuration

Run the following command to start the API in production mode:

```bash
docker-compose -f docker-compose.prod.yml --env-file .env up --build -d
```

This will:
-   Start **only** the API container (no local database).
-   Expose the API on port **80** (standard HTTP port).
-   Connect to your Supabase database using the connection string in `.env`.
-   Run in `Production` environment mode.

### 3. Verify Production Status

Check the logs to ensure successful connection:

```bash
docker logs cardex-api-prod
```

