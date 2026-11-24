namespace MonitorBridge.App.Models;

public class DisplayMonitor
{
    public string DeviceName {get; set;} = string.Empty;
    public string Name {get; set;} = string.Empty;
    public int Left {get; set;}
    public int Top {get;set;}
    public int Width {get;set;}
    public int Height {get;set;}
    public bool IsPrimary {get;set;}
    public string Resolution => $"{Width} x {Height}";

  public override string ToString()
  {
    return $"{Name} ({Resolution})";
  }
}