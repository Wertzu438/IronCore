namespace MCR_Engine.Core;

public enum ProcessAction
{
    Start, Pause, Stop
}
internal enum ProcessState
{
    Ready, Running, Paused, Stopped
}
public enum Direction
{
    None, Up, Down, Left, Right
}
public enum ProgramState
{
    Menu, Editor, Game, Quit
}