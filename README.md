#ConfOxide
ConfOxide is a library for writing settings classes.  You can define your classes using simple C# auto-implemented properties, and ConfOxide will let you easily clone them, compare them, reset them, or save them to (and load them from) JSON files.

Install ConfOxide from [NuGet](https://www.nuget.org/packages/ConfOxide/).

#Example
First, create a settings class:

```CSharp
sealed class ConnectionSettings : SettingsBase<ConnectionSettings> {
	[DefaultValue(2)]
	public int RetryCount { get; set; }
	[DefaultValue("00:00:30")]
	public TimeSpan RetryDelay { get; set; }
}

sealed class MyAppSettings : SettingsBase<MyAppSettings> {
	public IList<ConnectionSettings> Endpoints { get; private set; }

	[DefaultValue("2013-12-11")]
	public DateTime TimelineStart { get; set; }
	public int? MaxAge { get; set; }
}
```

Note that concrete settings classes must be `sealed`.  To create an inheritable settings class, you must add a type parameter; see below.

##Usage
To create a new settings type, just write `new MyAppSettings()`.  All collection & settings properties will be initialized to new instances, and all properties will be set to their default values.  All settings methods are implemented as extension methods in the `ConfOxide.SettingsExtensions` class (for performance reasons); to call these methods, you will need to add `using ConfOxide`.

##Working with JSON documents
 - To read a settings file from JSON, call `settings.ReadJson(json)` and pass a JSON.net `JObject` instance with the data to read.
 - To update a JSON object from an existing settings instance, call `settings.UpdateJson(json)` and pass the `JObject` to update.  The existing property order, as well as any extra properties, will be preserved.
 - To create a new JSON object from an existing settings instance, call `settings.ToJson()`.

##Working with JSON files
ConfOxide also includes helper methods to read and write JSON files from disk.  Call `settings.ReadJsonFile(filename)` to read an existing JSON file, if it exists.  Call `settings.WriteJsonFile(filename)` to create or update a JSON file from the settings object

##Working with Settings instances
 - Call `settings.IsEquivalentTo(otherSettings)` to check whether two settings instances hold the same values.  This is a deep comparison that will recursively compare collections and nested settings objects by value.
 - Call `settings.AssignFrom(sourceSettings)` to deeply assign the values from one settings instance to another.
 - Call `settings.CreateCopy()` to create a deep clone of a settings instance.  (this is shorthand for `new YourSettingsClass().AssignFrom(settings)`

These methods are particularly useful when creating cancellable Options dialogs.  You can call `settingsCreateCopy()` to bind your options dialog to a deep copy of the settings class, call `copy.IsEquivalentTo(copy)` to check whether there are any changes to apply, and call `settings.AssignFrom(copy)` to apply changes when clicking OK.

##Supported types
ConfOxide supports properties of all basic .Net types, including primitive numeric types, `decimal`, `string`, `DateTime`, `DateTimeOffset`, and `TimeSpan`, as well as nullable types thereof.
Properties containing other `SettingsBase<T>` classes are also supported, as long as there are no circular dependencies.
ConfOxide also supports collection properties of other `SettingsBae<T>` classes or of supported scalar types.  You can use any collection class that is writable, variable-sized, and has a default constructor.  If you make a property of type `IList<T>`, ConfOxide will create a `List<T>` to assign to the property.  Note that arrays are not supported.

#FAQ
 - **Q: What does the name mean?**<br />
     A: ConfOxide is a portmanteau of &ldquo;Configuration&rdquo; and &ldquo;Carbon Dioxide&rdquo; (dry ice).  The point of this library is to allow [DRY](http://en.wikipedia.org/wiki/Don't_repeat_yourself "Don't Repeat Yourself") configuration classes.

 - **Q: Wouldn't writing the serialization & cloning code by hand be faster?**<br />
     A: Nope!  
   ConfOxide uses advanced techniques to compile strongly-typed accessor code at runtime, avoiding all boxing (except when saving to JSON; JSON.Net does not expose any way to save value types without boxing).  
At the cost of a slight longer initialization time (to build the accessor code for each type using reflection), ConfOxide should be exactly as fast as code you write by hand.

 - **Q: Why are all of the utility methods defined as extension methods?**<br />
     A: To avoid extra casting.  Had those methods been defined in the base `SettingsBase<T>` class, it would have been impossible to access properties from derived classes without casting `this` (this is a limitation in C#'s type system).  By using extension methods with the [CRTP](http://en.wikipedia.org/wiki/Curiously_recurring_template_pattern "Curiously recurring template pattern"), I can access the properties of `T` directly using pre-created delegates.

 - **Q: Do I need to write a constructor?**<br />
     A: Nope!  The base `SettingsBase<T>` constructor will automatically initialize all nested collection & settings properties, and apply all declared default values.

 - **Q: How can I declare a default value for a collection property?**<br />
     A: .Net doesn't provide any decent way to do that.  Instead, override the `ResetCustom()` and populate the collection in code.  This method will be called after each instance is constructed, as well as after `ResetValues()` is called.

 - **Q: Can I create a base classes with common properties and have multiple concrete settings classes inherit it?**<br />
     A: Sure.  However, in order to make the type-safe accessor logic work, the intermediary class also needs to implement the [CRTP](http://en.wikipedia.org/wiki/Curiously_recurring_template_pattern "Curiously recurring template pattern"):
```CSharp
abstract class VersionedSettingsBase<T> : SettingsBase<T> where T : VersionedSettingsBase<T> {
	[DefaultValue(1)]
	public int Version { get; set; }
}
sealed class MyAppSettings : VersionedSettingsBase<MyAppSettings> {
	// ...
}
``` 

 - **Q: What about XML?**<br />
     A: If enough people are interested in serializing to XML in addition to JSON, I'll add support for that.

#License
[MIT](http://opensource.org/licenses/MIT)
