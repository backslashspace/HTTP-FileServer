using System;
using System.Data.SQLite;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using BSS.Logging;
using BSS.Threading;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendControlPanel(Socket connection, ref readonly UserDB.User user, String insertInfoString = null, Boolean setSelfURL = false)
            {
                Log.Debug("controlPanel\\controlPanel.html", "SendFile()");

                String fileContent = Worker.ReadFileText("controlPanel\\controlPanel.html");

                ControlPanelInsertUsers(ref fileContent, connection);

                fileContent = Regex.Replace(fileContent, "<!-- #DISPLAY#USERNAME#ANCHOR# -->", user.DisplayName);
                fileContent = Regex.Replace(fileContent, "<!-- #THREADPOOL#ANCHOR# -->", $"{ThreadPoolFast.Count}/{ThreadPoolFast.Capacity}");

                if (insertInfoString != null)
                {
                    fileContent = Regex.Replace(fileContent, "<!-- #INFO#ANCHOR# -->", insertInfoString);
                }

                if (setSelfURL)
                {
                    fileContent = Regex.Replace(fileContent, "<!-- #SCRIPT#ANCHOR# -->", "<script type=\"text/javascript\">\r\n\t\t\twindow.history.replaceState(null, document.title, \"/fileSharing/controlPanel\")\r\n\t\t</script>");
                }

                Byte[] buffer = Encoding.UTF8.GetBytes(fileContent);

                HTTP.ContentOptions contentOptions = new(HTTP.ContentType.HTML);
                HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, contentOptions, (UInt64)buffer.LongLength);
                HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

                connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
                connection.Send(buffer, 0, buffer.Length, SocketFlags.None);

                Worker.CloseConnection(connection);
            }
        }

        internal static Boolean ControlPanelInsertUsers(ref String fileContent, Socket connection)
        {
            if (!UserDB.GetDatabaseConnection(out SQLiteConnection databaseConnection))
            {
                Log.FastLog("Database not initialized, this should not happen ", LogSeverity.Critical, "ControlPanel");
                STATIC.Send_500(connection);
                return false;
            };

            SQLiteCommand command = databaseConnection.CreateCommand();
            command.CommandText = "SELECT DisplayName,IsAdministrator,IsEnabled,LoginUsername FROM User WHERE IsAdministrator = 0";
            SQLiteDataReader dataReader = command.ExecuteReader();

            if (!dataReader.Read())
            {
                databaseConnection.Close();
                databaseConnection.Dispose();
                return true;
            }

            if (!Tools.CheckFilesDirectory(connection)) return false;
            if (!Tools.GetUserFilesInfo(connection, dataReader.GetString(3), out String fileString)) return false;

            fileContent = Regex.Replace(fileContent, "<!-- #USER#HEAD#ANCHOR# -->", CreateListUser(fileString, dataReader));

            while (dataReader.Read())
            {
                if (!Tools.GetUserFilesInfo(connection, dataReader.GetString(3), out fileString)) return false;
                
                fileContent = Regex.Replace(fileContent, "<!-- #USER#ANCHOR# -->", CreateListUser(fileString, dataReader));
            }

            databaseConnection.Close();
            databaseConnection.Dispose();
            return true;
        }

        private static String CreateListUser(String fileString, SQLiteDataReader dataReader) => 
"<hr style=\"margin-top: 20px; margin-left: 10em; margin-right: 10em;\" />\r\n" +
"\r\n" +
"<div style=\"display: grid; grid-template-columns: 2fr 1fr 1fr 2fr; gap: 0px; margin-top: 20px\">\r\n" +
    "<div style=\"text-align: right;\">\r\n" +
        $"<p>'{HttpUtility.HtmlEncode(dataReader.GetString(0))}'  ({HttpUtility.HtmlEncode(dataReader.GetString(3))})</p>\r\n" +
    "</div>\r\n" +
    "<div style=\"text-align: center;\">\r\n" +
        "<form method=\"POST\" action=\"/fileSharing/controlPanel/config\">\r\n" +
            $"<input type=\"text\" name=\"name\" value=\"{HttpUtility.HtmlEncode(dataReader.GetString(3))}\" hidden />\r\n" +
            $"<button type=\"submit\" value=\"{HttpUtility.HtmlEncode(dataReader.GetString(3))}\" style=\"width: 120px; height:28px; font-size: 14px; margin-top: 12px\">Manage</button>\r\n" +
        "</form>\r\n" +
    "</div>\r\n" +
    "<div style=\"text-align: center;\">\r\n" +
        "<form method=\"POST\" action=\"/fileSharing/controlPanel/userFiles\">\r\n" +
            $"<input type=\"text\" name=\"name\" value=\"{HttpUtility.HtmlEncode(dataReader.GetString(3))}\" hidden />\r\n" +
            "<button type=\"submit\" style=\"width: 120px; height:28px; font-size: 14px; margin-top: 12px\">View Files</button>\r\n" +
        "</form>\r\n" +
    "</div>\r\n" +
    "<div style=\"text-align: left;\">\r\n" +
        $"<p>{fileString}</p>\r\n" +
    "</div>\r\n" +
"</div>\r\n" +
"\r\n" +
"<!-- #USER#ANCHOR# -->\r\n";
    }
}