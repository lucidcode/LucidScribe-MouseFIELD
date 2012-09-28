using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace lucidcode.LucidScribe.Plugin.Mouse
{

  public static class Device
  {

    private static bool m_boolInitialized;
    static int m_intLastX = -1;
    static int m_intLastY = -1;
    static double m_dblX;
    static double m_dblY;
    static double m_dblB;
    static double m_dblFIELD;
    static double m_dblFCount;
    static Boolean m_boolFirstRun = false;
    static Boolean m_boolCountdown = false;
    static Thread m_objMouseTimer;

    public static Boolean Initialize()
    {
      if (!m_boolInitialized)
      {

        // Make sure the monitoring process is not already running
        Process[] arrProcesses = Process.GetProcessesByName("lucidcode.LucidScribe.Plugin.MouseMonitor");
        if (arrProcesses.Length == 0)
        {
            Process.Start(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\lucidcode.LucidScribe.Plugin.MouseMonitor.exe");
        }

        // Start the update thread
        m_objMouseTimer = new Thread(new ThreadStart(UpdateMouse));
        m_objMouseTimer.Start();

        m_boolInitialized = true;
      }
      return true;
    }

    public static void Dispose()
    {
      if (m_boolInitialized)
      {
        m_boolInitialized = false;
        Process[] arrProcesses = Process.GetProcessesByName("lucidcode.LucidScribe.Plugin.MouseMonitor");
        if (arrProcesses.Length > 0)
        {
          arrProcesses[0].Kill();
        }
      }
    }

    private static void UpdateMouse()
    {
        do
        {
            Thread.Sleep(100);
            Process[] arrProcesses = Process.GetProcessesByName("lucidcode.LucidScribe.Plugin.MouseMonitor");

            if (arrProcesses.Length == 0)
            {
                m_dblFIELD = 0;
                m_dblB = 0;
                return;
            }

            String strValues = arrProcesses[0].MainWindowTitle.Replace("LucidScribe.Mouse(", "").Replace(")", "");
            String[] arrValues = strValues.Split(',');
            if (arrValues.Length > 2)
            {
                int intX = Convert.ToInt32(arrValues[0]);
                int intY = Convert.ToInt32(arrValues[1]);
                int intB = Convert.ToInt32(arrValues[2]);

                if (m_boolFirstRun)
                {
                    m_boolFirstRun = false;
                    m_intLastX = intX;
                    m_intLastY = intY;
                    return;
                }

                int intDeltaX = m_intLastX - intX;
                int intDeltaY = m_intLastY - intY;

                if (intDeltaX < 0) intDeltaX *= -1;
                if (intDeltaY < 0) intDeltaY *= -1;

                m_dblX += intDeltaX;
                m_dblY += intDeltaY;

                m_intLastX = intX;
                m_intLastY = intY;

                if (intB == 0)
                {
                    if (m_dblB > 0)
                    {
                        m_dblB += 1;
                        if (m_dblB > 800)
                        {
                            m_dblB = 800;
                        }
                    }
                }
                else if (intB == 1)
                {
                    if (m_dblB == 0)
                    {
                        m_dblB = 200;
                    }
                    else
                    {
                        m_dblB += 1;
                        if (m_dblB > 800)
                        {
                            m_dblB = 800;
                        }
                    }
                }
                else if (intB == -1)
                {
                    if (m_dblB == 800)
                    {
                        m_dblFCount = 220;
                    }
                    m_dblB = 0;
                }
            }

        } while (true);
    }

    public static Double GetX()
    {
      double dblValue = m_dblX;
      m_dblX = 0;
      return dblValue;
    }

    public static Double GetY()
    {
      double dblValue = m_dblY;
      m_dblY = 0;
      return dblValue;
    }

    public static Double ReadX()
    {
      return m_dblX;
    }

    public static Double ReadY()
    {
      return m_dblY;
    }

    public static Double GetB()
    {
      if (m_dblB > 0)
      {
        m_dblB += 1;
        if (m_dblB > 800)
        {
          m_dblB = 800;
        }
      }

      double dblValue = m_dblB;
      return dblValue;
    }

    public static Double GetFIELD()
    {
      if (m_dblFCount > 0)
      {
        m_dblFCount = m_dblFCount - 1;
        m_dblFIELD = 888;
      }
      else
      {
        m_dblFIELD = 0;
      }
      return m_dblFIELD;
    }

  }

  namespace Button
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "Button"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetB();
          if (dblValue > 999) { dblValue = 999; }
          return dblValue;
        }
      }
      public override void Dispose()
      {
        Device.Dispose();
      }
    }
  }

  namespace FIELD
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "FIELD"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetFIELD();
          if (dblValue > 999) { dblValue = 999; }
          return dblValue;
        }
      }
      public override void Dispose()
      {
        Device.Dispose();
      }
    }
  }

  namespace X
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "Mouse X"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetX();
          if (dblValue > 999) { dblValue = 999; }
          return dblValue;
        }
      }
      public override void Dispose()
      {
        Device.Dispose();
      }
    }
  }

  namespace Y
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "Mouse Y"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetY();
          if (dblValue > 999) { dblValue = 999; }
          return dblValue;
        }
      }
      public override void Dispose()
      {
        Device.Dispose();
      }
    }
  }

}
