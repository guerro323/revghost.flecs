using ManiaGen.SourceGenerator;
using ManiaGen.SourceGenerator.SubGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace revghost2.SourceGenerator;

public struct FieldInfo
{
    public bool IsBuiltin;
    
    public string Name;
    public string TypeName;
}

// TODO: This should be rewritten with a walker pattern instead of a visitor pattern
public struct EachCompiler
{
    public SemanticModel Model;
    public Dictionary<string, List<FieldInfo>> Terms;
    public CodeBuilder Builder;
    public SyntaxNode Body;

    public Action<string> Log;
    public bool HasArity;

    public IFieldSymbol[] originFields; 
    public ISymbol origin;

    public const string IteratorVarName = "__it__{0}";

    public void Compile()
    {
        if (!Body.IsKind(SyntaxKind.Block) && !Body.IsKind(SyntaxKind.ArrowExpressionClause))
            throw new InvalidOperationException("Expected a block/expression but had " + Body.Kind());
        
        if (!HasArity)
            Builder.AppendLine("[UnmanagedCallersOnly]");
        
        Builder.AppendLine("private static unsafe void EachUnmanaged(__FLECS__.ecs_iter_t* __it__)");
        Builder.BeginBracket();
        {
            Builder.AppendLine("var __state__ = (__SYSTEM_STATE__*) __it__->ctx;");
            Builder.AppendLine("var entities = __it__->entities;");

            var i = 1; // 1 => +entity
            foreach (var term in Terms[string.Empty])
            {
                if (term.IsBuiltin)
                    continue;
                
                Builder.AppendLine(
                    $"var c{i} = ({term.TypeName}*) __FLECS__.ecs_field_w_size(__it__, (ulong) Unsafe.SizeOf<{term.TypeName}>(), {i});"
                );
                i++;
            }

            Builder.AppendLine("for (var i0 = 0; i0 < __it__->count; i0++)");
            Builder.BeginBracket();
            {
                var rewriter = new Rewriter { Base = this, Log = Log, Terms = Terms };
                Body = rewriter.Visit(Body);
                Builder.AppendLine(Body.ToString());
            }
            Builder.EndBracket();
        }
        Builder.EndBracket();
    }

    class Rewriter : CSharpSyntaxRewriter
    {
        public EachCompiler Base;
        public Action<string> Log;
        public Dictionary<string, List<FieldInfo>> Terms;

        private int IndexInMap(string name)
        {
            if (string.IsNullOrEmpty(name))
                return 0;
            
            var i = 1;
            foreach (var (key, _) in Terms)
            {
                if (key == name)
                    return i;

                i += 1;
            }

            return -1;
        }
        
        private int IndexOfTerm(string fieldName, string name)
        {
            var i = 0;
            foreach (var term in Terms[fieldName])
            {
                Log($"{fieldName}.{name} == {term.Name}");
                if (term.IsBuiltin && term.Name == name)
                    return -2;
                if (term.Name == name)
                    return i;

                if (!term.IsBuiltin)
                    i += 1;
            }

            return -1;
        }

        private (string? filter, FieldInfo componentField)? GetFieldSymbolFrom(MemberAccessExpressionSyntax memberAccess)
        {
            Log($" searching: {memberAccess}");
            
            var symbolInfo = Base.Model.GetSymbolInfo(memberAccess.Expression);
            Log($"  maybe: {symbolInfo.Symbol} ({memberAccess})");
            foreach (var c in symbolInfo.CandidateSymbols)
            {
                Log($"     candidate: {c}");
            }

            if (symbolInfo.Symbol is { } maybe)
            {
                Log($"   candidate found... containing={maybe.ContainingType}");
                if (Base.originFields.FirstOrDefault(f => f.Type.Equals(maybe.ContainingType)) is { } field)
                {
                    Log($"   break now! found a good candidate {field.Name} <=> {maybe.Name}");
                    return (field.Name, Terms[field.Name].Find(f => f.Name == maybe.Name));
                }
                
                if (maybe.Name.StartsWith("__it__"))
                {
                    return (null, new FieldInfo {IsBuiltin = true, Name = memberAccess.Name.ToString()});
                }

                if (maybe is ILocalSymbol local)
                {
                    bool predicate(IFieldSymbol f)
                    {
                        return f.Type == local.Type && f.Type.Interfaces.Any(f => f.Name == "IEntityFilter");
                    }
                    
                    // If the local type come from a filter, then this mean we're accessing a filter component
                    if (Base.originFields.FirstOrDefault(predicate) is { } field2)
                        return (field2.Name, new FieldInfo {IsBuiltin = true, Name = memberAccess.Name.ToString()});

                    if (local.DeclaringSyntaxReferences.Length > 0 &&
                        local.DeclaringSyntaxReferences[0].GetSyntax() is { } fromIf &&
                        fromIf.Parent is PatternSyntax parentPattern &&
                        _replacedIdentifiers.TryGetValue(parentPattern, out var replaced))
                    {
                        Log($"Parent pattern span: {parentPattern.Span} {_replacedIdentifiers.ContainsKey(parentPattern)}");
                        return (replaced.filter, new FieldInfo {Name = replaced.componentName});
                    }
                    throw new InvalidOperationException($"Unknown for {local} -> {local.OriginalDefinition} -> {local.DeclaringSyntaxReferences[0].Span}");
                }

                // we add 'IsBuiltin' at the end since FirstOrDefault doesn't return a nullable struct, so we need to check if it's not a default value
                if (Terms[string.Empty].FirstOrDefault(t => t.IsBuiltin && t.Name == maybe.Name) is { IsBuiltin:true } fieldInfo)
                {
                    return (null, fieldInfo);
                }
            }

            var predicted = Base.Model.GetSymbolInfo(memberAccess.Expression).Symbol;
            if (predicted is IFieldSymbol pred)
            {
                Log($"   predicted = {pred}");
                if (Base.originFields.Contains(pred, SymbolEqualityComparer.Default))
                    return (null, Terms[string.Empty].Find(f => f.Name == pred.Name));
            }

            return null;
        }

