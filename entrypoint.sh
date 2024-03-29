#!/bin/sh

set -eo pipefail

if [ "$Environment" != "local" ]; then
    export Database__Password=$(decrs ${Encrypted__Database__Password}) || exit 1;
    export RecaptchaSettings__SecretKey=$(decrs ${Encrypted__RecaptchaSettings__SecretKey})
    runuser --user brighid /app/Service
    exit $?
fi

watch /app "runuser --user brighid /app/Service"