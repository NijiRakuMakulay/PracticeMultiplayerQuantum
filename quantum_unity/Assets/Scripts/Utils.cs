using Quantum;

public static class Utils
{
    public static bool TryGetQuantumFrame(out Frame f)
    {
        f = null;
        if (QuantumRunner.Default == null || QuantumRunner.Default.Game == null) { return false; }
        f = QuantumRunner.Default.Game.Frames.Predicted;
        if (f == default) { return false; } else { return true; }
    }
}