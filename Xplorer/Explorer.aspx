<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Explorer.aspx.cs" Inherits="Xplorer.Explorer" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>File Browser</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <link href="css/jquery-ui-1.7.1.custom.css" rel="stylesheet" type="text/css" />
    <link href="css/elfinder.css" rel="stylesheet" type="text/css" />
    <link href="css/codemirror.css" rel="stylesheet" type="text/css" />
    <script src="js/jquery-min.js" type="text/javascript"></script>
    <script src="js/jquery-ui-min.js" type="text/javascript"></script>
    <script src="js/elfinder.min.js" type="text/javascript"></script>
    <script src="js/codemirror.js" type="text/javascript"></script>
    <script src="js/htmlmixed.js" type="text/javascript"></script>
    <script src="js/css.js" type="text/javascript"></script>
    <script src="js/javascript.js" type="text/javascript"></script>
</head>
<body>
    <script type="text/javascript" charset="utf-8">
        $(function () {
            var f = $('#finder').elfinder({
                url: 'core/Connector.ashx',
                lang: 'en',
                //				editorCallback: function (url) {
                //					if (window.console && window.console.log) {
                //						window.console.log(url);
                //					} else {
                //						alert(url);
                //					}

                //				},
                closeOnEditorCallback: false,
                docked: false,
                height: $(document).height()-100,
                dirPath: '<%= BaseDirectory %>'
            })
        });
    </script>
    <div id="finder">
        finder</div>
</body>
</html>
