using Ceres.Annotations;
namespace Kurisu.NGDT
{
    [NodeInfo("Editor Module: Template Module is used to set default piece or option for current graph.")]
    [CeresGroup("Editor")]
    [ModuleOf(typeof(Piece))]
    [ModuleOf(typeof(Option))]
    public class TemplateModule : EditorModule
    {

    }
}
