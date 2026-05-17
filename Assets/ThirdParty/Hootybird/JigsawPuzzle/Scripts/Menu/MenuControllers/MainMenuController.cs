using HootyBird.JigsawPuzzleEngine.Tools;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Main menu controller. Active in main menu.
    /// </summary>
    public class MainMenuController : MenuController
    {
        protected override void Awake()
        {
            base.Awake();

            Settings.InternalAppSettings.MainMenuControllerName = name;
        }
    }
}
