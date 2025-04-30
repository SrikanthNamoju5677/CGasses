FROM node:12-alpine

WORKDIR /

COPY ["./package.json", "./app/package-lock.json*", "./"]

COPY . . 


CMD ["node", "src/index.js"]

EXPOSE 4000
EXPOSE 8080