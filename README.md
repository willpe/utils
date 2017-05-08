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

A few of the more frequently used hashing algorithms (CRC32, MD5, SHA256, SHA1) are included with concurrent wrappers. These make the non-thread safe hashing algorithms effectively thread safe by using one instance per thread.

````
ConcurrentSHA256.ComputeHash("Value...");
````

Results will be Base64 Encoded.

### CRC32

Includes [Damien Guard's CRC32 Implementation for C#](https://github.com/damieng/DamienGKit/blob/master/CSharp/DamienG.Library/Security/Cryptography/Crc32.cs). Thanks Damien!

### Deterministic Guid

Includes [Logos Bible Software's Implementation of RFC 4122 (Section 4.3) C#](https://github.com/LogosBible/Logos.Utility/blob/master/src/Logos.Utility/GuidUtility.cs) as `DeteministicGuid`. Thanks!

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

## Dates and Times

### Unix Timestamp

Simple conversions between DateTime objects and Unix Timestamps

````
var timestamp = DateTime.Now.ToUnixTime();

var dateTime = UnixTimestamp.ToDateTime(timestamp);
````

### DateMath

Implements an evaluator for *Date Math* style expressions. For example:

  - NOW -> 2014/10/27 12:41:32 PM
  - NOW/HOUR -> 2014/10/27 12:00:00 PM
  - NOW/DAY+4HOURS -> 2014/10/27 04:00:00 AM
  - 2014-01-13/MONTH+5HOURS -> 2014/01/01 05:00:00 AM

Syntax is roughly:

    [Date Expression] [Rounding Clause] [Offset Clause]

The date expression must be either 'NOW' or an ISO 8601 date (like 2014-10-27T19:21:32.321Z). If a date is specified, it's OK to omit any portion after the 'T'.

The **rounding clause** looks like `/HOUR` or `/DAY` and rounds down the date expression to the specified resolution. You can specify one of: Year, Month, Day, Hour, Minute or Second. The rounding clause is optional, and can be omitted.

The **offset clause** looks like `+2DAYS` or `-4HOURS` and adds (or subtracts) the specified interval to the date expression (which has already been rounded, if a rounding clause is specified). The clause consists of a **sign** (`+` or `-`), a **magnitude** (an integer) and a **unit** (one of: Year, Month, Day, Hour, Minute or Second). The unit may include an optional trailing 's' (so, you can do `+2Hours` or `+2hour`, it's up to you).

The expression is case insensitive (so `now/hour+2minutes` is fine).

````
DateTime startTime = DateMath.Evaluate("NOW/HOUR-4HOURS");
````

A `TryEvaluate` overload is available, too.