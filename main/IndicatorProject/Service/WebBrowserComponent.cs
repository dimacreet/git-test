

using System;
using System.Windows.Forms;

public class WebBrowserManagerComponent
{

    private Func<string> UpdateDataHandler;
    Action<WebBrowserManagerComponent, string, string> MethodDataHandler;

    public WebBrowser browser;

    public WebBrowserManagerComponent(WebBrowser browser, Func<string> UpdateDataHandler = null,
                                      Action<WebBrowserManagerComponent, string, string> MethodDataHandler = null, string html_css = "")
    {

        this.UpdateDataHandler = UpdateDataHandler;
        this.MethodDataHandler = MethodDataHandler;
        this.browser = browser;

        #region HTML STR

        var default_html_css = @"

<!DOCTYPE html>
<html>
<head>



<style>

.tr_hover {background-color:lightgray;mouse:hover;cursor:pointer;}
.tr_hover * {color:black}
.tr_outhover {cursor:pointer}

* {border:0px; margin:0px; padding:0px; font-family: border-collapse:collapse;font-size:15px;color:white;font-family:Calibri;}
th {background-color:gray}

table{border-collapse:collapse;}

H2 {font-size:20px;}

.guest_table {margin:25px;border-bottom:1px solid white; }
.guest_table TD, .guest_table TH  {border-top:1px solid white; padding:7px}
TH {text-align:left}

INPUT {color:white; background-color:#444444; width:50px;border-bottom:1px dotted white;}

</style>
</head>

<body bgcolor=#444444>
<span id=data>

</span>

</table>
</body>";

        if (html_css == "") html_css = default_html_css;

        browser.SetData(html_css);

        #endregion

        browser.SetScriptManager(HTMLAction);
        UpdateData();
    }


    public void UpdateData()
    {
        UpdateDataByID(browser, "data", UpdateDataHandler());
    }

    public static void UpdateDataByID(WebBrowser browser, string id, string html)
    {
        HtmlElement elementById = browser.Document.GetElementById(id);
        if (elementById == (HtmlElement)null)
            return;
        elementById.InnerHtml = html;
    }


    public void HTMLAction(string Method, string ID, WebBrowser browser)
    {
        MethodDataHandler(this, Method, ID);
    }
}