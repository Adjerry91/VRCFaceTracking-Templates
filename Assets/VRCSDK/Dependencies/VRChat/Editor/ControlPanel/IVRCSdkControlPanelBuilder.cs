
public interface IVRCSdkControlPanelBuilder
{
    void ShowSettingsOptions();
    bool IsValidBuilder(out string message);
    void ShowBuilder();
    void RegisterBuilder(VRCSdkControlPanel baseBuilder);
    void SelectAllComponents();
}
