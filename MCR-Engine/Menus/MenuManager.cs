using MCR_Engine.Core;
using MCR_Engine.Rendering.UI;

namespace MCR_Engine.Menus;

public class MenuManager
    {
        private const int MenuWidthPercent = 50;
        private readonly InfoBox _infoBox;
        private readonly List<Menu> _menus;
        internal MenuManager()
        {
            _infoBox = new InfoBox(MenuWidth, MenuHeight, InfoBoxX, InfoBoxY, true, true);
            _menus = [];
        }
        public static int InfoBoxX => (Console.WindowWidth - 2) / 2 - MenuWidth / 2;
        public static int InfoBoxY => 2;
        public static int MenuWidth => Console.WindowWidth * MenuWidthPercent / 100;
        public static int MenuHeight => 17;
        public void Add(Menu menu) => _menus.Add(menu);
        public ProgramState Launch(UIManager ui, Type? screenType = null)
        {
            ui.Clear(); ui.Add(_infoBox);
            ProgramState newState = ProgramState.Menu;
            do
            {
                switch (newState)
                {
                    case ProgramState.Menu: 
                        _infoBox.Visible = true; newState = GetMenu(screenType).Launch(_infoBox, out screenType); break;
                    case ProgramState.Editor: _infoBox.Visible = false; return ProgramState.Editor;
                    case ProgramState.Game: _infoBox.Visible = false; return ProgramState.Game;
                    case ProgramState.Quit: _infoBox.Visible = false; return ProgramState.Quit;
                    default: throw new NotImplementedException();
                }
            } while (true);
        }
        private Menu GetMenu(Type? screenType)
        {
            if (screenType == null)
                return _menus[0];
            foreach (Menu menu in _menus)
            {
                if (menu.GetType() == screenType)
                    return menu;
            }
            throw new KeyNotFoundException("Screen not found");
        }
    }