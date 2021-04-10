param ($param2, $param1)
if(!$param1)  {
	$param1 = "develop";
}

Switch ($param2) {
	"worker" {
		docker build --file '.\Worker\Dockerfile' . --tag registry.gitlab.com/blizztrack/blizztrack-v2/worker:$param1
		docker push registry.gitlab.com/blizztrack/blizztrack-v2/worker:$param1
	}
	"site" {
		docker build --file '.\BlizzTrack\Dockerfile' . --tag registry.gitlab.com/blizztrack/blizztrack-v2:$param1
		docker push registry.gitlab.com/blizztrack/blizztrack-v2:$param1
	}
	"meta" {
		docker build --file '.\BlizzMeta\Dockerfile' . --tag registry.gitlab.com/blizztrack/blizztrack-v2/meta:$param1
		docker push registry.gitlab.com/blizztrack/blizztrack-v2/meta:$param1
	}
	"notify" {
		docker build --file '.\Notifications\Dockerfile' . --tag registry.gitlab.com/blizztrack/blizztrack-v2/notify:$param1
		docker push registry.gitlab.com/blizztrack/blizztrack-v2/notify:$param1
	}
	default {
		docker build --file '.\Worker\Dockerfile' . --tag registry.gitlab.com/blizztrack/blizztrack-v2/worker:$param1
		docker push registry.gitlab.com/blizztrack/blizztrack-v2/worker:$param1
		docker build --file '.\BlizzTrack\Dockerfile' . --tag registry.gitlab.com/blizztrack/blizztrack-v2:$param1
		docker push registry.gitlab.com/blizztrack/blizztrack-v2:$param1
		docker build --file '.\BlizzMeta\Dockerfile' . --tag registry.gitlab.com/blizztrack/blizztrack-v2/meta:$param1
		docker push registry.gitlab.com/blizztrack/blizztrack-v2/meta:$param1
		docker build --file '.\Notifications\Dockerfile' . --tag registry.gitlab.com/blizztrack/blizztrack-v2/notify:$param1
		docker push registry.gitlab.com/blizztrack/blizztrack-v2/notify:$param1
	}
}
