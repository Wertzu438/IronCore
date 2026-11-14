namespace MCR_Engine.Core;

public abstract class ProcessTemplate
{
    protected ProcessAction? Action;
    protected ProcessState State;
    internal ProcessTemplate()
    {
        Action = null;
        State = ProcessState.Ready;
    }
    public void ChangeAction(ProcessAction newAction)
    {
        if (newAction is ProcessAction.Start && State is ProcessState.Ready or ProcessState.Paused)
        {
            Action = newAction;
            if (State is not ProcessState.Paused)
                StartLoop();
            else WaitForProcessState(ProcessState.Running);
        }
        else if (newAction is ProcessAction.Pause && State is ProcessState.Running)
        {
            Action = newAction;
            WaitForProcessState(ProcessState.Paused);
        }
        else if (newAction is ProcessAction.Stop && State is ProcessState.Running or ProcessState.Paused)
        {
            Action = newAction;
            WaitForProcessState(ProcessState.Stopped);
        }
        else throw new ArgumentException("Invalid action");
    }
    protected abstract void StartLoop();
    private void WaitForProcessState(ProcessState state)
    {
        int ticks = 0;
        while (State != state && ticks < 400)
        {
            ticks++;
            Task.Delay(1).Wait();
        }
        if (ticks >= 400)
            throw new Exception("Process state timed out");
    }
}