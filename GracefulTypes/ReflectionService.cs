using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace GracefulTypes
{
    /// <summary>
    /// Helper class for accessing values using reflection
    /// </summary>
    internal class ReflectionService
    {
        private static readonly MethodInfo ImmutableTreeAdd =
            typeof(ImmutableHashTree<string, object>).GetRuntimeMethods().First(m => m.Name == nameof(ImmutableHashTree<string, object>.Add));

        private static ImmutableHashTree<Type, PropertyDictionaryDelegate> propertyDelegates =
            ImmutableHashTree<Type, PropertyDictionaryDelegate>.Empty;
        private static ImmutableHashTree<Type, PropertyDictionaryDelegate> lowerCasePropertyDelegates =
            ImmutableHashTree<Type, PropertyDictionaryDelegate>.Empty;
        private static ImmutableHashTree<Type, PropertyDictionaryDelegate> upperCasePropertyDelegates =
            ImmutableHashTree<Type, PropertyDictionaryDelegate>.Empty;

        /// <summary>
        /// Delegate for creating dictionaries from object properties
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public delegate ImmutableHashTree<string, object> PropertyDictionaryDelegate(object instance, ImmutableHashTree<string, object> values);

        /// <summary>
        /// Method to get friendly version of a type name for display purposes
        /// </summary>
        /// <param name="type"></param>
        /// <param name="includeNamespace"></param>
        /// <returns></returns>
        public static string GetFriendlyNameForType(Type type, bool includeNamespace = false)
        {
            if (type.IsConstructedGenericType)
            {
                var builder = new StringBuilder();

                if (includeNamespace)
                {
                    builder.Append(type.Namespace);
                    builder.Append('.');
                }

                CreateFriendlyNameForType(type, builder);

                return builder.ToString();
            }

            return includeNamespace ? type.Namespace + '.' + type.Name : type.Name;
        }

        private static void CreateFriendlyNameForType(Type currentType, StringBuilder builder)
        {
            if (currentType.IsConstructedGenericType)
            {
                var tickIndex = currentType.Name.LastIndexOf('`');
                builder.Append(currentType.Name.Substring(0, tickIndex));
                builder.Append('<');

                var types = currentType.GenericTypeArguments;

                for (var i = 0; i < types.Length; i++)
                {
                    CreateFriendlyNameForType(types[i], builder);

                    if (i + 1 < types.Length)
                    {
                        builder.Append(',');
                    }
                }

                builder.Append('>');
            }
            else
            {
                builder.Append(currentType.Name);
            }
        }

        /// <summary>
        /// Checks to see if checkType is based on baseType
        /// Both inheritance and interface implementation is considered
        /// </summary>
        /// <param name="checkType">check type</param>
        /// <param name="baseType">base type</param>
        /// <returns>true if check type is base type</returns>
        public static bool CheckTypeIsBasedOnAnotherType(Type checkType, Type baseType)
        {
            if (checkType == baseType)
            {
                return true;
            }

            if (baseType.GetTypeInfo().IsInterface)
            {
                if (baseType.GetTypeInfo().IsGenericTypeDefinition)
                {
                    foreach (var implementedInterface in checkType.GetTypeInfo().ImplementedInterfaces)
                    {
                        if (implementedInterface.IsConstructedGenericType &&
                            implementedInterface.GetTypeInfo().GetGenericTypeDefinition() == baseType)
                        {
                            return true;

                        }
                    }
                }
                else if (checkType.GetTypeInfo().IsInterface)
                {
                    return baseType.GetTypeInfo().IsAssignableFrom(checkType.GetTypeInfo());
                }
                else if (checkType.GetTypeInfo().ImplementedInterfaces.Contains(baseType))
                {
                    return true;
                }
            }
            else
            {
                if (baseType.GetTypeInfo().IsGenericTypeDefinition)
                {
                    var currentBaseType = checkType;

                    while (currentBaseType != null)
                    {
                        if (currentBaseType.IsConstructedGenericType &&
                            currentBaseType.GetGenericTypeDefinition() == baseType)
                        {
                            return true;
                        }

                        currentBaseType = currentBaseType.GetTypeInfo().BaseType;
                    }
                }
                else
                {
                    return baseType.GetTypeInfo().IsAssignableFrom(checkType.GetTypeInfo());
                }
            }

            return false;
        }

        /// <summary>
        /// Casing for property names
        /// </summary>
        public enum PropertyCasing
        {
            /// <summary>
            /// Lower case all properties 
            /// </summary>
            Lower,
            /// <summary>
            /// Upper case all properties
            /// </summary>
            Upper,

            /// <summary>
            /// Default casing of property names
            /// </summary>
            Default,
        }

        /// <summary>
        /// Get dictionary of property values from an object
        /// </summary>
        /// <param name="annonymousObject">object to get properties from</param>
        /// <param name="values">collection to add to</param>
        /// <param name="casing">lowercase property names</param>
        /// <returns></returns>
        public static ImmutableHashTree<string, object> GetPropertiesFromObject(object annonymousObject,
            ImmutableHashTree<string, object> values = null, PropertyCasing casing = PropertyCasing.Default)
        {
            values = values ?? ImmutableHashTree<string, object>.Empty;

            if (annonymousObject == null)
            {
                return values;
            }

            if (annonymousObject is Array array)
            {
                var i = 0;

                foreach (var value in array)
                {
                    values = values.Add(i.ToString(), value);
                    i++;
                }

                return values;
            }

            if (annonymousObject is IDictionary<string, object> dictionary)
            {
                return dictionary.Aggregate(values,
                    (v, kvp) => v.Add(kvp.Key, kvp.Value));
            }

            var objectType = annonymousObject.GetType();

            PropertyDictionaryDelegate propertyDelegate = null;

            switch (casing)
            {
                case PropertyCasing.Default:
                    propertyDelegate = propertyDelegates.GetValueOrDefault(objectType);
                    break;
                case PropertyCasing.Lower:
                    propertyDelegate = lowerCasePropertyDelegates.GetValueOrDefault(objectType);
                    break;
                case PropertyCasing.Upper:
                    propertyDelegate = upperCasePropertyDelegates.GetValueOrDefault(objectType);
                    break;
            }

            if (propertyDelegate != null)
            {
                return propertyDelegate(annonymousObject, values);
            }

            propertyDelegate = CreateDelegateForType(objectType, casing);

            switch (casing)
            {
                case PropertyCasing.Default:
                    ImmutableHashTree.ThreadSafeAdd(ref propertyDelegates, objectType, propertyDelegate);
                    break;
                case PropertyCasing.Lower:
                    ImmutableHashTree.ThreadSafeAdd(ref lowerCasePropertyDelegates, objectType, propertyDelegate);
                    break;
                case PropertyCasing.Upper:
                    ImmutableHashTree.ThreadSafeAdd(ref upperCasePropertyDelegates, objectType, propertyDelegate);
                    break;
            }

            return propertyDelegate(annonymousObject, values);
        }

        private static PropertyDictionaryDelegate CreateDelegateForType(Type objectType, PropertyCasing casing)
        {
            // the parameter to call the method on
            var inputObject = Expression.Parameter(typeof(object), "inputObject");

            var treeParameter = Expression.Parameter(typeof(ImmutableHashTree<string, object>), "tree");

            // local variable of type declaringType
            var tVariable = Expression.Variable(objectType);

            // cast the input object to be declaring type
            Expression castExpression = Expression.Convert(inputObject, objectType);

            // assign the cast value to the tVariable variable
            Expression assignmentExpression = Expression.Assign(tVariable, castExpression);

            // keep a list of the variable we declare for use when we define the body
            var variableList = new List<ParameterExpression> { tVariable };

            var bodyExpressions = new List<Expression> { assignmentExpression };

            var updateDelegate =
                Expression.Constant(
                    new ImmutableHashTree<string, object>.UpdateDelegate((oldValue, newValue) => newValue));

            Expression tree = treeParameter;
            var currentType = objectType;

            while (currentType != null && currentType != typeof(object))
            {
                foreach (var property in currentType.GetTypeInfo().DeclaredProperties)
                {
                    if (property.CanRead &&
                        !property.GetMethod.IsStatic &&
                        property.GetMethod.IsPublic &&
                        property.GetMethod.GetParameters().Length == 0)
                    {
                        var propertyAccess = Expression.Property(tVariable, property.GetMethod);

                        var propertyCast = Expression.Convert(propertyAccess, typeof(object));

                        var propertyName = property.Name;

                        switch (casing)
                        {
                            case PropertyCasing.Lower:
                                propertyName = propertyName.ToLowerInvariant();
                                break;
                            case PropertyCasing.Upper:
                                propertyName = propertyName.ToUpperInvariant();
                                break;
                        }

                        tree = Expression.Call(tree, ImmutableTreeAdd, Expression.Constant(propertyName), propertyCast,
                            updateDelegate);
                    }
                }

                currentType = currentType.GetTypeInfo().BaseType;
            }

            bodyExpressions.Add(tree);

            var body = Expression.Block(variableList, bodyExpressions);

            return Expression.Lambda<PropertyDictionaryDelegate>(body, inputObject, treeParameter).Compile();
        }
    }
}
