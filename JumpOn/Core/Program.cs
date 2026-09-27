using System.Text;
using MCR_Engine.Core;

namespace JumpOn;

class Program
{
    static void Main(string[] args)
    {
        Console.CursorVisible = false;
        Console.OutputEncoding = Encoding.UTF8;
        Engine engine = new Engine();
        Application app = new Application(engine);
        engine.Run(app);
    }
}