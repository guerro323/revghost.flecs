# revghost.flecs
High-Level C# Wrapper around **[flecs](https://github.com/SanderMertens/flecs/)** with some source generator goodness!

The Low-Level Wrapper is **[flecs-cs](https://github.com/flecs-hub/flecs-cs/)** with some modifications such as:
- Supporting external constants, directly reading them from the library instead of using pinvoke.

## To do
- Rework the Source Generator code, it's extremely messy for now.
- Add the missing functions from flecs.
- Decide on how modules should work, should we force components to be in a module, etc...
- Remove **revghost2.Runner** folder
- More tests!
- Better logging