# Multi-stage build for React frontend
# Stage 1: Build the React application
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files
COPY CarDexFrontend/package*.json ./

# Install dependencies
RUN npm ci --only=production=false

# Copy source code
COPY CarDexFrontend/ .

# Build argument for API URL (can be overridden at build time)
ARG REACT_APP_API_URL=http://localhost:5009
ENV REACT_APP_API_URL=$REACT_APP_API_URL

# Build the application
RUN npm run build

# Stage 2: Serve with nginx
FROM nginx:alpine

# Copy custom nginx configuration
COPY CarDexFrontend/nginx.conf /etc/nginx/conf.d/default.conf

# Copy built files from build stage
COPY --from=build /app/build /usr/share/nginx/html

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
