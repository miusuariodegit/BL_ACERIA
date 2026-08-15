namespace GPX.Web.Data {

// [GPX-DOC-v1] ================================================================================
// Records usados para construir el menu de navegacion agrupado por modulo padre/hijo.
// ================================================================================================
    /// <summary>
    /// Registro ModuleDefinition. Records usados para construir el menu de navegacion agrupado por modulo
    /// padre/hijo.
    /// </summary>
    public sealed record ModuleDefinition(
        string Code,
        string Name,
        string Route,
        string Description,
        string IconCssClass,
        string ParentCode,
        string ParentName,
        string ParentIconCssClass,
        int ParentDisplayOrder,
        int DisplayOrder);

    /// <summary>
    /// Registro ModuleGroupDefinition. Records usados para construir el menu de navegacion agrupado por
    /// modulo padre/hijo.
    /// </summary>
    public sealed record ModuleGroupDefinition(
        string Code,
        string Name,
        string IconCssClass,
        int DisplayOrder);
}
