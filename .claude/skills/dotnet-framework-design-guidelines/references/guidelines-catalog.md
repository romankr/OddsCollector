# .NET Framework Design Guidelines — full rule catalog

Every DO / CONSIDER / AVOID / DO NOT rule from Microsoft Learn, "Framework design guidelines"
(https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/) and all its sub-pages, in TOC order.
Source: excerpts of *Framework Design Guidelines, 2nd ed.* (Cwalina, Abrams, 2008) — some rules are dated;
see SKILL.md for how they apply in OddsCollector. Explanatory text is trimmed to one line where it adds the *why*.
Legend: ✅ DO · 🟡 CONSIDER · ⚠️ AVOID · ❌ DO NOT

## Naming

### Capitalization Conventions  (capitalization-conventions)

**Capitalization Rules for Identifiers**

- ✅ **DO** use PascalCasing for all public member, type, and namespace names consisting of multiple words.
- ✅ **DO** use camelCasing for parameter names.

**Capitalizing Compound Words and Common Terms**

- ❌ **DO NOT** capitalize each word in so-called closed-form compound words. — _These are compound words written as a single word, such as endpoint._

**Case Sensitivity**

- ❌ **DO NOT** assume that all programming languages are case sensitive. They are not. Names cannot differ by case alone.

### General Naming Conventions  (general-naming-conventions)

**Word Choice**

- ✅ **DO** choose easily readable identifier names. — _For example, a property named `HorizontalAlignment` is more English-readable than `AlignmentHorizontal`._
- ✅ **DO** favor readability over brevity. — _The property name `CanScrollHorizontally` is better than `ScrollableX` (an obscure reference to the X-axis)._
- ❌ **DO NOT** use underscores, hyphens, or any other nonalphanumeric characters.
- ❌ **DO NOT** use Hungarian notation.
- ⚠️ **AVOID** using identifiers that conflict with keywords of widely used programming languages. — _According to Rule 4 of the Common Language Specification (CLS), all compliant languages must provide a mechanism that allows access to named items that use a keyword of that language as an identifier._

**Using Abbreviations and Acronyms**

- ❌ **DO NOT** use abbreviations or contractions as part of identifier names. — _For example, use `GetWindow` rather than `GetWin`._
- ❌ **DO NOT** use any acronyms that are not widely accepted, and even if they are, only when necessary.

**Avoiding Language-Specific Names**

