Colo - Caching Only Lives Once - The Need for Speed
====

Technology
---------------

In order to make the web as responsive as possible, caching is used for queries that are executed multiple times. 

In order to derive uniqueness for the cache, the queries being executed along with the parameters are pushed together then a hash is generated. This becomes the filename. When a query is to be executed, that file name is looked for on the system. If the cache file is found, then it deserializes it and loads the data; otherwise, the query is executed and the cache is saved. Under this system like queries across users, such as criteria, are shared. Caches are not specific to a user unless that is a part of the query. These are only shared if the like query was executed on the same web server. If your system employs multiple web servers, the same query can have a cache file on each server.

Cache files are automatically cleaned up after a time set in the web.config file. This time is a set time and does not slide when the same query is executed again.

Setup
-----

Web.Config
----------
There are two pieces required in Config for caching to be turned on.

* A section entry ```<section name="Caching" type="Colo.Configuration.CachingSection, Colo" />```
* A configuration entry ```<Caching path="/netwrite" isPathVirtual="true" cacheLife="10" useCache="true"></Caching>```
 
The above caching is for all items. If you want different cache times based on the object you can do the following:

```csharp
<Caching path="/netwrite" isPathVirtual="true" cacheLife="30" useCache="true">
    <CacheDefault>
        <add type="Namespaace.Class1" cacheLife="10"></add>
        <add type="Namespaace.Class2" cacheLife="20"></add>
        <add type="Namespaace.Class3" cacheLife ="45"></add>
        <add type="Namespaace.Class4" cacheLife ="21" enabled="false"></add>
    </CacheDefault>
</Caching>
```

POCOs
-----

In order to have a class take adavange of chaching, you will need to apply the protobuf attributes to the class and members.

```csharp
[ProtoContract]
public class Person
{
    [ProtoMember(1)]
    public string FirstName { get; set; }
    [ProtoMember(2)]
    public string LastName { get; set; }
    [ProtoMember(3)]
    public string Email { get; set; }
    [ProtoMember(4)]
    public string Phone { get; set; }
    [ProtoMember(5)]
    public bool HasCoolness { get; set; }
}
```
Notice the class needs the [ProtoContract] attribute while the properties of the class get [ProtoMember(1)]. The number is important on the member. There can not be any repeated numbers within a class. If your model inherits from another class, that class will also need to have the Protobuf attributes. There are some other special attributes to add in those cases and they are tricky so please refer to the Protobuf documentation for usage.

Usage
-----

For examples on using the library, refernce the test classes.
