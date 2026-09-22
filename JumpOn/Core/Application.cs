using JumpOn.Entities;
using MCR_Engine;
using MCR_Engine.Core;

namespace JumpOn;

public class Application : IGameProject
{
    private readonly Engine _engine;
    public Application(Engine engine)
    {
        _engine = engine;
    }
    public ProgramState OpenEditor()
    {
        throw new NotImplementedException();
    }
    public ProgramState PlayGame()
    {
        //SET SCENE HIER
        _engine.SetScene(LevelLoader.Read("test.txt", _engine));
        Game game = new Game(_engine);
        game.Play();
        return ProgramState.Game;
    }
    public ProgramState OpenMenu()
    {
        return ProgramState.Game;
        throw new NotImplementedException();
    }
}