        private ElementAccessExpressionSyntax CreateComponentAccess(string filterName, int componentIndex, SyntaxNode original)
        {
            var foreachIndex = IndexInMap(filterName);
            if (foreachIndex < 0)
                throw new InvalidOperationException($"filter not found? '{filterName}'");
            
            var list = new SeparatedSyntaxList<ArgumentSyntax>();
            list = list.Add(SyntaxFactory.Argument(
                SyntaxFactory.IdentifierName($"i{foreachIndex}")
            ));

            return SyntaxFactory.ElementAccessExpression(
                SyntaxFactory.IdentifierName($"c{filterName}{componentIndex + 1}"),
                SyntaxFactory.BracketedArgumentList(
                    list
                )
            ).WithTriviaFrom(original);
        }

        public override SyntaxNode? VisitForEachStatement(ForEachStatementSyntax node)
        {
            if (Base.Terms.ContainsKey(node.Expression.ToString()))
            {
                var varName = node.Expression.ToString();
                var iName = $"i{IndexInMap(varName)}";
                var varItName = string.Format(IteratorVarName, varName);

                var vars = new CodeBuilder();
                var i = 1; // 1 => +entity
                foreach (var term in Terms[varName])
                {
                    if (term.IsBuiltin)
                        continue;
                    
                    vars.AppendLine(
                        $"    var c{varName}{i} = ({term.TypeName}*) __FLECS__.ecs_field_w_size({varItName}, (ulong) Unsafe.SizeOf<{term.TypeName}>(), {i});"
                    );
                    i++;
                }
                
                Log("replace foreach");
                return SyntaxFactory.ParseStatement($$"""
var {{varItName}}_noptr = __FLECS__.ecs_query_iter(__it__->world, __state__->{{varName}});
var {{varItName}} = (__FLECS__.ecs_iter_t*) Unsafe.AsPointer(ref {{varItName}}_noptr);
while (__FLECS__.ecs_query_next({{varItName}})) {
    {{vars}}
    
    for (var {{iName}} = 0; {{iName}} < {{varItName}}->count; {{iName}}++) {
        {{Visit(node.Statement)}}
    }
}

""");
            }
            
            return base.VisitForEachStatement(node);
        }

        public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (GetFieldSymbolFrom(node) is { } tuple)
            {
                Log($">> {node} -> {tuple}");
                
                var filterField = tuple.filter ?? string.Empty;
                
                var subIndex = IndexOfTerm(filterField, tuple.componentField.Name);
                Log($"Component Access {filterField}.{tuple.componentField.Name}.{subIndex}");
                if (subIndex == -1)
                    return node;

                // Pardon my french, I didn't found an elegant way for prevent doubles and making sure that members are preserved
                // The best would be to rewrite this completely and use a recursive walker instead of the visitor pattern
                var leftOver = node.Name.ToString();
                if (leftOver == tuple.componentField.Name)
                    leftOver = string.Empty;
                else
                    leftOver = $".{leftOver}";
                
                if (tuple.componentField.IsBuiltin)
                {
                    var itVar = string.Format(IteratorVarName, filterField);
                    var indexName = $"i{IndexInMap(filterField)}";
                    if (tuple.componentField.Name == "Id")
                    {
                        return SyntaxFactory.ParseExpression(
                            $"Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref {itVar}->entities[{indexName}]){leftOver}"
                        );
                    }
                
                    if (tuple.componentField.Name == "Entity")
                    {
                        return SyntaxFactory.ParseExpression(
                            $"Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref {itVar}->entities[{indexName}]).WithWorld(global::revghost2.World.FromExistingUnsafe({itVar}->world)){leftOver}"
                        );
                    }
                    
                    if (tuple.componentField.Name == "DeltaTime")
                    {
                        return SyntaxFactory.ParseExpression(
                            $"global::revghost2.World.FromExistingUnsafe({itVar}->delta_time){leftOver}"
                        );
                    }
                    
                    if (tuple.componentField.Name == "SystemDeltaTime")
                    {
                        return SyntaxFactory.ParseExpression(
                            $"global::revghost2.World.FromExistingUnsafe({itVar}->system_delta_time){leftOver}"
                        );
                    }
                    
                    if (tuple.componentField.Name == "World")
                    {
                        return SyntaxFactory.ParseExpression(
                            $"global::revghost2.World.FromExistingUnsafe({itVar}->world){leftOver}"
                        );
                    }
                    
                    if (tuple.componentField.Name == "World")
                    {
                        return SyntaxFactory.ParseExpression(
                            $"global::revghost2.World.FromExistingUnsafe({itVar}->real_world){leftOver}"
                        );
                    }
                }
                
                Log($"replace with {CreateComponentAccess(filterField, subIndex, node)}.{node.Name}");
                if (string.IsNullOrEmpty(leftOver))
                    return CreateComponentAccess(filterField, subIndex, node);
                
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    CreateComponentAccess(tuple.filter ?? string.Empty, subIndex, node),
                    node.Name
                );
            }

