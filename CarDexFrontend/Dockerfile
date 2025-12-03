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
ARG REACT_APP_API_URL=http://localhost:8080
ENV REACT_APP_API_URL=$REACT_APP_API_URL

# Build the application
RUN npm run build

# Stage 2: Serve with nginx
FROM nginx:alpine

# Copy custom nginx configuration
COPY CarDexFrontend/nginx.conf /etc/nginx/conf.d/default.conf

# Copy built files from build stage
COPY --from=build /app/build /usr/share/nginx/html

# Create a non-root user and set up permissions
RUN chown -R nginx:nginx /usr/share/nginx/html && \
    chown -R nginx:nginx /var/cache/nginx && \
    chown -R nginx:nginx /var/log/nginx && \
    chown -R nginx:nginx /etc/nginx/conf.d && \
    touch /var/run/nginx.pid && \
    chown -R nginx:nginx /var/run/nginx.pid

# Switch to non-root user
USER nginx

# Expose port 8080 (non-privileged port for non-root user)
EXPOSE 8080

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
