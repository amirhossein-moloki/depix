FROM node:20-alpine

WORKDIR /app

COPY package.json tsconfig.json* ./
RUN npm install --omit=dev || npm install

COPY . .

EXPOSE 3001

CMD ["npx", "ts-node", "server.ts"]
