FROM nginx:alpine

# Copy static frontend HTML
COPY code.html /usr/share/nginx/html/index.html

# Copy custom Nginx configuration
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
