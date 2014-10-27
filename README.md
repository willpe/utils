WillPe Utilities
================

A set of utility classes for .Net.



## Async

### Async.CompletedTask

Returns a task that's marked as completed. Great for when you're implmenting an asynchronous, void method syncronously:

````
public Task DoThing()
{
	thing.Do();

	return Async.CompletedTask;
}
````

## Cryptography and Hashing

### Concurrent Hashing Algorithms

A few of the more frequently used hashing algorithms (CRC32, MD5, SHA256) are included with concurrent wrappers. These make the non-thread safe hashing algorithms effectively thread safe by using one instance per thread.

````
ConcurrentSHA256.ComputeHash("Value...");
````

Results will be Base64 Encoded.

### CRC32

Includes [Damien Guard's CRC32 Implementation for C#](https://github.com/damieng/DamienGKit/blob/master/CSharp/DamienG.Library/Security/Cryptography/Crc32.cs). Thanks Damien!

## Encoding and Serializing

### Base64 Encoding

A wrapper around Convert.ToBase64 with some slightly more friendly signatures and Base64-URL encoding by default.

````
var foo = Base64Encoding.ToBase64String("Another string, not base64 encoded");
````

### HexEncoding

A utility class to convert to and from base-16 encoding:

````
var hex = HexEncoding.ToHexString([24, 35, 127]);
````


