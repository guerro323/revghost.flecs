using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace revghost.flecs.SourceGenerator.SubGenerators;

public class GenerateDslSubGenerator : SubGenerator
{
    private Thread mainThread;
    private ConcurrentDictionary<Thread, List<string>> _logMap = new();

    protected override void Log<T>(T obj, int indent = 0)
    {
        if (Thread.CurrentThread == mainThread)
            base.Log(obj, indent);
        else
        {
            var list = _logMap.GetOrAdd(Thread.CurrentThread, new List<string>());
            list.Add(string.Join(null, Enumerable.Repeat("\t", indent)) + obj);
        }
    }

    public enum EKind
    {
        Filter,
        Processor,
        System,
        Observer,
    }

    private Dictionary<string, string> _finalMap = new();

    protected override void Generate()
    {
        mainThread = Thread.CurrentThread;
        
        Log("Generate DSLs");
        
        // Parallelize the work to reduce the generator time
        var tasks = new List<Action>();

        CreateTask(tasks, true);
        Parallel.Invoke(tasks.ToArray());
        try
        {
            var trees = new List<(string, SyntaxTree)>();
            foreach (var (fileName, str) in _finalMap)
            {
                trees.Add((fileName, CSharpSyntaxTree.ParseText(str, Context.ParseOptions as CSharpParseOptions)));
            }

            Compilation = Compilation.AddSyntaxTrees(trees.Select(tuple => tuple.Item2));
        }
        catch (Exception ex)
        {
            Context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                "MG001",
                $"Exception {ex.GetType()} on generation",
                $"{ex.GetType()}: {ex.Message}",
                "?",
                DiagnosticSeverity.Error,
                true,
                ex.ToString()
            ), null));
            
            throw new InvalidOperationException($"Failed to compile! {ex}");
        }
        
        tasks.Clear();
        CreateTask(tasks, false);
        Parallel.Invoke(tasks.ToArray());

        foreach (var list in _logMap)
        {
            Receiver.Log.AddRange(list.Value);
        }
    }

    private List<Term> ParseTermsFrom(ITypeSymbol symbol)
    {
        var fields = symbol
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => !f.IsStatic && f.AssociatedSymbol == null)
            .ToArray();

        var terms = new List<Term>();
        foreach (var field in fields)
        {
            var pairIndex = -1;
            var fieldAttributes = field.GetAttributes();
            if (fieldAttributes.Any(a => a.AttributeClass!.Name == "Pair"))
                pairIndex = 1;
            else if (fieldAttributes.Any(a => a.AttributeClass!.Name == "PairSecond"))
                pairIndex = 0;

            var fieldKind = Term.FilterKind.And;
            if (field.NullableAnnotation == NullableAnnotation.Annotated)
                fieldKind = Term.FilterKind.Optional;

            var fieldTypeName = field.Type.GetTypeName()
                .Replace("?", string.Empty);

            var traversalAttr =
                fieldAttributes.FirstOrDefault(a => a.AttributeClass!.Name.StartsWith("Traversal"));
            var traversals = new List<Term.TraversalInfo>();
            if (traversalAttr != null)
            {
                foreach (var gen in traversalAttr.AttributeClass.TypeArguments)
                {
                    if (gen.Name == "Parent")
                        traversals.Add(new(true, "EcsParent"));

                    if (gen.Name == "Cascade")
                        traversals.Add(new(true, "EcsCascade"));
                }
            }

            if (fieldTypeName.StartsWith("global::revghost.flecs.Entity")
                || fieldTypeName.StartsWith("global::revghost.flecs.Pair"))
            {
                fieldTypeName = "global::revghost.flecs.Wildcard";
            }
            
            // TODO: don't add to terms if it's a [Param], [State] or another filter field
            terms.Add(new Term
            {
                Symbol = field,
                FieldName = field.Name,
                NameLeft = pairIndex == 1
                    ? fieldAttributes.First(a => a.AttributeClass!.Name == "Pair")
                        .AttributeClass!
                        .TypeArguments[0].GetTypeName().Replace("?", string.Empty)
                    : fieldTypeName,
                NameRight = pairIndex >= 0
                    ? (pairIndex == 0
                        ? fieldAttributes.First(a => a.AttributeClass!.Name == "PairSecond")
                            .AttributeClass!
                            .TypeArguments[0].GetTypeName().Replace("?", string.Empty)
                        : fieldTypeName)
                    : null,
                SrcEntity = fieldAttributes.Any(a => a.AttributeClass!.Name.StartsWith("Singleton"))
                    ? "$singleton"
                    : null,
                Access = field.IsReadOnly ? Term.AccessModifier.In : Term.AccessModifier.InOut,
                Kind = fieldKind,
                Traversal = traversals
            });
        }

        return terms;
    }

    private void CreateTask(List<Action> tasks, bool filterOnly)
    {
        foreach (var tree in Compilation.SyntaxTrees)
        {
            var semanticModel = Compilation.GetSemanticModel(tree);
            foreach (var declare in tree
                         .GetRoot()
                         .DescendantNodesAndSelf())
            {
                if (string.IsNullOrEmpty(tree.FilePath)
                    || tree.FilePath.Contains("Generated/"))
                    continue;

                if (declare is not StructDeclarationSyntax)
                    continue;

                var symbolBase = (INamedTypeSymbol) semanticModel.GetDeclaredSymbol(declare)!;
                if (!symbolBase.AllInterfaces.Any(iface => iface.Name == "IEntityFilter"))
                    continue;
                
                if (filterOnly && symbolBase.AllInterfaces.Any(iface => iface.Name == "IProcessor"))
                    continue;
                if (!filterOnly && !symbolBase.AllInterfaces.Any(iface => iface.Name == "IProcessor"))
                    continue;

                tasks.Add(() =>
                {
                    Location? prevLocation = null;
                    try
                    {
                        var attributes = symbolBase.GetAttributes();
                        prevLocation = symbolBase.Locations.First();

                        foreach (var attribute in attributes)
                        {
                            Log($"Attribute {attribute.AttributeClass.Name} found");
                            foreach (var genVal in attribute.AttributeClass.TypeArguments)
                            {
                                Log($"gen: {genVal} ({genVal.GetType()})", 1);
                            }

                            foreach (var val in attribute.ConstructorArguments)
                            {
                                if (val.Value != null)
                                    Log($"arg: {val.Value} ({val.Value.GetType()})", 1);
                            }
                        }

                        var terms = new List<Term>();
                        // Write accessable fields first, then attributes (so that we have correct indices)
                        terms.AddRange(ParseTermsFrom(symbolBase));
                        
                        var currentTerm = new Term();
                        foreach (var attr in attributes)
                        {
                            var finishTerm = attr.AttributeClass.AllInterfaces.Any(i => i.Name == "IFinishTerm");
                            var attrName = attr.AttributeClass.Name;
                            Log($"Finish term? {finishTerm}, Name: {attrName}");
                            if (finishTerm)
                            {
                                if (!string.IsNullOrEmpty(currentTerm.NameLeft))
                                    terms.Add(currentTerm);

                                currentTerm = new Term();
                                currentTerm.Traversal = new List<Term.TraversalInfo>();

                                var i = 0;
                                foreach (var genVal in attr.AttributeClass.TypeArguments)
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            currentTerm.NameLeft = genVal.GetTypeName();
                                            break;
                                        case 1:
                                            currentTerm.NameRight = genVal.GetTypeName();
                                            break;
                                        default:
                                            throw new InvalidOperationException($"{i} > 1");
                                    }

                                    i += 1;
                                }

                                currentTerm.Access = attrName switch
                                {
                                    "InAttribute" => Term.AccessModifier.None,
                                    "RefAttribute" => Term.AccessModifier.None,
                                    "WriteAttribute" => Term.AccessModifier.Out,
                                    "NoneAttribute" => Term.AccessModifier.None,
                                    _ => throw new InvalidOperationException($"no modifier for '{attrName}'")
                                };
                                currentTerm.Kind = attrName switch
                                {
                                    "InAttribute" => Term.FilterKind.And,
                                    "NoneAttribute" => Term.FilterKind.Not,
                                    "None" => Term.FilterKind.Not,
                                    _ => Term.FilterKind.And
                                };
                                currentTerm.SrcEntity = attrName switch
                                {
                                    "WriteAttribute" => "default",
                                    _ => null
                                };
                            }
                        }

                        if (!string.IsNullOrEmpty(currentTerm.NameLeft))
                        {
                            terms.Add(currentTerm);
                            Log("Finished current term");
                        }

                        // Remove duplicates
                        terms.RemoveAll(
                            t => t.FieldName == null && terms.Any(
                                ot => ot.FieldName != null && ot.NameLeft == t.NameLeft && ot.NameRight == t.NameRight
                            )
                        );

                        var kind = EKind.Filter;
                        if (symbolBase.AllInterfaces.Any(iface => iface.Name == "ISystem"))
                            kind = EKind.System;
                        else if (symbolBase.AllInterfaces.Any(iface => iface.Name == "IEntityObserver"))
                            kind = EKind.Observer;
                        else if (symbolBase.AllInterfaces.Any(iface => iface.Name == "IProcessor"))
                            kind = EKind.Processor;

                        var export = Export.FromIncludeSelf(symbolBase);
                        GenerateDsl(terms, export with
                        {
                            Kind = kind
                        }, symbolBase, semanticModel);
                    }
                    catch (IgnoreFailFastException)
                    {
                        // ignore
                    }
                    catch (Exception ex)
                    {
                        Context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                            "MG001",
                            $"Exception {ex.GetType()} on generation",
                            $"{ex.GetType()}: {ex.Message}",
                            "?",
                            DiagnosticSeverity.Error,
                            true,
                            ex.ToString()
                        ), prevLocation));

                        Log($"{ex}", 1);
                    }
                });
            }
        }
    }

    private void AppendStaticEntityPath(StringBuilder sb, string qualifiedTypeName)
    {
        sb.Append('(');
        sb.Append("revghost.flecs.StaticEntity<");
        sb.Append(qualifiedTypeName);
        sb.Append(">.FullPath)");
    }
    
    private void AppendStaticEntityId(CodeBuilder sb, string qualifiedTypeName)
    {
        sb.Append('(');
        sb.Append("revghost.flecs.StaticEntity<");
        sb.Append(qualifiedTypeName);
        sb.Append(">.Id)");
    }

    public void GenerateDsl(IList<Term> terms, Export export, ISymbol symbol, SemanticModel model)
    {
        var codeBuilder = new CodeBuilder();
        codeBuilder.AppendLine("using __FLECS__ = flecs_hub.flecs;");
        codeBuilder.AppendLine("using System.Runtime.InteropServices;");
        codeBuilder.AppendLine("using System.Runtime.CompilerServices;");
        foreach (var import in export.Imports)
        {
            codeBuilder.AppendLine(import);
        }
        codeBuilder.AppendLine();
        codeBuilder.AppendLine();
        // HACK: Global namespaces start with '<'
        if (!export.Namespace.StartsWith("<"))
        {
            codeBuilder.Append("namespace ");
            codeBuilder.Append(export.Namespace);
            codeBuilder.Append(";");
        }
        codeBuilder.AppendLine();
        codeBuilder.AppendLine();
        foreach (var parent in export.Parents)
        {
            codeBuilder.AppendLine("unsafe partial ");
            codeBuilder.Append(parent.TypeKind);
            codeBuilder.Append(' ');
            codeBuilder.Append(parent.Name);
            if (parent.Arity > 0)
            {
                codeBuilder.Append('<');
                for (var i = 0; i < parent.Arity; i++)
                {
                    if (i > 0) codeBuilder.Append(',');
                    codeBuilder.Append('T');
                    codeBuilder.Append(i.ToString());
                }
                codeBuilder.Append('>');
            }
            codeBuilder.BeginBracket();
        }
        
        {
            codeBuilder.AppendLine("public static ReadOnlySpan<__FLECS__.ecs_term_t> Terms => new __FLECS__.ecs_term_t[]");
            codeBuilder.BeginBracket();
            foreach (var term in terms)
            {
                if (term.IsFilter)
                    continue;
                
                codeBuilder.AppendLine("new()");
                codeBuilder.BeginBracket();
                
                codeBuilder.AppendLine("id = ");
                var idBuilder = new CodeBuilder();
                if (term.IsPair)
                {
                    idBuilder.Append("__FLECS__.ecs_pair(");
                    AppendStaticEntityId(idBuilder, term.NameLeft);
                    idBuilder.Append(',');
                    AppendStaticEntityId(idBuilder, term.NameRight);
                    idBuilder.Append(')');
                }
                else
                {
                    AppendStaticEntityId(idBuilder, term.NameLeft);
                }
                codeBuilder.Append(idBuilder.ToString());

                codeBuilder.Append(',');
                if (term.Access != Term.AccessModifier.None)
                {
                    codeBuilder.AppendLine("inout = __FLECS__.ecs_inout_kind_t.Ecs");
                    codeBuilder.Append(term.Access.ToString());
                    codeBuilder.Append(',');
                }
                else
                {
                    codeBuilder.AppendLine("inout = __FLECS__.ecs_inout_kind_t.EcsInOutNone,");
                }

                if (term.Kind != Term.FilterKind.None)
                {
                    codeBuilder.AppendLine("oper = ");
                    switch (term.Kind)
                    {
                        case Term.FilterKind.Optional:
                            codeBuilder.Append("__FLECS__.ecs_oper_kind_t.EcsOptional");
                            break;
                        case Term.FilterKind.And:
                            codeBuilder.Append("__FLECS__.ecs_oper_kind_t.EcsAnd");
                            break;
                        case Term.FilterKind.Not:
                            codeBuilder.Append("__FLECS__.ecs_oper_kind_t.EcsNot");
                            break;
                    }

                    codeBuilder.Append(',');
                }

                codeBuilder.AppendLine("src = new()");
                codeBuilder.BeginBracket();
                if (term.Traversal.Count > 0)
                {
                    var data = string.Empty;
                    for (var i = 0; i < term.Traversal.Count; i++)
                    {
                        var traversal = term.Traversal[i];
                        if (i > 0) data += '|';

                        if (traversal.Builtin)
                            data += $"__FLECS__.{traversal.Name}";
                        else
                            throw new InvalidOperationException("no support for non builtin traversals yet");
                    }

                    codeBuilder.AppendLine($"flags = new() {{ Data = {data} }},");
                }

                if (term.SrcEntity != null)
                {
                    codeBuilder.AppendLine();
                    if (term.SrcEntity == "$singleton")
                    {
                        codeBuilder.Append($"id = {idBuilder.ToString()},");
                    }
                    else
                    {
                        codeBuilder.AppendLine($"id = {term.SrcEntity},");
                        codeBuilder.AppendLine($"flags = __FLECS__.EcsIsEntity,");
                    }
                }

                codeBuilder.EndBracket();
                codeBuilder.Append(',');

                codeBuilder.EndBracket();
                codeBuilder.Append(',');
            }
            codeBuilder.EndBracket();
            codeBuilder.Append(';');
            codeBuilder.AppendLine("public static __FLECS__.ecs_filter_desc_t Filter = new()");
            codeBuilder.BeginBracket();
            {
                codeBuilder.AppendLine("terms_buffer = (__FLECS__.ecs_term_t*) Unsafe.AsPointer(ref MemoryMarshal.GetReference(Terms)),");
                codeBuilder.AppendLine("terms_buffer_count = Terms.Length,");
            }
            codeBuilder.EndBracket();
            codeBuilder.Append(';');
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("static __FLECS__.ecs_filter_desc_t global::revghost.flecs.IEntityFilter.GetFilter() => Filter;");
        }
        
        codeBuilder.AppendLine("public global::revghost.flecs.Entity Entity { get; }");
        codeBuilder.AppendLine("public global::revghost.flecs.EntityId Id { get; }");

        // Partial each
        if (export.Kind is EKind.System or EKind.Processor or EKind.Observer) {
            var callback = "&EachUnmanaged";
            if (export.HasRecursiveArity)
            {
                callback = "(delegate*unmanaged<__FLECS__.ecs_iter_t*, void>) Marshal.GetFunctionPointerForDelegate(EachUnmanaged)";
            }
            
            codeBuilder.AppendLine($"static __FLECS__.ecs_iter_action_t global::revghost.flecs.IProcessor.GetAction() => new() {{ Pointer = {callback} }};");

            // System state
            {
                codeBuilder.AppendLine("public struct __SYSTEM_STATE__");
                codeBuilder.BeginBracket();
                foreach (var term in terms)
                {
                    if (!term.IsFilter)
                        continue;

                    codeBuilder.AppendLine($"public __FLECS__.ecs_query_t* {term.FieldName};");
                }

                codeBuilder.EndBracket();
            }

            // Setup
            {
                codeBuilder.AppendLine("public static unsafe void Setup(revghost.flecs.World world)");
                codeBuilder.BeginBracket();
                {
                    codeBuilder.AppendLine("var entity = __FLECS__.ecs_get_scope(world.Handle);");
                    // Build state
                    {
                        // TODO: We need to free this (We need observables implementation ASAP)
                        codeBuilder.AppendLine($"var state = (__SYSTEM_STATE__*) NativeMemory.AllocZeroed((nuint) Unsafe.SizeOf<__SYSTEM_STATE__>());");
                        foreach (var term in terms)
                        {
                            if (!term.IsFilter)
                            {
                                // Assert that static entity exists
                                if (!string.IsNullOrEmpty(term.NameLeft))
                                    codeBuilder.AppendLine($"_ = world.Get<{term.NameLeft}>();");
                                if (!string.IsNullOrEmpty(term.NameRight))
                                    codeBuilder.AppendLine($"_ = world.Get<{term.NameRight}>();");
                                
                                continue;
                            }

                            codeBuilder.BeginBracket();
                            codeBuilder.AppendLine("var queryDesc = new __FLECS__.ecs_query_desc_t()");
                            codeBuilder.BeginBracket();
                            codeBuilder.AppendLine($"filter = {term.Symbol.Type.GetTypeName()}.Filter");
                            codeBuilder.EndBracket();
                            codeBuilder.Append(";");
                            codeBuilder.AppendLine($"state->{term.FieldName} = __FLECS__.ecs_query_init(world.Handle, &queryDesc);");
                            codeBuilder.EndBracket();
                        }
                    }
                    
                    if (export.Kind == EKind.System)
                        codeBuilder.AppendLine($"ProcessorUtility.SetupSystemManaged(world.Handle, entity, Filter, state, typeof(__THIS__), {callback});");
                    else if (export.Kind == EKind.Observer)
                        codeBuilder.AppendLine($"ProcessorUtility.SetupObserverManaged(world.Handle, entity, Filter, state, typeof(__THIS__), {callback});");
                    
                    codeBuilder.AppendLine("_ = nameof(Each); // reference it so the editor and compiler doesn't complain it's unused");
                }
                codeBuilder.EndBracket();
            }

            // Each
            try
            {
                var transformedTerms = new Dictionary<string, List<FieldInfo>>();

                void AddTerms(string key, IList<Term> terms)
                {
                    ref var list =
                        ref CollectionsMarshal.GetValueRefOrAddDefault(transformedTerms, key, out var exists);
                    list ??= new List<FieldInfo>();

                    list.Add(new FieldInfo {IsBuiltin = true, Name = "Entity"});
                    list.Add(new FieldInfo {IsBuiltin = true, Name = "Id"});
                    if (string.IsNullOrEmpty(key) && export.Kind != EKind.Filter)
                    {
                        list.Add(new FieldInfo {IsBuiltin = true, Name = "SystemDeltaTime"});
                        list.Add(new FieldInfo {IsBuiltin = true, Name = "DeltaTime"});
                        list.Add(new FieldInfo {IsBuiltin = true, Name = "World"});
                        list.Add(new FieldInfo {IsBuiltin = true, Name = "RealWorld"});

                        //if (export.Kind is EKind.Observer)
                        {
                            list.Add(new FieldInfo {IsBuiltin = true, Name = "Event"});
                            list.Add(new FieldInfo {IsBuiltin = true, Name = "EventId"});
                        }
                    }

                    foreach (var term in terms)
                    {
                        if (term.Symbol == null)
                            continue;

                        if (!term.HaveFieldAccess)
                            continue;

                        if (term.IsFilter)
                        {
                            AddTerms(term.FieldName, ParseTermsFrom(term.Symbol.Type));
                            continue;
                        }

                        list.Add(new FieldInfo
                        {
                            Name = term.Symbol.Name,
                            TypeName = term.Symbol.Type.GetTypeNameWithoutAnnotations(),
                            IsSingle = !string.IsNullOrEmpty(term.SrcEntity)
                        });
                    }
                }

                AddTerms(string.Empty, terms);
                
                var eachCompiler = new EachCompiler();
                eachCompiler.Builder = new(codeBuilder);
                eachCompiler.Terms = transformedTerms;
                eachCompiler.IsForeach = export.Kind == EKind.Observer || terms.Any(t => string.IsNullOrEmpty(t.SrcEntity));
                eachCompiler.Body = ((INamedTypeSymbol) symbol).GetMembers("Each").First().DeclaringSyntaxReferences
                    .First()
                    .GetSyntax()
                    .ChildNodes()
                    .Last();
                eachCompiler.Log = (str) => Log(str, 1);
                eachCompiler.HasArity = export.HasRecursiveArity;
                eachCompiler.originFields = ((INamedTypeSymbol) symbol).GetMembers().OfType<IFieldSymbol>().ToArray();
                eachCompiler.Model = model;
                eachCompiler.origin = symbol;
                eachCompiler.Compile();
                
                eachCompiler.Builder.FlushScope();
                codeBuilder.Append(eachCompiler.Builder.ToString());
            }
            catch (Exception ex)
            {
                Log($"EX in {symbol}: {ex}");
            }
        }
        // Filters
        else
        {
            // Each
            codeBuilder.AppendLine("///<summary>");
            codeBuilder.AppendLine("/// Iterate entities from a filter");
            codeBuilder.AppendLine("///</summary>");
            codeBuilder.AppendLine("///<remarks>");
            codeBuilder.AppendLine("///This should only be called in a System or a Processor");
            codeBuilder.AppendLine("///</remarks>");
            codeBuilder.AppendLine("public Span<__THIS__>.Enumerator GetEnumerator() => throw new global::System.Diagnostics.UnreachableException(\"This code should have been replaced\");");
        }

        // Log(codeBuilder.ToString());

        var hint = "Dsl";
        foreach (var exp in export.Parents)
        {
            hint += $".{exp.Name}";
        }

        lock (this) {
            var thisType = export.Current.Name;
            if (export.Current.Arity > 0)
            {
                var genericArgs = string.Empty;
                for (var i = 0; i < export.Current.Arity; i++)
                {
                    if (i > 0) genericArgs += ",";
                    genericArgs += $"T{i}";
                }
                        
                thisType = $"{thisType}<{genericArgs}>";
            }

            var result = codeBuilder
                .ToString()
                .Replace("__THIS__", thisType);
            
            _finalMap[hint] = result;
            Context.AddSource(hint, result);
        }
    }

    public record struct ExternalSymbol(string Visibility, string TypeKind, string Name, int Arity);

    public record struct Export(IList<string> Imports, string Namespace, IList<ExternalSymbol> Parents)
    {
        public EKind Kind;
        public ExternalSymbol Current => Parents[^1];
        public bool HasRecursiveArity => Parents.Any(p => p.Arity > 0);

        private static IEnumerable<INamedTypeSymbol> GetParentTypes(ISymbol method)
        {
            var type = method.ContainingType;
            while (type != null)
            {
                yield return type;
                type = type.ContainingType;
            }
        }

        public static Export FromIncludeSelf(INamedTypeSymbol symbol)
        {
            return new Export(
                symbol.DeclaringSyntaxReferences
                    .First()
                    .SyntaxTree
                    .GetRoot()
                    .DescendantNodes()
                    .Where(node => node is UsingDirectiveSyntax)
                    .Select(node => node.ToString())
                    .ToArray(),
                symbol.ContainingNamespace?.ToString() ?? string.Empty,
                GetParentTypes(symbol)
                    .Reverse()
                    .Append(symbol)
                    .Select(s => new ExternalSymbol(
                        Visibility: s.DeclaredAccessibility.ToString().ToLower(),
                        TypeKind: s.TypeKind.ToString().ToLower(),
                        Name: s.Name,
                        Arity: s.Arity
                    ))
                    .ToArray()
            );
        }
    }
    
    public struct Term
    {
        public IFieldSymbol Symbol;
        
        public enum AccessModifier
        {
            In,
            Out,
            InOut,
            None
        }

        public enum FilterKind
        {
            None,
            Optional,
            And,
            Not
        }

        public record struct TraversalInfo(bool Builtin, string Name);
        
        public AccessModifier Access;
        public FilterKind Kind;
        public IList<TraversalInfo> Traversal;
        
        public string NameLeft;
        public string? NameRight;
        public string FieldName;

        public string? SrcEntity; // src.entity (used for Write())

        public bool HaveFieldAccess => Access != AccessModifier.None;
        public bool IsFilter => Symbol != null && Symbol.Type.Interfaces.Any(iface => iface.Name == "IEntityFilter");
        public string FieldType => IsPair ? NameRight : NameLeft;

        [MemberNotNullWhen(true, nameof(NameRight))]
        public bool IsPair => NameRight != null;
    }
}