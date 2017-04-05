using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public static class Extensions123
{
    public static void SetScriptManager(this WebBrowser browser, Action<string, string, WebBrowser> act)
    {
        browser.ObjectForScripting = new ScriptManager(act, browser);
    }
}


[ComVisible(true)]
public class ScriptManager
{
    private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
    private List<Tuple<string, string, WebBrowser>> Actions = new List<Tuple<string, string, WebBrowser>>();
    private SimpleSpinLock slock = new SimpleSpinLock();
    private Action<string, string, WebBrowser> func;
    private WebBrowser browser;

    public ScriptManager(Action<string, string, WebBrowser> func, WebBrowser browser)
    {
        this.func = func;
        this.timer.Interval = 1;
        this.timer.Start();
        this.timer.Tick += new EventHandler(this.timer_Tick);
        this.browser = browser;
    }

    public void HTMLAction(string method, string parameters)
    {
        this.slock.Enter();
        this.Actions.Add(Tuple.Create<string, string, WebBrowser>(method, parameters, browser));
        this.slock.Leave();
    }

    private void timer_Tick(object sender, EventArgs e)
    {
        this.timer.Stop();
        if (this.Actions.Count == 0)
        {
            this.timer.Start();
        }
        else
        {
            this.slock.Enter();
            foreach (Tuple<string, string, WebBrowser> tuple in this.Actions)
                this.func(tuple.Item1, tuple.Item2, browser);

            this.Actions.Clear();
            this.slock.Leave();
            this.timer.Start();
        }
    }
}
