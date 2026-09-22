using MCR_Engine.Core;
using MCR_Engine.Rendering.UI;

namespace MCR_Engine.Menus;

public class MenuManager
    {
        private const int MenuWidthPercent = 50;
        //private readonly Engine engine;
        private readonly UIElement infoBox;
        private readonly List<Menu> menus;
        internal MenuManager()//Engine engine)
        {
            infoBox = new UIElement(MenuWidth, MenuHeight, InfoBoxX, InfoBoxY, false, true);
            //this.engine = engine;
            menus = [];
        }
        public UIElement InfoBox => infoBox;
        public static int InfoBoxX => (Console.WindowWidth - 2) / 2 - MenuWidth / 2;
        public static int InfoBoxY => 2;
        public static int MenuWidth => Console.WindowWidth * MenuWidthPercent / 100;
        public static int MenuHeight => 15;
        public void AddMenu(Menu menu) => menus.Add(menu);
        public ProgramState Launch()
        {
            //engine.UI.Clear();
            //engine.UI.AddUIElement(infoBox);
            ProgramState newState = ProgramState.Menu;
            //engine.Scene.Update(false, true);
            infoBox.Visible = true;
            int? index = 0;
            do
            {
                switch (newState)
                {
                    case ProgramState.Menu: newState = menus[index.Value].Launch(out index); break;
                    case ProgramState.Editor: InfoBox.Visible = false; return ProgramState.Editor;
                    case ProgramState.Game: InfoBox.Visible = false; return ProgramState.Game;
                    case ProgramState.Quit: InfoBox.Visible = false; return ProgramState.Quit;
                    default: throw new NotImplementedException();
                }
            } while (true);
        }
        public static string[] GetGameHeading()
        {
            return 
            [
                "!     ██╗██╗   ██╗███╗   ███╗██████╗    ██████╗ ███╗   ██╗",
                "!     ██║██║   ██║████╗ ████║██╔══██╗  ██╔═══██╗████╗  ██║",
                "!     ██║██║   ██║██╔████╔██║██████╔╝  ██║   ██║██╔██╗ ██║",
                "!██   ██║██║   ██║██║╚██╔╝██║██╔═══╝   ██║   ██║██║╚██╗██║",
                "!╚█████╔╝╚██████╔╝██║ ╚═╝ ██║██║       ╚██████╔╝██║ ╚████║",
                "! ╚════╝  ╚═════╝ ╚═╝     ╚═╝╚═╝        ╚═════╝ ╚═╝  ╚═══╝"
            ];
        }
    }