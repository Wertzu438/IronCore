using MCR_Engine.Core;

namespace JumpOn;

class Program
{
    static void Main(string[] args)
    {
        Console.CursorVisible = false;
        Engine engine = new Engine();
        Application app = new Application(engine);
        engine.Run(app);
    }
}