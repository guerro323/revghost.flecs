using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace revghost.flecs.SourceGenerator.SubGenerators;

public class ComponentSubGenerator : SubGenerator
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

    protected override void Generate()
    {
        var tasks = new List<Action>();
        CreateTask(tasks);
        Parallel.Invoke(tasks.ToArray());

        foreach (var list in _logMap)
        {
            Receiver.Log.AddRange(list.Value);
        }
    }

    private void CreateTask(List<Action> tasks)
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

                if (declare is not TypeDeclarationSyntax)
                    continue;
                

                tasks.Add(() =>
                {
                    var symbolBase = (INamedTypeSymbol) semanticModel.GetDeclaredSymbol(declare)!;
                    Log($"found {symbolBase} ?", 0);
                    if (!symbolBase.AllInterfaces.Any(iface => iface.Name == "IComponent" || iface.Name.StartsWith("IComponent<")))
                        return;

                    Log($"use {symbolBase}", 0);
                    Location? prevLocation = null;
                    try
                    {
                        prevLocation = symbolBase.Locations.First();
                        
                        GenerateComponent(Export.FromIncludeSelf(symbolBase), symbolBase);
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

    private void GenerateComponent(Export export, INamedTypeSymbol symbol)
    {
        var attributes = symbol.GetAttributes();
        if (attributes.Any(a => a.AttributeClass!.Name == "DisableSourceGenerator"))
            return;
        
        Log($"generating {symbol.Name}", 1);

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
            if (parent.Arity.Length > 0)
            {
                codeBuilder.Append('<');
                for (var i = 0; i < parent.Arity.Length; i++)
                {
                    if (i > 0) codeBuilder.Append(',');
                    codeBuilder.Append(parent.Arity[i]);
                }
                codeBuilder.Append('>');
            }
            codeBuilder.BeginBracket();
        }
        
        codeBuilder.AppendLine("// Auto Generated Fields (managed, ...)");

        var managedHooksSetup = new Dictionary<string, CodeBuilder>();

        CodeBuilder getHookSetup(string name)
        {
            ref var val = ref CollectionsMarshal.GetValueRefOrAddDefault(managedHooksSetup, name, out _);
            if (val is null)
            {
                val = new CodeBuilder(codeBuilder);
                val.BeginScope();
                val.BeginScope();
            }

            return val;
        }

        foreach (var attr in symbol.GetAttributes())
        {
            if (!attr.AttributeClass!.Name.StartsWith("ManagedField"))
                continue;

            var type = string.Empty;
            var name = (string) attr.ConstructorArguments[0].Value;
            if (attr.AttributeClass!.Arity > 0)
            {
                type = attr.AttributeClass.TypeArguments[0].GetTypeName();
            }
            else
            {
                type = (string) attr.ConstructorArguments[1].Value;
            }
            
            codeBuilder.AppendLine($"private global::revghost.flecs.InternalGCHandle {name}Handle;");
            codeBuilder.AppendLine($"public {type} {name}");
            codeBuilder.BeginBracket();
            codeBuilder.AppendLine($"get => ({type}) {name}Handle.Target;");
            codeBuilder.AppendLine($"set {{ if ({name}Handle.IsAllocated) {name}Handle.Free(); {name}Handle = InternalGCHandle.Alloc(value); }}");
            codeBuilder.EndBracket();
            
            getHookSetup("ctor").AppendLine($"self.{name}Handle = global::revghost.flecs.InternalGCHandle.Alloc(null);");
            getHookSetup("dtor").AppendLine($"if (!self.{name}Handle.Equals(default)) self.{name}Handle.Free();");
            getHookSetup("move").AppendLine($"if (!selfDst.{name}Handle.Equals(default)) selfDst.{name}Handle.Free();");
            getHookSetup("cpy").AppendLine($"if (!selfDst.{name}Handle.Equals(default)) selfDst.{name}Handle.Free();");
        }

        foreach (var member in symbol.GetMembers())
        {
            if (member is not IFieldSymbol field)
                continue;

            if (!field.Type.AllInterfaces.Any(iface => iface.Name.StartsWith("IMemoryObject")))
                continue;
            
            getHookSetup("ctor").AppendLine($"self.{field.Name}.Constructor();");
            getHookSetup("dtor").AppendLine($"self.{field.Name}.Destructor();");
            getHookSetup("move").AppendLine($"selfDst.{field.Name}.Move(ref selfSrc.{field.Name});");
            getHookSetup("cpy").AppendLine($"selfDst.{field.Name}.Copy(in selfSrc.{field.Name});");
        }

        var additionalSetup = new CodeBuilder(codeBuilder);
        var additionalSetupFinal = new CodeBuilder(codeBuilder);
        
        additionalSetupFinal.AppendLine("ComponentUtility.SetHookCallbacks(world.Handle, entity");
        
        foreach (var (method, cb) in managedHooksSetup)
        {
            cb.FlushScope();

            var callback = $"&_{method}";
            if (export.HasRecursiveArity)
            {
                var param = "void*, void*, int, __FLECS__.ecs_type_info_t*";
                if (method is "ctor" or "dtor")
                    param = "void*, int, __FLECS__.ecs_type_info_t*";
                callback = $"(delegate*unmanaged<{param}, void>) Marshal.GetFunctionPointerForDelegate(_{method})";
            }
            
            additionalSetupFinal.AppendLine($", {method}: {callback}");
            if (!export.HasRecursiveArity)
                additionalSetup.AppendLine("[UnmanagedCallersOnly]");
            if (method is "ctor" or "dtor")
            {
                additionalSetup.AppendLine(
                    $"static void _{method}(void* ptr, int count, __FLECS__.ecs_type_info_t* typeInfo)");
                additionalSetup.BeginBracket();
                {
                    additionalSetup.AppendLine("var span = new Span<__THIS__>(ptr, count);");
                    if (method is "ctor")
                        additionalSetup.AppendLine("span.Clear();");
                    
                    additionalSetup.AppendLine("foreach (ref var self in span)");
                    additionalSetup.BeginBracket();
                    additionalSetup.Append(cb.ToString());
                    additionalSetup.EndBracket();
                }
                additionalSetup.EndBracket();
            }
            else
            {
                additionalSetup.AppendLine(
                    $"static void _{method}(void* dst, void* src, int count, __FLECS__.ecs_type_info_t* typeInfo)");
                additionalSetup.BeginBracket();
                {
                    additionalSetup.AppendLine("var spanDst = new Span<__THIS__>(dst, count);");
                    additionalSetup.AppendLine("var spanSrc = new Span<__THIS__>(src, count);");
                    additionalSetup.AppendLine("for (var i = 0; i < count; i++)");
                    additionalSetup.BeginBracket();
                    additionalSetup.AppendLine("ref var selfDst = ref spanDst[i];");
                    additionalSetup.AppendLine("ref var selfSrc = ref spanSrc[i];");
                    additionalSetup.Append(cb.ToString());
                    additionalSetup.EndBracket();
                    if (method is "move")
                    {
                        additionalSetup.AppendLine("spanSrc.CopyTo(spanDst);");
                        additionalSetup.AppendLine("spanSrc.Clear();");
                    }
                    else
                    {
                        additionalSetup.AppendLine("spanSrc.CopyTo(spanDst);");
                    }
                }
                additionalSetup.EndBracket();
            }
        }
        
        additionalSetupFinal.AppendLine(");");
        if (additionalSetup.StringBuilder.Length == 0)
            additionalSetupFinal.StringBuilder.Clear();
        
        additionalSetup.FlushScope();
        additionalSetupFinal.FlushScope();

        codeBuilder.AppendLine("// Implementations WIP");
        codeBuilder.AppendLine($$$"""
    static unsafe void IStaticEntitySetup.Setup(global::revghost.flecs.World world)
    {
        var entity = __FLECS__.ecs_get_scope(world.Handle);
        var desc = new __FLECS__.ecs_component_desc_t
        {
            entity = entity,
            type = new __FLECS__.ecs_type_info_t
            {
                size = ManagedTypeData<__THIS__>.Size,
                alignment = ManagedTypeData<__THIS__>.Alignment
            }
        };
        
        var ret = __FLECS__.ecs_component_init(world.Handle, &desc);

        if (ret.Data.Data != entity.Data.Data)
            throw new InvalidOperationException();

        if (StaticEntity<__THIS__>.Members is {Length: > 0} members)
        {
            foreach (var field in members)
            {
                world.Register(field.Type);
            }
            
            var structDesc = new __FLECS__.ecs_struct_desc_t();
            structDesc.entity = entity;
            
            var i = 0;
            foreach (var field in members)
            {
                // TODO: dispose it (outside of this scope)
                var nativeName = new NativeString(field.Name);
                structDesc.members[i++] = new __FLECS__.ecs_member_t
                {
                    name = nativeName,
                    type = field.Type
                };

                Console.WriteLine($"add member [{i}] {field.Name} ({StaticEntity.GetData(field.Type).Name})");
            }

            __FLECS__.ecs_struct_init(world.Handle, &structDesc);
        }

        {{{additionalSetupFinal}}}
        {{{additionalSetup}}}
    }

    static StaticEntityTypeData.Field[] IStaticEntityIsType.Members()
    {
        var fields = typeof(__THIS__).GetFields((global::System.Reflection.BindingFlags) 0x34);
        return fields.Select(f =>
        {
            return new StaticEntityTypeData.Field
            {
                Name = f.Name,
                Type = (EntityId) typeof(StaticEntity<>)
                    .MakeGenericType(typeof(AnyEntity<>).MakeGenericType(f.FieldType))
                    .GetField("Id")!
                    .GetValue(null)!
            };
        }).ToArray();
    }
""");

        lock (this) {
            var hint = "Comp";
            foreach (var exp in export.Parents)
            {
                hint += $".{exp.Name}";
            }
            
            var thisType = export.Current.Name;
            if (export.Current.Arity.Length > 0)
            {
                var genericArgs = string.Empty;
                for (var i = 0; i < export.Current.Arity.Length; i++)
                {
                    if (i > 0)
                    {
                        genericArgs += ",";
                    }
                    genericArgs += export.Current.Arity[i];
                }
                        
                thisType = $"{thisType}<{genericArgs}>";
            }
            
            var result = codeBuilder
                .ToString()
                .Replace("__THIS__", thisType);
            Context.AddSource(hint, result);
        }
    }
    
    public record struct ExternalSymbol(string Visibility, string TypeKind, string Name, string[] Arity);
    
    public record struct Export(IList<string> Imports, string Namespace, IList<ExternalSymbol> Parents)
    {
        public ExternalSymbol Current => Parents[^1];
        
        public bool HasRecursiveArity => Parents.Any(p => p.Arity.Length > 0);
        
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
                        TypeKind: $"{(s.IsRecord ? "record " : string.Empty)}" + s.TypeKind.ToString().ToLower(),
                        Name: s.Name,
                        Arity: s.TypeParameters.Select(t => t.Name).ToArray()
                    ))
                    .ToArray()
            );
        }
    }
}