using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public class ChessAI
{
    private Process engine;

    /// <summary>Invoked on a background thread when Stockfish outputs a "bestmove" line. Main should CallDeferred to handle on main thread.</summary>
    public Action<string> OnBestMoveReceived;

    public void Start()
    {
        engine = new Process();
        string path = ProjectSettings.GlobalizePath("res://engines/stockfish.exe");
        engine.StartInfo.FileName = path;
        engine.StartInfo.UseShellExecute = false;
        engine.StartInfo.RedirectStandardInput = true;
        engine.StartInfo.RedirectStandardOutput = true;
        engine.StartInfo.CreateNoWindow = true;

        engine.Start();

        Task.Run(ReadOutput);

        SendCommand("uci");
    }

    public void SendCommand(string command)
    {
        engine.StandardInput.WriteLine(command);
    }

    void ReadOutput()
    {
        while (!engine.StandardOutput.EndOfStream)
        {
            string line = engine.StandardOutput.ReadLine();
            GD.Print("[Stockfish] " + line);
            if (line != null && line.StartsWith("bestmove "))
                OnBestMoveReceived?.Invoke(line);
        }
    }
}
