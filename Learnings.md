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

** Grpc
- Enum values must be unique per *package* (damn you c++)
- Some funny interactions between opt_csharp_namespace and package. Namespace name inference from package is pretty good so just use that (as it allows you to make use of packages as namepsaces within the protobug files)
- Putting contracts in nuget package makes them easy to share, fairly easy to only get the bits you need out the package
- don't put them in the nuget package until they've stabilised, it's a pain the arse while you're iterating.
- Namespace collisions are easy when you have lots of contracts, make liberal use of packages
- clean rebuilds often, the protobuf compiler gets wedged frequently