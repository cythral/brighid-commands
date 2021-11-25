ARG CONFIGURATION=Release

FROM public.ecr.aws/cythral/brighid/base:0.1.13
ARG CONFIGURATION

ENV CONFIGURATION=${CONFIGURATION}
WORKDIR /app
COPY ./entrypoint.sh /
COPY ./bin/Service/${CONFIGURATION}/linux-musl-x64/publish ./

RUN \
    mkdir -p /var/brighid/commands && \
    setcap 'cap_net_bind_service=+ep' /app/Service

EXPOSE 80
ENTRYPOINT [ "/entrypoint.sh" ]