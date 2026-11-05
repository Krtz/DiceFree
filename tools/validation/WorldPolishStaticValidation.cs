using DiceFree.EditorTools;
// Recovery scenes make the complete audit slow. Invoke run_script with
// --timeout_ms 540000 and a CLI --timeout 600; do not interrupt an active audit.
public static class WorldPolishStaticValidation
{
 public static string Run(){ArchitectureValidation.Run();return "Architecture, managed references, assembly ownership and dependency policy passed.";}
}