- ✅ **DO** use semantically interesting names rather than language-specific keywords for type names. — _For example, `GetLength` is a better name than `GetInt`._
- ✅ **DO** use a generic CLR type name, rather than a language-specific name, in the rare cases when an identifier has no semantic meaning beyond its type. — _For example, a method converting to `System.Int64` should be named `ToInt64`, not `ToLong` (because `System.Int64` is a CLR name for the C#-specific alias `long`)._
- ✅ **DO** use a common name, such as `value` or `item`, rather than repeating the type name, in the rare cases when an identifier has no semantic meaning and the type of the parameter is not important.

**Naming New Versions of Existing APIs**

- ✅ **DO** use a name similar to the old API when creating new versions of an existing API. — _This helps to highlight the relationship between the APIs._
- ✅ **DO** prefer adding a suffix rather than a prefix to indicate a new version of an existing API. — _This will assist discovery when browsing documentation, or using IntelliSense._
- 🟡 **CONSIDER** using a brand new, but meaningful identifier, instead of adding a suffix or a prefix.
- ✅ **DO** use a numeric suffix to indicate a new version of an existing API, particularly if the existing name of the API is the only name that makes sense (i.e., if it is an industry standard) and if adding any meaningful suffix (or changing the name) is not an appropriate option.
- ❌ **DO NOT** use the "Ex" (or a similar) suffix for an identifier to distinguish it from an earlier version of the same API.
- ✅ **DO** use the "64" suffix when introducing versions of APIs that operate on a 64-bit integer (a long integer) instead of a 32-bit integer. You only need to take this approach when the existing 32-bit API exists; don't do it for brand new APIs with only a 64-bit version.

### Names of Assemblies and DLLs  (names-of-assemblies-and-dlls)

- ✅ **DO** choose names for your assembly DLLs that suggest large chunks of functionality, such as System.Data. — _Assembly and DLL names don’t have to correspond to namespace names, but it is reasonable to follow the namespace name when naming assemblies._
- 🟡 **CONSIDER** naming DLLs according to the following pattern: — _where `<Component>` contains one or more dot-separated clauses._

### Names of Namespaces  (names-of-namespaces)

- ✅ **DO** prefix namespace names with a company name to prevent namespaces from different companies from having the same name.
- ✅ **DO** use a stable, version-independent product name at the second level of a namespace name.
- ❌ **DO NOT** use organizational hierarchies as the basis for names in namespace hierarchies, because group names within corporations tend to be short-lived. Organize the hierarchy of namespaces around groups of related technologies.
- ✅ **DO** use PascalCasing, and separate namespace components with periods (e.g., `Microsoft.Office.PowerPoint`). If your brand employs nontraditional casing, you should follow the casing defined by your brand, even if it deviates from normal namespace casing.
- 🟡 **CONSIDER** using plural namespace names where appropriate. — _For example, use `System.Collections` instead of `System.Collection`._
- ❌ **DO NOT** use the same name for a namespace and a type in that namespace. — _For example, do not use `Debug` as a namespace name and then also provide a class named `Debug` in the same namespace._

**Namespaces and Type Name Conflicts**

- ❌ **DO NOT** introduce generic type names such as `Element`, `Node`, `Log`, and `Message`. — _There is a very high probability that doing so will lead to type name conflicts in common scenarios._
- ❌ **DO NOT** give the same name to types in namespaces within a single application model. — _For example, do not add a type named `Page` to the `System.Web.UI.Adapters` namespace, because the `System.Web.UI` namespace already contains a type named `Page`._
- ❌ **DO NOT** give types names that would conflict with any type in the Core namespaces. — _For example, never use `Stream` as a type name._
- ❌ **DO NOT** assign type names that would conflict with other types within a single technology.
- ❌ **DO NOT** introduce type name conflicts between types in technology namespaces and an application model namespace (unless the technology is not intended to be used with the application model).

### Names of Classes, Structs, and Interfaces  (names-of-classes-structs-and-interfaces)

- ✅ **DO** name classes and structs with nouns or noun phrases, using PascalCasing. — _This distinguishes type names from methods, which are named with verb phrases._
- ✅ **DO** name interfaces with adjective phrases, or occasionally with nouns or noun phrases. — _Nouns and noun phrases should be used rarely and they might indicate that the type should be an abstract class, and not an interface._
- ❌ **DO NOT** give class names a prefix (e.g., "C").
- 🟡 **CONSIDER** ending the name of derived classes with the name of the base class. — _This is very readable and explains the relationship clearly._
- ✅ **DO** prefix interface names with the letter I, to indicate that the type is an interface. — _For example, `IComponent` (descriptive noun), `ICustomAttributeProvider` (noun phrase), and `IPersistable` (adjective) are appropriate interface names._
- ✅ **DO** ensure that the names differ only by the "I" prefix on the interface name when you are defining a class–interface pair where the class is a standard implementation of the interface.

**Names of Generic Type Parameters**

- ✅ **DO** name generic type parameters with descriptive names unless a single-letter name is completely self-explanatory and a descriptive name would not add value.
- 🟡 **CONSIDER** using `T` as the type parameter name for types with one single-letter type parameter. — _public int IComparer<T> { ..._
- ✅ **DO** prefix descriptive type parameter names with `T`. — _public interface ISessionChannel<TSession> where TSession : ISession {_
- 🟡 **CONSIDER** indicating constraints placed on a type parameter in the name of the parameter. — _For example, a parameter constrained to `ISession` might be called `TSession`._

**Names of Common Types**

- ✅ **DO** follow the guidelines described in the following table when naming types derived from or implementing certain .NET Framework types.

**Naming Enumerations**

- ✅ **DO** use a singular type name for an enumeration unless its values are bit fields.
- ✅ **DO** use a plural type name for an enumeration with bit fields as values, also called flags enum.
- ❌ **DO NOT** use an "Enum" suffix in enum type names.
- ❌ **DO NOT** use "Flag" or "Flags" suffixes in enum type names.
- ❌ **DO NOT** use a prefix on enumeration value names (e.g., "ad" for ADO enums, "rtf" for rich text enums, etc.).

### Names of Type Members  (names-of-type-members)

**Names of Methods**

- ✅ **DO** give methods names that are verbs or verb phrases. — _public class String {_

**Names of Properties**

- ✅ **DO** name properties using a noun, noun phrase, or adjective.
- ❌ **DO NOT** have properties that match the name of "Get" methods as in the following example: — _This pattern typically indicates that the property should really be a method._
- ✅ **DO** name collection properties with a plural phrase describing the items in the collection instead of using a singular phrase followed by "List" or "Collection".
- ✅ **DO** name Boolean properties with an affirmative phrase (`CanSeek` instead of `CantSeek`). Optionally, you can also prefix Boolean properties with "Is", "Can", or "Has", but only where it adds value.
- 🟡 **CONSIDER** giving a property the same name as its type. — _For example, the following property correctly gets and sets an enum value named `Color`, so the property is named `Color`:_

**Names of Events**

- ✅ **DO** name events with a verb or a verb phrase. — _Examples include `Clicked`, `Painting`, `DroppedDown`, and so on._
- ✅ **DO** give events names with a concept of before and after, using the present and past tenses. — _For example, a close event that is raised before a window is closed would be called `Closing`, and one that is raised after the window is closed would be called `Closed`._
- ❌ **DO NOT** use "Before" or "After" prefixes or postfixes to indicate pre- and post-events. Use present and past tenses as just described.
- ✅ **DO** name event handlers (delegates used as types of events) with the "EventHandler" suffix, as shown in the following example:
- ✅ **DO** use two parameters named `sender` and `e` in event handlers. — _The sender parameter represents the object that raised the event._
- ✅ **DO** name event argument classes with the "EventArgs" suffix.

**Names of Fields**

- ✅ **DO** use PascalCasing in field names.
- ✅ **DO** name fields using a noun, noun phrase, or adjective.
- ❌ **DO NOT** use a prefix for field names. — _For example, do not use "g_" or "s_" to indicate static fields._

### Naming Parameters  (naming-parameters)

- ✅ **DO** use camelCasing in parameter names.
- ✅ **DO** use descriptive parameter names.
- 🟡 **CONSIDER** using names based on a parameter’s meaning rather than the parameter’s type.

**Naming Operator Overload Parameters**

- ✅ **DO** use `left` and `right` for binary operator overload parameter names if there is no meaning to the parameters.
- ✅ **DO** use `value` for unary operator overload parameter names if there is no meaning to the parameters.
- 🟡 **CONSIDER** meaningful names for operator overload parameters if doing so adds significant value.
- ❌ **DO NOT** use abbreviations or numeric indices for operator overload parameter names.

### Naming Resources  (naming-resources)

- ✅ **DO** use PascalCasing in resource keys.
- ✅ **DO** provide descriptive rather than short identifiers.
- ❌ **DO NOT** use language-specific keywords of the main CLR languages.
- ✅ **DO** use only alphanumeric characters and underscores in naming resources.
- ✅ **DO** use the following naming convention for exception message resources. — _The resource identifier should be the exception type name plus a short identifier of the exception:_

## Type design

### Choosing Between Class and Struct  (choosing-between-class-and-struct)

- 🟡 **CONSIDER** defining a struct instead of a class if instances of the type are small and commonly short-lived or are commonly embedded in other objects.
- ⚠️ **AVOID** defining a struct unless the type has all of the following characteristics: — _In all other cases, you should define your types as classes._

### Abstract Class Design  (abstract-class)

- ❌ **DO NOT** define public or protected internal constructors in abstract types. — _Constructors should be public only if users will need to create instances of the type._
- ✅ **DO** define a protected or an internal constructor in abstract classes. — _A protected constructor is more common and simply allows the base class to do its own initialization when subtypes are created._
- ✅ **DO** provide at least one concrete type that inherits from each abstract class that you ship. — _Doing this helps to validate the design of the abstract class._

### Static Class Design  (static-class)

- ✅ **DO** use static classes sparingly. — _Static classes should be used only as supporting classes for the object-oriented core of the framework._
- ❌ **DO NOT** treat static classes as a miscellaneous bucket.
- ❌ **DO NOT** declare or override instance members in static classes.
- ✅ **DO** declare static classes as sealed, abstract, and add a private instance constructor if your programming language does not have built-in support for static classes.

### Interface Design  (interface)

- ✅ **DO** define an interface if you need some common API to be supported by a set of types that includes value types.
- 🟡 **CONSIDER** defining an interface if you need to support its functionality on types that already inherit from some other type.
- ⚠️ **AVOID** using marker interfaces (interfaces with no members). — _If you need to mark a class as having a specific characteristic (marker), in general, use a custom attribute rather than an interface._
- ✅ **DO** provide at least one type that is an implementation of an interface. — _Doing this helps to validate the design of the interface._
- ✅ **DO** provide at least one API that consumes each interface you define (a method taking the interface as a parameter or a property typed as the interface). — _Doing this helps to validate the interface design._
- ❌ **DO NOT** add members to an interface that has previously shipped. — _Doing so would break implementations of the interface._

### Struct Design  (struct)

- ❌ **DO NOT** provide a parameterless constructor for a struct. — _Following this guideline allows arrays of structs to be created without having to run the constructor on each item of the array._
- ❌ **DO NOT** define mutable value types. — _Mutable value types have several problems._
- ✅ **DO** ensure that a state where all instance data is set to zero, false, or null (as appropriate) is valid. — _This prevents accidental creation of invalid instances when an array of the structs is created._
- ✅ **DO** implement `System.IEquatable<T>` on value types. — _The `System.Object.Equals` method on value types causes boxing, and its default implementation is not very efficient, because it uses reflection._
- ❌ **DO NOT** explicitly extend `System.ValueType`. In fact, most languages prevent this. — _In general, structs can be very useful but should only be used for small, single, immutable values that will not be boxed frequently._

### Enum Design  (enum)

- ✅ **DO** use an enum to strongly type parameters, properties, and return values that represent sets of values.
- ✅ **DO** favor using an enum instead of static constants.
- ❌ **DO NOT** use an enum for open sets (such as the operating system version, names of your friends, etc.).
- ❌ **DO NOT** provide reserved enum values that are intended for future use. — _You can always simply add values to the existing enum at a later stage._
- ⚠️ **AVOID** publicly exposing enums with only one value. — _A common practice for ensuring future extensibility of C APIs is to add reserved parameters to method signatures._
- ❌ **DO NOT** include sentinel values in enums. — _Although they are sometimes helpful to framework developers, sentinel values are confusing to users of the framework._
- ✅ **DO** provide a value of zero on simple enums. — _Consider calling the value something like "None." If such a value is not appropriate for this particular enum, the most common default value for the enum should be assigned the underlying value of zero._
- 🟡 **CONSIDER** using `System.Int32` (the default in most programming languages) as the underlying type of an enum unless any of the following is true: — _For in-memory usage, be aware that managed objects are always `DWORD`-aligned, so you effectively need multiple enums or other small structures in an instance to pack a smaller enum with in order to make a difference, because the total instance size is always going to be rounded up to a `DWORD`._
- ✅ **DO** name flag enums with plural nouns or noun phrases and simple enums with singular nouns or noun phrases.
- ❌ **DO NOT** extend `System.Enum` directly.

**Designing Flag Enums**

- ✅ **DO** apply the `System.FlagsAttribute` to flag enums. Do not apply this attribute to simple enums.
- ✅ **DO** use powers of two for the flag enum values so they can be freely combined using the bitwise OR operation.
- 🟡 **CONSIDER** providing special enum values for commonly used combinations of flags. — _Bitwise operations are an advanced concept and should not be required for simple tasks._
- ⚠️ **AVOID** creating flag enums where certain combinations of values are invalid.
- ⚠️ **AVOID** using flag enum values of zero unless the value represents "all flags are cleared" and is named appropriately, as prescribed by the next guideline.
- ✅ **DO** name the zero value of flag enums `None`. For a flag enum, the value must always mean "all flags are cleared."

**Adding Value to Enums**

- 🟡 **CONSIDER** adding values to enums, despite a small compatibility risk. — _If you have real data about application incompatibilities caused by additions to an enum, consider adding a new API that returns the new and old values, and deprecate the old API, which should continue returning just the old values._

### Nested Types  (nested-types)

- ✅ **DO** use nested types when the relationship between the nested type and its outer type is such that member-accessibility semantics are desirable.
- ❌ **DO NOT** use public nested types as a logical grouping construct; use namespaces for this.
- ⚠️ **AVOID** publicly exposed nested types. The only exception to this is if variables of the nested type need to be declared only in rare scenarios such as subclassing or other advanced customization scenarios.
- ❌ **DO NOT** use nested types if the type is likely to be referenced outside of the containing type. — _For example, an enum passed to a method defined on a class should not be defined as a nested type in the class._
- ❌ **DO NOT** use nested types if they need to be instantiated by client code.  If a type has a public constructor, it should probably not be nested. — _If a type can be instantiated, that seems to indicate the type has a place in the framework on its own (you can create it, work with it, and destroy it without ever using the outer type), and thus should not be nested._
- ❌ **DO NOT** define a nested type as a member of an interface. Many languages do not support such a construct.

## Member design

### Member Overloading  (member-overloading)

- ✅ **DO** try to use descriptive parameter names to indicate the default used by shorter overloads.
- ⚠️ **AVOID** arbitrarily varying parameter names in overloads. If a parameter in one overload represents the same input as a parameter in another overload, the parameters should have the same name.
- ⚠️ **AVOID** being inconsistent in the ordering of parameters in overloaded members. Parameters with the same name should appear in the same position in all overloads.
- ✅ **DO** make only the longest overload virtual (if extensibility is required). Shorter overloads should simply call through to a longer overload.
- ❌ **DO NOT** use `ref` or `out` modifiers to overload members. — _Some languages cannot resolve calls to overloads like this._
- ❌ **DO NOT** have overloads with parameters at the same position and similar types yet with different semantics.
- ✅ **DO** allow `null` to be passed for optional arguments.
- ✅ **DO** use member overloading rather than defining members with default arguments. — _Default arguments are not CLS compliant._

### Property Design  (property)

- ✅ **DO** create get-only properties if the caller should not be able to change the value of the property. — _Keep in mind that if the type of the property is a mutable reference type, the property value can be changed even if the property is get-only._
- ❌ **DO NOT** provide set-only properties or properties with the setter having broader accessibility than the getter. — _For example, do not use properties with a public setter and a protected getter._
- ✅ **DO** provide sensible default values for all properties, ensuring that the defaults do not result in a security hole or terribly inefficient code.
- ✅ **DO** allow properties to be set in any order even if this results in a temporary invalid state of the object. — _It is common for two or more properties to be interrelated to a point where some values of one property might be invalid given the values of other properties on the same object._
- ✅ **DO** preserve the previous value if a property setter throws an exception.
- ⚠️ **AVOID** throwing exceptions from property getters. — _Property getters should be simple operations and should not have any preconditions._

**Indexed Property Design**

- 🟡 **CONSIDER** using indexers to provide access to data stored in an internal array.
- 🟡 **CONSIDER** providing indexers on types representing collections of items.
- ⚠️ **AVOID** using indexed properties with more than one parameter. — _If the design requires multiple parameters, reconsider whether the property really represents an accessor to a logical collection._
- ⚠️ **AVOID** indexers with parameter types other than `System.Int32`, `System.Int64`, `System.String`, `System.Object`, or an enum. — _If the design requires other types of parameters, strongly reevaluate whether the API really represents an accessor to a logical collection._
- ✅ **DO** use the name `Item` for indexed properties unless there is an obviously better name (e.g., see the `System.String.Chars` property on `System.String`). — _In C#, indexers are by default named Item._
- ❌ **DO NOT** provide both an indexer and methods that are semantically equivalent.
- ❌ **DO NOT** provide more than one family of overloaded indexers in one type. — _This is enforced by the C# compiler._
- ❌ **DO NOT** use nondefault indexed properties. — _This is enforced by the C# compiler._

**Property Change Notification Events**

- 🟡 **CONSIDER** raising change notification events when property values in high-level APIs (usually designer components) are modified. — _If there is a good scenario for a user to know when a property of an object is changing, the object should raise a change notification event for the property._
- 🟡 **CONSIDER** raising change notification events when the value of a property changes via external forces. — _If a property value changes via some external force (in a way other than by calling methods on the object), raise events indicate to the developer that the value is changing and has changed._

### Constructor Design  (constructor)

- 🟡 **CONSIDER** providing simple, ideally default, constructors. — _A simple constructor has a very small number of parameters, and all parameters are primitives or enums._
- 🟡 **CONSIDER** using a static factory method instead of a constructor if the semantics of the desired operation do not map directly to the construction of a new instance, or if following the constructor design guidelines feels unnatural.
- ✅ **DO** use constructor parameters as shortcuts for setting main properties. — _There should be no difference in semantics between using the empty constructor followed by some property sets and using a constructor with multiple arguments._
- ✅ **DO** use the same name for constructor parameters and a property if the constructor parameters are used to simply set the property. — _The only difference between such parameters and the properties should be casing._
- ✅ **DO** minimal work in the constructor. — _Constructors should not do much work other than capture the constructor parameters._
- ✅ **DO** throw exceptions from instance constructors, if appropriate.
- ✅ **DO** explicitly declare the public parameterless constructor in classes, if such a constructor is required. — _If you don’t explicitly declare any constructors on a type, many languages (such as C#) will automatically add a public parameterless constructor._
- ⚠️ **AVOID** explicitly defining parameterless constructors on structs. — _This makes array creation faster, because if the parameterless constructor is not defined, it does not have to be run on every slot in the array._
- ⚠️ **AVOID** calling virtual members on an object inside its constructor. — _Calling a virtual member will cause the most derived override to be called, even if the constructor of the most derived type has not been fully run yet._

**Type Constructor Guidelines**

- ✅ **DO** make static constructors private. — _A static constructor, also called a class constructor, is used to initialize a type._
- ❌ **DO NOT** throw exceptions from static constructors. — _If an exception is thrown from a type constructor, the type is not usable in the current application domain._
- 🟡 **CONSIDER** initializing static fields inline rather than explicitly using static constructors, because the runtime is able to optimize the performance of types that don’t have an explicitly defined static constructor.

### Event Design  (event)

- ✅ **DO** use the term "raise" for events rather than "fire" or "trigger."
- ✅ **DO** use `System.EventHandler<T>` instead of manually creating new delegates to be used as event handlers.
- 🟡 **CONSIDER** using a subclass of `System.EventArgs` as the event argument, unless you are absolutely sure the event will never need to carry any data to the event handling method, in which case you can use the `EventArgs` type directly. — _If you ship an API using `EventArgs` directly, you will never be able to add any data to be carried with the event without breaking compatibility._
- ✅ **DO** use a protected virtual method to raise each event. This is only applicable to nonstatic events on unsealed classes, not to structs, sealed classes, or static events. — _The purpose of the method is to provide a way for a derived class to handle the event using an override._
- ✅ **DO** take one parameter to the protected method that raises an event. — _The parameter should be named `e` and should be typed as the event argument class._
- ❌ **DO NOT** pass null as the sender when raising a nonstatic event.
- ✅ **DO** pass null as the sender when raising a static event.
- ❌ **DO NOT** pass null as the event data parameter when raising an event. — _You should pass `EventArgs.Empty` if you don’t want to pass any data to the event handling method._
- 🟡 **CONSIDER** raising events that the end user can cancel. This only applies to pre-events. — _Use `System.ComponentModel.CancelEventArgs` or its subclass as the event argument to allow the end user to cancel events._

**Custom Event Handler Design**

- ✅ **DO** use a return type of void for event handlers. — _An event handler can invoke multiple event handling methods, possibly on multiple objects._
- ✅ **DO** use `object` as the type of the first parameter of the event handler, and call it `sender`.
- ✅ **DO** use `System.EventArgs` or its subclass as the type of the second parameter of the event handler, and call it `e`.
- ❌ **DO NOT** have more than two parameters on event handlers.

### Field Design  (field)

- ❌ **DO NOT** provide instance fields that are public or protected. — _You should provide properties for accessing fields instead of making them public or protected._
- ✅ **DO** use constant fields for constants that will never change. — _The compiler burns the values of const fields directly into calling code._
- ✅ **DO** use public static `readonly` fields for predefined object instances. — _If there are predefined instances of the type, declare them as public read-only static fields of the type itself._
- ❌ **DO NOT** assign instances of mutable types to `readonly` fields. — _A mutable type is a type with instances that can be modified after they are instantiated._

### Extension Methods  (extension-methods)

- ⚠️ **AVOID** frivolously defining extension methods, especially on types you don't own. — _If you do own source code of a type, consider using regular instance methods instead._
- 🟡 **CONSIDER** using extension methods in any of the following scenarios:
- ⚠️ **AVOID** defining extension methods on `System.Object`. — _VB users will not be able to call such methods on object references using the extension method syntax._
- ❌ **DO NOT** put extension methods in the same namespace as the extended type unless it is for adding methods to interfaces or for dependency management.
- ⚠️ **AVOID** defining two or more extension methods with the same signature, even if they reside in different namespaces.
- 🟡 **CONSIDER** defining extension methods in the same namespace as the extended type if the type is an interface and if the extension methods are meant to be used in most or all cases.
- ❌ **DO NOT** define extension methods implementing a feature in namespaces normally associated with other features. Instead, define them in the namespace associated with the feature they belong to.
- ⚠️ **AVOID** generic naming of namespaces dedicated to extension methods (e.g., "Extensions"). Use a descriptive name (e.g., "Routing") instead.

### Operator Overloads  (operator-overloads)

- ⚠️ **AVOID** defining operator overloads, except in types that should feel like primitive (built-in) types.
- 🟡 **CONSIDER** defining operator overloads in a type that should feel like a primitive type. — _For example, `System.String` has `operator==` and `operator!=` defined._
- ✅ **DO** define operator overloads in structs that represent numbers (such as `System.Decimal`).
- ❌ **DO NOT** be cute when defining operator overloads. — _Operator overloading is useful in cases in which it is immediately obvious what the result of the operation will be._
- ❌ **DO NOT** provide operator overloads unless at least one of the operands is of the type defining the overload.
- ✅ **DO** overload operators in a symmetric fashion. — _For example, if you overload the `operator==`, you should also overload the `operator!=`._
- 🟡 **CONSIDER** providing methods with friendly names that correspond to each overloaded operator. — _Many languages do not support operator overloading._

**Overloading Operator ==**

**Conversion Operators**

- ❌ **DO NOT** provide a conversion operator if such conversion is not clearly expected by the end users.
- ❌ **DO NOT** define conversion operators outside of a type’s domain. — _For example, `System.Int32`, `System.Double`, and `System.Decimal` are all numeric types, whereas `System.DateTime` is not._
- ❌ **DO NOT** provide an implicit conversion operator if the conversion is potentially lossy. — _For example, there should not be an implicit conversion from `Double` to `Int32` because `Double` has a wider range than `Int32`._
- ❌ **DO NOT** throw exceptions from implicit casts. — _It is very difficult for end users to understand what is happening, because they might not be aware that a conversion is taking place._
- ✅ **DO** throw `System.InvalidCastException` if a call to a cast operator results in a lossy conversion and the contract of the operator does not allow lossy conversions.

### Parameter Design  (parameter-design)

- ✅ **DO** use the least derived parameter type that provides the functionality required by the member. — _For example, suppose you want to design a method that enumerates a collection and prints each item to the console._
- ❌ **DO NOT** use reserved parameters. — _If more input to a member is needed in some future version, a new overload can be added._
- ❌ **DO NOT** have publicly exposed methods that take pointers, arrays of pointers, or multidimensional arrays as parameters. — _Pointers and multidimensional arrays are relatively difficult to use properly._
- ✅ **DO** place all `out` parameters following all of the by-value and `ref` parameters (excluding parameter arrays), even if it results in an inconsistency in parameter ordering between overloads (see Member Overloading). — _The `out` parameters can be seen as extra return values, and grouping them together makes the method signature easier to understand._
- ✅ **DO** be consistent in naming parameters when overriding members or implementing interface members. — _This better communicates the relationship between the methods._

**Choosing Between Enum and Boolean Parameters**

- ✅ **DO** use enums if a member would otherwise have two or more Boolean parameters.
- ❌ **DO NOT** use Booleans unless you are absolutely sure there will never be a need for more than two values. — _Enums give you some room for future addition of values, but you should be aware of all the implications of adding values to enums, which are described in Enum Design._
- 🟡 **CONSIDER** using Booleans for constructor parameters that are truly two-state values and are simply used to initialize Boolean properties.

**Validating Arguments**

- ✅ **DO** validate arguments passed to public, protected, or explicitly implemented members. Throw `System.ArgumentException`, or one of its subclasses, if the validation fails.
- ✅ **DO** throw `System.ArgumentNullException` if a null argument is passed and the member does not support null arguments.
- ✅ **DO** validate enum parameters. — _Do not assume enum arguments will be in the range defined by the enum._
- ❌ **DO NOT** use `System.Enum.IsDefined` for enum range checks.
- ✅ **DO** be aware that mutable arguments might have changed after they were validated. — _If the member is security sensitive, you are encouraged to make a copy and then validate and process the argument._

**Parameter Passing**

- ⚠️ **AVOID** using `out` or `ref` parameters. — _Using `out` or `ref` parameters requires experience with pointers, understanding how value types and reference types differ, and handling methods with multiple return values._
- ❌ **DO NOT** pass reference types by reference. — _There are some limited exceptions to the rule, such as a method that can be used to swap references._

**Members with Variable Number of Parameters**

- 🟡 **CONSIDER** adding the params keyword to array parameters if you expect the end users to pass arrays with a small number of elements. If it's expected that lots of elements will be passed in common scenarios, users will probably not pass these elements inline anyway, and so the params keyword is not necessary.
- ⚠️ **AVOID** using params arrays if the caller would almost always have the input already in an array. — _For example, members with byte array parameters would almost never be called by passing individual bytes._
- ❌ **DO NOT** use params arrays if the array is modified by the member taking the params array parameter. — _Because of the fact that many compilers turn the arguments to the member into a temporary array at the call site, the array might be a temporary object, and therefore any modifications to the array will be lost._
- 🟡 **CONSIDER** using the params keyword in a simple overload, even if a more complex overload could not use it. — _Ask yourself if users would value having the params array in one overload even if it wasn't in all overloads._
- ✅ **DO** try to order parameters to make it possible to use the params keyword.
- 🟡 **CONSIDER** providing special overloads and code paths for calls with a small number of arguments in extremely performance-sensitive APIs. — _This makes it possible to avoid creating array objects when the API is called with a small number of arguments._
- ✅ **DO** be aware that null could be passed as a params array argument. — _You should validate that the array is not null before processing._
- ❌ **DO NOT** use the `varargs` methods, otherwise known as the ellipsis. — _Some CLR languages, such as C++, support an alternative convention for passing variable parameter lists called `varargs` methods._

**Pointer Parameters**

- ✅ **DO** provide an alternative for any member that takes a pointer argument, because pointers are not CLS-compliant.
- ⚠️ **AVOID** doing expensive argument checking of pointer arguments.
- ✅ **DO** follow common pointer-related conventions when designing members with pointers. — _For example, there is no need to pass the start index, because simple pointer arithmetic can be used to accomplish the same result._

## Designing for extensibility

### Unsealed Classes  (unsealed-classes)

- 🟡 **CONSIDER** using unsealed classes with no added virtual or protected members as a great way to provide inexpensive yet much appreciated extensibility to a framework. — _Developers often want to inherit from unsealed classes so as to add convenience members such as custom constructors, new methods, or method overloads._

### Protected Members  (protected-members)

- 🟡 **CONSIDER** using protected members for advanced customization.
- ✅ **DO** treat protected members in unsealed classes as public for the purpose of security, documentation, and compatibility analysis. — _Anyone can inherit from a class and access the protected members._

### Events and Callbacks  (events-and-callbacks)

- 🟡 **CONSIDER** using callbacks to allow users to provide custom code to be executed by the framework.
- 🟡 **CONSIDER** using events to allow users to customize the behavior of a framework without the need for understanding object-oriented design.
- ✅ **DO** prefer events over plain callbacks, because they are more familiar to a broader range of developers and are integrated with Visual Studio statement completion.
- ⚠️ **AVOID** using callbacks in performance-sensitive APIs.
- ✅ **DO** use the new `Func<...>`, `Action<...>`, or `Expression<...>` types instead of custom delegates, when defining APIs with callbacks.
- ✅ **DO** measure and understand performance implications of using `Expression<...>`, instead of using `Func<...>` and `Action<...>` delegates.
- ✅ **DO** understand that by calling a delegate, you are executing arbitrary code and that could have security, correctness, and compatibility repercussions.

### Virtual Members  (virtual-members)

- ❌ **DO NOT** make members virtual unless you have a good reason to do so and you are aware of all the costs related to designing, testing, and maintaining virtual members. — _Virtual members are less forgiving in terms of changes that can be made to them without breaking compatibility._
- 🟡 **CONSIDER** limiting extensibility to only what is absolutely necessary.
- ✅ **DO** prefer protected accessibility over public accessibility for virtual members. Public members should provide extensibility (if required) by calling into a protected virtual member. — _The public members of a class should provide the right set of functionality for direct consumers of that class._

### Abstractions (Abstract Types and Interfaces)  (abstractions-abstract-types-and-interfaces)

- ❌ **DO NOT** provide abstractions unless they are tested by developing several concrete implementations and APIs consuming the abstractions.
- ✅ **DO** choose carefully between an abstract class and an interface when designing an abstraction.
- 🟡 **CONSIDER** providing reference tests for concrete implementations of abstractions. Such tests should allow users to test whether their implementations correctly implement the contract.

### Base Classes for Implementing Abstractions  (base-classes-for-implementing-abstractions)

- 🟡 **CONSIDER** making base classes abstract even if they don’t contain any abstract members. This clearly communicates to the users that the class is designed solely to be inherited from.
- 🟡 **CONSIDER** placing base classes in a separate namespace from the mainline scenario types. By definition, base classes are intended for advanced extensibility scenarios and therefore are not interesting to the majority of users.
- ⚠️ **AVOID** naming base classes with a "Base" suffix if the class is intended for use in public APIs.

### Sealing  (sealing)

- ❌ **DO NOT** seal classes without having a good reason to do so. — _Sealing a class because you cannot think of an extensibility scenario is not a good reason._
- ❌ **DO NOT** declare protected or virtual members on sealed types. — _By definition, sealed types cannot be inherited from._
- 🟡 **CONSIDER** sealing members that you override. — _Problems that can result from introducing virtual members (discussed in Virtual Members) apply to overrides as well, although to a slightly lesser degree._

## Exceptions

### Exception Throwing  (exception-throwing)

- ❌ **DO NOT** return error codes. — _Exceptions are the primary means of reporting errors in frameworks._
- ✅ **DO** report execution failures by throwing exceptions.
- 🟡 **CONSIDER** terminating the process by calling `System.Environment.FailFast` (.NET Framework 2.0 feature) instead of throwing an exception if your code encounters a situation where it is unsafe for further execution.
- ❌ **DO NOT** use exceptions for the normal flow of control, if possible. — _Except for system failures and operations with potential race conditions, framework designers should design APIs so users can write code that does not throw exceptions._
- 🟡 **CONSIDER** the performance implications of throwing exceptions. Throw rates above 100 per second are likely to noticeably impact the performance of most applications.
- ✅ **DO** document all exceptions thrown by publicly callable members because of a violation of the member contract (rather than a system failure) and treat them as part of your contract. — _Exceptions that are a part of the contract should not change from one version to the next (i.e., exception type should not change, and new exceptions should not be added)._
- ❌ **DO NOT** have public members that can either throw or not based on some option.
- ❌ **DO NOT** have public members that return exceptions as the return value or an `out` parameter. — _Returning exceptions from public APIs instead of throwing them defeats many of the benefits of exception-based error reporting._
- 🟡 **CONSIDER** using exception builder methods. — _It is common to throw the same exception from different places._
- ❌ **DO NOT** throw exceptions from exception filter blocks. — _When an exception filter raises an exception, the exception is caught by the CLR, and the filter returns false._
- ⚠️ **AVOID** explicitly throwing exceptions from finally blocks. Implicitly thrown exceptions resulting from calling methods that throw are acceptable.

### Using Standard Exception Types  (using-standard-exception-types)

**Exception and SystemException**

- ❌ **DO NOT** throw `System.Exception` or `System.SystemException`.
- ❌ **DO NOT** catch `System.Exception` or `System.SystemException` in framework code, unless you intend to rethrow.
- ⚠️ **AVOID** catching `System.Exception` or `System.SystemException`, except in top-level exception handlers.

**ApplicationException**

- ❌ **DO NOT** throw or derive from `System.ApplicationException`.

**InvalidOperationException**

- ✅ **DO** throw an `System.InvalidOperationException` if the object is in an inappropriate state.

**ArgumentException, ArgumentNullException, and ArgumentOutOfRangeException**

- ✅ **DO** throw `System.ArgumentException` or one of its subtypes if bad arguments are passed to a member. Prefer the most derived exception type, if applicable.
- ✅ **DO** set the `ParamName` property when throwing one of the subclasses of `ArgumentException`.
- ✅ **DO** use `value` for the name of the implicit value parameter of property setters.

**NullReferenceException, IndexOutOfRangeException, and AccessViolationException**

- ❌ **DO NOT** allow publicly callable APIs to explicitly or implicitly throw `System.NullReferenceException`, `System.AccessViolationException`, or `System.IndexOutOfRangeException`. These exceptions are reserved and thrown by the execution engine and in most cases indicate a bug. — _Do argument checking to avoid throwing these exceptions._

**StackOverflowException**

- ❌ **DO NOT** explicitly throw `System.StackOverflowException`. The exception should be explicitly thrown only by the CLR.
- ❌ **DO NOT** catch `StackOverflowException`. — _It is almost impossible to write managed code that remains consistent in the presence of arbitrary stack overflows._

**OutOfMemoryException**

- ❌ **DO NOT** explicitly throw `System.OutOfMemoryException`. This exception is to be thrown only by the CLR infrastructure.

**ComException, SEHException, and ExecutionEngineException**

- ❌ **DO NOT** explicitly throw `System.Runtime.InteropServices.COMException`,  `System.ExecutionEngineException`, and `System.Runtime.InteropServices.SEHException`. These exceptions are to be thrown only by the CLR infrastructure.

### Exceptions and Performance  (exceptions-and-performance)

- ❌ **DO NOT** use error codes because of concerns that exceptions might affect performance negatively. — _To improve performance, it is possible to use either the Tester-Doer Pattern or the Try-Parse Pattern, described in the next two sections._

**Tester-Doer Pattern**

- 🟡 **CONSIDER** the Tester-Doer Pattern for members that might throw exceptions in common scenarios to avoid performance problems related to exceptions.

**Try-Parse Pattern**

- 🟡 **CONSIDER** the Try-Parse Pattern for members that might throw exceptions in common scenarios to avoid performance problems related to exceptions.
- ✅ **DO** use the prefix "Try" and Boolean return type for methods implementing this pattern.
- ✅ **DO** provide an exception-throwing member for each member using the Try-Parse Pattern.

## Usage

### Arrays  (arrays)

- ✅ **DO** prefer using collections over arrays in public APIs. The Collections section provides details about how to choose between collections and arrays.
- ❌ **DO NOT** use read-only array fields. The field itself is read-only and can't be changed, but elements in the array can be changed.
- 🟡 **CONSIDER** using jagged arrays instead of multidimensional arrays. — _A jagged array is an array with elements that are also arrays._

### Attributes  (attributes)

- ✅ **DO** name custom attribute classes with the suffix "Attribute."
- ✅ **DO** apply the `System.AttributeUsageAttribute` to custom attributes.
- ✅ **DO** provide settable properties for optional arguments.
- ✅ **DO** provide get-only properties for required arguments.
- ✅ **DO** provide constructor parameters to initialize properties corresponding to required arguments. Each parameter should have the same name (although with different casing) as the corresponding property.
- ⚠️ **AVOID** providing constructor parameters to initialize properties corresponding to the optional arguments.
- ⚠️ **AVOID** overloading custom attribute constructors. — _Having only one constructor clearly communicates to the user which arguments are required and which are optional._
- ✅ **DO** seal custom attribute classes, if possible. This makes the look-up for the attribute faster.

### Guidelines for Collections  (guidelines-for-collections)

- ❌ **DO NOT** use weakly typed collections in public APIs. — _The type of all return values and parameters representing collection items should be the exact item type, not any of its base types (this applies only to public members of the collection)._
- ❌ **DO NOT** use `System.Collections.ArrayList` or `System.Collections.Generic.List<T>` in public APIs. — _These types are data structures designed to be used in internal implementation, not in public APIs._
- ❌ **DO NOT** use `Hashtable` or `Dictionary<TKey,TValue>` in public APIs. — _These types are data structures designed to be used in internal implementation._
- ❌ **DO NOT** use `System.Collections.Generic.IEnumerator<T>`, `System.Collections.IEnumerator`, or any other type that implements either of these interfaces, except as the return type of a `GetEnumerator` method. — _Types returning enumerators from methods other than `GetEnumerator` cannot be used with the `foreach` statement._
- ❌ **DO NOT** implement both `IEnumerator<T>` and `IEnumerable<T>` on the same type. The same applies to the nongeneric interfaces `IEnumerator` and `IEnumerable`.

**Collection Parameters**

- ✅ **DO** use the least-specialized type possible as a parameter type. Most members taking collections as parameters use the `IEnumerable<T>` interface.
- ⚠️ **AVOID** using `System.Collections.Generic.ICollection<T>` or `System.Collections.ICollection` as a parameter just to access the `Count` property. — _Instead, consider using `IEnumerable<T>` or `IEnumerable` and dynamically checking whether the object implements `ICollection<T>` or `ICollection`._

**Collection Properties and Return Values**

- ❌ **DO NOT** provide settable collection properties. — _Users can replace the contents of the collection by clearing the collection first and then adding the new contents._
- ✅ **DO** use `Collection<T>` or a subclass of `Collection<T>` for properties or return values representing read/write collections. — _If `Collection<T>` does not meet some requirement (e.g., the collection must not implement `System.Collections.IList`), use a custom collection by implementing `IEnumerable<T>`, `ICollection<T>`, or `System.Collections.Generic.IList<T>`._
- ✅ **DO** use `System.Collections.ObjectModel.ReadOnlyCollection<T>`, a subclass of `ReadOnlyCollection<T>`, or in rare cases `IEnumerable<T>` for properties or return values representing read-only collections. — _In general, prefer `ReadOnlyCollection<T>`._
- 🟡 **CONSIDER** using subclasses of generic base collections instead of using the collections directly. — _This allows for a better name and for adding helper members that are not present on the base collection types._
- 🟡 **CONSIDER** returning a subclass of `Collection<T>` or `ReadOnlyCollection<T>` from very commonly used methods and properties. — _This will make it possible for you to add helper methods or change the collection implementation in the future._
- 🟡 **CONSIDER** using a keyed collection if the items stored in the collection have unique keys (names, IDs, etc.). Keyed collections are collections that can be indexed by both an integer and a key and are usually implemented by inheriting from `KeyedCollection<TKey,TItem>`. — _Keyed collections usually have larger memory footprints and should not be used if the memory overhead outweighs the benefits of having the keys._
- ❌ **DO NOT** return null values from collection properties or from methods returning collections. Return an empty collection or an empty array instead. — _The general rule is that null and empty (0 item) collections or arrays should be treated the same._

**Snapshots Versus Live Collections**

- ❌ **DO NOT** return snapshot collections from properties. Properties should return live collections. — _Property getters should be very lightweight operations._
- ✅ **DO** use either a snapshot collection or a live `IEnumerable<T>` (or its subtype) to represent collections that are volatile (i.e., that can change without explicitly modifying the collection). — _In general, all collections representing a shared resource (e.g., files in a directory) are volatile._

**Choosing Between Arrays and Collections**

- ✅ **DO** prefer collections over arrays. — _Collections provide more control over contents, can evolve over time, and are more usable._
- 🟡 **CONSIDER** using arrays in low-level APIs to minimize memory consumption and maximize performance.
- ✅ **DO** use byte arrays instead of collections of bytes.
- ❌ **DO NOT** use arrays for properties if the property would have to return a new array (e.g., a copy of an internal array) every time the property getter is called.

**Implementing Custom Collections**

- 🟡 **CONSIDER** inheriting from `Collection<T>`, `ReadOnlyCollection<T>`, or `KeyedCollection<TKey,TItem>` when designing new collections.
- ✅ **DO** implement `IEnumerable<T>` when designing new collections. Consider implementing `ICollection<T>` or even `IList<T>` where it makes sense. — _When implementing such custom collection, follow the API pattern established by `Collection<T>` and `ReadOnlyCollection<T>` as closely as possible._
- 🟡 **CONSIDER** implementing nongeneric collection interfaces (`IList` and `ICollection`) if the collection will often be passed to APIs taking these interfaces as input.
- ⚠️ **AVOID** implementing collection interfaces on types with complex APIs unrelated to the concept of a collection.
- ❌ **DO NOT** inherit from nongeneric base collections such as `CollectionBase`. Use `Collection<T>`, `ReadOnlyCollection<T>`, and `KeyedCollection<TKey,TItem>` instead.

**Naming Custom Collections**

- ✅ **DO** use the "Dictionary" suffix in names of abstractions implementing `IDictionary` or `IDictionary<TKey,TValue>`.
- ✅ **DO** use the "Collection" suffix in names of types implementing `IEnumerable` (or any of its descendants) and representing a list of items.
- ✅ **DO** use the appropriate data structure name for custom data structures.
- ⚠️ **AVOID** using any suffixes implying particular implementation, such as "LinkedList" or "Hashtable," in names of collection abstractions.
- 🟡 **CONSIDER** prefixing collection names with the name of the item type. For example, a collection storing items of type `Address` (implementing `IEnumerable<Address>`) should be named `AddressCollection`. If the item type is an interface, the "I" prefix of the item type can be omitted. Thus, a collection of `System.IDisposable` items can be called `DisposableCollection`.
- 🟡 **CONSIDER** using the "ReadOnly" prefix in names of read-only collections if a corresponding writeable collection might be added or already exists in the framework. — _For example, a read-only collection of strings should be called `ReadOnlyStringCollection`._

### Serialization  (serialization)

- ✅ **DO** think about serialization when you design new types.

**Choosing the Right Serialization Technology to Support**

- 🟡 **CONSIDER** supporting Data Contract Serialization if instances of your type might need to be persisted or used in Web Services.
- 🟡 **CONSIDER** supporting the XML Serialization instead of or in addition to Data Contract Serialization if you need more control over the XML format that is produced when the type is serialized. — _This may be necessary in some interoperability scenarios where you need to use an XML construct that is not supported by Data Contract Serialization, for example, to produce XML attributes._
- 🟡 **CONSIDER** supporting the Runtime Serialization if instances of your type need to travel across .NET Remoting boundaries.
- ⚠️ **AVOID** supporting Runtime Serialization or XML Serialization just for general persistence reasons. Prefer Data Contract Serialization instead.

**Supporting Data Contract Serialization**

- 🟡 **CONSIDER** marking data members of your type public if the type can be used in partial trust. — _In full trust, Data Contract serializers can serialize and deserialize nonpublic types and members, but only public members can be serialized and deserialized in partial trust._
- ✅ **DO** implement a getter and setter on all properties that have `System.Runtime.Serialization.DataMemberAttribute`. Data Contract serializers require both the getter and the setter for the type to be considered serializable. (In .NET Framework 3.5 SP1, some collection properties can be get-only.) If the type won’t be used in partial trust, one or both of the property accessors can be nonpublic.
- 🟡 **CONSIDER** using the serialization callbacks for initialization of deserialized instances. — _Constructors are not called when objects are deserialized._
- 🟡 **CONSIDER** using the `System.Runtime.Serialization.KnownTypeAttribute` to indicate concrete types that should be used when deserializing a complex object graph.
- ✅ **DO** consider backward and forward compatibility when creating or changing serializable types. — _Keep in mind that serialized streams of future versions of your type can be deserialized into the current version of the type, and vice versa._
- 🟡 **CONSIDER** implementing `System.Runtime.Serialization.IExtensibleDataObject` to allow round-tripping between different versions of the type. — _The interface allows the serializer to ensure that no data is lost during round-tripping._

**Supporting XML Serialization**

- ⚠️ **AVOID** designing your types specifically for XML Serialization, unless you have a very strong reason to control the shape of the XML produced. This serialization technology has been superseded by the Data Contract Serialization discussed in the previous section.
- 🟡 **CONSIDER** implementing the `System.Xml.Serialization.IXmlSerializable` interface if you want even more control over the shape of the serialized XML than what’s offered by applying the XML Serialization attributes. Two methods of the interface, `System.Xml.Serialization.IXmlSerializable.ReadXml` and `System.Xml.Serialization.IXmlSerializable.WriteXml`, allow you to fully control the serialized XML stream. You can also control the XML schema that gets generated for the type by applying the `XmlSchemaProviderAttribute`.

**Supporting Runtime Serialization**

- 🟡 **CONSIDER** supporting Runtime Serialization if your types will be used with .NET Remoting. For example, the `System.AddIn` namespace uses .NET Remoting, and so all types exchanged between `System.AddIn` add-ins need to support Runtime Serialization.
- 🟡 **CONSIDER** implementing the Runtime Serializable Pattern if you want complete control over the serialization process. For example, if you want to transform data as it gets serialized or deserialized. — _The pattern is very simple._
- ✅ **DO** make the serialization constructor protected and provide two parameters typed and named exactly as shown in the sample here. — _[Serializable]_
- ✅ **DO** implement the `System.Runtime.Serialization.ISerializable` members explicitly.
- ✅ **DO** apply a link demand to `System.Runtime.Serialization.ISerializable.GetObjectData` implementation. This ensures that only fully trusted core and the Runtime Serializer have access to the member.

### System.Xml Usage  (system-xml-usage)

- ❌ **DO NOT** use `System.Xml.XmlNode` or `System.Xml.XmlDocument` to represent XML data. Favor using instances of `System.Xml.XPath.IXPathNavigable`, `System.Xml.XmlReader`, `System.Xml.XmlWriter`, or subtypes of `System.Xml.Linq.XNode` instead. `XmlNode` and `XmlDocument` are not designed for exposing in public APIs.
- ✅ **DO** use `XmlReader`, `IXPathNavigable`, or subtypes of `XNode` as input or output of members that accept or return XML. — _Use these abstractions instead of `XmlDocument`, `XmlNode`, or `System.Xml.XPath.XPathDocument`, because this decouples the methods from specific implementations of an in-memory XML document and allows them to work with virtual XML data sources that expose `XNode`, `XmlReader`, or `System.Xml.XPath.XPathNavigator`._
- ❌ **DO NOT** subclass `XmlDocument` if you want to create a type representing an XML view of an underlying object model or data source.

### Equality Operators  (equality-operators)

- ❌ **DO NOT** overload one of the equality operators and not the other.
- ✅ **DO** ensure that `System.Object.Equals` and the equality operators have exactly the same semantics and similar performance characteristics. — _This often means that `Object.Equals` needs to be overridden when the equality operators are overloaded._
- ⚠️ **AVOID** throwing exceptions from equality operators. — _For example, return false if one of the arguments is null instead of throwing `NullReferenceException`._

**Equality Operators on Value Types**

- ✅ **DO** overload the equality operators on value types, if equality is meaningful. — _In most programming languages, there is no default implementation of `operator==` for value types._

**Equality Operators on Reference Types**

- ⚠️ **AVOID** overloading equality operators on mutable reference types. — _Many languages have built-in equality operators for reference types._
- ⚠️ **AVOID** overloading equality operators on reference types if the implementation would be significantly slower than that of reference equality.

## Common design patterns

### Dependency Properties  (dependency-properties)

- ✅ **DO** provide the dependency properties, if you need the properties to support WPF features such as styling, triggers, data binding, animations, dynamic resources, and inheritance.

**Dependency Property Design**

- ✅ **DO** inherit from `System.Windows.DependencyObject`, or one of its subtypes, when implementing dependency properties. The type provides a very efficient implementation of a property store and automatically supports WPF data binding.
- ✅ **DO** provide a regular CLR property and public static read-only field storing an instance of `System.Windows.DependencyProperty` for each dependency property.
- ✅ **DO** implement dependency properties by calling instance methods `System.Windows.DependencyObject.GetValue` and `System.Windows.DependencyObject.SetValue`.
- ✅ **DO** name the dependency property static field by suffixing the name of the property with "Property."
- ❌ **DO NOT** set default values of dependency properties explicitly in code; set them in metadata instead. — _If you set a property default explicitly, you might prevent that property from being set by some implicit means, such as a styling._
- ❌ **DO NOT** put code in the property accessors other than the standard code to access the static field. — _That code won’t execute if the property is set by implicit means, such as a styling, because styling uses the static field directly._
- ❌ **DO NOT** use dependency properties to store secure data. Even private dependency properties can be accessed publicly.

**Attached Dependency Property Design**

**Dependency Property Validation**

- ❌ **DO NOT** put dependency property validation logic in the property’s accessors. Instead, pass a validation callback to `DependencyProperty.Register` method.

**Dependency Property Change Notifications**

- ❌ **DO NOT** implement change notification logic in dependency property accessors. Dependency properties have a built-in change notifications feature that must be used by supplying a change notification callback to the `System.Windows.PropertyMetadata`.

**Dependency Property Value Coercion**

- ❌ **DO NOT** implement coercion logic in dependency property accessors. — _Dependency properties have a built-in coercion feature, and it can be used by supplying a coercion callback to the `PropertyMetadata`._

### Dispose Pattern  (dispose-pattern)

- ✅ **DO** implement the Basic Dispose Pattern on types containing instances of disposable types. See the Basic Dispose Pattern section for details on the basic pattern. — _If a type is responsible for the lifetime of other disposable objects, developers need a way to dispose of them, too._
- ✅ **DO** implement the Basic Dispose Pattern and provide a finalizer on types holding resources that need to be freed explicitly and that do not have finalizers. — _For example, the pattern should be implemented on types storing unmanaged memory buffers._
- 🟡 **CONSIDER** implementing the Basic Dispose Pattern on classes that themselves don’t hold unmanaged resources or disposable objects but are likely to have subtypes that do. — _A great example of this is the `System.IO.Stream` class._

**Basic Dispose Pattern**

- ✅ **DO** declare a `protected virtual void Dispose(bool disposing)` method to centralize all logic related to releasing unmanaged resources. — _All resource cleanup should occur in this method._
- ✅ **DO** implement the `IDisposable` interface by simply calling `Dispose(true)` followed by `GC.SuppressFinalize(this)`. — _The call to `SuppressFinalize` should only occur if `Dispose(true)` executes successfully._
- ❌ **DO NOT** make the parameterless `Dispose` method virtual. — _The `Dispose(bool)` method is the one that should be overridden by subclasses._
- ❌ **DO NOT** declare any overloads of the `Dispose` method other than `Dispose()` and `Dispose(bool)`.
- ✅ **DO** allow the `Dispose(bool)` method to be called more than once. The method might choose to do nothing after the first call. — _public class DisposableResourceHolder : IDisposable {_
- ⚠️ **AVOID** throwing an exception from within `Dispose(bool)` except under critical situations where the containing process has been corrupted (leaks, inconsistent shared state, etc.). — _Users expect that a call to `Dispose` will not raise an exception._
- ✅ **DO** throw an `System.ObjectDisposedException` from any member that cannot be used after the object has been disposed of. — _public class DisposableResourceHolder : IDisposable {_
- 🟡 **CONSIDER** providing method `Close()`, in addition to the `Dispose()`, if close is standard terminology in the area. — _When doing so, it is important that you make the `Close` implementation identical to `Dispose` and consider implementing the `IDisposable.Dispose` method explicitly._

**Finalizable Types**

- ⚠️ **AVOID** making types finalizable. — _Carefully consider any case in which you think a finalizer is needed._
- ❌ **DO NOT** make value types finalizable. — _Only reference types actually get finalized by the CLR, and thus any attempt to place a finalizer on a value type will be ignored._
- ✅ **DO** make a type finalizable if the type is responsible for releasing an unmanaged resource that does not have its own finalizer. — _When implementing the finalizer, simply call `Dispose(false)` and place all resource cleanup logic inside the `Dispose(bool disposing)` method._
- ✅ **DO** implement the Basic Dispose Pattern on every finalizable type. — _This gives users of the type a means to explicitly perform deterministic cleanup of those same resources for which the finalizer is responsible._
- ❌ **DO NOT** access any finalizable objects in the finalizer code path, because there is significant risk that they will have already been finalized. — _For example, a finalizable object A that has a reference to another finalizable object B cannot reliably use B in A’s finalizer, or vice versa._
- ✅ **DO** make your `Finalize` method protected. — _C#, C++, and VB.NET developers do not need to worry about this, because the compilers help to enforce this guideline._
- ❌ **DO NOT** let exceptions escape from the finalizer logic, except for system-critical failures. — _If an exception is thrown from a finalizer, the CLR will shut down the entire process (as of .NET Framework version 2.0), preventing other finalizers from executing and resources from being released in a controlled manner._
- 🟡 **CONSIDER** creating and using a critical finalizable object (a type with a type hierarchy that contains `System.Runtime.ConstrainedExecution.CriticalFinalizerObject`) for situations in which a finalizer absolutely must execute even in the face of forced application domain unloads and thread aborts.

