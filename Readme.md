this is an implementation of ICrmService that does not persist data to disk
or database. It is able to do simple queries, but no link entities.

Only string equality comparisons are possible currently - no other data types
or comparison operators are supported. Note that there are a lot of operators,
so it is unlikely that I will ever support them all.

Some other things to think about is exactly which exceptions or errors are thrown
and how do we simulate SoapExceptions. What gets returned when nothing matches,
is it an empty collection, etc. These details of the api are always tricky.