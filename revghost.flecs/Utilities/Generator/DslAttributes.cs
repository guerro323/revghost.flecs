namespace revghost.flecs.Utilities.Generator;

public class QueryDslAttribute : Attribute {}

public interface IFinishTerm {}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
public class RefAttribute<T> : QueryDslAttribute, IFinishTerm
{
    public RefAttribute(string name) {}
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
public class RefAttribute<TLeft, TRight> : QueryDslAttribute, IFinishTerm
{
    public RefAttribute(string name) {}
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
public class InAttribute<T> : QueryDslAttribute, IFinishTerm
{
    public InAttribute(string name) {}
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
public class InAttribute<TLeft, TRight> : QueryDslAttribute, IFinishTerm
{
    public InAttribute(string name) {}
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
public class NoneAttribute<T> : QueryDslAttribute
{
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
public class NoneAttribute<TLeft, TRight> : QueryDslAttribute
{
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
public class OrAttribute<T> : QueryDslAttribute
{
    public OrAttribute(string name) {}
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
public class OrAttribute<TLeft, TRight> : QueryDslAttribute
{
    public OrAttribute(string name) {}
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
public class WriteAttribute<TLeft, TRight> : QueryDslAttribute, IFinishTerm
{
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
public class WriteAttribute<T> : QueryDslAttribute, IFinishTerm
{
}

public struct Parent {}
public struct None {}
public struct Self {}

public struct Up<T> where T : IComponent {}
public struct Down<T> where T : IComponent {}
public struct Cascade {}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
public class TraversalAttribute<T> : QueryDslAttribute
{
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class TraversalAttribute<T1, T2> : QueryDslAttribute
{
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class Pair<T> : QueryDslAttribute
{
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class PairSecond<T> : QueryDslAttribute
{
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
public class TermOptionalAttribute : QueryDslAttribute {}

[AttributeUsage(AttributeTargets.Struct)]
public class MultiThreadedAttribute : Attribute
{
} 

[AttributeUsage(AttributeTargets.Struct)]
public class NoReadOnlyAttribute : Attribute
{
} 