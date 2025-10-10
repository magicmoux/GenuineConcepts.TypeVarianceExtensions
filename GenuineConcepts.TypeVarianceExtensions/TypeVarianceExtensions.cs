using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.TypeVarianceExtensions
{
    //TODO decide whether we return the most direct type in the inheritance chain or all matches
    // 
    /// <summary>
    /// From <a href="https://learn.microsoft.com/fr-fr/dotnet/api/system.type.getinterfaces?view=net-9.0">Type.GetInterfaces Method</a>
    /// <quote>
    /// In .NET 6 and earlier versions, the GetInterfaces method does not return interfaces in a particular order, such as alphabetical or declaration order. Your code must not depend on the order in which interfaces are returned, because that order varies. However, starting with .NET 7, the ordering is deterministic based upon the metadata ordering in the assembly.
    /// </quote>
    /// </summary>
    public static class TypeVarianceExtensions
    {
        #region Type Variance Checking

        internal static readonly ConcurrentBag<Tuple<Type, Type>> _nonConvertibleAssociations = new ConcurrentBag<Tuple<Type, Type>>();
        internal static readonly ConcurrentDictionary<Tuple<Type, Type>, Tuple<Type, Lazy<Delegate>>> _conversionCache = new ConcurrentDictionary<Tuple<Type, Type>, Tuple<Type, Lazy<Delegate>>>();


#if NET45_OR_GREATER || NETSTANDARD2_0
        /// <summary>
        /// Returns whether a runtime type is a variant type (according to the Liskov/Wing principle) of the expected Type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expectedType"></param>
        /// <returns></returns>
        public static bool IsVariantOf(this TypeInfo type, Type expectedType)
        {
            return IsVariantOf(type.AsType(), expectedType, out var substitutionType);
        }

        /// <summary>
        /// Returns whether a runtime type is a variant type (according to the Liskov/Wing principle) of the expected Type and returns the first valid substitution i possible
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expectedType"></param>
        /// <param name="substitutionType"></param>
        /// <returns></returns>
        public static bool IsVariantOf(this TypeInfo type, Type expectedType, out Type substitutionType)
        {
            return IsVariantOf(type.AsType(), expectedType, out substitutionType);
        }
#endif

        /// <summary>
        /// Returns whether a runtime type is a variant type (according to the Liskov/Wing principle) of the expected Type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expectedType"></param>
        /// <returns></returns>
        public static bool IsVariantOf(this Type type, Type expectedType)
        {
            return IsVariantOf(type, expectedType, out var substitutionType);
        }

        /// <summary>
        /// Returns whether a runtime type is a variant type (according to the Liskov/Wing principle) of the expected Type and returns the first valid substitution i possible
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expectedType"></param>
        /// <param name="runtimeType"></param>
        /// <returns></returns>
        public static bool IsVariantOf(this Type type, Type expectedType, out Type runtimeType)
        {
            runtimeType = null;

            // obvious cases
            if (type == null || expectedType == null) return false;
            if (type == expectedType)
            {
                runtimeType = type;
                return true;
            }

            var cacheKey = Tuple.Create(type, expectedType);
            if (_nonConvertibleAssociations.Contains(cacheKey))
            {
                return false;
            }
            //TODO consider other potential non matches
            else if (type.ContainsGenericParameters && !expectedType.ContainsGenericParameters)
            {
                _nonConvertibleAssociations.Add(cacheKey);
                return false;
            }

            // native .NET inheritance checking
            if (expectedType.IsAssignableFrom(type))
            {
                runtimeType = _conversionCache.GetOrAdd(cacheKey, Tuple.Create(expectedType, BuildDelegate(cacheKey, expectedType))).Item1;
                return true;
            }
            else if (_conversionCache.TryGetValue(cacheKey, out var cachedValue))
            {
                runtimeType = cachedValue.Item1;
                return true;
            }

            // At this point we should safely assume we deal with generic types only, otherwise IsAssignableFrom should have already determined the inheritance
            var expectedTypeGenericDefinition = expectedType.IsGenericType ? expectedType.GetGenericTypeDefinition() : null;
            var expectedTypeArguments = expectedType.GetGenericArguments();
            if (!expectedType.IsInterface)
            {
                do
                {
                    if (type.IsGenericType)
                    {
                        var genericTypeDefinition = type.GetGenericTypeDefinition();
                        //We can assume safely that the subsitution is valid since otherwise an error would be throw at compile time
                        if (genericTypeDefinition == expectedTypeGenericDefinition)
                        {
                            runtimeType = _conversionCache.GetOrAdd(cacheKey, Tuple.Create(type, BuildDelegate(cacheKey, type))).Item1;
                            return true;
                        }
                    }
                    type = type.BaseType;
                } while (type != null);
            }
            else
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == expectedTypeGenericDefinition && SatisfiesTypeConstraints(type, expectedType, out runtimeType))
                {
                    return true;
                }
                // we discard non-generic interfaces since checking should already have returns a result on them
                var potentialMatches = type.GetInterfaces()
                    .Where(iface => iface.IsGenericType
                        && expectedTypeGenericDefinition == iface.GetGenericTypeDefinition()
                        && iface.GetGenericArguments().Count() == expectedTypeArguments.Count());
                foreach (var iface in potentialMatches)
                {
                    if (iface.GetGenericTypeDefinition().IsVariantOf(expectedTypeGenericDefinition) && SatisfiesTypeConstraints(iface, expectedType, out runtimeType))
                    {
                        _conversionCache.GetOrAdd(cacheKey, Tuple.Create(runtimeType, BuildDelegate(cacheKey, runtimeType)));
                        return true;
                    }
                }
            }
            _nonConvertibleAssociations.Add(cacheKey);
            return false;
        }

        /// <summary>
        /// Checks whether a type statisfies the potential genericParameter's constraints of the expected type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expectedType"></param>
        /// <param name="constrainedType"></param>
        /// <returns></returns>
        private static bool SatisfiesTypeConstraints(Type type, Type expectedType, out Type constrainedType)
        {
            constrainedType = null;

            Type[] genericTypeArguments = type.GetGenericArguments();
            Type[] expectedTypeArguments = expectedType.GetGenericArguments();

            //obvious cases
            if (genericTypeArguments.Count() != expectedTypeArguments.Count()) return false;

            var substitutedArgs = new List<Type>();
            for (int i = 0, l = genericTypeArguments.Count(); i < l; i++)
            {
                var typeArg = genericTypeArguments.ElementAt(i);
                var expectedTypeArg = expectedTypeArguments.ElementAt(i);
                if (!expectedTypeArg.IsGenericParameter)
                {
                    if (!typeArg.IsVariantOf(expectedTypeArg, out var substitutedArg)) return false;
                    substitutedArgs.Add(substitutedArg);
                }
                else if (expectedTypeArg.IsGenericParameter)
                {
                    foreach (var typeContraint in expectedTypeArg.GetGenericParameterConstraints())
                    {
                        if (!typeArg.IsVariantOf(typeContraint, out var substitutedArg)) return false;
                    }
                    substitutedArgs.Add(typeArg);
                }
            }
            constrainedType = expectedType.GetGenericTypeDefinition().MakeGenericType(substitutedArgs.ToArray());
            return true;
        }

        #endregion

        #region Instances effective conversions to covariant type

        private static MethodInfo ConvertAsDefinition = typeof(TypeVarianceExtensions).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(m => m.Name == "ConvertAs" && m.GetGenericArguments().Count() == 2)
            .FirstOrDefault().GetGenericMethodDefinition();

        public static object ConvertAs<T>(this T instance, Type expectedType)
        {
            if (typeof(T).IsVariantOf(expectedType, out var runtimeType))
            {
                var method = ConvertAsDefinition.MakeGenericMethod(typeof(T), runtimeType);
                return method.Invoke(null, new object[] { instance, expectedType });
            }
            return null;
        }

        private static TOut ConvertAs<T, TOut>(this T instance, Type expectedType)
        {
            if (_conversionCache.TryGetValue(Tuple.Create(typeof(T), expectedType), out var conversion))
            {
                return (TOut)conversion.Item2.Value.DynamicInvoke(instance);
            }
            return default(TOut);
        }

        public static bool IsInstanceOf<T, TOut>(this T instance)
        {
            return typeof(T).IsVariantOf(typeof(TOut));
        }

        public static bool IsInstanceOf<T>(this T instance, Type targetType)
        {
            return typeof(T).IsVariantOf(targetType);
        }

        public static bool IsInstanceOf<T>(this T instance, Type targetType, out Type runtimeType)
        {
            return typeof(T).IsVariantOf(targetType, out runtimeType);
        }

        static readonly Func<Tuple<Type, Type>, Type, Lazy<Delegate>> BuildDelegate =
            (t, rt) => new Lazy<Delegate>(() => CreateDelegate(t.Item1, rt));

        public static Delegate CreateDelegate(Type sourceType, Type runtimeType)
        {
            var lbdParam = Expression.Parameter(sourceType);
            return Expression.Lambda(Expression.Convert(lbdParam, runtimeType), lbdParam).Compile();
        }

        #endregion
    }
}