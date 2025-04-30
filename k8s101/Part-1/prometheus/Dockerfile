FROM prom/prometheus:v2.21.0

WORKDIR /prometheus

COPY prometheus.yml /prometheus/config/prometheus.yml
COPY ./entrypoint.sh ./

CMD ["sh", "./entrypoint.sh"]