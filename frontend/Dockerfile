# Сборка (п. 5): npm ci по зафиксированному package-lock.json -> статика в образе.
# Публичный адрес API (VITE_API_URL) передаётся как build-аргумент,
# в код он не зашит (п. 3).

FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
# Чистое восстановление зависимостей строго по lock-файлу (п. 2)
RUN npm ci

COPY . .
ARG VITE_API_URL=/api
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# --- Артефакт: статика + nginx, раздаёт UI и проксирует /api на бэкенд ------
FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
