#!/bin/bash

set -eo pipefail

if [ "$CODEBUILD_GIT_BRANCH" = "master" ]; then
    docker push $COMMANDS_IMAGE_TAG;
fi