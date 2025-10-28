A helper library that I designed as a POC on former R&D projects (when learning to deal with .NET generic types) that I recently wanted to formalize.

The main goal of this library is to check whether a runtime type is a variant (including generic type-constraints checking) of a generic type or interface based on the Liskov-Wing substitution principle for cases that fail or fall outside of the native .NET `Type.IsAssignableFrom` method.

The use is somewhat limited (for instance you can not check against a typeof `IComparable<IEnumerable<>>` since .NET won't event allow to write something like this) but sugar-helps in some cases where the is no non-generic inheritance or interfaces (like `IComparable<>` for instance)
