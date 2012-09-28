using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace lucidcode.LucidScribe.Plugin.Mouse.Monitor
{
  public partial class MainForm : Form
  {

    public MainForm()
    {
      InitializeComponent();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
      MouseWatcher.MouseMove += new MouseWatcher.MouseHandler(MouseWatcher_MouseMove);
      MouseWatcher.SetHook();
    }

    void MouseWatcher_MouseMove(int X, int Y, int Button)
    {
      Text = "LucidScribe.Mouse(" + X + "," + Y + "," + Button + ")";
    }

  }

}
