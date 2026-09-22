namespace MCR_Engine.Core;

public interface IGameProject
{
    public ProgramState OpenMenu();
    public ProgramState PlayGame();
    public ProgramState OpenEditor();
}