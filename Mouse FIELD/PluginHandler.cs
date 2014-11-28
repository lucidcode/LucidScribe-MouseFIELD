using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace lucidcode.LucidScribe.Plugin.Mouse
{

  public static class Device
  {

    private static bool m_boolInitialized;
    static int m_intLastX = -1;
    static int m_intLastY = -1;
    static double m_dblX;
    static double m_dblY;
    static int m_intTotal;
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

                m_intTotal += intDeltaX + intDeltaY;

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

        // Jump to show dashes for TCMP
        if (m_dblB > 220 & m_dblB < 320)
        {
          m_dblB = 320;
        }

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

    public static int GetTotal()
    {
      int value = m_intTotal;
      m_intTotal = 0;
      return value;
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

  namespace REM
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      List<int> History = new List<int>();
      public override string Name
      {
        get { return "Mouse REM"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double total = Device.GetTotal();
          if (total > 999) { total = 999; }
          if (total < 0) { total = 0; }

          History.Add(Convert.ToInt32(total));
          if (History.Count > 256) { History.RemoveAt(0); }

          // Check for eye movements
          int movements = 0;
          foreach (int value in History)
          {
            if (value >= 4)
            {
              movements += 1;
            }
          }

          if (movements >= 8)
          {
            return 888;
          }

          if (movements > 10) { movements = 10; }
          return movements * 100;
        }
      }
      public override void Dispose()
      {
        Device.Dispose();
      }
    }
  }

  namespace MouseTCMP
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase, lucidcode.LucidScribe.TCMP.ITransConsciousnessPlugin
    {

      public override string Name
      {
        get
        {
          return "Mouse TCMP";
        }
      }

      public override bool Initialize()
      {
        try
        {
          return Device.Initialize();
        }
        catch (Exception ex)
        {
          throw (new Exception("The '" + Name + "' plugin failed to initialize: " + ex.Message));
        }
      }

      private static String Morse = "";
      Dictionary<char, String> Code = new Dictionary<char, String>()
          {
              {'A' , ".-"},
              {'B' , "-..."},
              {'C' , "-.-."},
              {'D' , "-.."},
              {'E' , "."},
              {'F' , "..-."},
              {'G' , "--."},
              {'H' , "...."},
              {'I' , ".."},
              {'J' , ".---"},
              {'K' , "-.-"},
              {'L' , ".-.."},
              {'M' , "--"},
              {'N' , "-."},
              {'O' , "---"},
              {'P' , ".--."},
              {'Q' , "--.-"},
              {'R' , ".-."},
              {'S' , "..."},
              {'T' , "-"},
              {'U' , "..-"},
              {'V' , "...-"},
              {'W' , ".--"},
              {'X' , "-..-"},
              {'Y' , "-.--"},
              {'Z' , "--.."},
              {'0' , "-----"},
              {'1' , ".----"},
              {'2' , "..----"},
              {'3' , "...--"},
              {'4' , "....-"},
              {'5' , "....."},
              {'6' , "-...."},
              {'7' , "--..."},
              {'8' , "---.."},
              {'9' , "----."},
          };

      List<int> m_arrHistory = new List<int>();
      Boolean FirstTick = false;
      Boolean SpaceSent = true;
      int TicksSinceSpace = 0;
      Boolean Started = false;
      int PreliminaryTicks = 0;


      public override double Value
      {
        get
        {
          int tempValue = Device.GetTotal();
          if (tempValue > 999) { tempValue = 999; }
          if (tempValue < 0) { tempValue = 0; }

          if (!Started)
          {
            PreliminaryTicks++;
            if (PreliminaryTicks > 10)
            {
              Started = true;
            }

            return 0;
          }

          int signalLength = 0;
          int dotHeight = 32;
          int dashHeight = 100;

          // Update the mem list
          String signal = "";

          if (!FirstTick && (tempValue > dotHeight))
          {
            m_arrHistory.Add(Convert.ToInt32(tempValue));
          }

          if (!FirstTick && m_arrHistory.Count > 0)
          {
            m_arrHistory.Add(Convert.ToInt32(tempValue));
          }

          if (FirstTick && (tempValue > dotHeight))
          {
            FirstTick = false;
          }

          if (!SpaceSent & m_arrHistory.Count == 0)
          {
            TicksSinceSpace++;
            if (TicksSinceSpace > 32)
            {
              // Send the space key
              Morse = " ";
              SendKeys.Send(" ");
              SpaceSent = true;
              TicksSinceSpace = 0;
            }
          }

          if (!FirstTick && m_arrHistory.Count > 32)
          {
            int nextOffset = 0;
            do
            {
              int fivePointValue = 0;
              for (int i = nextOffset; i < m_arrHistory.Count; i++)
              {
                for (int x = i; x < m_arrHistory.Count; x++)
                {
                  if (m_arrHistory[x] > fivePointValue)
                  {
                    fivePointValue = m_arrHistory[x];
                  }

                  if (m_arrHistory[x] < 8)
                  {
                    nextOffset = x + 1;
                    break;
                  }

                  if (x == m_arrHistory.Count - 1)
                  {
                    nextOffset = -1;
                  }
                }

                if (fivePointValue >= dashHeight)
                {
                  signal += "-";
                  signalLength++;
                  break;
                }
                else if (fivePointValue >= dotHeight)
                {
                  signal += ".";
                  signalLength++;
                  break;
                }

                if (i == m_arrHistory.Count - 1)
                {
                  nextOffset = -1;
                }

              }

              if (nextOffset < 0 | nextOffset == m_arrHistory.Count)
              {
                break;
              }

            } while (true);

            m_arrHistory.RemoveAt(0);

            // Check if the signal is morse
            try
            {
              // Make sure that we have a signal
              if (signal != "")
              {
                var myValue = Code.First(x => x.Value == signal);
                Morse = myValue.Key.ToString();
                SendKeys.Send(myValue.Key.ToString());
                signal = "";
                m_arrHistory.Clear();
                SpaceSent = false;
                TicksSinceSpace = 0;
              }
            }
            catch (Exception ex)
            {
              String err = ex.Message;
            }
          }

          if (m_arrHistory.Count > 0)
          { return 888; }

          return 0;
        }
      }

      string lucidcode.LucidScribe.TCMP.ITransConsciousnessPlugin.MorseCode
      {
        get
        {
          String temp = Morse;
          Morse = "";
          return temp;
        }
      }

      public override void Dispose()
      {
        Device.Dispose();
      }

    }

  }

  namespace ButtonTCMP
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase, lucidcode.LucidScribe.TCMP.ITransConsciousnessPlugin
    {

      public override string Name
      {
        get
        {
          return "Button TCMP";
        }
      }

      public override bool Initialize()
      {
        try
        {
          return Device.Initialize();
        }
        catch (Exception ex)
        {
          throw (new Exception("The '" + Name + "' plugin failed to initialize: " + ex.Message));
        }
      }

      private static String Morse = "";
      Dictionary<char, String> Code = new Dictionary<char, String>()
          {
              {'A' , ".-"},
              {'B' , "-..."},
              {'C' , "-.-."},
              {'D' , "-.."},
              {'E' , "."},
              {'F' , "..-."},
              {'G' , "--."},
              {'H' , "...."},
              {'I' , ".."},
              {'J' , ".---"},
              {'K' , "-.-"},
              {'L' , ".-.."},
              {'M' , "--"},
              {'N' , "-."},
              {'O' , "---"},
              {'P' , ".--."},
              {'Q' , "--.-"},
              {'R' , ".-."},
              {'S' , "..."},
              {'T' , "-"},
              {'U' , "..-"},
              {'V' , "...-"},
              {'W' , ".--"},
              {'X' , "-..-"},
              {'Y' , "-.--"},
              {'Z' , "--.."},
              {'0' , "-----"},
              {'1' , ".----"},
              {'2' , "..----"},
              {'3' , "...--"},
              {'4' , "....-"},
              {'5' , "....."},
              {'6' , "-...."},
              {'7' , "--..."},
              {'8' , "---.."},
              {'9' , "----."},
          };

      List<int> m_arrHistory = new List<int>();
      Boolean FirstTick = false;
      Boolean SpaceSent = true;
      int TicksSinceSpace = 0;
      Boolean Started = false;
      int PreliminaryTicks = 0;


      public override double Value
      {
        get
        {
          int tempValue = (int)Device.GetB();
          if (tempValue > 999) { tempValue = 999; }
          if (tempValue < 0) { tempValue = 0; }

          if (!Started)
          {
            PreliminaryTicks++;
            if (PreliminaryTicks > 10)
            {
              Started = true;
            }

            return 0;
          }

          int signalLength = 0;
          int dotHeight = 200;
          int dashHeight = 320;

          // Update the mem list
          String signal = "";

          if (!FirstTick && (tempValue > dotHeight))
          {
            m_arrHistory.Add(Convert.ToInt32(tempValue));
          }

          if (!FirstTick && m_arrHistory.Count > 0)
          {
            m_arrHistory.Add(Convert.ToInt32(tempValue));
          }

          if (FirstTick && (tempValue > dotHeight))
          {
            FirstTick = false;
          }

          if (!SpaceSent & m_arrHistory.Count == 0)
          {
            TicksSinceSpace++;
            if (TicksSinceSpace > 32)
            {
              // Send the space key
              Morse = " ";
              SendKeys.Send(" ");
              SpaceSent = true;
              TicksSinceSpace = 0;
            }
          }

          if (!FirstTick && m_arrHistory.Count > 96)
          {
            int nextOffset = 0;
            do
            {
              int fivePointValue = 0;
              for (int i = nextOffset; i < m_arrHistory.Count; i++)
              {
                int nextPointValue = 0;
                for (int x = i; x < m_arrHistory.Count; x++)
                {
                  if (m_arrHistory[x] > fivePointValue)
                  {
                    fivePointValue = m_arrHistory[x];
                    if (x + 1 < m_arrHistory.Count)
                    {
                      nextPointValue = m_arrHistory[x + 1];
                    }
                  }

                  if (m_arrHistory[x] < 8)
                  {
                    nextOffset = x + 1;
                    break;
                  }

                  if (x == m_arrHistory.Count - 1)
                  {
                    nextOffset = -1;
                  }
                }

                if (fivePointValue >= dashHeight)
                {
                  signal += "-";
                  signalLength++;
                  break;
                }
                else if (fivePointValue >= dotHeight & nextPointValue == 0)
                {
                  signal += ".";
                  signalLength++;
                  break;
                }

                if (i == m_arrHistory.Count - 1)
                {
                  nextOffset = -1;
                }

              }

              if (nextOffset < 0 | nextOffset == m_arrHistory.Count)
              {
                break;
              }

            } while (true);

            m_arrHistory.RemoveAt(0);

            // Check if the signal is morse
            try
            {
              // Make sure that we have a signal
              if (signal != "")
              {
                var myValue = Code.First(x => x.Value == signal);
                Morse = myValue.Key.ToString();
                SendKeys.Send(myValue.Key.ToString());
                signal = "";
                m_arrHistory.Clear();
                SpaceSent = false;
                TicksSinceSpace = 0;
              }
            }
            catch (Exception ex)
            {
              String err = ex.Message;
            }
          }

          if (m_arrHistory.Count > 0)
          { return 888; }

          return 0;
        }
      }

      string lucidcode.LucidScribe.TCMP.ITransConsciousnessPlugin.MorseCode
      {
        get
        {
          String temp = Morse;
          Morse = "";
          return temp;
        }
      }

      public override void Dispose()
      {
        Device.Dispose();
      }

    }

  }

}
