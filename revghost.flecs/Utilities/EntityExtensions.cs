using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Drawing;
using flecs_hub;
using static flecs_hub.flecs;

namespace revghost.flecs;

public static unsafe class EntityExtensions
{
    public static void Run<T>(this Entity entity, float deltaTime, ref T arguments)
    {
        fixed (T* ptrArg = &arguments)
        {
            ecs_run(entity.World.Handle, entity.Id, deltaTime, ptrArg);
        }
    }

    public static void Run(this Entity entity, float deltaTime)
    {
        if (entity.Has(EcsPhase))
        {
            var desc = new ecs_filter_desc_t();
            desc.terms[0] = new ecs_term_t
            {
                id = entity.Id
            };

            var filter = ecs_filter_init(entity.World.Handle, &desc);
            foreach (var ent in new IteratorRoot(ecs_filter_iter(entity.World.Handle, filter)).Entities)
            {
                (entity with {Id = ent}).Run(deltaTime);
            }
            ecs_filter_fini(filter);
            
            return;
        }
        
        ecs_run(entity.World.Handle, entity.Id, deltaTime, null);
    }

    public static Entity AddChild(this Entity parent, Entity child)
    {
        child.Add((EcsChildOf, parent));
        return parent;
    }

    public static Entity AddChildIsA(this Entity parent, Entity child, ReadOnlySpan<char> name)
    {
        child.World.New(new World.EntityCreation
        {
            Name = name,
            Add = stackalloc Identifier[]
            {
                PairId.From(EcsChildOf, parent),
                PairId.From(EcsIsA, child),
            }
        });

        return parent;
    }

    public static Entity SetParent(this Entity child, Entity parent)
    {
        child.Add((EcsChildOf, parent));
        return child;
    }
    
    public static Entity GetParent(this Entity child)
    {
        return child with {Id = ecs_get_parent(child.World.Handle, child.Id)};
    }

    public static Entity DependsOn(this Entity entity, Entity dependency)
    {
        entity.Add((EcsDependsOn, dependency));
        return entity;
    }

    public static IteratorRoot Children(this Entity parent)
    {
        return new IteratorRoot(ecs_children(parent.World.Handle, parent.Id));
    }

    public static IteratorRoot IterateFirst(this Entity entity, Entity left)
    {
        var term = new ecs_term_t
        {
            id = Pair.From(left, entity).Id.Handle
        };

        return new IteratorRoot(ecs_term_iter(entity.World.Handle, &term));
    }

    public static IteratorRoot IterateFirst<TLeft>(this Entity entity)
        where TLeft : IComponent
    {
        var term = new ecs_term_t
        {
            id = PairId.From(entity.World.Get<TLeft>(), entity).Handle
        };

        return new IteratorRoot(ecs_term_iter(entity.World.Handle, &term));
    }

    public static IteratorRoot IterateSecond(this Entity entity, Entity right)
    {
        var term = new ecs_term_t
        {
            id = Pair.From(entity, right).Id.Handle
        };

        return new IteratorRoot(ecs_term_iter(entity.World.Handle, &term));
    }
    
    public static IteratorRoot IterateSecond<TRight>(this Entity entity)
        where TRight : IComponent
    {
        var term = new ecs_term_t
        {
            id = PairId.From(entity, entity.World.Get<TRight>().Id).Handle
        };

        return new IteratorRoot(ecs_term_iter(entity.World.Handle, &term));
    }

    public static Entity SetDocumentName(this Entity entity, ReadOnlySpan<char> chars)
    {
        using var nstring = new NativeString(chars);
        ecs_doc_set_name(entity.World.Handle, entity.Id, nstring);
        return entity;
    }
    
    public static void SetDocumentName(this Entity entity, ReadOnlySpan<byte> chars)
    {
        using var nstring = new NativeString(chars);
        ecs_doc_set_name(entity.World.Handle, entity.Id, nstring);
    }
    
    public static NativeStringView GetDocumentName(this Entity entity)
    {
        return new NativeStringView(ecs_doc_get_name(entity.World.Handle, entity.Id));
    }

    public static Entity SetDocumentColor(this Entity entity, ReadOnlySpan<char> htmlColor)
    {
        using var nstring = new NativeString(htmlColor);
        ecs_doc_set_color(entity.World.Handle, entity.Id, nstring);

        return entity;
    }

    public static Entity SetDocumentColor(this Entity entity, Color color)
    {
        const string Format = "#RRGGBB";

        Span<char> chars = stackalloc char[Format.Length];
        chars[0] = '#';
        color.R.TryFormat(chars[1..], out _, "X2");
        color.G.TryFormat(chars[3..], out _, "X2");
        color.B.TryFormat(chars[5..], out _, "X2");

        return entity.SetDocumentColor(chars);
    }
}