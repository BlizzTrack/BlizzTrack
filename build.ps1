docker build --file '.\BlizzTrack\Dockerfile' . --tag registry.gitlab.com/blizztrack/blizztrack-v2:latest
docker build --file '.\Worker\Dockerfile' . --tag registry.gitlab.com/blizztrack/blizztrack-v2/worker:latest

docker push registry.gitlab.com/blizztrack/blizztrack-v2:latest
docker push registry.gitlab.com/blizztrack/blizztrack-v2/worker:latest
