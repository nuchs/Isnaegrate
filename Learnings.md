* Things wot I done wrong

So much pain...

** Event Store
- its EventStore.Client.Grpc.Streams not EventStore.Client.Grpc you need from nuget
- Uid's need to be set by the publisher rather than be generated further down the chain (e.g. in the proxy or event store itself). This allows for idempotent publishing
- I believe ES is using Guids as event id's but as this isn't a native transport type for GRPC it extracts the bytes and sends them as a set of longs
- The Uuid type does not expose the longs so you need to
	1. use EventStore types everywhere
	2. Convert the id to a guid (it provides a helper) and turn to bytes
	3. convert the id to a string (it provides a helper which basically turns the long to a guid and then to a string)
  Option three is easiest to work with, until it causes issues go with that
- Because the event store api records payload data as byte[] then we have the following options for our payload type (assuming json serialization)
	1. T -> json string -> bytestring [send to proxy ]-> byte[] -> bytestring [send to event store ] -> byte[]
	2. T -> json string [send to proxy ] -> byte[] -> bytestring [send to event store ] -> byte[]
- Subscription connecitions need to stay open so events can be sent back 
- if connection to es drops then a EventStoreClient will spin pretty fast trying to reconnect. Can;t see an obvious way to turn it off. Could probably be done with an interceptor but would be easy to have unintended consequences
- You can get an off by one error on events, subscriptions/reads start after teh point you specify. The point is unsigned so min is 0 but 0 is valid event so need to specify null
- Projections have a warm up - they shouldn't be serving until they're up to date. You need to be careful with this because the DI system in asp won't instantiate an instance of an object until it's needed. This will often be on the first request

** Grpc
- Enum values must be unique per *package* (damn you c++)
- Some funny interactions between opt_csharp_namespace and package. Namespace name inference from package is pretty good so just use that (as it allows you to make use of packages as namepsaces within the protobug files)
- Putting contracts in nuget package makes them easy to share, fairly easy to only get the bits you need out the package
- don't put them in the nuget package until they've stabilised, it's a pain the arse while you're iterating.
- Namespace collisions are easy when you have lots of contracts, make liberal use of packages
- clean rebuilds often, the protobuf compiler gets wedged frequently
- Everythign is a value type. You need to think how to deal with the absence of a response - null is not an option try Maybe
- grpc can only convert from datetimes if they are utc

** Docker
- the build system needs to be in a separate stack from the deployed app (otherwise it can't come up to build stuff)
- You can specify a network to attach to during build, you need to do this if you want it to access stuff on your network? 
- build only supprts host network
- tcp logging drivers mean a container won't start if server is unavailable
- blocking literally stops the container, bad if the log is a network call
- non blokcing has a buffer which can overflow
- if you depend on container startup order make sure the dependencies actually report when ready not hwen the container has started
- docker compose and caching... make sure everythign is clean or you  will lose your sanity
- host names all change when running in compose, connections to localhost now go to a service whose name is defined in the compose file
- don't change the service names in docker compose while the service is running as compose will no longer recognise it
- docker compose up doesn't auto rebuild

** Elastic
- turn datastream off for the logstash destination or stuff doesn't get added to the right index
- filebeat is just pain to set up

