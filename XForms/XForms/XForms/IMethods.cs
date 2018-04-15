
namespace XForms.XForms
{
    public interface IXFormBuild
    {
        void SetTitle(string title);
        void AddGroup(int groupid);
        void AddControl(Controls control, int? sectionID);
        void AddPageStub(int groupid, int? sectionID);
    }
}
