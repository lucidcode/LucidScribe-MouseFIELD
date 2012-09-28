using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace lucidcode.LucidScribe.Plugin.Mouse.Monitor
{

  public class MouseWatcher
  {
    private static event MouseHandler s_MouseMove;

    public delegate void MouseHandler(int X, int Y, int Button);

    public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    static int m_intUp;
    static int m_intDown;

    //Declare the hook handle as an int.
    static int hHook = 0;

    //Declare the global mouse hook constant.
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x201;
    private const int WM_LBUTTONUP = 0x202;

    //Declare MouseHookProcedure as a HookProc type.
    static HookProc MouseHookProcedure;

    /// <summary>
    /// Occurs when the mouse pointer is moved. 
    /// </summary>
    public static event MouseHandler MouseMove
    {
      add
      {
        s_MouseMove += value;
      }

      remove
      {
        s_MouseMove -= value;
      }
    }

    //Declare the wrapper managed POINT class.
    [StructLayout(LayoutKind.Sequential)]
    public class POINT
    {
      public int x;
      public int y;
    }

    //Declare the wrapper managed MouseHookStruct class.
    [StructLayout(LayoutKind.Sequential)]
    public class MouseHookStruct
    {
      public POINT pt;
      public int hwnd;
      public int wHitTestCode;
      public int dwExtraInfo;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern int SetWindowsHookEx(int idHook, HookProc lpfn,
    IntPtr hInstance, int threadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern bool UnhookWindowsHookEx(int idHook);

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern int CallNextHookEx(int idHook, int nCode,
    IntPtr wParam, IntPtr lParam);

    public static Boolean SetHook()
    {
      // Create an instance of HookProc.
      MouseHookProcedure = new HookProc(MouseHookProc);

      // Windows 7 
        hHook = SetWindowsHookEx(WH_MOUSE_LL, MouseHookProcedure, IntPtr.Zero, 0);

        // Server 2003
        if (hHook == 0) hHook = SetWindowsHookEx(WH_MOUSE_LL, MouseHookProcedure, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
     
        if (hHook == 0) return false;
      return true;
    }

    public static int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
      //Marshall the data from the callback.
      MouseHookStruct MyMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));

      int intButton = 0;
      switch ((int)wParam)
      {
        case WM_LBUTTONDOWN:
          intButton = 1;
          m_intUp = 0;
          m_intDown = 50;
          break;
        case WM_LBUTTONUP:
          intButton = -1;
          m_intUp = 50;
          m_intDown = 0;
          break;
      }

      if (m_intUp > 0)
      {
        m_intUp = m_intUp - 1;
        intButton = -1;
      }

      if (m_intDown > 0)
      {
        m_intDown = m_intDown - 1;
        intButton = 1;
      }

      s_MouseMove.Invoke(MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y, intButton);
      return CallNextHookEx(hHook, nCode, wParam, lParam);
    }

    /// <summary>
    /// Provides data for the MouseClickExt and MouseMoveExt events. It also provides a property Handled.
    /// Set this property to <b>true</b> to prevent further processing of the event in other applications.
    /// </summary>
    public class MouseEventExtArgs : MouseEventArgs
    {
      /// <summary>
      /// Initializes a new instance of the MouseEventArgs class. 
      /// </summary>
      /// <param name="buttons">One of the MouseButtons values indicating which mouse button was pressed.</param>
      /// <param name="clicks">The number of times a mouse button was pressed.</param>
      /// <param name="x">The x-coordinate of a mouse click, in pixels.</param>
      /// <param name="y">The y-coordinate of a mouse click, in pixels.</param>
      /// <param name="delta">A signed count of the number of detents the wheel has rotated.</param>
      public MouseEventExtArgs(MouseButtons buttons, int clicks, int x, int y, int delta)
        : base(buttons, clicks, x, y, delta)
      { }

      /// <summary>
      /// Initializes a new instance of the MouseEventArgs class. 
      /// </summary>
      /// <param name="e">An ordinary <see cref="MouseEventArgs"/> argument to be extended.</param>
      internal MouseEventExtArgs(MouseEventArgs e)
        : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
      { }

      private bool m_Handled;

      /// <summary>
      /// Set this property to <b>true</b> inside your event handler to prevent further processing of the event in other applications.
      /// </summary>
      public bool Handled
      {
        get { return m_Handled; }
        set { m_Handled = value; }
      }
    }

  }
}