            return base.VisitMemberAccessExpression(node);
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            Log($"Work on {node} (+Parent {node.Parent})");
            if (node.Parent is MemberAccessExpressionSyntax)
                return base.VisitIdentifierName(node);

            var name = node.Identifier.ToString();
            //if (_replacedIdentifiers.TryGetValue(name, out var replaced))
            //    return replaced;
            
            var index = IndexOfTerm(string.Empty, name);
            if (index < 0)
            {
                if (name == "SystemDeltaTime")
                {
                    return SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.PointerMemberAccessExpression,
                        SyntaxFactory.IdentifierName("__it__"),
                        SyntaxFactory.IdentifierName("delta_system_time")
                    );
                }
                 
                if (name == "DeltaTime")
                {
                    return SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.PointerMemberAccessExpression,
                        SyntaxFactory.IdentifierName("__it__"),
                        SyntaxFactory.IdentifierName("delta_time")
                    );
                }

                if (name == "World")
                {
                    return SyntaxFactory.ParseExpression("global::revghost2.World.FromExistingUnsafe(__it__->world)");
                }
                
                if (name == "RealWorld")
                {
                    return SyntaxFactory.ParseExpression("global::revghost2.World.FromExistingUnsafe(__it__->real_world)");
                }
                
                if (name == "Id")
                {
                    return SyntaxFactory.ParseExpression("Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref entities[i0])");
                }
                
                if (name == "Entity")
                {
                    return SyntaxFactory.ParseExpression("Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref entities[i0]).WithWorld(global::revghost2.World.FromExistingUnsafe(__it__->world))");
                }

                return base.VisitIdentifierName(node);
            }

            return CreateComponentAccess(string.Empty, index, node);
        }

        private Dictionary<SyntaxNode, (string filter, string componentName)> _replacedIdentifiers = new();

        public override SyntaxNode? VisitIsPatternExpression(IsPatternExpressionSyntax node)
        {
            if (node.Pattern is RecursivePatternSyntax {Designation: not null} recursive)
            {
                Log($"Span: {node.Pattern.Span}");
                if (node.Expression is IdentifierNameSyntax identifierName)
                {
                    var name = identifierName.ToString();
                    var index = IndexOfTerm(string.Empty, name) + 1;
                    if (index < 0)
                        return base.VisitIsPatternExpression(node);
                    
                    _replacedIdentifiers[recursive] = (string.Empty, name);
                    return SyntaxFactory.ParseExpression($"c{index} != null");
                }
                
                if (node.Expression is MemberAccessExpressionSyntax ma && GetFieldSymbolFrom(ma) is { } tuple)
                {
                    var subIndex = IndexOfTerm(tuple.filter, tuple.componentField.Name);
                    if (subIndex == -1)
                        return node;
                    if (subIndex == -2)
                        throw new InvalidOperationException("builtin not supported");
                    
                    var foreachIndex = IndexInMap(tuple.filter);
                    if (foreachIndex < 0)
                        throw new InvalidOperationException($"filter not found? '{tuple.filter}'");
                    
                    _replacedIdentifiers[recursive] = (tuple.filter, tuple.componentField.Name);
                    return SyntaxFactory.ParseExpression($"c{tuple.filter}{subIndex + 1} != null");
                }
            }

            return base.VisitIsPatternExpression(node);
        }

        // TODO: This should be remade when having filter fields
        /*public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (node.Expression.ToString() == "entity")
            {
                var index = IndexOfTerm(node.Name.ToString());
                if (index < 0)
                    throw new InvalidOperationException($"Expected {node.Name.ToString()} to be a field");

                var list = new SeparatedSyntaxList<ArgumentSyntax>();
                list = list.Add(SyntaxFactory.Argument(
                    SyntaxFactory.IdentifierName("i")
                ));

                return SyntaxFactory.ElementAccessExpression(
                    SyntaxFactory.IdentifierName($"c{index + 1}"),
                    SyntaxFactory.BracketedArgumentList(
                        list
                    )
                ).WithTriviaFrom(node);
            }
            
            return base.VisitMemberAccessExpression(node);
        }*/
    }
}