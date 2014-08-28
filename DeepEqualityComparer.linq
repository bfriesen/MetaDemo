<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

#define NONEST
void Main()
{
    var item1 = new Foo
    {
        Bar = "abc",
    };
    
    var item2 = new Foo
    {
        Bar = "abc",
    };
    
    var item3 = new Foo
    {
        Bar = "abcd",
    };
    
    var item4 = new Bar
    {
        Baz = "abc",
    };
    
    IEqualityComparer comparer = DeepEqualityComparer.Instance;
    IEqualityComparer<Foo> fooComparer = DeepEqualityComparer.GetGenericInstance<Foo>();
    
    comparer.Equals(item1, item2).Dump("comparer.Equals(item1, item2)");
    comparer.Equals(item1, item3).Dump("comparer.Equals(item1, item3)");
    comparer.Equals(item1, item4).Dump("comparer.Equals(item1, item4)");
    
    comparer.GetHashCode(item1).Dump("comparer.GetHashCode(item1)");
    comparer.GetHashCode(item2).Dump("comparer.GetHashCode(item2)");
    comparer.GetHashCode(item3).Dump("comparer.GetHashCode(item3)");
    comparer.GetHashCode(item4).Dump("comparer.GetHashCode(item4)");
//    "".Dump().Dump().Dump().Dump();
//    fooComparer.Equals(item1, item2).Dump("fooComparer.Equals(item1, item2)");
//    fooComparer.Equals(item1, item3).Dump("fooComparer.Equals(item1, item3)");
////    fooComparer.Equals(item1, item4).Dump("Equals(item1, item4)");
//    
//    fooComparer.GetHashCode(item1).Dump("fooComparer.GetHashCode(item1)");
//    fooComparer.GetHashCode(item2).Dump("fooComparer.GetHashCode(item2)");
//    fooComparer.GetHashCode(item3).Dump("fooComparer.GetHashCode(item3)");
////    fooComparer.GetHashCode(item4).Dump("GetHashCode(item4)");

//    comparer.Equals(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }).Dump();
//    comparer.GetHashCode(new[] { 1, 2, 3 }).Dump();

    // Start with making private constructor, creating singleton field & property,
    // and making the private generic class, its singleton members, & the outer
    // accessor method.
    // 
    // Next, implement the interfaces. The inner class can be fully completed at
    // this point. Leave the two interface method unimplemented in the outer class.
    //
    // Move on to the implementation of the Equals method.
    // Let's deal with null straight away.
    // Once we're sure we don't have any nulls, if the type we have a primitive-like,
    //     use the first object's Equals instance method.
    // If we're dealing with an IEnumerable of some sort, deal with it.
    // Go through all the public properties of each instance, recursively calling
    //     the Equals method with the property value of lhs and rhs.
    //
    // Finally, GetHashCode.
    // Again, deal with null first.
    // If the type is primitive-like, return the value from its GetHashCode method.
    // 
}

public class Foo
{
    public string Bar { get; set; }
}

public class Bar
{
    public string Baz { get; set; }
}

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
    
    private readonly ConcurrentDictionary<Type, IEnumerable<Func<object, object>>> _propertyAccessorsMap = new ConcurrentDictionary<Type, IEnumerable<Func<object, object>>>();

    private DeepEqualityComparer()
    {
    }
    
    public static DeepEqualityComparer Instance
    {
        get { return _instance; }
    }
    
    public static IEqualityComparer<T> GetGenericInstance<T>()
    {
        return GenericDeepEqualityComparer<T>.Instance;
    }
            
    public new bool Equals(object lhs, object rhs)
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
    
    public int GetHashCode(object instance)
    {
        if (instance == null)
        {
            return 0;
        }
        
        var type = instance.GetType();
        
        if (IsPrimitivish(type))
        {
            return instance.GetHashCode();
        }
        
        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            return this.GetAggregatedHashCode(type, ((IEnumerable)instance).Cast<object>());
        }
        
        return this.GetAggregatedHashCode(type, GetPropertyAccessors(type).Select(getPropertyValue => getPropertyValue(instance)));
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
                (   from p in t.GetProperties()
                    where p.CanRead
                    let getMethod = p.GetGetMethod()
                    where getMethod != null && getMethod.IsPublic
                    select CreatePropertyAccessor(p)   )
                .ToList());
    }
    
    private Func<object, object> CreatePropertyAccessor(PropertyInfo property)
    {
        var objParameter = Expression.Parameter(typeof(object), "obj");
        
        var lambda =
            Expression.Lambda<Func<object, object>>(
                Expression.Property(
                    Expression.Convert(
                        objParameter,
                        property.DeclaringType),
                    property),
                objParameter);
    
        return lambda.Dump().Compile();
    }
    
    private bool IsPrimitivish(Type type)
    {
        return IsNonNullablePrimitivish(type) || IsNullablePrimitivish(type);
    }
    
    private bool IsNonNullablePrimitivish(Type type)
    {
        return
            type.IsPrimitive
            || _primitivishTypes.Contains(type);
    }
    
    private bool IsNullablePrimitivish(Type type)
    {
        return
            type.IsGenericType
            && type.GetGenericTypeDefinition() == typeof(Nullable<>)
            && IsNonNullablePrimitivish(type.GetGenericArguments()[0]);
    }
    
    private class GenericDeepEqualityComparer<T> : IEqualityComparer<T>
    {
        private static readonly GenericDeepEqualityComparer<T> _instance = new GenericDeepEqualityComparer<T>();
        
        private GenericDeepEqualityComparer()
        {
        }
        
        public static GenericDeepEqualityComparer<T> Instance
        {
            get { return _instance; }
        }
        
        public bool Equals(T lhs, T rhs)
        {
            return DeepEqualityComparer.Instance.Equals(lhs, rhs);
        }
        
        public int GetHashCode(T instance)
        {
            return DeepEqualityComparer.Instance.GetHashCode(instance);
        }
    }
}