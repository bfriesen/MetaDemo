using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq.Expressions;

namespace MetaDemo
{
    public sealed class DeepEqualityComparer : IEqualityComparer
    {
        private static readonly DeepEqualityComparer _instance = new DeepEqualityComparer();

        private readonly Type[] _primitivishTypes =
        {
            typeof(string),
            typeof(decimal),
            typeof(DateTime),
            typeof(Guid)
        };

        private readonly ConcurrentDictionary<Type, IEnumerable<Func<object, object>>> _propertyAccessorsMap =
            new ConcurrentDictionary<Type, IEnumerable<Func<object, object>>>();

        private DeepEqualityComparer()
        {
        }

        public static IEqualityComparer Instance
        {
            get { return _instance; }
        }

        private static IEqualityComparer<T> GetGenericInstance<T>()
        {
            return GenericDeepEqualityComparer<T>.Instance;
        }

        public bool Equals(object lhs, object rhs)
        {
            if (lhs == null && rhs == null)
            {
                return true;
            }

            if (lhs == null
                || rhs == null
                || lhs.GetType() != rhs.GetType())
            {
                return false;
            }

            var type = lhs.GetType();

            if (IsPrimitivish(type))
            {
                return lhs.Equals(rhs);
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var lhsList = ((IEnumerable)lhs).Cast<object>().ToList();
                var rhsList = ((IEnumerable)rhs).Cast<object>().ToList();

                if (lhsList.Count != rhsList.Count)
                {
                    return false;
                }

                return
                    lhsList
                        .Zip(rhsList, (l, r) => this.Equals(l, r))
                        .All(x => x);
            }

            return
                GetPropertyAccessors(type)
                    .All(getPropertyValue => this.Equals(getPropertyValue(lhs), getPropertyValue(rhs)));
        }

        public int GetHashCode(object obj)
        {
            if (obj == null)
            {
                return 0;
            }

            var type = obj.GetType();

            if (IsPrimitivish(type))
            {
                return obj.GetHashCode();
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return this.GetAggregatedHashCode(type, ((IEnumerable)obj).Cast<object>());
            }

            return this.GetAggregatedHashCode(type, GetPropertyAccessors(type).Select(getPropertyValue => getPropertyValue(obj)));
        }

        private int GetAggregatedHashCode(Type type, IEnumerable<object> values)
        {
            unchecked
            {
                return
                    values.Aggregate(
                        type.FullName.GetHashCode(),
                        (hashCode, value) => (hashCode * 397) ^ this.GetHashCode(value));
            }
        }

        private IEnumerable<Func<object, object>> GetPropertyAccessors(Type type)
        {
            return
                _propertyAccessorsMap.GetOrAdd(
                     type,
                     t =>
                     (from p in t.GetProperties()
                      where p.CanRead
                      let getMethod = p.GetGetMethod()
                      where getMethod != null && getMethod.IsPublic
                      select CreatePropertyAccessor(p)).ToList());
        }

        private Func<object, object> CreatePropertyAccessor(PropertyInfo property)
        {
            var objParameter = Expression.Parameter(typeof(object), "obj");

            Expression getPropertyValueExpression =
                Expression.Property(
                    Expression.Convert(
                        objParameter,
                        property.DeclaringType),
                    property);

            if (property.PropertyType.IsValueType)
            {
                getPropertyValueExpression = Expression.Convert(getPropertyValueExpression, typeof(object));
            }

            var lambda = Expression.Lambda<Func<object, object>>(getPropertyValueExpression, objParameter);
            return lambda.Compile();
        }

        private bool IsPrimitivish(Type type)
        {
            return IsNonNullablePrimitivish(type) || IsNullablePrimitivish(type);
        }

        private bool IsNonNullablePrimitivish(Type type)
        {
            return type.IsPrimitive || _primitivishTypes.Contains(type);
        }

        private bool IsNullablePrimitivish(Type type)
        {
            return
                type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                && IsNonNullablePrimitivish(type.GetGenericArguments()[0]);
        }

        private sealed class GenericDeepEqualityComparer<T> : IEqualityComparer<T>
        {
            private static readonly GenericDeepEqualityComparer<T> _instance = new GenericDeepEqualityComparer<T>();

            private GenericDeepEqualityComparer()
            {
            }

            public static IEqualityComparer<T> Instance
            {
                get { return _instance; }
            }

            public bool Equals(T lhs, T rhs)
            {
                return DeepEqualityComparer.Instance.Equals(lhs, rhs);
            }

            public int GetHashCode(T obj)
            {
                return DeepEqualityComparer.Instance.GetHashCode(obj);
            }
        }
    }
}